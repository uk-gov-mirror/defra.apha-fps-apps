using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Grade maintenance business logic.
    /// Orchestrates repository calls and enforces business rules extracted from frmMaintGrade VBA analysis.
    /// </summary>
    public class GradeService : IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IDivisionGradeRepository _divisionGradeRepository;
        private readonly IProfitCentreGradeRepository _profitCentreGradeRepository;
        private readonly IWorkGroupGradeRepository _workGroupGradeRepository;
        private readonly IMapper _mapper;

        public GradeService(
            IGradeRepository gradeRepository,
            IDivisionGradeRepository divisionGradeRepository,
            IProfitCentreGradeRepository profitCentreGradeRepository,
            IWorkGroupGradeRepository workGroupGradeRepository,
            IMapper mapper)
        {
            _gradeRepository             = gradeRepository             ?? throw new ArgumentNullException(nameof(gradeRepository));
            _divisionGradeRepository     = divisionGradeRepository     ?? throw new ArgumentNullException(nameof(divisionGradeRepository));
            _profitCentreGradeRepository = profitCentreGradeRepository ?? throw new ArgumentNullException(nameof(profitCentreGradeRepository));
            _workGroupGradeRepository    = workGroupGradeRepository    ?? throw new ArgumentNullException(nameof(workGroupGradeRepository));
            _mapper                      = mapper                      ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public async Task<PaginatedResult<GradeDto>> GetAllPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedGrades = await _gradeRepository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<GradeDto>>(pagedGrades);
        }

        /// <inheritdoc />
        public async Task<GradeDto?> GetByIdAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                throw new ArgumentException("Grade code cannot be null or empty.", nameof(gradeCode));
            }

            var grade = await _gradeRepository.GetByIdAsync(gradeCode);
            return grade == null ? null : _mapper.Map<GradeDto>(grade);
        }

        /// <inheritdoc />
        public async Task<GradeDto> CreateAsync(GradeDto gradeDto)
        {
            ArgumentNullException.ThrowIfNull(gradeDto);

            if (string.IsNullOrWhiteSpace(gradeDto.GradeCode))
            {
                throw new ArgumentException("Grade code is required.", nameof(gradeDto));
            }

            // Guard: check for duplicate GradeCode in the active FPS year
            var existing = await _gradeRepository.GetByIdAsync(gradeDto.GradeCode);
            if (existing != null)
            {
                throw new InvalidOperationException($"Grade '{gradeDto.GradeCode}' already exists for the current FPS year.");
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            var createdGrade = await _gradeRepository.CreateAsync(grade);
            return _mapper.Map<GradeDto>(createdGrade);
        }

        /// <inheritdoc />
        public async Task<GradeDto> UpdateAsync(string originalCode, GradeDto gradeDto)
        {
            ArgumentNullException.ThrowIfNull(gradeDto);

            if (string.IsNullOrWhiteSpace(originalCode))
            {
                throw new ArgumentException("Original grade code is required to identify the record.", nameof(originalCode));
            }

            if (string.IsNullOrWhiteSpace(gradeDto.GradeCode))
            {
                throw new ArgumentException("Grade code is required.", nameof(gradeDto));
            }

            // Guard: verify the record to update actually exists
            var existingGrade = await _gradeRepository.GetByIdAsync(originalCode);
            if (existingGrade == null)
            {
                throw new KeyNotFoundException($"Grade '{originalCode}' not found in the current FPS year.");
            }

            // Guard: if GradeCode is being renamed, ensure the new code does not already exist
            if (!originalCode.Equals(gradeDto.GradeCode, StringComparison.OrdinalIgnoreCase))
            {
                var conflictingGrade = await _gradeRepository.GetByIdAsync(gradeDto.GradeCode);
                if (conflictingGrade != null)
                {
                    throw new InvalidOperationException($"Cannot rename to '{gradeDto.GradeCode}' — a grade with that code already exists for the current FPS year.");
                }
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            var updatedGrade = await _gradeRepository.UpdateAsync(originalCode, grade);
            return _mapper.Map<GradeDto>(updatedGrade);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string gradeCode)
        {
            if (string.IsNullOrWhiteSpace(gradeCode))
            {
                throw new ArgumentException("Grade code cannot be null or empty.", nameof(gradeCode));
            }

            // Guard: verify the record to delete actually exists
            var existing = await _gradeRepository.GetByIdAsync(gradeCode);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Grade '{gradeCode}' not found in the current FPS year.");
            }

            // Guard: block delete if the grade is referenced by Division Grade records
            if (await _divisionGradeRepository.ExistsForGradeCodeAsync(gradeCode))
            {
                throw new InvalidOperationException(
                    $"Cannot delete grade '{gradeCode}' because it is referenced by one or more Division Grade records.");
            }

            // Guard: block delete if the grade is referenced by RC Grade (ProfitCentreGrade) records
            if (await _profitCentreGradeRepository.ExistsForGradeCodeAsync(gradeCode))
            {
                throw new InvalidOperationException(
                    $"Cannot delete grade '{gradeCode}' because it is referenced by one or more RC Grade records.");
            }

            // Guard: block delete if the grade is referenced by WG Grade (WorkgroupGrade) records
            if (await _workGroupGradeRepository.ExistsForGradeCodeAsync(gradeCode))
            {
                throw new InvalidOperationException(
                    $"Cannot delete grade '{gradeCode}' because it is referenced by one or more WG Grade records.");
            }

            return await _gradeRepository.DeleteAsync(gradeCode);
        }
    }
}
