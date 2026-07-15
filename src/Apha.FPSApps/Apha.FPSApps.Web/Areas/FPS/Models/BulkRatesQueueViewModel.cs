using Apha.FPSApps.Application.Dtos.FPS.BulkRates;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesQueueViewModel
    {
        public List<BulkRatesQueueEntryDto> Entries { get; set; } = [];
        public string? JobNameFilter { get; set; }
        public int? FpsYearFilter { get; set; }
        public string? StatusFilter { get; set; }
    }
}
