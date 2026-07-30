using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class SubContractRmsViewModel
    {
        public int? Month { get; set; }
        public List<SelectListItem> FilterMonths { get; set; } = new List<SelectListItem>();
        public DataGridConfig<SubContractRmsItem> SubContractsGrid { get; set; } = new DataGridConfig<SubContractRmsItem>();
        public DataGridConfig<SubContractRmsFailedItem> FailedSubContractsGrid { get; set; } = new DataGridConfig<SubContractRmsFailedItem>();
        public List<SelectListItem> Projects { get; set; } = new List<SelectListItem>();
    }
}
