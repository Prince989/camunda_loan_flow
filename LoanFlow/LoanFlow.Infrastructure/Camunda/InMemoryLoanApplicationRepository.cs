using LoanFlow.Application.BoundedContext;
using LoanFlow.Domain.Entities;

namespace LoanFlow.Infrastructure.Camunda;

public class InMemoryLoanApplicationRepository : ILoanApplicationRepository
{
    private static readonly Dictionary<Guid, LoanApplication> Store = new();

    public Task AddAsync(LoanApplication application, CancellationToken cancellationToken = default)
    {
        Store[application.Id] = application;
        return Task.CompletedTask;
    }

    public Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Store.TryGetValue(id, out var app);
        return Task.FromResult(app);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
