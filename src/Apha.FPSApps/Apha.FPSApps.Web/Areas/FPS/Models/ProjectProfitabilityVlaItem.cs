using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Project Profitability VLA DataGrid.
    /// Derived from the JS DataGridComponent columns array in projectprofitability_vla.js.
    /// Property names must exactly match <c>ProjectProfitabilityVlaDto</c> for AutoMapper
    /// convention mapping registered in <c>FpsViewModelMapper</c>.
    /// </summary>
    public class ProjectProfitabilityVlaItem
    {
        //   used as KeyProperty only. Nullable int? mirrors frontend DTO.
        [GridColumn(IsVisible = false)]
        public int? Id { get; set; }

        //   — DTO property is JobCode; Display Name "Project" matches the JS header.
        //   Convention AutoMapper maps ProjectProfitabilityVlaDto.JobCode → JobCode here.
        [Display(Name = "Project")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Program")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        [Display(Name = "Staff Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal StaffCosts { get; set; }

        [Display(Name = "Test Cost")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TestCost { get; set; }

        //   DTO property name is AnimalCosts (not 'animal'); Display Name "Animal" matches JS header.
        [Display(Name = "Animal")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal AnimalCosts { get; set; }

        //   DTO property name is AdditionalCosts (not 'addCosts'); Display Name "Add Costs" matches JS header.
        [Display(Name = "Add Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal AdditionalCosts { get; set; }

        [Display(Name = "Total Costs")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal? Budget { get; set; }

        [Display(Name = "Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal Profit { get; set; }

        [Display(Name = "Target Profit")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal TargetProfit { get; set; }

        //   Negative value triggers red highlight (fps-profit-offtarget CSS class) in Razor view.
        [Display(Name = "Off-Target")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 130, Type = GridColumnType.GbpValueRounded, IsFilterable = false)]
        public decimal OffTarget { get; set; }

        //   retained as hidden properties to allow round-trip filter state if needed.
        [GridColumn(IsVisible = false)]
        public string? Manager { get; set; }

        [GridColumn(IsVisible = false)]
        public string? Status { get; set; }
    }
}
