namespace LoanFlow.Api.Security;

public class HeaderCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;

    public HeaderCurrentUser(IHttpContextAccessor http) => _http = http;

    public string UserId =>
        _http.HttpContext?.Request.Headers["X-User"].ToString()
        ?? "demo";

    public IReadOnlyCollection<string> Groups =>
        (_http.HttpContext?.Request.Headers["X-Groups"].ToString() ?? "loan_officers")
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}