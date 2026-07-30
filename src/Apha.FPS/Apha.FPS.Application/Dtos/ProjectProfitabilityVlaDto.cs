namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Service-layer DTO for a single row in the Project Profitability VLA list.
    /// Maps to <see cref="Apha.FPS.Core.Entities.ProjectProfitabilityVlaView"/> and is
    /// consumed by the backend <c>IProjectService.GetProjectProfitabilityVlaAsync()</c>.
    /// Property names are aligned with <c>ProjectProfitabilityVlaRes</c> to simplify
    /// the API-layer mapper.
    /// </summary>
    public class ProjectProfitabilityVlaDto
    {
        /// <summary>Optional numeric row identifier from the underlying view.</summary>
        public int? Id { get; set; }

        /// <summary>Job code (project code). The natural row key for the VLA profitability list.</summary>
        public string JobCode { get; set; } = null!;

        /// <summary>Program number. Used to populate the Program filter dropdown.</summary>
        public string? Program { get; set; }

        /// <summary>Customer name. VLA-specific field absent from base ProjectProfitabilityDto.</summary>
        public string? Customer { get; set; }

        /// <summary>Manager name. VLA-specific field absent from base ProjectProfitabilityDto.</summary>
        public string? Manager { get; set; }

        /// <summary>Project status. Static filter options: "Approved", "Completed", "Not Approved".</summary>
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
        /// A negative value triggers the red highlight in projectprofitability_vla.js.
        /// </summary>
        public decimal OffTarget { get; set; }
    }
}
