using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestReqBreakdownServiceTest
{
    public class TestReqBreakdownServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestRequirementApiClient _apiClient;
        private readonly TestReqBreakdownService _service;

        public TestReqBreakdownServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient = Substitute.For<IPactTestRequirementApiClient>();
            _pactClient.PactTestRequirement.Returns(_apiClient);
            _service = new TestReqBreakdownService(_pactClient);
        }

        #region GetPlannedTestsByWorkgroupAsync

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse(
            [
                new TestReqBreakdownDto { TestCode = "BLOOD", Project = "PRJ001", Pc = "PC01", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m }
            ]);
            _apiClient.GetPlannedTestsByWorkgroupAsync(query).Returns(expected);

            var result = await _service.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPlannedTestsByWorkgroupAsync(query);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([]);
            _apiClient.GetPlannedTestsByWorkgroupAsync(query).Returns(expected);

            var result = await _service.GetPlannedTestsByWorkgroupAsync(query);

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var expected = ApiResponseDto<List<TestReqBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetPlannedTestsByWorkgroupAsync(query).Returns(expected);

            var result = await _service.GetPlannedTestsByWorkgroupAsync(query);

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_AllDtoPropertiesPopulated_ReturnsAllValues()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new TestReqBreakdownDto
            {
                TestCode = "BLOOD",
                ShortDescription = "Blood Test",
                Program = "PROG01",
                Project = "PRJ001",
                Pc = "PC01",
                WorkG = "WG01",
                WgPrice = 15.5m,
                TotalCost = 77.5m
            };
            var expected = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([dto]);
            _apiClient.GetPlannedTestsByWorkgroupAsync(query).Returns(expected);

            var result = await _service.GetPlannedTestsByWorkgroupAsync(query);

            var item = result.Data!.Single();
            Assert.Equal("BLOOD", item.TestCode);
            Assert.Equal("Blood Test", item.ShortDescription);
            Assert.Equal("PROG01", item.Program);
            Assert.Equal("PRJ001", item.Project);
            Assert.Equal("PC01", item.Pc);
            Assert.Equal("WG01", item.WorkG);
            Assert.Equal(15.5m, item.WgPrice);
            Assert.Equal(77.5m, item.TotalCost);
        }

        #endregion
    }
}
