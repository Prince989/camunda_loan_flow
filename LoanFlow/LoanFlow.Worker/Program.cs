// See https://aka.ms/new-console-template for more information
using LoanFlow.Worker.Camunda;

Console.WriteLine("LoanFlow Worker started");

var http = new HttpClient
{
    BaseAddress = new Uri("http://localhost:8080/engine-rest/")
};

var workerId = $"loanflow-worker-{Environment.MachineName}";
var camunda = new CamundaExternalTaskClient(http, workerId);

while (true)
{
    await HandleLoanRegister(camunda);
    await HandleRiskCheck(camunda);
    await HandleLoanApprove(camunda);
    await HandleLoanReject(camunda);
    await HandleNotifyApplicant(camunda);
    
    await Task.Delay(1000);
}

static async Task HandleLoanRegister(CamundaExternalTaskClient camunda)
{
    var tasks = await camunda.FetchAsync("loan_register");

    foreach (var task in tasks)
    {
        Console.WriteLine($"[loan_register] Handling task {task.Id}");

        // nothing special for now – just acknowledge
        await camunda.CompleteAsync(task.Id);
    }
}

static async Task HandleRiskCheck(CamundaExternalTaskClient camunda)
{
    var tasks = await camunda.FetchAsync("risk_check");

    foreach (var task in tasks)
    {
        var income = task.Variables["monthlyIncome"].AsDecimal();
        var amount = task.Variables["amount"].AsDecimal();

        var riskScore =
            amount > income * 20 ? 50 :
            amount > income * 10 ? 50 :
            50;

        Console.WriteLine($"[risk_check] Task {task.Id} → riskScore={riskScore}");

        await camunda.CompleteAsync(task.Id, new { riskScore });
    }
}

static async Task HandleLoanApprove(CamundaExternalTaskClient camunda)
{
    var tasks = await camunda.FetchAsync("loan_approve");

    foreach (var task in tasks)
    {
        Console.WriteLine($"[loan_approve] Handling task {task.Id}");

        // Here is where we'd call domain: application.Approve()
        // For now, we just set a workflow variable:
        await camunda.CompleteAsync(task.Id, new
        {
            decision = "APPROVED",
            decisionAt = DateTime.UtcNow.ToString("O")
        });
    }
}

static async Task HandleLoanReject(CamundaExternalTaskClient camunda)
{
    var tasks = await camunda.FetchAsync("loan_reject");

    foreach (var task in tasks)
    {
        Console.WriteLine($"[loan_reject] Handling task {task.Id}");

        await camunda.CompleteAsync(task.Id, new
        {
            decision = "REJECTED",
            rejectionReason = "Risk score too high",
            decisionAt = DateTime.UtcNow.ToString("O")
        });
    }
}

static async Task HandleNotifyApplicant(CamundaExternalTaskClient camunda)
{
    var tasks = await camunda.FetchAsync("notify_applicant");

    foreach (var task in tasks)
    {
        var email = task.Variables.TryGetValue("email", out var e) ? e.AsString() : null;
        var decision = task.Variables.TryGetValue("decision", out var d) ? d.AsString() : null;

        Console.WriteLine($"[notify_applicant] To={email}, decision={decision}, task={task.Id}");

        // Pretend to send email/SMS/etc.
        await camunda.CompleteAsync(task.Id);
    }
}

