using Apha.FPSApps.Web.Validation;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TotalBusinessOverheadsViewModel
    {
        [CurrencyRange]
        public decimal? TotalBusinessOverheads { get; set; }

        public int FpsYear { get; set; }
    }
}
