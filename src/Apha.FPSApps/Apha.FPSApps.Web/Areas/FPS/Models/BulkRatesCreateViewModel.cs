using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class BulkRatesCreateViewModel
    {
        [Required]
        public string JobName { get; set; } = "BulkTestRatesUpdate";

        [Required]
        [Range(2000, 2100, ErrorMessage = "FPS Year must be between 2000 and 2100.")]
        public int FpsYear { get; set; }
    }
}
