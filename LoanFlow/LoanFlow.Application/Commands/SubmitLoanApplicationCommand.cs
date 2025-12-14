using LoanFlow.Domain.Entities;

namespace LoanFlow.Application.Commands;

public record SubmitLoanApplicationCommand(
    string ApplicantName,
    string Email,
    decimal Amount,
    decimal CreditScore,
    int EmploymentYears,
    decimal ExistingDebt,
    CountryRisk CountryRisk,
    decimal MonthlyIncome,
    string Currency = "EUR");