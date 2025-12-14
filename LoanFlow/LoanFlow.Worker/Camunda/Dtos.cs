using System.Text.Json;

namespace LoanFlow.Worker.Camunda;

public record FetchAndLockRequest(
    string WorkerId,
    int MaxTasks,
    List<TopicSubscription> Topics);

public record TopicSubscription(
    string TopicName,
    int LockDuration);
    
    
public class ExternalTaskDto
{
    public string Id { get; set; } = default!;
    public string TopicName { get; set; } = default!;
    public Dictionary<string, CamundaVariable> Variables { get; set; } = new();
}

public class CamundaVariable
{
    public JsonElement Value { get; set; }

    public decimal AsDecimal()
        => Value.ValueKind switch
        {
            JsonValueKind.Number => Value.GetDecimal(),
            JsonValueKind.String => decimal.Parse(Value.GetString()!),
            _ => throw new InvalidOperationException($"Cannot convert {Value.ValueKind} to decimal")
        };

    public int AsInt()
        => Value.ValueKind switch
        {
            JsonValueKind.Number => Value.GetInt32(),
            JsonValueKind.String => int.Parse(Value.GetString()!),
            _ => throw new InvalidOperationException($"Cannot convert {Value.ValueKind} to int")
        };

    public string? AsString()
        => Value.ValueKind == JsonValueKind.Null ? null : Value.GetString();
}

public class Dtos
{
}