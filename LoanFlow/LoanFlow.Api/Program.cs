using LoanFlow.Api.Security;
using LoanFlow.Application.Abstractions;
using LoanFlow.Application.BoundedContext;
using LoanFlow.Application.Commands;
using LoanFlow.Application.UseCases;
using LoanFlow.Infrastructure.Camunda;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IWorkflowService, CamundaWorkflowService>(http =>
{
    http.BaseAddress = new Uri("http://localhost:8080/engine-rest/");
});

// Temporary: wire handler directly (later we’ll use MediatR)
builder.Services.AddScoped<SubmitLoanApplicationHandler>();

// TODO later: repository + DB
builder.Services.AddSingleton<ILoanApplicationRepository, InMemoryLoanApplicationRepository>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, HeaderCurrentUser>();

builder.Services.AddHttpClient<CamundaTaskClient>(http =>
{
    http.BaseAddress = new Uri("http://localhost:8080/engine-rest/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/loans", async (
    SubmitLoanApplicationCommand command,
    SubmitLoanApplicationHandler handler,
    CancellationToken ct) =>
{
    var id = await handler.HandleAsync(command, ct);
    return Results.Ok(new { loanApplicationId = id });
});

// curl -H "X-User: andro" -H "X-Groups: loan_officers" http://localhost:5000/api/tasks
app.MapGet("/api/tasks", async (ICurrentUser user, CamundaTaskClient camunda) =>
{
    // Show:
    // - tasks already assigned to me
    // - tasks available to my groups (unassigned claimable tasks)
    var assigned = await camunda.GetAssignedTasksAsync(user.UserId);

    var groupTasks = new List<CamundaTaskDto>();
    foreach (var g in user.Groups)
        groupTasks.AddRange(await camunda.GetCandidateGroupTasksAsync(g));

    // remove duplicates if any
    var all = assigned.Concat(groupTasks)
        .GroupBy(t => t.Id)
        .Select(g => g.First())
        .ToList();

    return Results.Ok(all);
});

// curl -X POST -H "X-User: andro" http://localhost:5000/api/tasks/<TASK_ID>/claim
app.MapPost("/api/tasks/{taskId}/claim", async (string taskId, ICurrentUser user, CamundaTaskClient camunda) =>
{
    await camunda.ClaimAsync(taskId, user.UserId);
    return Results.Ok();
});

/*
 curl -X POST -H "X-User: andro" -H "Content-Type: application/json" \
  -d '{"approved": true, "comment": "OK"}' \
  http://localhost:5000/api/tasks/<TASK_ID>/complete 
 */
app.MapPost("/api/tasks/{taskId}/complete", async (
    string taskId,
    ICurrentUser user,
    CamundaTaskClient camunda,
    ManualReviewDecisionDto body) =>
{
    // IMPORTANT: In your real app you should verify:
    // - task is assigned to user.UserId
    // - user has permission based on your domain rules
    // For now, we keep it simple.

    await camunda.CompleteAsync(taskId, new Dictionary<string, object?>
    {
        ["manualApproved"] = body.Approved,
        ["manualComment"] = body.Comment
    });

    return Results.Ok();
});


app.Run();