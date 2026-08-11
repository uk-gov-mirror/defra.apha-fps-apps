using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class FrequencyItem
    {
        [Display(Name = "FrequencyID")]
        [GridColumn(Order = 1, Width = 80, Type = GridColumnType.Number, IsFilterable = true)]
        public int Frequencyid { get; set; }

        [Required(ErrorMessage = "Frequency value is required")]
        [Display(Name = "Frequency")]
        [GridColumn(Order = 2, Width = 280, Type = GridColumnType.Text, IsFilterable = true)]
        public string? FrequencyValue { get; set; }
    }
}
