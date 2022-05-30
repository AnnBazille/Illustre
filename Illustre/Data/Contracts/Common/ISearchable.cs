namespace Data.Contracts.Common;

public interface ISearchable
{
    public int Skip { get; set; }

    public string? SearchPattern { get; set; }
}
