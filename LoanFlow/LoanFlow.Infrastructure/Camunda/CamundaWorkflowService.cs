using System.Net.Http.Json;
using LoanFlow.Application.Abstractions;

namespace LoanFlow.Infrastructure.Camunda;

public class CamundaWorkflowService : IWorkflowService
{
    private readonly HttpClient _http;

    public CamundaWorkflowService(HttpClient http)
    {
        _http = http;
    }

    public async Task StartLoanApprovalProcessAsync(Guid loanApplicationId, object variables)
    {
        // Camunda expects variables in a special format: { varName: { value: ..., type: ... } }
        // We'll build a minimal payload using "value" only (Camunda infers type in many cases).
        // If you ever hit type issues, we’ll upgrade to explicit type mapping.
        var payload = new
        {
            businessKey = loanApplicationId.ToString(),
            variables = ToCamundaVariables(variables)
        };

        var url = "process-definition/key/loan_approval/start";
        var response = await _http.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Camunda start process failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{body}");
        }
    }

    private static Dictionary<string, object> ToCamundaVariables(object obj)
    {
        // Converts anonymous object -> Camunda variable map:
        // { "amount": { "value": 123 }, "riskScore": { "value": 40 } }
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        var props = obj.GetType().GetProperties();
        foreach (var p in props)
        {
            var value = p.GetValue(obj);
            dict[p.Name] = new { value };
        }

        return dict;
    }
}