using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProfitCentreGradeMaintViewModel
    {
        public DataGridConfig<ProfitCentreGradeMaintItem> RcGradeMaintenanceGrid { get; set; } = new DataGridConfig<ProfitCentreGradeMaintItem>();
        public List<SelectListItem> DivisionGradeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> GradeCodeList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProfitCentreList { get; set; } = new List<SelectListItem>();
    }

    public class ProfitCentreGradeMaintItem
    {
        [Display(Name = "RCGrade")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsVisible = true, IsFilterable = true)]
        public string PcGrade { get; set; } = null!;

        [GridColumn(Width = 150, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string? DivisionGrade { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [Display(Name = "RC")]
        [GridColumn(Width = 200, Type = GridColumnType.Dropdown, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? ChargeRate { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? DirectRate { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PayRate { get; set; }

        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? NPR { get; set; }

        [GridColumn(Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? OHR { get; set; }

        [GridColumn(Width = 120, Type = GridColumnType.Number, IsFilterable = false)]
        public double? HrsAvailable { get; set; }
    }
}
