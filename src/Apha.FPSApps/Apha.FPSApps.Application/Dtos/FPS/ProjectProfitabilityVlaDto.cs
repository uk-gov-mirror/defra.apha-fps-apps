namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for a single row in the Project Profitability VLA list.
    /// Mirrors <c>Apha.FPS.Application.Dtos.ProjectProfitabilityVlaDto</c> (backend Phase 3).
    /// Consumed by <c>IFpsProjectApiClient.GetProjectProfitabilityVlaAsync()</c> and
    /// the FPS ProjectProfitabilityVla Razor view / PageModel.
    /// </summary>
    public class ProjectProfitabilityVlaDto
    {
        /// <summary>Optional numeric row identifier from the underlying view.</summary>
        public int? Id { get; set; }

        /// <summary>Job code (project code). The natural row key for the VLA profitability list.</summary>
        public string JobCode { get; set; } = null!;

        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
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

        /// <summary>Sum of all cost categories.</summary>
        public decimal TotalCosts { get; set; }

        /// <summary>Budget (CVL) for the project. Nullable if no budget has been set.</summary>
        public decimal? Budget { get; set; }

        /// <summary>Actual profit for the job code (Budget − TotalCosts).</summary>
        public decimal Profit { get; set; }

        /// <summary>Target profit for the programme.</summary>
        public decimal TargetProfit { get; set; }

        /// <summary>
        /// Difference between actual profit and target profit.
        /// A negative value triggers the red highlight in the ProjectProfitabilityVla grid
        /// (mirrors projectprofitability_vla.js updateSummary behaviour).
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
