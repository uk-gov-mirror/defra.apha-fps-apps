using Apha.Common.Utilities.GenericExcelExport.Attributes;

namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class AnimalDto
    {
        public string AnimalType { get; set; } = null!;

        public string? Species { get; set; }

        public string? SecurityLevel { get; set; }

        public decimal? DailyRate { get; set; }

        public bool PlanByWeek { get; set; }

        public decimal? DefraDailyRate { get; set; }
        
        [ExcelIgnore]
        public int? FpsCalYear { get; set; }
    }
}
