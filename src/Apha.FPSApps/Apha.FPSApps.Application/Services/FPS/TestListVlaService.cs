using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service delegate for TestOrProduct VLA list view operations.
    /// Forwards calls to IPactApiClient.PactTestList — FpsYear filter is applied
    /// automatically by the PACT DbContext global query filter; no explicit year needed.
    /// </summary>
    public class TestListVlaService : ITestListVlaService
    {
        private readonly IPactApiClient _client;

        public TestListVlaService(IPactApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllAsync(QueryParameters<string> query)
            => await _client.PactTestList.GetPagedTestOrProductsAsync(query);

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllByYearAsync()
            => await _client.PactTestList.GetAllTestorProductsAsync();

        public async Task<ApiResponseDto<TestorProductDto>> GetByIdAsync(string itemCode)
            => await _client.PactTestList.GetTestOrProductByIdAsync(itemCode);
    }
}
