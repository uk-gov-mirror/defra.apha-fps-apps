using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AnimalSnapshotDataViewModel
    {
        public DataGridConfig<AnimalSnapshotItem> SnapShotAnimalDataGrid { get; set; } = new DataGridConfig<AnimalSnapshotItem>();
    }
}
