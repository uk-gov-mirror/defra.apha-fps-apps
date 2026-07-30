namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a single row in the Project Profitability VLA list
    /// (<c>GET /api/v1/project/profitability-vla</c>).
    /// Includes the project identifier, filter-dimension fields (Program, Customer,
    /// Manager, Status), and all nine financial summary columns rendered in the
    /// HTML prototype summary bar.
    /// </summary>
    public class ProjectProfitabilityVlaRes
    {
        /// <summary>Row identifier from the underlying view.</summary>
        public int Id { get; set; }

        /// <summary>Project code or short name displayed in the grid Project column.</summary>
        public string Project { get; set; } = null!;

        /// <summary>Program number / name. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        /// <summary>Customer name. Used to populate the Customer filter dropdown.</summary>
        public string? Customer { get; set; }

        /// <summary>Manager name. Used to populate the Manager filter dropdown.</summary>
        public string? Manager { get; set; }

        /// <summary>Project status (e.g. "Approved", "Completed", "Not Approved").</summary>
        public string? Status { get; set; }

        // ── Financial columns ─────────────────────────────────────────────────

        /// <summary>Total staff costs for the job code.</summary>
        public decimal StaffCosts { get; set; }

        /// <summary>Total test costs for the job code.</summary>
        public decimal TestCost { get; set; }

        /// <summary>Total animal costs for the job code.</summary>
        public decimal AnimalCosts { get; set; }

        /// <summary>Total additional costs for the job code.</summary>
        public decimal AdditionalCosts { get; set; }

        /// <summary>Sum of all cost categories (StaffCosts + TestCost + AnimalCosts + AdditionalCosts).</summary>
        public decimal TotalCosts { get; set; }

        /// <summary>Budget (CVL) for the project. Nullable if not set.</summary>
        public decimal? Budget { get; set; }

        /// <summary>Actual profit for the job code.</summary>
        public decimal Profit { get; set; }

        /// <summary>Target profit for the project.</summary>
        public decimal TargetProfit { get; set; }

        /// <summary>Difference between actual profit and target profit. Negative value triggers red highlight in UI.</summary>
        public decimal OffTarget { get; set; }

        // ── Pagination metadata ───────────────────────────────────────────────

        /// <summary>
        /// Total number of records matching the current filter, used by the frontend
        /// DataGrid for server-side pagination.  Populated on list responses only;
        /// defaults to 0 for single-record lookups.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
