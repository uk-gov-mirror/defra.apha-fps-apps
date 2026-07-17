using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    public class StaffExportRow
    {
        [Display(Name = "PcGrade")]
        public string PcGrade { get; set; } = string.Empty;

        [Display(Name = "Pay Rate")]
        public decimal? PayRate { get; set; }

        [Display(Name = "NPR")]
        public decimal? Npr { get; set; }

        [Display(Name = "OHR")]
        public decimal? Ohr { get; set; }
    }
}
