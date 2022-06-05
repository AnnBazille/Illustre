namespace Data.Contracts.Common;

public abstract class FailableRequest
{
    public bool IsFirstAttempt { get; set; } = true;

    public string? ErrorMessage { get; set; }
}
