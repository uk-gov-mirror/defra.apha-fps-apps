using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class RecreateSummariesViewModel
    {
        public List<SelectListItem> Months { get; set; } = [];
        public int? SelectedMonth { get; set; }
        public bool CanRunJob { get; set; }
        public required DataGridConfig<BatchJobHistoryItem> HistoryGrid { get; set; }
    }

    public class BatchJobHistoryItem
    {
        [Display(Name = "Job Name")]
        [GridColumn(Order = 1, Width = 160, Type = GridColumnType.Text)]
        public string JobName { get; set; } = null!;

        [Display(Name = "Requested By")]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text)]
        public string RequestedBy { get; set; } = null!;

        [Display(Name = "Start Date/Time")]
        [GridColumn(Order = 3, Width = 180, Type = GridColumnType.Text)]
        public DateTime StartDateTime { get; set; }

        [Display(Name = "End Date/Time")]
        [GridColumn(Order = 4, Width = 180, Type = GridColumnType.Text)]
        public DateTime? EndDateTime { get; set; }

        [Display(Name = "Status")]
        [GridColumn(Order = 5, Width = 120, Type = GridColumnType.Text)]
        public string Status { get; set; } = null!;

        [Display(Name = "Error Message")]
        [GridColumn(Order = 6, Width = 300, Type = GridColumnType.Text)]
        public string? ErrorMessage { get; set; }
    }
}
