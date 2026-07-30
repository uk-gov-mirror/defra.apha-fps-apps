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
    public class YearlyFinancialDataService : IYearlyFinancialDataService
    {
        private readonly IYearlyFinancialDataRepository _repository;
        private readonly IMapper _mapper;

        public YearlyFinancialDataService(IYearlyFinancialDataRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<YearlyFinancialDataDto>> GetAllAsync(QueryParameters<string> parameters)
        {
            if (parameters is null)
                throw new ArgumentException("Query parameters must not be null.", nameof(parameters));

            PaginationParameters<string> paginationParams =
                _mapper.Map<PaginationParameters<string>>(parameters);

            PagedData<YearlyFinancialData> pagedData =
                await _repository.GetAllAsync(parameters.Filter ?? string.Empty, paginationParams);

            return new PaginatedResult<YearlyFinancialDataDto>
            {
                Data = _mapper.Map<List<YearlyFinancialDataDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }

        public async Task<YearlyFinancialDataDto?> GetByKeyAsync(short year, string project)
        {
            YearlyFinancialData? entity = await _repository.GetByKeyAsync(year, project);
            return entity is null ? null : _mapper.Map<YearlyFinancialDataDto>(entity);
        }

       
        public async Task<YearlyFinancialDataDto> CreateAsync(YearlyFinancialDataDto dto)
        {
            if (dto is null)
                throw new ArgumentException("YearlyFinancialData DTO must not be null.", nameof(dto));

            dto.Project = dto.Project?.Trim();

           
            List<BusinessValidationError> errors = ValidateForSave(dto);
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

           
            bool duplicate = await _repository.ExistsAsync(dto.Year, dto.Project!);
            if (duplicate)
            {
                errors.Add(new BusinessValidationError(
                    $"A yearly financial data record for year {dto.Year} and project '{dto.Project}' already exists.",
                    "DUPLICATE_YEARLY_FINANCIAL_DATA"));
                throw new BusinessValidationErrorException(errors);
            }

            await ApplyLegacyCostingRulesAsync(dto);

            YearlyFinancialData newEntity = _mapper.Map<YearlyFinancialData>(dto);
            YearlyFinancialData created = await _repository.CreateAsync(newEntity);
            return _mapper.Map<YearlyFinancialDataDto>(created);
        }


        public async Task<YearlyFinancialDataDto> UpdateAsync(YearlyFinancialDataDto dto)
        {
            if (dto is null)
                throw new ArgumentException("YearlyFinancialData DTO must not be null.", nameof(dto));

            dto.Project = dto.Project?.Trim();

            
            List<BusinessValidationError> errors = ValidateForSave(dto);
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            
            YearlyFinancialData existing =
                await _repository.GetByKeyAsync(dto.Year, dto.Project!)
                ?? throw new KeyNotFoundException(
                    $"Yearly financial data record for year {dto.Year} and project '{dto.Project}' was not found.");

            await ApplyLegacyCostingRulesAsync(dto);

            
            _mapper.Map(dto, existing);

            YearlyFinancialData updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<YearlyFinancialDataDto>(updated);
        }

        private List<BusinessValidationError> ValidateForSave(YearlyFinancialDataDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto.Year <= 0)
                errors.Add(new BusinessValidationError(
                    "Year is required and must be a valid financial year.",
                    "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError(
                    "Project is required.",
                    "PROJECT_REQUIRED"));

            if ((dto.Adjustment ?? 0m) != 0m && string.IsNullOrWhiteSpace(dto.AdjustmentComment))
                errors.Add(new BusinessValidationError(
                    "Please enter reason for adjustment figure, (or remove the adjustment figure).",
                    "ADJUSTMENT_COMMENT_REQUIRED"));

            return errors;
        }

        private async Task ApplyLegacyCostingRulesAsync(YearlyFinancialDataDto dto)
        {
            PactYearSnapshot pact = await GetPactYearSnapshotAsync(dto.Project!, dto.Year);
            ApplyChangedFlags(dto, pact);
            ApplyReportedFigures(dto, pact);
        }

        private async Task<PactYearSnapshot> GetPactYearSnapshotAsync(string project, short year)
        {
            IReadOnlyList<PactProjectYearCosts> rows = await _repository.GetPactCostsAsync(project, year);

            if (rows.Count == 0)
            {
                return new PactYearSnapshot(null, null, null, null, null, null, null);
            }

            return new PactYearSnapshot(
                rows.Sum(x => x.TotalCosts ?? 0m),
                rows.Sum(x => x.Hours ?? 0d),
                rows.Sum(x => x.Pay ?? 0m),
                rows.Sum(x => x.NonPayOH ?? 0m),
                rows.Sum(x => x.Tests ?? 0m),
                rows.Sum(x => x.Animals ?? 0m),
                rows.Sum(x => (x.SubContracts ?? 0m) - (x.Animals ?? 0m)));
        }

        private static void ApplyChangedFlags(YearlyFinancialDataDto dto, PactYearSnapshot pact)
        {
            dto.ManHoursChanged = GetChangedFlag(dto.ManHours, pact.Hours);
            dto.PayCostsChanged = GetChangedFlag(dto.PayCosts, pact.Pay);
            dto.NonPayOhCostsChanged = GetChangedFlag(dto.NonPayOhCosts, pact.NonPayOH);
            dto.TestCostsChanged = GetChangedFlag(dto.TestCosts, pact.Tests);
            dto.AnimalCostsChanged = GetChangedFlag(dto.AnimalCosts, pact.Animals);
            dto.NonAnimalCostsChanged = GetChangedFlag(dto.NonAnimalCosts, pact.ProjectSpecific);
        }

        private static void ApplyReportedFigures(YearlyFinancialDataDto dto, PactYearSnapshot pact)
        {
            if (dto.Locked == 0)
            {
                return;
            }

            dto.ActualExpenditure = dto.Adjustment.HasValue
                ? (pact.TotalCosts ?? 0m) + dto.Adjustment.Value
                : null;

            if (!dto.DateCosted.HasValue)
            {
                dto.DateCosted = DateTime.Now;
            }
        }

        private static short GetChangedFlag(decimal? currentValue, decimal? pactValue)
        {
            if (!currentValue.HasValue && !pactValue.HasValue)
            {
                return 0;
            }

            if (!currentValue.HasValue || !pactValue.HasValue)
            {
                return 1;
            }

            return currentValue.Value == pactValue.Value ? (short)0 : (short)1;
        }

        private static short GetChangedFlag(double? currentValue, double? pactValue)
        {
            if (!currentValue.HasValue && !pactValue.HasValue)
            {
                return 0;
            }

            if (!currentValue.HasValue || !pactValue.HasValue)
            {
                return 1;
            }

            return Math.Abs(currentValue.Value - pactValue.Value) < 0.000001d ? (short)0 : (short)1;
        }

        private sealed record PactYearSnapshot(
            decimal? TotalCosts,
            double? Hours,
            decimal? Pay,
            decimal? NonPayOH,
            decimal? Tests,
            decimal? Animals,
            decimal? ProjectSpecific);

       
        public async Task<bool> DeleteAsync(short year, string project)
            => await _repository.DeleteAsync(year, project);

       
        public async Task<IReadOnlyList<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("Project is required.", nameof(project));

            if (year <= 0)
                throw new ArgumentException("Year must be a valid financial year.", nameof(year));

            IReadOnlyList<PactProjectYearCosts> rows =
                await _repository.GetPactCostsAsync(project, year);

            return _mapper.Map<IReadOnlyList<PactProjectYearCostsDto>>(rows);
        }

        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            return (await _repository.GetSettingValueByIdAsync(id.Trim())) ?? string.Empty;
        }
    }
}
