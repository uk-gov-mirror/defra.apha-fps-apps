using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the ASU Data View page (frmAnimalCosts).
    /// Contains the animal-type filter dropdown and the read-only animal costs DataGrid.
    /// </summary>
    public class AnimalCostsViewModel
    {
        /// <summary>Selected animal type from the filter dropdown (bound to PickAnimalType combo in Access).</summary>
        public string? SelectedAnimalType { get; set; }

        /// <summary>Animal type dropdown items — AnimalType + DailyRate (mirrors RowSource of pickAnimalType).</summary>
        public List<SelectListItem> AnimalTypeList { get; set; } = new List<SelectListItem>();

        /// <summary>Read-only DataGrid configuration for fsubAnimalCosts (DefaultView=2, all Allow* disabled).</summary>
        public DataGridConfig<AnimalCostsItem> AnimalCostsGrid { get; set; } = new DataGridConfig<AnimalCostsItem>();
    }
}
