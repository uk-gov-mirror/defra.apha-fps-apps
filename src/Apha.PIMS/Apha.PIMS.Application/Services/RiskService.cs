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
    public class RiskService : IRiskService
    {
        private readonly IRiskRepository _repository;
        private readonly IMapper _mapper;

        public RiskService(IRiskRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<RiskDto>> GetAllRiskRatingsAsync()
        {
            List<Risk> entities = await _repository.GetAllRiskRatingsAsync();
            return _mapper.Map<List<RiskDto>>(entities);
        }

        public async Task<PaginatedResult<RiskDto>> GetPagedRiskRatingsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedRiskRatingsAsync(parameters);
            return _mapper.Map<PaginatedResult<RiskDto>>(pagedData);
        }

        public async Task<RiskDto?> GetRiskRatingByIdAsync(int riskId)
        {
            Risk? entity = await _repository.GetRiskRatingByIdAsync(riskId);
            return entity is null ? null : _mapper.Map<RiskDto>(entity);
        }

        public async Task<RiskDto> CreateRiskRatingAsync(RiskDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool duplicate = await _repository.RiskRatingExistsAsync(dto.RiskId);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A risk rating with ID '{dto.RiskId}' already exists.",
                        "RISK_DUPLICATE")
                ]);

            Risk entity = _mapper.Map<Risk>(dto);
            Risk created = await _repository.AddRiskRatingAsync(entity);
            return _mapper.Map<RiskDto>(created);
        }

        public async Task<RiskDto> UpdateRiskRatingAsync(RiskDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.RiskRatingExistsAsync(dto.RiskId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Risk rating with riskId {dto.RiskId} was not found.",
                        "RISK_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            Risk entity = _mapper.Map<Risk>(dto);
            Risk updated = await _repository.UpdateRiskRatingAsync(entity);
            return _mapper.Map<RiskDto>(updated);
        }

        public async Task<bool> DeleteRiskRatingAsync(int riskId)
        {
            bool exists = await _repository.RiskRatingExistsAsync(riskId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Risk rating with riskId {riskId} was not found.",
                        "RISK_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeleteRiskRatingAsync(riskId);
        }

        public async Task<bool> RiskRatingExistsAsync(int riskId)
        {
            return await _repository.RiskRatingExistsAsync(riskId);
        }
    }
}
