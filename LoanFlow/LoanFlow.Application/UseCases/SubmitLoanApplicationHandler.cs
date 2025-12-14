using LoanFlow.Application.Abstractions;
using LoanFlow.Application.BoundedContext;
using LoanFlow.Application.Commands;
using LoanFlow.Domain.Entities;

namespace LoanFlow.Application.UseCases;

public class SubmitLoanApplicationHandler
{
    private readonly ILoanApplicationRepository _repository;
    private readonly IWorkflowService _workflowService;

    public SubmitLoanApplicationHandler(
        ILoanApplicationRepository repository,
        IWorkflowService workflowService)
    {
        _repository = repository;
        _workflowService = workflowService;
    }

    public async Task<Guid> HandleAsync(
        SubmitLoanApplicationCommand command,
        CancellationToken cancellationToken = default)
    {
        var application = LoanApplication.Submit(
            command.ApplicantName,
            command.Email,
            command.Amount,
            command.CreditScore,
            command.EmploymentYears,
            command.ExistingDebt,
            command.CountryRisk,
            command.MonthlyIncome,
            command.Currency);

        await _repository.AddAsync(application, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var workflowVariables = new
        {
            loanApplicationId = application.Id.ToString(),
            amount = (double)application.Amount,
            monthlyIncome = (double)application.MonthlyIncome,
            applicantName = application.ApplicantName,
            email = application.Email,
            existingDebt = (double)application.ExistingDebt,
            countryRisk = application.CountryRisk.ToString(),
            creditScore = (double)application.CreditScore,
            employmentYears = application.EmploymentYears,
        };

        await _workflowService.StartLoanApprovalProcessAsync(
            application.Id,
            workflowVariables);

        return application.Id;
    }
}