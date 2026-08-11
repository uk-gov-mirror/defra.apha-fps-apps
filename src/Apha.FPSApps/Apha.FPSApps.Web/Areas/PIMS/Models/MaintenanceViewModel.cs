using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class MaintenanceViewModel
    {
        public DataGridConfig<ReportItem> ReportsGrid { get; set; } = new();

        public DataGridConfig<ReportGroupItem> ReportGroupsGrid { get; set; } = new();

        public int? SelectedReportId { get; set; }

        public DataGridConfig<RadTrackProgItem> RadTrackProgsGrid { get; set; } = new();

        public DataGridConfig<ProjectManagerItem> ProjectManagersGrid { get; set; } = new();

        public DataGridConfig<ProgramManagerLinkItem> ProgramManagerLinksGrid { get; set; } = new();

        public DataGridConfig<ProfitCentreManagerLinkItem> ProfitCentreManagerLinksGrid { get; set; } = new();

        public string? SelectedManagerName { get; set; }

        public SettingItem? WorkingHoursSettingItem { get; set; }

        public SettingItem? WorkingDaysSettingItem { get; set; }

        public DataGridConfig<AccessUserItem> AccessUsersGrid { get; set; } = new();

        public DataGridConfig<AccessUserLevelItem> AccessUserLevelsGrid { get; set; } = new();

        public DataGridConfig<FrequencyItem> FrequenciesGrid { get; set; } = new();

        public DataGridConfig<ReviewItemItem> ReviewItemsGrid { get; set; } = new();

        public DataGridConfig<RiskItem> RisksGrid { get; set; } = new();

        public DataGridConfig<PublicationTypeItem> PublicationTypesGrid { get; set; } = new();
    }
}
