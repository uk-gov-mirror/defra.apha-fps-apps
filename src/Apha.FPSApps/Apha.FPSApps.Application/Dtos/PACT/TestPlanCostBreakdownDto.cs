namespace Apha.FPSApps.Application.Dtos.PACT;

public class TestPlanCostBreakdownDto
{
    public List<string> Columns { get; set; } = [];
    public List<Dictionary<string, string?>> Rows { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
