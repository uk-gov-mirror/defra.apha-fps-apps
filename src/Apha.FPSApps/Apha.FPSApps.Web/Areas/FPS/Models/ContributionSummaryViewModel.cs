using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// View model for the Income/Contribution from Time Sales page (frmTimeSellerPC).
    /// Read-only grid — no add/edit/delete functionality.
    /// </summary>
    public class ContributionSummaryViewModel
    {
        /// <summary>All profit centres available in the Selling PC dropdown.</summary>
        public List<SelectListItem> SellingProfitCentres { get; set; } = [];

        /// <summary>Currently selected Selling PC identifier.</summary>
        public string? SelectedSellingPc { get; set; }

        /// <summary>Row grid config — null until a Selling PC is chosen.</summary>
        public DataGridConfig<ContributionSummaryRowItem>? RowGrid { get; set; }

        /// <summary>Footer totals — null until a Selling PC is chosen.</summary>
        public ContributionSummaryTotalsDto? Totals { get; set; }
    }
}
