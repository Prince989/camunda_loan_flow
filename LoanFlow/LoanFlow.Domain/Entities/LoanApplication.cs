namespace LoanFlow.Domain.Entities;

public enum LoanApplicationStatus
{
    Submitted,
    InReview,
    Approved,
    Rejected
}

public enum CountryRisk
{
    LOW,
    MEDIUM,
    HIGH
}

public class LoanApplication
{
    public Guid Id { get; private set; }
    public string ApplicantName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EUR";
    public decimal CreditScore { get; private set; }
    public int EmploymentYears { get; private set; }
    public decimal ExistingDebt { get; private set; }
    public CountryRisk CountryRisk { get; private set; } = CountryRisk.LOW;
    public decimal MonthlyIncome { get; private set; }
    
    public LoanApplicationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DecisionAt { get; private set; }
    public string? RejectionReason { get; private set; }

    // For EF Core / serializers
    private LoanApplication() { }

    private LoanApplication(
        Guid id,
        string applicantName,
        string email,
        decimal amount,
        decimal creditScore,
        int employmentYears,
        decimal existingDebt,
        CountryRisk countryRisk,
        decimal monthlyIncome,
        string currency)
    {
        Id = id;
        ApplicantName = applicantName;
        Email = email;
        Amount = amount;
        MonthlyIncome = monthlyIncome;
        Currency = currency;
        EmploymentYears = employmentYears;
        ExistingDebt = existingDebt;
        CountryRisk = countryRisk;
        CreditScore = creditScore;
        Status = LoanApplicationStatus.Submitted;
        CreatedAt = DateTime.UtcNow;
    }

    public static LoanApplication Submit(
        string applicantName,
        string email,
        decimal amount,
        decimal creditScore,
        int employmentYears,
        decimal existingDebt,
        CountryRisk countryRisk,
        decimal monthlyIncome,
        string currency = "EUR")
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (monthlyIncome <= 0) throw new ArgumentOutOfRangeException(nameof(monthlyIncome));
        if (string.IsNullOrWhiteSpace(applicantName))
            throw new ArgumentException("Applicant name is required", nameof(applicantName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        return new LoanApplication(
            Guid.NewGuid(),
            applicantName,
            email,
            amount,
            creditScore,
            employmentYears,
            existingDebt,
            countryRisk,
            monthlyIncome,
            currency);
    }

    public void MarkInReview()
    {
        if (Status != LoanApplicationStatus.Submitted)
            return;

        Status = LoanApplicationStatus.InReview;
    }

    public void Approve()
    {
        if (Status == LoanApplicationStatus.Approved)
            return;

        Status = LoanApplicationStatus.Approved;
        DecisionAt = DateTime.UtcNow;
        RejectionReason = null;
    }

    public void Reject(string reason)
    {
        if (Status == LoanApplicationStatus.Rejected)
            return;

        Status = LoanApplicationStatus.Rejected;
        DecisionAt = DateTime.UtcNow;
        RejectionReason = reason;
    }
}
