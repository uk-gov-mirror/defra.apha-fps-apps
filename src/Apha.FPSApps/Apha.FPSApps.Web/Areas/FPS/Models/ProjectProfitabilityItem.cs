using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProjectProfitabilityItem
    {
        [Display(Name = "Project")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Staff Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal JcTotalStaffCosts { get; set; }

        [Display(Name = "Test Cost")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal JcTotalTestCosts { get; set; }

        [Display(Name = "Animal")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal JcTotalAnimalCosts { get; set; }

        [Display(Name = "Add Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal JcTotalAdditionalCosts { get; set; }

        [Display(Name = "Total Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal JcProfit { get; set; }

        [Display(Name = "Target Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TargetProfit { get; set; }

        [Display(Name = "Off-Target")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal OffTarget { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ProjectStatus { get; set; }

        [GridColumn(IsVisible = false)]
        public string? ProgramNo { get; set; }

        [GridColumn(IsVisible = false)]
        public decimal? ProgrammeTarget { get; set; }
    }
}
