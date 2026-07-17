using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    public class AnimalExportRow
    {
        [Display(Name = "AnimalType")]
        public string AnimalType { get; set; } = string.Empty;

        [Display(Name = "Species")]
        public string? Species { get; set; }

        [Display(Name = "Security Level")]
        public string? SecurityLevel { get; set; }

        [Display(Name = "Daily Rate")]
        public decimal? DailyRate { get; set; }

        [Display(Name = "Defra Daily Rate")]
        public decimal? DefraDailyRate { get; set; }

        [Display(Name = "Plan By Week")]
        public bool? PlanByWeek { get; set; }
    }
}
