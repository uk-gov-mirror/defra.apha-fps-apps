using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProfitCentreService : IProfitCentreService
    {
        private readonly IProfitCentreRepository _repository;
        private readonly IMapper _mapper;

        public ProfitCentreService(IProfitCentreRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<ProfitCentreDto>> GetProfitCentresAsync()
        {
            var result = await _repository.GetProfitCentresAsync();
            return _mapper.Map<List<ProfitCentreDto>>(result);
        }

        public async Task<IEnumerable<ProfitCentreDto>> GetAllProfitCentresAsync()
        {
            var views = await _repository.GetAllProfitCentresAsync();
            return _mapper.Map<IEnumerable<ProfitCentreDto>>(views);
        }

        public async Task<PaginatedResult<ProfitCentreDto>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllProfitCentresPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<ProfitCentreDto>>(pagedResult);
        }

        public async Task<ProfitCentreDto?> GetProfitCentreByIdAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            var profitCentre = await _repository.GetProfitCentreByIdAsync(profitCentreId);
            return profitCentre == null ? null : _mapper.Map<ProfitCentreDto>(profitCentre);
        }

        public async Task<ProfitCentreDto> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            ArgumentNullException.ThrowIfNull(profitCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreDto.ProfitCentreId);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreDto.ProfitCentreName);

            if (await _repository.ProfitCentreExistsAsync(profitCentreDto.ProfitCentreId))
                throw new InvalidOperationException($"Profit centre '{profitCentreDto.ProfitCentreId}' already exists.");

            var entity = _mapper.Map<ProfitCentre>(profitCentreDto);
            var created = await _repository.CreateProfitCentreAsync(entity);
            return _mapper.Map<ProfitCentreDto>(created);
        }

        public async Task<ProfitCentreDto> UpdateProfitCentreAsync(string originalProfitCentreId, ProfitCentreDto profitCentreDto)
        {
            ArgumentNullException.ThrowIfNull(profitCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(originalProfitCentreId);

            if (!await _repository.ProfitCentreExistsAsync(originalProfitCentreId))
                throw new InvalidOperationException($"Profit centre '{originalProfitCentreId}' not found.");

            var entity = _mapper.Map<ProfitCentre>(profitCentreDto);
            var updated = await _repository.UpdateProfitCentreAsync(originalProfitCentreId, entity);
            return _mapper.Map<ProfitCentreDto>(updated);
        }

        public async Task<bool> DeleteProfitCentreAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            if (await _repository.HasLinkedGradesAsync(profitCentreId))
                throw new InvalidOperationException("Cannot delete profit centre: it is referenced by profit centre grade records.");

            if (await _repository.HasLinkedWorkgroupsAsync(profitCentreId))
                throw new InvalidOperationException("Cannot delete profit centre: it is referenced by work group records.");

            return await _repository.DeleteProfitCentreAsync(profitCentreId);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            return await _repository.UpdateProfitCentreSettingsAsync(profitCentre, timesheet, outputsheet, timesheetlayout);
        }

        public async Task<PaginatedResult<ProfitCentreCostDto>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber)
        {
            ArgumentNullException.ThrowIfNull(query);

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedProfitCenterCostSummaryAsync(parameters, monthNumber);

            return _mapper.Map<PaginatedResult<ProfitCentreCostDto>>(pagedData);
        }

        public async Task<PaginatedResult<WgStaffPlanViewDto>> GetPagedWgStaffPlanAsync(
            QueryParameters<string> query, string workGroup)
        {
            ArgumentNullException.ThrowIfNull(query);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedWgStaffPlanAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WgStaffPlanViewDto>>(pagedData);
        }
    }
}
