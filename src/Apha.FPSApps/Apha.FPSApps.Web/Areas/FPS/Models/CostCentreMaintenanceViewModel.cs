using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Cost Centre Maintenance DataGrid form (frmMaintCostCentres).
    /// Grid config is built explicitly in <c>CostCentreMaintenanceController.Index()</c>.
    /// ProfitCentreList is populated from the workgroup lookup via
    /// <c>ICostCentreService.GetAllCostCentresAsync()</c> for use in the Add/Edit modal.
    /// </summary>
    public class CostCentreMaintenanceViewModel
    {
        // Leaving as new() would render an empty grid with default Add button regardless of JS-derived
        // operations profile (AllowAdd/Edit/Delete from costcenter_maintenance.js).
        public DataGridConfig<CostCentreItem> CostCentreGrid { get; set; } = new DataGridConfig<CostCentreItem>();

        // CostCentreWorkgroupDto.ProfitCentre via GetAllCostCentresAsync(); used in the
        // _AddEditCostCentre modal partial for the Profit Centre dropdown (modal-cc-profit).
        public List<SelectListItem> ProfitCentreList { get; set; } = new List<SelectListItem>();
    }
}
