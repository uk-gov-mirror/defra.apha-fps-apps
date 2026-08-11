using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestRequirementServiceTest
{
    public class TestRequirementServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestRequirementApiClient _apiClient;
        private readonly TestRequirementService _service;

        public TestRequirementServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient = Substitute.For<IPactTestRequirementApiClient>();
            _pactClient.PactTestRequirement.Returns(_apiClient);
            _service = new TestRequirementService(_pactClient);
        }

        #region GetPagedTestReqmtAsync

        [Fact]
        public async Task GetPagedTestReqmtAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" }]);
            _apiClient.GetPagedTestReqmtAsync(query, "BLOOD").Returns(expected);

            var result = await _service.GetPagedTestReqmtAsync(query, "BLOOD");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedTestReqmtAsync(query, "BLOOD");
        }

        #endregion

        #region GetPagedTestReqmtbyProjectAsync

        [Fact]
        public async Task GetPagedTestReqmtbyProjectAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" }]);
            _apiClient.GetPagedTestReqmtbyProjectAsync(query, "PRJ1").Returns(expected);

            var result = await _service.GetPagedTestReqmtbyProjectAsync(query, "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedTestReqmtbyProjectAsync(query, "PRJ1");
        }

        [Fact]
        public async Task GetPagedTestReqmtbyProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetPagedTestReqmtbyProjectAsync(query, "MISSING").Returns(expected);

            var result = await _service.GetPagedTestReqmtbyProjectAsync(query, "MISSING");

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestReqmtbyProjectAsync_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]);
            _apiClient.GetPagedTestReqmtbyProjectAsync(query, "PRJ1").Returns(expected);

            var result = await _service.GetPagedTestReqmtbyProjectAsync(query, "PRJ1");

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetAllTestReqmtForExportAsync

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [new TestRequirementDto { TestCode = "BLOOD" }]);
            _apiClient.GetAllTestReqmtForExportAsync("BLOOD", "{}").Returns(expected);

            var result = await _service.GetAllTestReqmtForExportAsync("BLOOD", "{}");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllTestReqmtForExportAsync("BLOOD", "{}");
        }

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WithNullFilter_PassesNullToClient()
        {
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]);
            _apiClient.GetAllTestReqmtForExportAsync("BLOOD", null).Returns(expected);

            var result = await _service.GetAllTestReqmtForExportAsync("BLOOD", null);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllTestReqmtForExportAsync("BLOOD", null);
        }

        #endregion

        #region GetTestReqmtByIdAsync

        [Fact]
        public async Task GetTestReqmtByIdAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" });
            _apiClient.GetTestReqmtByIdAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtByIdAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtByIdAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task GetTestReqmtByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestReqmtByIdAsync("MISSING", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtByIdAsync("MISSING", "PRJ1");

            Assert.False(result.Success);
        }

        #endregion

        #region CreateTestReqmtAsync

        [Fact]
        public async Task CreateTestReqmtAsync_DelegatesToApiClient_ReturnsCreatedDto()
        {
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var expected = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            _apiClient.CreateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.CreateTestReqmtAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).CreateTestReqmtAsync(dto);
        }

        [Fact]
        public async Task CreateTestReqmtAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT" } };
            var expected = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.CreateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.CreateTestReqmtAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestReqmtAsync

        [Fact]
        public async Task UpdateTestReqmtAsync_DelegatesToApiClient_ReturnsUpdatedDto()
        {
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var expected = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            _apiClient.UpdateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.UpdateTestReqmtAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).UpdateTestReqmtAsync(dto);
        }

        #endregion

        #region DeleteTestReqmtAsync

        [Fact]
        public async Task DeleteTestReqmtAsync_DelegatesToApiClient_ReturnsTrue()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _apiClient.DeleteTestReqmtAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.DeleteTestReqmtAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).DeleteTestReqmtAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task DeleteTestReqmtAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.DeleteTestReqmtAsync("MISSING", "PRJ1").Returns(expected);

            var result = await _service.DeleteTestReqmtAsync("MISSING", "PRJ1");

            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedBySupplierTestCodeAsync

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
            [
                new TestSupplierViewDto { TestCode = "BLOOD", Buyer = "PRJ1", TestCost = 30m },
                new TestSupplierViewDto { TestCode = "BLOOD", Buyer = "PRJ2", TestCost = 10m }
            ]);
            _apiClient.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(expected);

            var result = await _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedBySupplierTestCodeAsync(query, "BLOOD", false);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedTrue_PassesFlagToApiClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([]);
            _apiClient.GetPagedBySupplierTestCodeAsync(query, "BLOOD", true).Returns(expected);

            var result = await _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: true);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedBySupplierTestCodeAsync(query, "BLOOD", true);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([]);
            _apiClient.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(expected);

            var result = await _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetPagedBySupplierTestCodeAsync(query, "MISSING", false).Returns(expected);

            var result = await _service.GetPagedBySupplierTestCodeAsync(query, "MISSING", showRejected: false);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_AllDtoPropertiesPopulated_ReturnsAllValues()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new TestSupplierViewDto
            {
                TestCode = "BLOOD",
                Buyer = "PRJ1",
                ProjectManager = "PM001",
                NoRequired = 5.0,
                UnitPrice = 20.0m,
                TestCost = 100.0m,
                ProjectStatus = "Active"
            };
            var expected = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([dto]);
            _apiClient.GetPagedBySupplierTestCodeAsync(query, "BLOOD", false).Returns(expected);

            var result = await _service.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var item = result.Data!.Single();
            Assert.Equal("BLOOD", item.TestCode);
            Assert.Equal("PRJ1", item.Buyer);
            Assert.Equal("PM001", item.ProjectManager);
            Assert.Equal(5.0, item.NoRequired);
            Assert.Equal(20.0m, item.UnitPrice);
            Assert.Equal(100.0m, item.TestCost);
            Assert.Equal("Active", item.ProjectStatus);
        }

        #endregion

        #region GetAllActiveAsync

        [Fact]
        public async Task GetAllActiveAsync_WhenApiReturnsSuccess_DelegatesAndReturnsResponse()
        {
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                [new TestRequirementDto { TestCode = "PT0001", Buyer = "SV3300" }]);
            _apiClient.GetAllActiveAsync().Returns(expected);

            var result = await _service.GetAllActiveAsync();

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllActiveAsync();
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            var expected = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]);
            _apiClient.GetAllActiveAsync().Returns(expected);

            var result = await _service.GetAllActiveAsync();

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllActiveAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var expected = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetAllActiveAsync().Returns(expected);

            var result = await _service.GetAllActiveAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetTestReqmtPricingAsync

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithTestCodeOnly_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 10.5m });
            _apiClient.GetTestReqmtPricingAsync("BLOOD", null).Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("BLOOD");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtPricingAsync("BLOOD", null);
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithProjectCode_PassesProjectCodeToClient()
        {
            var expected = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 5.0m });
            _apiClient.GetTestReqmtPricingAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtPricingAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestReqmtPricingAsync("MISSING", null).Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("MISSING");

            Assert.False(result.Success);
        }

        #endregion
    }
}
