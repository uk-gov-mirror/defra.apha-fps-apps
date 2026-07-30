using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class StagingMilestoneItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Id { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Project { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool IsAddingNew { get; set; }

        [RegularExpression(@"^\d{1,2}/\d{1,2}$", ErrorMessage = "Number must be in format 00/00 (digits only, e.g. 01/01)")]        
        [Display(Name = "Number")]
        [GridColumn(Order = 1, Width = 30, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Number { get; set; }

        [Required(ErrorMessage = "Date Due is required")]
        [Display(Name = "Date Due")]
        [GridColumn(Order = 2, Width = 60, Type = GridColumnType.Date)]
        public DateTime DateDue { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        [GridColumn(Order = 3, Width = 240, Type = GridColumnType.Text)]
        public string? Description { get; set; }

        [Display(Name = "Error")]
        [GridColumn(Order = 4, Width = 200, Type = GridColumnType.ReadOnly)]
        public string? Note { get; set; }

        [Display(Name = "Alt Number")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Text, IsVisible = false)]
        public string? AltNumber { get; set; }

        [Display(Name = "Alt Description")]
        [GridColumn(Order = 6, Width = 200, Type = GridColumnType.Text, IsVisible = false)]
        public string? AltDescription { get; set; }

        [Display(Name = "Alt Date")]
        [GridColumn(Order = 7, Width = 110, Type = GridColumnType.Text, IsVisible = false)]
        public string? AltDate { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? TypeId { get; set; }
    }
}