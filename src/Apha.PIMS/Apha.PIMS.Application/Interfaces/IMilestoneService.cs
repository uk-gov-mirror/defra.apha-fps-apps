using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IMilestoneService
    {
        Task<PaginatedResult<MilestoneDto>> GetAllMilestonesAsync(QueryParameters<string> parameters, string project);
        Task<PaginatedResult<MilestoneDto>> GetPMDMilestonesAsync(QueryParameters<string> parameters, string project);
        Task<MilestoneDto?> GetMilestoneAsync(string project, string number);
        Task<MilestoneDto> SaveMilestoneAsync(MilestoneDto dto, string? changedBy = null);
        Task<MilestoneDto> UpdateMilestoneAsync(MilestoneDto dto, string? changedBy = null);
        Task<MilestoneDto> UpdateMilestoneAsync_PMD(string project, string number, short underReview, short onTarget, DateTime? dateCompleted, string? projectLeaderComment, string? changedBy = null);
        Task<bool> DeleteMilestoneAsync(string project, string number);
        Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired);

        Task<List<MilestoneTypeDto>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);


        Task<PaginatedResult<MilestoneFormDatesDto>> GetAllMilestoneFormDatesAsync(QueryParameters<string> parameters, string parentProject);
        Task<MilestoneFormDatesDto?> GetMilestoneFormDatesAsync(short year, string parentProject);
        Task<MilestoneFormDatesDto> SaveMilestoneFormDatesAsync(MilestoneFormDatesDto dto);
        Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject);

        Task<PaginatedResult<LogMilestoneDto>> GetLogMilestonesAsync(QueryParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);

        // Staging / Import operations
        Task<List<StagingMilestoneDto>> GetStagingRowsAsync(int id);

        Task<PaginatedResult<StagingMilestoneDto>> GetAllStagingRowsAsync(QueryParameters<string> parameters, string? createdBy = null);
        Task<StagingMilestoneDto> AddStagingRowAsync(StagingMilestoneDto dto, int year, string? createdBy = null);
        Task<StagingMilestoneDto> UpdateStagingRowAsync(StagingMilestoneDto dto, string? createdBy = null);
        Task<bool> DeleteStagingRowAsync(int id, string? createdBy = null);
        Task<int> ClearStagingAsync(string project, string? createdBy = null);
        Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode, string? createdBy = null);
        Task<int> ImportStagingAsync(string project, string? changedBy = null, string? createdBy = null);
        Task<int> ImportWithOverwriteAsync(string project, string? changedBy = null, string? createdBy = null);
        Task<string> GetNextMilestoneNumberAsync(string project, int year);

        Task<List<ProjectYearManagerDto>> GetProjectYearManagersAsync(int year);
    }
}
