using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for Division maintenance page.
    /// </summary>
    public class DivisionMaintenanceViewModel
    {
        /// <summary>
        /// DataGrid configuration for divisions list.
        /// </summary>
        public DataGridConfig<DivisionViewModel> DivisionGrid { get; set; } = new DataGridConfig<DivisionViewModel>();
    }

    /// <summary>
    /// ViewModel for individual Division records in the grid.
    /// </summary>
    public class DivisionViewModel
    {
        /// <summary>
        /// Division identifier (regular integer field, not auto-generated).
        /// </summary>
        [Display(Name = "Division ID")]
        [GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
        [Required(ErrorMessage = "Division ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Division ID must be a positive number")]
        public int? DivisionId { get; set; }

        /// <summary>
        /// Parent agency identifier (foreign key to fps.tlkpagency).
        /// </summary>
        [Display(Name = "Agency ID")]
        [GridColumn(Width = 100, Type = GridColumnType.Number, IsFilterable = true)]
        [Required(ErrorMessage = "Agency is required")]
        public int AgencyId { get; set; }

        /// <summary>
        /// Division name (primary key).
        /// </summary>
        [Display(Name = "Division Name")]
        [GridColumn(Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "Division name is required")]
        [StringLength(255, ErrorMessage = "Division name cannot exceed 255 characters")]
        public string DivName { get; set; } = null!;

        /// <summary>
        /// Central overhead cost allocation.
        /// </summary>
        [Display(Name = "Central Overhead")]
        [GridColumn(Width = 150, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? CentOverhead { get; set; }

        /// <summary>
        /// Parent agency name for display.
        /// </summary>
        [Display(Name = "Agency Name")]
        [GridColumn(Width = 200, Type = GridColumnType.Text, IsFilterable = true, IsVisible = false)]
        public string? AgencyName { get; set; }
    }
}
