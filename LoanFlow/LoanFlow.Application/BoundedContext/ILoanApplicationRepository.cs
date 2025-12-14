using LoanFlow.Domain.Entities;

namespace LoanFlow.Application.BoundedContext;

public interface ILoanApplicationRepository
{
    Task AddAsync(LoanApplication application, CancellationToken cancellationToken = default);
    Task<LoanApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}