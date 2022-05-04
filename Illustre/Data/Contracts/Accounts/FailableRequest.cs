namespace Data.Contracts.Accounts;

public abstract class FailableRequest
{
    public bool IsFirstAttempt { get; set; } = true;
}
