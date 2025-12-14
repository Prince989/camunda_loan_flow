using System.Net.Http.Json;

namespace LoanFlow.Infrastructure.Camunda;

public class CamundaTaskClient
{
    private readonly HttpClient _http;

    public CamundaTaskClient(HttpClient http) => _http = http;

    public async Task<List<CamundaTaskDto>> GetCandidateGroupTasksAsync(string group)
    {
        var url = $"task?candidateGroup={Uri.EscapeDataString(group)}";
        return await _http.GetFromJsonAsync<List<CamundaTaskDto>>(url) ?? new();
    }

    public async Task<List<CamundaTaskDto>> GetAssignedTasksAsync(string assignee)
    {
        var url = $"task?assignee={Uri.EscapeDataString(assignee)}";
        return await _http.GetFromJsonAsync<List<CamundaTaskDto>>(url) ?? new();
    }

    public async Task ClaimAsync(string taskId, string userId)
    {
        var resp = await _http.PostAsJsonAsync($"task/{taskId}/claim", new { userId });
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Claim failed: {resp.StatusCode}\n{body}");
        }
    }

    public async Task CompleteAsync(string taskId, Dictionary<string, object?> vars)
    {
        var payload = new
        {
            variables = vars.ToDictionary(
                kv => kv.Key,
                kv => new { value = kv.Value })
        };

        var resp = await _http.PostAsJsonAsync($"task/{taskId}/complete", payload);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Complete failed: {resp.StatusCode}\n{body}");
        }
    }
}

public class CamundaTaskDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Assignee { get; set; }
    public string? ProcessInstanceId { get; set; }
    public string? TaskDefinitionKey { get; set; }
    public string? Created { get; set; }
}