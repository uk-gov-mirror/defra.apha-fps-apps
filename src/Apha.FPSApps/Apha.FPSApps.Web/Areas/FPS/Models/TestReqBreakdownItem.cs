using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestReqBreakdownItem
    {
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ShortDescription { get; set; }

        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Project { get; set; } = null!;

        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? PC { get; set; }

        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkG { get; set; }

        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? WGPrice { get; set; }

        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? TotalCost { get; set; }
    }
}
