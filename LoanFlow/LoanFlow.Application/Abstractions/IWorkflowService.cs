namespace LoanFlow.Application.Abstractions;

public interface IWorkflowService
{
    Task StartLoanApprovalProcessAsync(Guid loanApplicationId, object variables);
}