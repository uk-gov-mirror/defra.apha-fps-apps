using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    public class AgrupExportRow
    {
        [Display(Name = "Test Code")]
        public string TestCode { get; set; } = string.Empty;

        [Display(Name = "Buyer")]
        public string Buyer { get; set; } = string.Empty;

        [Display(Name = "Agrup")]
        public decimal? Agrup { get; set; }

        [Display(Name = "Agrup New")]
        public decimal? AgrupNew { get; set; }

        [Display(Name = "Change")]
        public decimal? Change { get; set; }

        [Display(Name = "No Required")]
        public double? NoRequired { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Active")]
        public short? Active { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }

        // ── DR-UI-02: routing fields (CR056/CR059/CR062) ─────────────────────────
        // Existing rows: reference-only (protected, see BuildFecAgrupSheets) — the current
        // live values, for visibility only; the API rejects any attempted change regardless
        // (DR-API-05). New rows: the user supplies routing through these controlled columns
        // (ProjectBuyerCode and/or TestBuyerWorkGroup) rather than hand-authoring a
        // concatenated code — TestBuyerWorkGroup has no live counterpart, so it is always
        // blank on a downloaded existing row.

        [Display(Name = "Project Buyer Code")]
        public string? ProjectBuyerCode { get; set; }

        [Display(Name = "Test Buyer Code")]
        public string? TestBuyerCode { get; set; }

        [Display(Name = "Test Buyer Work Group")]
        public string? TestBuyerWorkGroup { get; set; }
    }
}
