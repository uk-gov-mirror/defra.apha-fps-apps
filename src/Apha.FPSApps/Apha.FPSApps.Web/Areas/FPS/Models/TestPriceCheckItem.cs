using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestPriceCheckItem
    {
        [Display(Name = "Test Code")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string TestCode { get; set; } = null!;

        [Display(Name = "Project")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Manager")]
        [GridColumn(Order = 3, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Program")]
        [GridColumn(Order = 4, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        private double? _noTests;

        [Display(Name = "No. Tests")]
        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.Number)]
        public double? NoTests
        {
            get => _noTests;
            set => _noTests = value.HasValue ? Math.Round(value.Value) : null;
        }

        [Display(Name = "Agr Price")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? TestPrice { get; set; }

        [Display(Name = "Standard Price *")]
        [GridColumn(Order = 7, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? NormalPrice { get; set; }

        [Display(Name = "Defra Project?")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.Checkbox)]
        public short IsDefraProject { get; set; }

        [GridColumn(IsVisible = false)]
        public List<SelectListItem> IsDefraProjectList { get; set; } = new();

        [Display(Name = "Non Defra Price")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? UnitPriceVla { get; set; }

        [Display(Name = "Defra Price")]
        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? DefraUnitPrice { get; set; }

        [Display(Name = "Owner")]
        [GridColumn(Order = 11, Width = 60, Type = GridColumnType.Text)]
        public string? Owner { get; set; }

        [GridColumn(IsVisible = false)]
        public bool IsZeroPrice { get; set; }

        [GridColumn(IsVisible = false)]
        public bool IsNotStandard { get; set; }

        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}
