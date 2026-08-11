using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IMaintenanceService
    {
        Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync();

        Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id);
        Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto);
        Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto);
        Task<ApiResponseDto<bool>> DeleteReportAsync(int id);

        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync();
        Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null);
        Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId);
        Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId);
        Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto);
        Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId);

        // ── ReportGroupLink ─────────────────────────────────────────────────────────
       

        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync();
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportid);
        Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportid, int groupid);
        Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportid, int groupid);

        // ── ProjectManager ──────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync(QueryParameters<string>? query = null);
        Task<ApiResponseDto<PaginatedResult<ProjectManagerDto>>> GetPagedProjectManagersAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<string>>> GetManagerNamesAsync();
        Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByIdAsync(string projectmanager);
        Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto);
        Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectmanager, ProjectManagerDto dto);
        Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectmanager);

        // ── ProgramManagerLink ──────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync();
        Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedProgramManagerLinksByManagerAsync(QueryParameters<string> query, string manager);
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByProgramAsync(string program);
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByManagerAsync(string manager);
        Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager);
        Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager);
        Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync();

        // ── ProfitCentreManagerLink ─────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync();
        Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedProfitCentreManagerLinksByManagerAsync(QueryParameters<string> query, string manager);
        Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByProfitCentreAsync(string profitcentre);
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByManagerAsync(string manager);
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitcentre, string manager);
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitcentre, string manager);

        // ── Setting ─────────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync();
        Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync();
        Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id);
        Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto);

        // ── AccessUser ──────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<AccessUserDto>>> GetAllAccessUsersAsync();
        Task<ApiResponseDto<PaginatedResult<AccessUserDto>>> GetPagedAccessUsersAsync(QueryParameters<string> request);
        Task<ApiResponseDto<List<AccessUserDto>>> GetAccessUsersBySystemIdAsync(int systemid);
        Task<ApiResponseDto<AccessUserDto>> GetAccessUserByIdAsync(int systemid, string ntlogin);
        Task<ApiResponseDto<AccessUserDto>> CreateAccessUserAsync(AccessUserDto dto);
        Task<ApiResponseDto<AccessUserDto>> UpdateAccessUserAsync(int systemid, string ntlogin, AccessUserDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessUserAsync(int systemid, string ntlogin);

        // ── AccessLevel ─────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAccessLevelsAsync();
        Task<ApiResponseDto<List<AccessLevelDto>>> GetAccessLevelsBySystemIdAsync(int systemid);
        Task<ApiResponseDto<AccessLevelDto>> GetAccessLevelByIdAsync(int systemid, int accesslevelid);
        Task<ApiResponseDto<AccessLevelDto>> CreateAccessLevelAsync(AccessLevelDto dto);
        Task<ApiResponseDto<AccessLevelDto>> UpdateAccessLevelAsync(int systemid, int accesslevelid, AccessLevelDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessLevelAsync(int systemid, int accesslevelid);

        // ── AccessUserLevel ─────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAccessUserLevelsAsync(QueryParameters<string> request);
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsBySystemIdAsync(int systemid);
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsByUserAsync(int systemid, string ntlogin);
        Task<ApiResponseDto<AccessUserLevelDto>> GetAccessUserLevelByIdAsync(int systemid, string ntlogin, int accesslevelid);
        Task<ApiResponseDto<AccessUserLevelDto>> CreateAccessUserLevelAsync(AccessUserLevelDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessUserLevelAsync(int systemid, string ntlogin, int accesslevelid);

        // ── AccessSystem (lookup — read-only) ───────────────────────────────────────
        

        Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAccessSystemsAsync();
        Task<ApiResponseDto<AccessSystemDto>> GetAccessSystemByIdAsync(int systemid);

        // ── Frequency ───────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync();
        Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query);
        Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId);
        Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto);
        Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto);
        Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId);

        // ── ReviewItem ──────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync();
        Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId);
        Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto);
        Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto);
        Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId);

        // ── Risk ──────────────────────────────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<RiskDto>>> GetAllRiskRatingsAsync();
        Task<ApiResponseDto<PaginatedResult<RiskDto>>> GetPagedRiskRatingsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<RiskDto>> GetRiskRatingByIdAsync(int riskId);
        Task<ApiResponseDto<RiskDto>> CreateRiskRatingAsync(RiskDto dto);
        Task<ApiResponseDto<RiskDto>> UpdateRiskRatingAsync(int riskId, RiskDto dto);
        Task<ApiResponseDto<bool>> DeleteRiskRatingAsync(int riskId);

        // ── PublicationType ───────────────────────────────────────────────────────────────────────────
        

        Task<ApiResponseDto<List<PublicationTypeDto>>> GetAllPublicationTypesAsync();
        Task<ApiResponseDto<PaginatedResult<PublicationTypeDto>>> GetPagedPublicationTypesAsync(QueryParameters<string> query);
        Task<ApiResponseDto<PublicationTypeDto>> GetPublicationTypeByCodeAsync(string type);
        Task<ApiResponseDto<PublicationTypeDto>> CreatePublicationTypeAsync(PublicationTypeDto dto);
        Task<ApiResponseDto<PublicationTypeDto>> UpdatePublicationTypeAsync(string type, PublicationTypeDto dto);
        Task<ApiResponseDto<bool>> DeletePublicationTypeAsync(string type);

        // ── RadTrackProg
        

        Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync();
        Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByIdAsync(string program);
        Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto);
        Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto);
        Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program);

        // Returns distinct non-null Programme names from MY_tlkpProject for dropdown binding
        Task<ApiResponseDto<List<string>>> GetRadTrackProgProgramsAsync();
    }
}
