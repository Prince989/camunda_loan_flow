using System.Net.Http.Json;

namespace LoanFlow.Worker.Camunda;

public sealed class CamundaTypedVariable
{
    public object? value { get; set; }
    public string? type { get; set; }
}

public class CamundaExternalTaskClient
{
    private readonly HttpClient _http;
    private readonly string _workerId;

    public CamundaExternalTaskClient(HttpClient http, string workerId)
    {
        _http = http;
        _workerId = workerId;
    }

    public async Task<List<ExternalTaskDto>> FetchAsync(string topic)
    {
        var request = new FetchAndLockRequest(
            WorkerId: _workerId,
            MaxTasks: 5,
            Topics: new()
            {
                new TopicSubscription(topic, LockDuration: 10_000)
            });

        var response = await _http.PostAsJsonAsync(
            "external-task/fetchAndLock", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<ExternalTaskDto>>()
               ?? new();
    }

    public async Task CompleteAsync(string taskId, object? variables = null)
    {
        Dictionary<string, CamundaTypedVariable>? camundaVars = null;

        if (variables != null)
        {
            camundaVars = variables.GetType()
                .GetProperties()
                .ToDictionary(
                    p => p.Name,
                    p =>
                    {
                        var v = p.GetValue(variables);

                        return v switch
                        {
                            int i => new CamundaTypedVariable { value = i, type = "Integer" },
                            long l => new CamundaTypedVariable { value = l, type = "Long" },
                            decimal d => new CamundaTypedVariable { value = (double)d, type = "Double" },
                            double db => new CamundaTypedVariable { value = db, type = "Double" },
                            bool b => new CamundaTypedVariable { value = b, type = "Boolean" },
                            string s => new CamundaTypedVariable { value = s, type = "String" },
                            null => new CamundaTypedVariable { value = null, type = "Null" },
                            _ => new CamundaTypedVariable { value = v.ToString(), type = "String" }
                        };
                    });
        }

        var payload = new
        {
            workerId = _workerId,
            variables = camundaVars
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        Console.WriteLine($"[complete payload] {json}");
        
        var resp = await _http.PostAsJsonAsync($"external-task/{taskId}/complete", payload);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Complete failed: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{body}");
        }
    }
}