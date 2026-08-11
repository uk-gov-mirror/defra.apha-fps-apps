using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class ReportGroupViewModel
    {
        [Required(ErrorMessage = "Report Group is required")]
        [Display(Name = "Report Group")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Number)]
        public int GroupId { get; set; }

        public int Reportid { get; set; }

        public string? Description { get; set; }

        public List<SelectListItem> ReportGroups { get; set; } = new();
    }
}
