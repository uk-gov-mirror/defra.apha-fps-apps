using Apha.Common.Utilities.ExcelExport;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class StagingMonthlyOutputExportItem
    {
        [Display(Name = "Work Group")]
        public string? WorkGroup { get; set; }

        [Display(Name = "Test Code")]
        public string? TestCode { get; set; }

        public string? Buyer { get; set; }

        public double? Month { get; set; }

        public double? Volume { get; set; }

        public bool? Passed { get; set; }

        [Display(Name = "Failure Comments")]
        public string? FailureComments { get; set; }

        public string? Filename { get; set; }

        [Display(Name = "StagingId")]
        [ExcelHiddenColumn]
        public int Id { get; set; }
    }
}
