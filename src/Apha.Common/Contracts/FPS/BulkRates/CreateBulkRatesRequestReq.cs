using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class CreateBulkRatesRequestReq
    {
        /// <summary>
        /// Job name identifying the rate stream: BulkTestRatesUpdate | BulkStaffRatesUpdate | BulkAnimalRatesUpdate.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string JobName { get; set; } = null!;

        /// <summary>FPS year that the uploaded rates will apply to.</summary>
        [Required]
        [Range(2000, 2100)]
        public int FpsYear { get; set; }
    }
}
