using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class MonthlyOutputService : IMonthlyOutputService
    {
        private readonly IFpsApiClient _fpsClient;

        public MonthlyOutputService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        private static readonly HashSet<string> ComputedSortColumns =
            new(StringComparer.OrdinalIgnoreCase) { nameof(MonthlyOutputDto.TestPrice), nameof(MonthlyOutputDto.Charge) };

        public async Task<ApiResponseDto<List<MonthlyOutputDto>>> GetMonthlyOutputByProjectAsync(QueryParameters<string> query, string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup)
        {
            // TestPrice (Rate) and Charge are computed in this layer from the price lookup and do not
            // exist in the data store, so the API cannot sort by them. When sorting by these computed
            // columns, fetch the full filtered set, enrich, then sort and page in memory.
            if (!string.IsNullOrEmpty(query.SortBy) && ComputedSortColumns.Contains(query.SortBy))
                return await GetSortedByComputedColumnAsync(query, projectCode, priceLookup);

            var result = await _fpsClient.FpsMonthlyOutput.GetByProjectAsync(query, projectCode);
            if (!result.Success || result.Data == null)
                return result;

            EnrichWithPrices(result.Data, priceLookup);
            return result;
        }

        private async Task<ApiResponseDto<List<MonthlyOutputDto>>> GetSortedByComputedColumnAsync(
            QueryParameters<string> query, string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup)
        {
            var allQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = int.MaxValue,
                Filter = query.Filter,
                Search = query.Search
            };

            var result = await _fpsClient.FpsMonthlyOutput.GetByProjectAsync(allQuery, projectCode);
            if (!result.Success || result.Data == null)
                return result;

            EnrichWithPrices(result.Data, priceLookup);

            Func<MonthlyOutputDto, double> keySelector = string.Equals(query.SortBy, nameof(MonthlyOutputDto.Charge), StringComparison.OrdinalIgnoreCase)
                ? item => item.Charge ?? 0
                : item => item.TestPrice ?? 0;

            var sorted = (query.Descending
                ? result.Data.OrderByDescending(keySelector)
                : result.Data.OrderBy(keySelector)).ToList();

            var totalRecords = sorted.Count;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;
            var page = query.Page <= 0 ? 1 : query.Page;

            result.Data = sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            result.Pagination = new PaginationDto
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };

            return result;
        }

        public async Task<ApiResponseDto<double>> GetTotalActualByProjectAsync(string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup)
        {
            var totalCost = await ComputeTotalCostAsync(projectCode, priceLookup);
            return new ApiResponseDto<double> { Success = true, Data = totalCost };
        }

        private async Task<double> ComputeTotalCostAsync(string projectCode, Dictionary<(string TestCode, string Buyer), decimal> priceLookup)
        {
            var allQuery = new QueryParameters<string> { Page = 1, PageSize = 9999 };
            var dataResult = await _fpsClient.FpsMonthlyOutput.GetByProjectAsync(allQuery, projectCode);
            if (!dataResult.Success || dataResult.Data == null)
                return 0;

            return dataResult.Data.Sum(item =>
            {
                var key = (item.TestCode ?? string.Empty, item.Buyer ?? string.Empty);
                return priceLookup.TryGetValue(key, out var unitPrice)
                    ? (item.Volume ?? 0) * (double)unitPrice
                    : 0;
            });
        }

        public async Task<ApiResponseDto<bool>> DeleteMonthlyOutputAsync(string buyer, string testCode, double month, string workGroup)
            => await _fpsClient.FpsMonthlyOutput.DeleteMonthlyOutputAsync(buyer, testCode, month, workGroup);

        private static void EnrichWithPrices(List<MonthlyOutputDto> items, Dictionary<(string TestCode, string Buyer), decimal> priceLookup)
        {
            foreach (var item in items)
            {
                var key = (item.TestCode ?? string.Empty, item.Buyer ?? string.Empty);
                if (priceLookup.TryGetValue(key, out var unitPrice))
                {
                    item.TestPrice = (double)unitPrice;
                    item.Charge    = Math.Round((item.Volume ?? 0) * (double)unitPrice, 2);
                }
            }
        }
    }
}
