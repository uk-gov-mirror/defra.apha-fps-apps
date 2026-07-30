namespace Apha.PACT.Core.Entities
{
    /// <summary>
    /// Represents a row in fps.vworkgroup_general.
    /// Used exclusively by the Plan CrossTab pivot to replicate the
    /// INNER JOIN fps.vworkgroup_general in vw_test_plan_cost_pivot_src.
    /// </summary>
    public partial class WorkGroupGeneralView
    {
        public string WorkGroup { get; set; } = null!;

        public string? ProfitCentre { get; set; }

        public int? FpsYear { get; set; }
    }
}
