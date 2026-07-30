using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestCapabilityItem
    {
        [Display(Name = "Test Code")]
        [Required(ErrorMessage = "Test Code is required.")]
        [GridColumn(Order = 1, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "WorkGroup")]
        [Required(ErrorMessage = "Work Group is required.")]
        [GridColumn(Order = 2, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroup { get; set; } = null!;

        [Display(Name = "Plan Portfolio")]
        [Required(ErrorMessage = "Plan Portfolio is required.")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string PlanPortfolio { get; set; } = null!;

        [Display(Name = "Unit Cost")]
        [GridColumn(IsVisible = false)]
        public decimal? UnitCost { get; set; }

        [Display(Name = "Pred Outturn")]
        [GridColumn(IsVisible = false)]
        public double? PredOutturn { get; set; }

        [Display(Name = "SOP")]
        [GridColumn(IsVisible = false)]
        public string? Sop { get; set; }

        [Display(Name = "SMS Code")]
        [GridColumn(IsVisible = false)]
        public string? SmsCode { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
