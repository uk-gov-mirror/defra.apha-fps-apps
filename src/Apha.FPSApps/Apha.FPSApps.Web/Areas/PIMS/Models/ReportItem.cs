using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ReportItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true, IsVisible = true)]
        public string? ReportName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = true, IsVisible = true)]
        public string? ReportDescription { get; set; }

        [Required(ErrorMessage = "Report Help is required")]
        [Display(Name = "Report Help")]
        [GridColumn(Order = 3, Width = 80, Type = GridColumnType.ReadOnly, IsVisible = true)]
        public string? ReportHelp { get; set; }

        [Required(ErrorMessage = "Mail Comment is required")]
        [Display(Name = "Mail Comment")]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.ReadOnly, IsVisible = true)]
        public string? MailComment { get; set; }

        [Required(ErrorMessage = "Mail Title is required")]
        [Display(Name = "Mail Title")]
        [GridColumn(Order = 5, Width = 80, Type = GridColumnType.ReadOnly, IsVisible = true)]
        public string? MailTitle { get; set; }

        [Display(Name = "Email-able")]
        [GridColumn(Order = 6, Width = 50, Type = GridColumnType.Checkbox, IsFilterable = true, IsVisible = true)]
        public bool Emailable { get; set; }

        [Display(Name = "Order")]
        [GridColumn(Order = 7, Width = 50, Type = GridColumnType.Number, IsFilterable = true, IsVisible = true)]
        public int? SortOrder { get; set; }

        [Display(Name = "Filter")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Filter { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickProgramme { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickProject { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickManager { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickContract { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickCustomer { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickMonth { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public bool AllowPickFYear { get; set; }

        [Display(Name = "Type")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string Type { get; set; } = string.Empty;
    }
}
