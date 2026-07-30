using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using Npgsql;

namespace Apha.FPS.Application.Services
{
    public class CostCentreService : ICostCentreService
    {
        private readonly ICostCentreRepository _repository;
        private readonly IProfitCentreRepository _profitCentreRepository;
        private readonly IMapper _mapper;

        public CostCentreService(
            ICostCentreRepository repository,
            IProfitCentreRepository profitCentreRepository,
            IMapper mapper)
        {
            _repository = repository;
            _profitCentreRepository = profitCentreRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<CostCentreDto>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var queryParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedResult = await _repository.GetAllPagedAsync(queryParams);
            return _mapper.Map<PaginatedResult<CostCentreDto>>(pagedResult);
        }

        public async Task<CostCentreDto?> GetCostCentreByIdAsync(double costCentreNo, int fpsYear)
        {
            var entity = await _repository.GetByIdAsync(costCentreNo, fpsYear);
            return entity == null ? null : _mapper.Map<CostCentreDto>(entity);
        }

        //   Guard 1: ArgumentNullException if dto null (fail fast before any I/O)
        //   Guard 2: InvalidOperationException if composite key already exists (duplicate prevention from VBA analysis)
        //   Guard 3: InvalidOperationException if ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            if (await _repository.ExistsAsync(costCentreDto.CostCentreNo, costCentreDto.FpsYear))
                throw new InvalidOperationException(
                    $"A cost centre with number '{costCentreDto.CostCentreNo}' already exists for FPS year '{costCentreDto.FpsYear}'.");

            //   Validates that the supplied ProfitCentre code exists in fps.tblkpprofitcentre before inserting
            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<CostCentreDto>(created);
        }

        //   Guard 1: ArgumentNullException if dto null
        //   Guard 2: KeyNotFoundException if original record does not exist
        //   Guard 3: InvalidOperationException if new ProfitCentre FK does not exist in tblkpprofitcentre (transform-plan.md item 1)
        public async Task<CostCentreDto> UpdateCostCentreAsync(double originalCostCentreNo, int fpsYear, CostCentreDto costCentreDto)
        {
            ArgumentNullException.ThrowIfNull(costCentreDto);
            ArgumentException.ThrowIfNullOrWhiteSpace(costCentreDto.ProfitCentre);

            if (!await _repository.ExistsAsync(originalCostCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{originalCostCentreNo}' for FPS year '{fpsYear}' was not found.");

            //   Guard: if the cost centre number is being changed, the new number must not already exist
            if (originalCostCentreNo != costCentreDto.CostCentreNo
                && await _repository.ExistsAsync(costCentreDto.CostCentreNo, fpsYear))
                throw new InvalidOperationException(
                    $"A cost centre with number '{costCentreDto.CostCentreNo}' already exists for FPS year '{fpsYear}'.");

            var profitCentreExists = await _profitCentreRepository.ProfitCentreExistsAsync(costCentreDto.ProfitCentre);
            if (!profitCentreExists)
                throw new InvalidOperationException(
                    $"Profit centre '{costCentreDto.ProfitCentre}' does not exist. Select a valid profit centre.");

            var entity = _mapper.Map<CostCentre>(costCentreDto);
            try
            {
                var updated = await _repository.UpdateAsync(originalCostCentreNo, fpsYear, entity);
                return _mapper.Map<CostCentreDto>(updated);
            }
            catch (Exception ex) when (IsWorkgroupForeignKeyViolation(ex))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        "There are associated records in the workgroup table so this record cannot be edited. Please update to a cost center which does not have any associated records in the workgorup table.",
                        "WORKGROUP_FK_VIOLATION")
                });
            }
        }

        //   Guard: KeyNotFoundException if record does not exist before attempting delete
        public async Task<bool> DeleteCostCentreAsync(double costCentreNo, int fpsYear)
        {
            if (!await _repository.ExistsAsync(costCentreNo, fpsYear))
                throw new KeyNotFoundException(
                    $"Cost centre '{costCentreNo}' for FPS year '{fpsYear}' was not found.");

            try
            {
                return await _repository.DeleteAsync(costCentreNo, fpsYear);
            }
            catch (Exception ex) when (IsWorkgroupForeignKeyViolation(ex))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        "There are associated records in the workgroup table so this record cannot be deleted.",
                        "WORKGROUP_FK_VIOLATION")
                });
            }
        }

        // Screen-specific handling for the Maintain Cost Centres workgroup foreign key constraint.
        // The violation surfaces as a PostgresException (SqlState 23503) usually wrapped inside a DbUpdateException.
        private static bool IsWorkgroupForeignKeyViolation(Exception? ex)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation
                    && pgEx.ConstraintName?.Contains("fk_workgroup_costcentre", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
