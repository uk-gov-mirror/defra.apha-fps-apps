using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IPimsApiClient _client;

        public MaintenanceService(IPimsApiClient client)
        {
            _client = client;
        }

        // ── Report ──────────────────────────────────────────────────────────────────
       

        public async Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync()
            => await _client.PimsReport.GetAllReportsAsync();

        public async Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query)
            => await _client.PimsReport.GetPagedReportsAsync(query);

        public async Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id)
            => await _client.PimsReport.GetReportByIdAsync(id);

        public async Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto)
            => await _client.PimsReport.CreateReportAsync(dto);

        public async Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto)
            => await _client.PimsReport.UpdateReportAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteReportAsync(int id)
            => await _client.PimsReport.DeleteReportAsync(id);

        // ── ReportGroup ─────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync()
            => await _client.PimsReportGroup.GetAllReportGroupsAsync();

        public async Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null)
            => await _client.PimsReportGroup.GetPagedReportGroupsAsync(query, reportId);

        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId)
            => await _client.PimsReportGroup.GetReportGroupsByReportIdAsync(reportId);

        public async Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId)
            => await _client.PimsReportGroup.GetReportGroupByIdAsync(groupId);

        public async Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto)
            => await _client.PimsReportGroup.CreateReportGroupAsync(dto);

        public async Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto)
            => await _client.PimsReportGroup.UpdateReportGroupAsync(groupId, dto);

        public async Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId)
            => await _client.PimsReportGroup.DeleteReportGroupAsync(groupId);

        // ── ReportGroupLink ─────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync()
            => await _client.PimsReportGroupLink.GetAllReportGroupLinksAsync();

        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportId)
            => await _client.PimsReportGroupLink.GetReportGroupLinksByReportIdAsync(reportId);

        public async Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportId, int groupId)
            => await _client.PimsReportGroupLink.GetReportGroupLinkByIdAsync(reportId, groupId);

        public async Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto)
            => await _client.PimsReportGroupLink.CreateReportGroupLinkAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportId, int groupId)
            => await _client.PimsReportGroupLink.DeleteReportGroupLinkAsync(reportId, groupId);

        // ── ProjectManager ──────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync(QueryParameters<string>? query = null)
            => await _client.PimsProjectManager.GetAllProjectManagersAsync(query);

        public async Task<ApiResponseDto<PaginatedResult<ProjectManagerDto>>> GetPagedProjectManagersAsync(QueryParameters<string> query)
            => await _client.PimsProjectManager.GetPagedProjectManagersAsync(query);

        public async Task<ApiResponseDto<List<string>>> GetManagerNamesAsync()
            => await _client.PimsProjectManager.GetManagerNamesAsync();

        public async Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByIdAsync(string projectManagerName)
            => await _client.PimsProjectManager.GetProjectManagerByNameAsync(projectManagerName);

        public async Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto)
            => await _client.PimsProjectManager.CreateProjectManagerAsync(dto);

        public async Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectManagerName, ProjectManagerDto dto)
            => await _client.PimsProjectManager.UpdateProjectManagerAsync(projectManagerName, dto);

        public async Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectManagerName)
            => await _client.PimsProjectManager.DeleteProjectManagerAsync(projectManagerName);

        // ── ProgramManagerLink ──────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync()
            => await _client.PimsProgramManagerLink.GetAllProgramManagerLinksAsync();

        public async Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedProgramManagerLinksByManagerAsync(QueryParameters<string> query, string manager)
            => await _client.PimsProgramManagerLink.GetPagedByManagerAsync(query, manager);

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByProgramAsync(string program)
            => await _client.PimsProgramManagerLink.GetByProgramAsync(program);

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByManagerAsync(string manager)
            => await _client.PimsProgramManagerLink.GetByManagerAsync(manager);

        public async Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager)
            => await _client.PimsProgramManagerLink.GetProgramManagerLinkByIdAsync(program, manager);

        public async Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto)
            => await _client.PimsProgramManagerLink.CreateProgramManagerLinkAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager)
            => await _client.PimsProgramManagerLink.DeleteProgramManagerLinkAsync(program, manager);

        public async Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync()
            => await _client.PimsProgramManagerLink.GetProgramsAsync();

        // ── ProfitCentreManagerLink ─────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync()
            => await _client.PimsProfitCentreManagerLink.GetAllProfitCentreManagerLinksAsync();

        public async Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedProfitCentreManagerLinksByManagerAsync(QueryParameters<string> query, string manager)
            => await _client.PimsProfitCentreManagerLink.GetPagedByManagerAsync(query, manager);

        public async Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync()
            => await _client.PimsProfitCentreManagerLink.GetProfitCentresAsync();

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByProfitCentreAsync(string profitCentre)
            => await _client.PimsProfitCentreManagerLink.GetByProfitCentreAsync(profitCentre);

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByManagerAsync(string manager)
            => await _client.PimsProfitCentreManagerLink.GetByManagerAsync(manager);

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager)
            => await _client.PimsProfitCentreManagerLink.GetProfitCentreManagerLinkByIdAsync(profitCentre, manager);

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto)
            => await _client.PimsProfitCentreManagerLink.CreateProfitCentreManagerLinkAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager)
            => await _client.PimsProfitCentreManagerLink.DeleteProfitCentreManagerLinkAsync(profitCentre, manager);

        // ── Setting ─────────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
            => await _client.PimsSetting.GetAllSettingsAsync();

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync()
            => await _client.PimsSetting.GetAllUserUpdateableSettingsAsync();

        public async Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id)
            => await _client.PimsSetting.GetSettingByIdAsync(id);

        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto)
            => await _client.PimsSetting.UpdateSettingAsync(id, dto);

        // ── AccessUser ──────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAllAccessUsersAsync()
            => await _client.PimsAccessUser.GetAllAsync();

        public async Task<ApiResponseDto<PaginatedResult<AccessUserDto>>> GetPagedAccessUsersAsync(QueryParameters<string> request)
            => await _client.PimsAccessUser.GetPagedAsync(request);

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAccessUsersBySystemIdAsync(int systemid)
            => await _client.PimsAccessUser.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<AccessUserDto>> GetAccessUserByIdAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUser.GetByIdAsync(systemid, ntlogin);

        public async Task<ApiResponseDto<AccessUserDto>> CreateAccessUserAsync(AccessUserDto dto)
            => await _client.PimsAccessUser.CreateAsync(dto);

        public async Task<ApiResponseDto<AccessUserDto>> UpdateAccessUserAsync(int systemid, string ntlogin, AccessUserDto dto)
            => await _client.PimsAccessUser.UpdateAsync(systemid, ntlogin, dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessUserAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUser.DeleteAsync(systemid, ntlogin);

        // ── AccessLevel ───────────────────────────────────────────────────────────

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAccessLevelsAsync()
            => await _client.PimsAccessLevel.GetAllAsync();

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAccessLevelsBySystemIdAsync(int systemid)
            => await _client.PimsAccessLevel.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<AccessLevelDto>> GetAccessLevelByIdAsync(int systemid, int accesslevelid)
            => await _client.PimsAccessLevel.GetByIdAsync(systemid, accesslevelid);

        public async Task<ApiResponseDto<AccessLevelDto>> CreateAccessLevelAsync(AccessLevelDto dto)
            => await _client.PimsAccessLevel.CreateAsync(dto);

        public async Task<ApiResponseDto<AccessLevelDto>> UpdateAccessLevelAsync(int systemid, int accesslevelid, AccessLevelDto dto)
            => await _client.PimsAccessLevel.UpdateAsync(systemid, accesslevelid, dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessLevelAsync(int systemid, int accesslevelid)
            => await _client.PimsAccessLevel.DeleteAsync(systemid, accesslevelid);

        public async Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAccessUserLevelsAsync(QueryParameters<string> request)
            => await _client.PimsAccessUserLevel.GetPagedAsync(request);

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsBySystemIdAsync(int systemid)
            => await _client.PimsAccessUserLevel.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsByUserAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUserLevel.GetByUserAsync(systemid, ntlogin);

        public async Task<ApiResponseDto<AccessUserLevelDto>> GetAccessUserLevelByIdAsync(int systemid, string ntlogin, int accesslevelid)
            => await _client.PimsAccessUserLevel.GetByIdAsync(systemid, ntlogin, accesslevelid);

        public async Task<ApiResponseDto<AccessUserLevelDto>> CreateAccessUserLevelAsync(AccessUserLevelDto dto)
            => await _client.PimsAccessUserLevel.CreateAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessUserLevelAsync(int systemid, string ntlogin, int accesslevelid)
            => await _client.PimsAccessUserLevel.DeleteAsync(systemid, ntlogin, accesslevelid);

        // ── AccessSystem (lookup — read-only) ───────────────────────────────────────
        

        public async Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAccessSystemsAsync()
            => await _client.PimsAccessSystem.GetAllAsync();

        public async Task<ApiResponseDto<AccessSystemDto>> GetAccessSystemByIdAsync(int systemid)
            => await _client.PimsAccessSystem.GetByIdAsync(systemid);

        // ── Frequency ───────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync()
            => await _client.PimsFrequency.GetAllFrequenciesAsync();

        public async Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query)
            => await _client.PimsFrequency.GetPagedFrequenciesAsync(query);

        public async Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId)
            => await _client.PimsFrequency.GetFrequencyByIdAsync(frequencyId);

        public async Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto)
            => await _client.PimsFrequency.CreateFrequencyAsync(dto);

        public async Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto)
            => await _client.PimsFrequency.UpdateFrequencyAsync(frequencyId, dto);

        public async Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId)
            => await _client.PimsFrequency.DeleteFrequencyAsync(frequencyId);

        // ── ReviewItem ──────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync()
            => await _client.PimsReviewItem.GetAllReviewItemsAsync();

        public async Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query)
            => await _client.PimsReviewItem.GetPagedReviewItemsAsync(query);

        public async Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId)
            => await _client.PimsReviewItem.GetReviewItemByIdAsync(itemId);

        public async Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto)
            => await _client.PimsReviewItem.CreateReviewItemAsync(dto);

        public async Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto)
            => await _client.PimsReviewItem.UpdateReviewItemAsync(itemId, dto);

        public async Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId)
            => await _client.PimsReviewItem.DeleteReviewItemAsync(itemId);

        // ── RadTrackProg ────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync()
            => await _client.PimsRadTrackProg.GetAllRadTrackProgsAsync();

        public async Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query)
            => await _client.PimsRadTrackProg.GetPagedRadTrackProgsAsync(query);

        public async Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByIdAsync(string program)
            => await _client.PimsRadTrackProg.GetRadTrackProgByProgramAsync(program);

        public async Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto)
            => await _client.PimsRadTrackProg.CreateRadTrackProgAsync(dto);

        public async Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto)
            => await _client.PimsRadTrackProg.UpdateRadTrackProgAsync(program, dto);

        public async Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program)
            => await _client.PimsRadTrackProg.DeleteRadTrackProgAsync(program);

        public async Task<ApiResponseDto<List<string>>> GetRadTrackProgProgramsAsync()
            => await _client.PimsRadTrackProg.GetAllProgramNamesAsync();

        // ── Risk ──
        public async Task<ApiResponseDto<List<RiskDto>>> GetAllRiskRatingsAsync()
            => await _client.PimsRisk.GetAllRiskRatingsAsync();

        public async Task<ApiResponseDto<PaginatedResult<RiskDto>>> GetPagedRiskRatingsAsync(QueryParameters<string> query)
            => await _client.PimsRisk.GetPagedRiskRatingsAsync(query);

        public async Task<ApiResponseDto<RiskDto>> GetRiskRatingByIdAsync(int riskId)
            => await _client.PimsRisk.GetRiskRatingByIdAsync(riskId);

        public async Task<ApiResponseDto<RiskDto>> CreateRiskRatingAsync(RiskDto dto)
            => await _client.PimsRisk.CreateRiskRatingAsync(dto);

        public async Task<ApiResponseDto<RiskDto>> UpdateRiskRatingAsync(int riskId, RiskDto dto)
            => await _client.PimsRisk.UpdateRiskRatingAsync(riskId, dto);

        public async Task<ApiResponseDto<bool>> DeleteRiskRatingAsync(int riskId)
            => await _client.PimsRisk.DeleteRiskRatingAsync(riskId);

        // ── PublicationType ─────────────────────────────────────────────────────────────────────────
        

        public async Task<ApiResponseDto<List<PublicationTypeDto>>> GetAllPublicationTypesAsync()
            => await _client.PimsPublicationType.GetAllPublicationTypesAsync();

        public async Task<ApiResponseDto<PaginatedResult<PublicationTypeDto>>> GetPagedPublicationTypesAsync(QueryParameters<string> query)
            => await _client.PimsPublicationType.GetPagedPublicationTypesAsync(query);

        public async Task<ApiResponseDto<PublicationTypeDto>> GetPublicationTypeByCodeAsync(string type)
            => await _client.PimsPublicationType.GetPublicationTypeByCodeAsync(type);

        public async Task<ApiResponseDto<PublicationTypeDto>> CreatePublicationTypeAsync(PublicationTypeDto dto)
            => await _client.PimsPublicationType.CreatePublicationTypeAsync(dto);

        public async Task<ApiResponseDto<PublicationTypeDto>> UpdatePublicationTypeAsync(string type, PublicationTypeDto dto)
            => await _client.PimsPublicationType.UpdatePublicationTypeAsync(type, dto);

        public async Task<ApiResponseDto<bool>> DeletePublicationTypeAsync(string type)
            => await _client.PimsPublicationType.DeletePublicationTypeAsync(type);
    }
}
