using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Project Specific Query read-only page.
    /// </summary>
    public class ProjectSpecificQueryViewModel
    {
        /// <summary>
        /// DataGrid configuration for the project specific query list.
        /// </summary>
        public DataGridConfig<ProjectSpecificQueryItem> ProjectSpecificQueryGrid { get; set; } = new DataGridConfig<ProjectSpecificQueryItem>();
    }

    /// <summary>
    /// Read-only grid row model for project specific query data.
    /// </summary>
    public class ProjectSpecificQueryItem
    {
        [Display(Name = "Program")]
        [GridColumn(Order = 1, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Parent Project")]
        [GridColumn(Order = 2, Width = 140, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ParentProject { get; set; }

        [Display(Name = "Project Title")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Short Title")]
        [GridColumn(Order = 99, Width = 150, Type = GridColumnType.Text, IsFilterable = true, IsVisible = false)]
        public string? ShortTitle { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Order = 4, Width = 130, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Account")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Account { get; set; }

        [Display(Name = "Description")]
        [GridColumn(Order = 9, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        [Display(Name = "Account Description")]
        [GridColumn(Order = 7, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? AccountDescription { get; set; }

        [Display(Name = "Constituent Account Codes")]
        [GridColumn(Order = 8, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ConstituentAccountCodes { get; set; }

        [Display(Name = "Freq")]
        [GridColumn(Order = 10, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Freq { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Order = 11, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Supplier { get; set; }

        [Display(Name = "Item Cost")]
        [GridColumn(Order = 12, Width = 120, Type = GridColumnType.GbpValue, IsFilterable = true)]
        public decimal ItemCost { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Order = 5, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Manager { get; set; }
    }
}
