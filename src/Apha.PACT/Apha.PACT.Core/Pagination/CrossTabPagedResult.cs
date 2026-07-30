namespace Apha.PACT.Core.Pagination;

public class CrossTabPagedResult
{
    public List<string> Columns { get; set; } = [];
    public List<Dictionary<string, string?>> Rows { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
