using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class RejectBulkRatesRequestReq
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
