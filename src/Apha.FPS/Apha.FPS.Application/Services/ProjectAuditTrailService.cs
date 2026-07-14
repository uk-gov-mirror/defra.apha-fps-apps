using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectAuditTrailService : IProjectAuditTrailService
    {
        private readonly IProjectAuditTrailRepository _repository;
        private readonly IMapper _mapper;

        public ProjectAuditTrailService(IProjectAuditTrailRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectLogDto>> GetProjectLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetProjectLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<ProjectLogDto>>(data);
        }

        public async Task<PaginatedResult<StaffJobLogDto>> GetStaffJobLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetStaffJobLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<StaffJobLogDto>>(data);
        }

        public async Task<PaginatedResult<TestRequirementLogDto>> GetTestRequirementLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetTestRequirementLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<TestRequirementLogDto>>(data);
        }

        public async Task<PaginatedResult<AnimalRequestLogDto>> GetAnimalRequestLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetAnimalRequestLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<AnimalRequestLogDto>>(data);
        }

        public async Task<PaginatedResult<AdditionalCostLogDto>> GetAdditionalCostLogsAsync(
            QueryParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentProject);

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetAdditionalCostLogsAsync(filter, parentProject, fromDate, toDate);
            return _mapper.Map<PaginatedResult<AdditionalCostLogDto>>(data);
        }
    }
}
