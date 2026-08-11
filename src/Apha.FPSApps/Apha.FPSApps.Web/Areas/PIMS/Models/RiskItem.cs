using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class RiskItem
    {
        [Display(Name = "RiskID")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Riskid { get; set; }

        [Required(ErrorMessage = "Risk rating is required")]
        [Display(Name = "RiskRating")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Riskrating { get; set; }
    }
}
