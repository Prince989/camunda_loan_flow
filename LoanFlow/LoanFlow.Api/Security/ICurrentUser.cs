namespace LoanFlow.Api.Security;

public record ManualReviewDecisionDto(bool Approved, string? Comment);

public interface ICurrentUser
{
    string UserId { get; }
    IReadOnlyCollection<string> Groups { get; }
}