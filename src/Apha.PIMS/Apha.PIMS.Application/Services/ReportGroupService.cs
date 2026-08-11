using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    
    public class ReportGroupService : IReportGroupService
    {
        private readonly IReportGroupRepository _repository;
        private readonly IMapper _mapper;

        public ReportGroupService(IReportGroupRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

       
        public async Task<List<ReportGroupDto>> GetAllReportGroupsAsync()
        {
            List<ReportGroup> entities = await _repository.GetAllReportGroupsAsync();
            return _mapper.Map<List<ReportGroupDto>>(entities);
        }

        
        public async Task<PaginatedResult<ReportGroupDto>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedReportGroupsAsync(parameters, reportId);
            return _mapper.Map<PaginatedResult<ReportGroupDto>>(pagedData);
        }

        
        public async Task<List<ReportGroupDto>> GetReportGroupsByReportIdAsync(int reportId)
        {
            List<ReportGroup> entities = await _repository.GetReportGroupsByReportIdAsync(reportId);
            return _mapper.Map<List<ReportGroupDto>>(entities);
        }

        
        public async Task<ReportGroupDto?> GetReportGroupByIdAsync(int groupId)
        {
            ReportGroup? entity = await _repository.GetReportGroupByIdAsync(groupId);
            return entity is null ? null : _mapper.Map<ReportGroupDto>(entity);
        }

        
        public async Task<ReportGroupDto> CreateReportGroupAsync(ReportGroupDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Group description is required.", nameof(dto));

            bool duplicate = await _repository.ReportGroupExistsAsync(dto.GroupId);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A report group with ID '{dto.GroupId}' already exists.",
                        "REPORT_GROUP_DUPLICATE")
                ]);

            ReportGroup entity = _mapper.Map<ReportGroup>(dto);
            ReportGroup created = await _repository.AddReportGroupAsync(entity);
            return _mapper.Map<ReportGroupDto>(created);
        }

        
        public async Task<ReportGroupDto> UpdateReportGroupAsync(ReportGroupDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Group description is required.", nameof(dto));

            bool exists = await _repository.ReportGroupExistsAsync(dto.GroupId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"ReportGroup with groupid {dto.GroupId} was not found.",
                        "REPORT_GROUP_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            ReportGroup entity = _mapper.Map<ReportGroup>(dto);
            ReportGroup updated = await _repository.UpdateReportGroupAsync(entity);
            return _mapper.Map<ReportGroupDto>(updated);
        }

        
        public async Task<bool> DeleteReportGroupAsync(int groupId)
        {
            bool exists = await _repository.ReportGroupExistsAsync(groupId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"ReportGroup with groupid {groupId} was not found.",
                        "REPORT_GROUP_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            bool hasLinkedReports = await _repository.HasLinkedReportsAsync(groupId);
            if (hasLinkedReports)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Report Group {groupId} cannot be deleted because it is currently linked to one or more reports. Please remove the linked reports before deleting this group.",
                        "REPORT_GROUP_HAS_LINKED_REPORTS")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeleteReportGroupAsync(groupId);
        }

        public async Task<bool> ReportGroupExistsAsync(int groupId)
        {
            return await _repository.ReportGroupExistsAsync(groupId);
        }
    }
}
