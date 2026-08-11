using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    /// <summary>
    /// PMD-specific milestone model for grid display.
    /// Uses same data structure as MilestoneItem but with PMD-specific column ordering and visibility.
    /// </summary>
    public class PMDMilestoneItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string Project { get; set; } = null!;

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool IsAddingNew { get; set; }

               
        [Display(Name = "Ref")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public string Number { get; set; } = null!;

        [Display(Name = "Type")]
        [Required(ErrorMessage = "Type is required")]
        [GridColumn(Order = 2, Width = 120, Type = GridColumnType.Dropdown)]
        public string? IdType { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text)]
        public string? Description { get; set; }

        [Display(Name = "Due")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.Date)]
        public DateTime? DateDue { get; set; }

        [Display(Name = "On Target")]
        [GridColumn(Order = 5, Width = 90, Type = GridColumnType.Checkbox)]
        public short OnTarget { get; set; }


        [Display(Name = "Under Review?")]
        [GridColumn(Order = 6, Width = 100, Type = GridColumnType.Checkbox)]
        public short UnderSdReview { get; set; }

        [Display(Name = "Completed/Delivered")]
        [GridColumn(Order = 7, Width = 130, Type = GridColumnType.Date)]
        public DateTime? DateCompleted { get; set; }

        [Display(Name = "P Leader's Comment")]
        [GridColumn(Order = 8, Width = 180, Type = GridColumnType.Text)]
        public string? ProjectLeaderComment { get; set; }  
       
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool IsLate { get; set; }

        [Display(Name = "Is Late")]
        [GridColumn(Order = 9, Width = 30, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string DisplayLate => IsLate ? "Late" : string.Empty;

        // Hidden properties (used for data but not displayed in grid)
        [GridColumn(Order = 10, Width = 180, Type = GridColumnType.Text, IsVisible = false)]
        public string? CapsComment { get; set; }

        [GridColumn(Order = 11, Width = 0, Type = GridColumnType.ReadOnly, IsVisible = false)]
        public DateTime? DateFormReceived { get; set; }
    }
}
