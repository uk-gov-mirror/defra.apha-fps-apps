using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class YearlyFinancialDataViewModel
    {
       
        public string Parentproject { get; set; } = string.Empty;

        
        public string SelectedProject { get; set; } = string.Empty;

        
        public string StartDate { get; set; } = string.Empty;

        public string EndDate { get; set; } = string.Empty;

      
        public double HoursInDay { get; set; }

        
        public double DaysInYear { get; set; }        

       
        public List<SelectListItem> ProjectList { get; set; } = [];

       
        public DataGridConfig<YearlyFinancialDataItem> CostCenterListGrid { get; set; } = new();
    }
}
