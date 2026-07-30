using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for WorkgroupGrade CRUD and lookup operations.
    /// </summary>
    public class WorkGroupGradeService : IWorkGroupGradeService
    {
        private readonly IWorkGroupGradeRepository _repository;
        private readonly IWorkGroupEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public WorkGroupGradeService(IWorkGroupGradeRepository repository, IWorkGroupEmployeeRepository employeeRepository, IMapper mapper)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkgroupGradeDto>> GetAllWorkgroupGradesPagedAsync(
            QueryParameters<string> query)
        {
            if (query is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Query parameters cannot be null.", "WORKGROUPGRADE_INVALID_QUERY")
                ]);
            }

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetAllWorkgroupGradesPagedAsync(filter);
            return _mapper.Map<PaginatedResult<WorkgroupGradeDto>>(result);
        }

        public async Task<WorkgroupGradeDto?> GetByWgGradeAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WgGrade cannot be null or empty.", "WORKGROUPGRADE_INVALID_CODE")
                ]);
            }

            var entity = await _repository.GetByWgGradeAsync(wgGrade);
            return entity is null ? null : _mapper.Map<WorkgroupGradeDto>(entity);
        }

        public async Task<WorkgroupGradeDto> CreateAsync(WorkgroupGradeDto dto)
        {
            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkgroupGrade data cannot be null.", "WORKGROUPGRADE_INVALID_DATA")
                ]);
            }

            var entity = _mapper.Map<WorkgroupGrade>(dto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<WorkgroupGradeDto>(created);
        }

        public async Task<WorkgroupGradeDto> UpdateAsync(WorkgroupGradeDto dto)
        {
            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkgroupGrade data cannot be null.", "WORKGROUPGRADE_INVALID_DATA")
                ]);
            }

            var entity = _mapper.Map<WorkgroupGrade>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<WorkgroupGradeDto>(updated);
        }

        public async Task<bool> DeleteAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WgGrade cannot be null or empty.", "WORKGROUPGRADE_INVALID_CODE")
                ]);
            }

            var hasAssociations = await _employeeRepository.HasAssociatedStaffAsync(wgGrade);
            if (hasAssociations)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"WorkgroupGrade '{wgGrade}' is associated with existing staff records and cannot be deleted.",
                        "WORKGROUPGRADE_HAS_ASSOCIATIONS")
                ]);
            }

            return await _repository.DeleteAsync(wgGrade);
        }

        public async Task<List<string>> GetAllGradeCodesAsync()
            => await _repository.GetAllGradeCodesAsync();

        public async Task<List<WorkgroupGradeDto>> GetWorkgroupGradesByWorkGroupAsync(
            string workGroup)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroup);
            var result = await _repository.GetWorkgroupGradesByWorkGroupAsync(workGroup);
            return _mapper.Map<List<WorkgroupGradeDto>>(result);
        }

        // Existing methods for backward compatibility
        public async Task<PaginatedResult<WorkgroupGradeDto>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentreGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreGrade);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupGradesAsync(filter, profitCentreGrade);
            return _mapper.Map<PaginatedResult<WorkgroupGradeDto>>(pagedData);
        }

        public async Task<bool> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            return await _repository.DeleteWorkGroupGradeAsync(wgGrade);
        }
    }
}
