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
    }
}
