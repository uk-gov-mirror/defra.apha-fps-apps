using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class StaffPlanDetailsViewItem
    {
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Order = 3, Width = 70, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? GradeCode { get; set; }

        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [GridColumn(Order = 5, Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [GridColumn(Order = 7, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.ReadOnly)]
        public double? PlannedHours { get; set; }

        [GridColumn(Order = 9, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? ChargeRate { get; set; }

        [GridColumn(Order = 10, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? Cost { get; set; }
    }
}
