using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestActualBreakdownItem
    {
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Portfolio { get; set; }

        [GridColumn(Order = 3, Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Buyer { get; set; } = null!;

        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProfitCentre { get; set; }

        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? WorkGroup { get; set; }

        [GridColumn(Order = 7, Width = 60, Type = GridColumnType.ReadOnly)]
        public int? Month { get; set; }

        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? PCPrice { get; set; }

        [GridColumn(Order = 9, Width = 90, Type = GridColumnType.GbpValue)]
        public decimal? PCCost { get; set; }

        [GridColumn(Order = 10, Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ShortDescription { get; set; }
    }
}