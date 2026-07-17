using System.ComponentModel.DataAnnotations;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    public class FecExportRow
    {
        [Display(Name = "TestCode")]
        public string TestCode { get; set; } = string.Empty;

        [Display(Name = "Unit Price VLA")]
        public decimal? UnitPriceVla { get; set; }

        [Display(Name = "Defra Unit Price")]
        public decimal? DefraUnitPrice { get; set; }

        [Display(Name = "FEC New")]
        public decimal? FecNew { get; set; }

        [Display(Name = "Change")]
        public decimal? Change { get; set; }

        [Display(Name = "Item Description")]
        public string? ItemDescription { get; set; }

        [Display(Name = "Short Description")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Owner")]
        public string? Owner { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }
    }
}
