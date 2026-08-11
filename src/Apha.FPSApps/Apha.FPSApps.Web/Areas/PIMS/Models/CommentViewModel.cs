using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class CommentViewModel
    {
        
        public string Parentproject { get; set; } = string.Empty;

        public string? SelectedProject { get; set; }

       
        public double? ForecastSpend { get; set; }

        public string? SelectedTopic { get; set; }

        
        public string? SelectedYear { get; set; }

        
        public List<SelectListItem> ProjectOptions { get; set; } = [];

        
        public List<SelectListItem> TopicOptions { get; set; } = [];

       
        public List<SelectListItem> YearOptions { get; set; } = [];

        
        public DataGridConfig<ProjectCommentItem> CommentsGrid { get; set; } = new();
    }
}
