using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class CancelBulkRatesRequestReq
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
