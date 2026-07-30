using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestRequirementApiClientTest
{
    public class PactTestRequirementApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTestRequirementApiClient _client;

        public PactTestRequirementApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTestRequirementApiClient(_http, _mapper);
        }

        // ── GetPagedTestReqmtAsync ────────────────────────────────────────────

        #region GetPagedTestReqmtAsync Tests

        [Fact]
        public async Task GetPagedTestReqmtAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestRequirementtRes>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ001" }
            };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto> { new() { TestCode = "BLOOD", Buyer = "PRJ001" } });

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Is<string>(url => url.Contains("BLOOD")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTestReqmtAsync(query, "BLOOD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestReqmtAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTestReqmtAsync(query, "BLOOD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestReqmtAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestRequirementDto>>
            {
                Success = false,
                Errors = [new() { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedTestReqmtAsync(query, "BLOOD");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── GetPagedTestReqmtbyProjectAsync ───────────────────────────────────

        #region GetPagedTestReqmtbyProjectAsync Tests

        [Fact]
        public async Task GetPagedTestReqmtbyProjectAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestRequirementtRes>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ001" },
                new() { TestCode = "URINE", Buyer = "PRJ001" }
            };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto>
                {
                    new() { TestCode = "BLOOD", Buyer = "PRJ001" },
                    new() { TestCode = "URINE", Buyer = "PRJ001" }
                });

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Is<string>(url => url.Contains("PRJ001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTestReqmtbyProjectAsync(query, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
        }

        [Fact]
        public async Task GetPagedTestReqmtbyProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestRequirementDto>>
            {
                Success = false,
                Errors = [new() { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedTestReqmtbyProjectAsync(query, "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        // ── GetAllTestReqmtForExportAsync ─────────────────────────────────────

        #region GetAllTestReqmtForExportAsync Tests

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var responseItems = new List<TestRequirementtRes>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ001" },
                new() { TestCode = "BLOOD", Buyer = "PRJ002" }
            };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto>
                {
                    new() { TestCode = "BLOOD", Buyer = "PRJ001" },
                    new() { TestCode = "BLOOD", Buyer = "PRJ002" }
                });

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllTestReqmtForExportAsync("BLOOD", null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
        }

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WithFilter_AppendsFilterToUrl()
        {
            // Arrange
            const string filter = "{\"Buyer\":\"PRJ\"}";
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Is<string>(url => url.Contains("filter=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllTestReqmtForExportAsync("BLOOD", filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestRequirementtRes>>(
                Arg.Is<string>(url => url.Contains("filter=")));
        }

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<TestRequirementtRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestRequirementDto>>
            {
                Success = false,
                Errors = [new() { Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestRequirementtRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllTestReqmtForExportAsync("BLOOD", null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestReqmtByIdAsync ─────────────────────────────────────────────

        #region GetTestReqmtByIdAsync Tests

        [Fact]
        public async Task GetTestReqmtByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var responseItem = new TestRequirementtRes { TestCode = "BLOOD", Buyer = "PRJ001" };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = true, Data = responseItem };
            var expectedDto = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" });

            _http.GetAsync<TestRequirementtRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTestReqmtByIdAsync("BLOOD", "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("BLOOD", result.Data!.TestCode);
        }

        [Fact]
        public async Task GetTestReqmtByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = [new() { Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<TestRequirementtRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTestReqmtByIdAsync("MISSING", "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── CreateTestReqmtAsync ──────────────────────────────────────────────

        #region CreateTestReqmtAsync Tests

        [Fact]
        public async Task CreateTestReqmtAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ001" };
            var responseItem = new TestRequirementtRes { TestCode = "BLOOD", Buyer = "PRJ001" };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = true, Data = responseItem };
            var expectedDto = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementReq>(dto).Returns(request);
            _http.PostAsync<TestRequirementReq, TestRequirementtRes>(Arg.Any<string>(), request)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateTestReqmtAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("BLOOD", result.Data!.TestCode);
        }

        [Fact]
        public async Task CreateTestReqmtAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ001" };
            var errors = new List<ApiError> { new() { Message = "Conflict", Code = "CONFLICT" } };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = [new() { Code = "CONFLICT" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TestRequirementReq>(dto).Returns(request);
            _http.PostAsync<TestRequirementReq, TestRequirementtRes>(Arg.Any<string>(), request)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateTestReqmtAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── UpdateTestReqmtAsync ──────────────────────────────────────────────

        #region UpdateTestReqmtAsync Tests

        [Fact]
        public async Task UpdateTestReqmtAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ001" };
            var responseItem = new TestRequirementtRes { TestCode = "BLOOD", Buyer = "PRJ001" };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = true, Data = responseItem };
            var expectedDto = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementReq>(dto).Returns(request);
            _http.PutAsync<TestRequirementReq, TestRequirementtRes>(Arg.Any<string>(), request)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateTestReqmtAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var request = new TestRequirementReq { TestCode = "BLOOD", Buyer = "PRJ001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = [new() { Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TestRequirementReq>(dto).Returns(request);
            _http.PutAsync<TestRequirementReq, TestRequirementtRes>(Arg.Any<string>(), request)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateTestReqmtAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── DeleteTestReqmtAsync ──────────────────────────────────────────────

        #region DeleteTestReqmtAsync Tests

        [Fact]
        public async Task DeleteTestReqmtAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteTestReqmtAsync("BLOOD", "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteTestReqmtAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new() { Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteTestReqmtAsync("MISSING", "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        // ── GetTestReqmtPricingAsync ──────────────────────────────────────────

        #region GetTestReqmtPricingAsync Tests

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithTestCodeOnly_ReturnsRecUnitPrice()
        {
            // Arrange
            var responseItem = new TestRequirementtRes { TestCode = "BLOOD", RecUnitPrice = 10.5m };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = true, Data = responseItem };
            var expectedDto = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 10.5m });

            _http.GetAsync<TestRequirementtRes>(Arg.Is<string>(url =>
                url.Contains("testCode=BLOOD") && !url.Contains("projectCode")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTestReqmtPricingAsync("BLOOD");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(10.5m, result.Data!.RecUnitPrice);
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithProjectCode_IncludesProjectCodeInUrl()
        {
            // Arrange
            var responseItem = new TestRequirementtRes { TestCode = "BLOOD", RecUnitPrice = 5.0m, IsDefraProject = 1 };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = true, Data = responseItem };
            var expectedDto = ApiResponseDto<TestRequirementDto>.SuccessResponse(
                new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 5.0m, IsDefraProject = 1 });

            _http.GetAsync<TestRequirementtRes>(Arg.Is<string>(url =>
                url.Contains("testCode=BLOOD") && url.Contains("projectCode=PRJ001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTestReqmtPricingAsync("BLOOD", "PRJ001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(5.0m, result.Data!.RecUnitPrice);
            await _http.Received(1).GetAsync<TestRequirementtRes>(
                Arg.Is<string>(url => url.Contains("projectCode=PRJ001")));
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TestRequirementtRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = [new() { Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<TestRequirementtRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTestReqmtPricingAsync("MISSING");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedBySupplierTestCodeAsync Tests

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestSupplierViewRes>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ001", UnitPrice = 10m, NoRequired = 3, TestCost = 30m, ProjectStatus = "Active" },
                new() { TestCode = "BLOOD", Buyer = "PRJ002", UnitPrice = 5m,  NoRequired = 2, TestCost = 10m, ProjectStatus = "Active" }
            };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                new List<TestSupplierViewDto>
                {
                    new() { TestCode = "BLOOD", Buyer = "PRJ001", TestCost = 30m },
                    new() { TestCode = "BLOOD", Buyer = "PRJ002", TestCost = 10m }
                });

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Is<string>(url =>
                url.Contains("BLOOD") && url.Contains("showRejected=False")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedTrue_IncludesFlagInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Is<string>(url =>
                url.Contains("showRejected=True")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: true);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestSupplierViewRes>>(
                Arg.Is<string>(url => url.Contains("showRejected=True")));
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestSupplierViewDto>>
            {
                Success = false,
                Errors = [new() { Message = "Not Found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCodeIsEscaped_UrlContainsEscapedCode()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string testCodeWithSpace = "BLOOD TEST";
            var apiResponse = new ApiResponse<List<TestSupplierViewRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestSupplierViewRes>>(Arg.Is<string>(url =>
                url.Contains("BLOOD%20TEST")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedBySupplierTestCodeAsync(query, testCodeWithSpace, showRejected: false);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<TestSupplierViewRes>>(
                Arg.Is<string>(url => url.Contains("BLOOD%20TEST")));
        }

        #endregion

        // ── GetPlannedTestsByWorkgroupAsync ───────────────────────────────────

        #region GetPlannedTestsByWorkgroupAsync Tests

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestReqBreakdownRes>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", Pc = "PC01", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m }
            };
            var apiResponse = new ApiResponse<List<TestReqBreakdownRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse(
                new List<TestReqBreakdownDto>
                {
                    new() { TestCode = "BLOOD", Project = "PRJ001", Pc = "PC01", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m }
                });

            _http.GetAsync<List<TestReqBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPlannedTestsByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("BLOOD", result.Data![0].TestCode);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestReqBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestReqBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPlannedTestsByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestReqBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestReqBreakdownRes>>(Arg.Is<string>(url =>
                url.Contains("testrequirement/testreqbreakdown")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPlannedTestsByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<TestReqBreakdownRes>>(
                Arg.Is<string>(url => url.Contains("testrequirement/testreqbreakdown")));
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<TestReqBreakdownRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestReqBreakdownDto>>
            {
                Success = false,
                Errors = [new() { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TestReqBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestReqBreakdownDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPlannedTestsByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
