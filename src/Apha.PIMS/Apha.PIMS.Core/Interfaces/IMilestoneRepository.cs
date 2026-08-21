using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IMilestoneRepository
    {
        Task<PagedData<Milestone>> GetAllMilestonesAsync(PaginationParameters<string> parameters, string project);
        Task<PagedData<Milestone>> GetPMDMilestonesAsync(PaginationParameters<string> parameters, string project);
        Task<Milestone?> GetMilestoneAsync(string project, string number);
        Task<string> GetProgramByProjectAsync(string project);
        Task<Milestone> AddMilestoneAsync(Milestone entity, string? changedBy);
        Task<Milestone> UpdateMilestoneAsync(Milestone entity, string? changedBy);
        Task<Milestone> UpdateMilestoneAsync_PMD(string project, string number, short underReview, short onTarget, DateTime? dateCompleted, string? projectLeaderComment, string? changedBy);
        Task<bool> DeleteMilestoneAsync(string project, string number);
        Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired);
        // Lookup
        Task<List<MilestoneType>> GetMilestoneTypesAsync(string? milestoneDeliverable = null);

        // MilestoneFormDates operations
        Task<PagedData<MilestoneFormDates>> GetAllMilestoneFormDatesAsync(PaginationParameters<string> parameters, string parentProject);
        Task<MilestoneFormDates?> GetMilestoneFormDatesAsync(short year, string parentProject);
        Task<MilestoneFormDates> AddMilestoneFormDatesAsync(MilestoneFormDates entity);
        Task<MilestoneFormDates> UpdateMilestoneFormDatesAsync(MilestoneFormDates entity);
        Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject);

        // Log Milestone operations
        Task<PagedData<LogMilestone>> GetLogMilestonesAsync(PaginationParameters<string> parameters, string? project, string? numberPart1, string? numberPart2);
        // Staging / Import operations
        Task<List<StagingMilestone>> GetStagingRowsAsync(int id);

        Task<PagedData<StagingMilestone>> GetAllStagingRowsAsync(PaginationParameters<string> parameters, string? createdBy = null);
        Task<StagingMilestone> AddStagingRowAsync(StagingMilestone entity, string? createdBy = null);
        Task<StagingMilestone> UpdateStagingRowAsync(StagingMilestone entity, string? createdBy = null);
        Task<bool> DeleteStagingRowAsync(int id, string? createdBy = null);
        Task<int> ClearStagingAsync(string project, string? createdBy = null);
        Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode, string? createdBy = null);
        Task<int> ImportStagingAsync(string project, string? changedBy = null, string? createdBy = null);
        Task<int> ImportWithOverwriteAsync(string project, string? changedBy = null, string? createdBy = null);
        Task<string> GetNextMilestoneNumberAsync(string project, int year);

        // Project Year Manager operations
        Task<List<ProjectYearManager>> GetProjectYearManagersAsync(int year, string? loginEmail = null, bool viewSpecificProject = false);
    }
}
