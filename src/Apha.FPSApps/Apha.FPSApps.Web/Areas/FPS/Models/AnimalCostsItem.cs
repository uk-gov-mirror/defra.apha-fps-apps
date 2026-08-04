using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid item model for the ASU Data View (frmAnimalCosts / fsubAnimalCosts).
    /// Read-only — all Allow* properties in the Access subform are NotDefault (disabled).
    /// Columns mirror the visible columns in the subform datasheet:
    ///   JobCode (visible), AnimalDays = NumberOfDays × NumberOfAnimals (visible), AnimalCost (visible).
    ///   AnimalType, NumberOfDays, NumberOfAnimals are ColumnHidden = NotDefault in the Access subform.
    /// </summary>
    public class AnimalCostsItem
    {
        // PK — hidden from the grid, used as KeyProperty
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int IndCounter { get; set; }

        // Hidden — used as filter context, not shown in the grid (ColumnHidden = NotDefault in fsubAnimalCosts)
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? AnimalType { get; set; }

        [Display(Name = "Project")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? JobCode { get; set; }

        // Computed in the repository: NumberOfDays × NumberOfAnimals (two-step post-query arithmetic)
        [Display(Name = "Animal Days")]
        [GridColumn(Width = 110, Type = GridColumnType.DecimalNumber)]
        public double TotalDays { get; set; }

        [Display(Name = "Cost")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal? AnimalCost { get; set; }
    }
}
