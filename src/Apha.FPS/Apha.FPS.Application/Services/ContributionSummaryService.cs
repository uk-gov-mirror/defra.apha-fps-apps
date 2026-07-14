using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service for the Income/Contribution from Time Sales summary (frmTimeSellerPC).
    /// </summary>
    public class ContributionSummaryService : IContributionSummaryService
    {
        private const string AsuSellingPc = "ASU";

        private readonly IContributionSummaryRepository _repository;
        private readonly IAnimalRepository _animalRepository;

        public ContributionSummaryService(
            IContributionSummaryRepository repository,
            IAnimalRepository animalRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _animalRepository = animalRepository ?? throw new ArgumentNullException(nameof(animalRepository));
        }

        /// <inheritdoc/>
        public async Task<List<ContributionSummaryRowDto>> GetRowsAsync(
            string sellingPc,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sellingPc);

            var rows = await _repository.GetBySellingPcAsync(sellingPc);

            return rows.Select(r => new ContributionSummaryRowDto
            {
                WgGrade = r.WgGrade,
                WorkGroup = r.WorkGroup,
                ProfitCentreGrade = r.ProfitCentreGrade,
                Hrs = r.Hrs,
                AvHrs = r.AvHrs,
                ChargeRate = r.ChargeRate,
                Ohr = r.Ohr,
                Fec = r.Fec,
                Contribution = r.Contribution,
                AppHours = r.AppHours,
                AppFec = r.AppFec,
                // % Planned = Hrs / AvHrs; return null when AvHrs is 0 (form showed "!" in that case)
                PctPlanned = (r.AvHrs.HasValue && r.AvHrs.Value != 0 && r.Hrs.HasValue)
                    ? r.Hrs.Value / r.AvHrs.Value
                    : (double?)null,
                // % Assured Planned = AppHours / AvHrs
                PctAssuredPlanned = (r.AvHrs.HasValue && r.AvHrs.Value != 0 && r.AppHours.HasValue)
                    ? r.AppHours.Value / r.AvHrs.Value
                    : (double?)null
            }).ToList();
        }

        /// <inheritdoc/>
        public async Task<ContributionSummaryTotalsDto> GetTotalsAsync(
            string sellingPc,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sellingPc);

            var rows = await _repository.GetBySellingPcAsync(sellingPc);

            // ContTarget and SumOfGenBid are the same on every row
            var firstRow = rows.FirstOrDefault();
            var contTarget = firstRow?.ContTarget ?? 0m;
            var sumOfGenBid = firstRow?.SumOfGenBid ?? 0m;

            // Footer aggregates — Sum across all rows
            var totalFec = rows.Sum(r => r.Fec ?? 0m);
            var totalContribution = rows.Sum(r => r.Contribution ?? 0m);
            var totalAppFec = rows.Sum(r => r.AppFec ?? 0m);

            // Total To Recover = ContTarget + SumOfGenBid
            var totalToRecover = contTarget + sumOfGenBid;

            // ASU special case: add global animal costs to the surplus calculation
            var isAsuMode = string.Equals(sellingPc, AsuSellingPc, StringComparison.OrdinalIgnoreCase);
            var animalCosts = isAsuMode
                ? await _animalRepository.GetGlobalAnimalCostAsync()
                : 0m;

            // Surplus/Shortfall (Total Time panel) = TotalFec - TotalToRecover + AnimalCosts
            var surplus = totalFec - totalToRecover + animalCosts;

            // Surplus/Shortfall (Assured Time panel) = TotalAppFec - TotalToRecover
            var assuredSurplus = totalAppFec - totalToRecover;

            return new ContributionSummaryTotalsDto
            {
                SellingPc = sellingPc,
                ContTarget = contTarget,
                SumOfGenBid = sumOfGenBid,
                TotalFec = totalFec,
                TotalContribution = totalContribution,
                TotalAppFec = totalAppFec,
                TotalToRecover = totalToRecover,
                Surplus = surplus,
                AssuredSurplus = assuredSurplus,
                AnimalCosts = animalCosts,
                IsAsuMode = isAsuMode
            };
        }
    }
}
