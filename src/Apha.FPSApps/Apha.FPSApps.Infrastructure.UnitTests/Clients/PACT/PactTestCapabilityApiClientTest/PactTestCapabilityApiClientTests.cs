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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestCapabilityApiClientTest
{
    public class PactTestCapabilityApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTestCapabilityApiClient _client;

        public PactTestCapabilityApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTestCapabilityApiClient(_http, _mapper);
        }

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithValidParams_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [new TestCapabilityRes { TestCode = "TC1", WorkGroup = "WG1" }] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/workgroup") && url.Contains("WG1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/workgroup")));
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<List<TestCapabilityDto>> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithValidParams_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [new TestCapabilityRes { TestCode = "TC1" }] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/testcode") && url.Contains("TC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedByTestCodeAsync(query, "TC1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/testcode")));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<List<TestCapabilityDto>> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetPagedByTestCodeAsync(query, "TC1");

            Assert.False(result.Success);
        }

        #endregion

        #region GetTestCapabilityByIdAsync

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WithValidKeys_ReturnsMappedDto()
        {
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1", WorkGroup = "WG1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _http.GetAsync<TestCapabilityRes>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/testcapability?testCode=") &&
                url.Contains(Uri.EscapeDataString("TC1")) && url.Contains(Uri.EscapeDataString("WG1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "NOT_FOUND" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.GetAsync<TestCapabilityRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.False(result.Success);
        }

        #endregion

        #region CreateTestCapabilityAsync

        [Fact]
        public async Task CreateTestCapabilityAsync_WithValidDto_PostsAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1" };
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.CreateTestCapabilityAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req);
        }

        [Fact]
        public async Task CreateTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq();
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "CONFLICT" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "CONFLICT" }], Meta = new ApiMetaDto() };

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PostAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.CreateTestCapabilityAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestCapabilityAsync

        [Fact]
        public async Task UpdateTestCapabilityAsync_WithValidDto_PutsAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1" };
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = true, Data = new TestCapabilityRes { TestCode = "TC1" } };
            var expectedDto = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1" });

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(
                Arg.Is<string>(url => url.Contains("api/v1/testcapability/testcapability")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateTestCapabilityAsync(dto);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var req = new TestCapabilityReq();
            var apiResponse = new ApiResponse<TestCapabilityRes> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<TestCapabilityDto> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _mapper.Map<TestCapabilityReq>(dto).Returns(req);
            _http.PutAsync<TestCapabilityReq, TestCapabilityRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestCapabilityDto>>(apiResponse).Returns(mappedDto);

            var result = await _client.UpdateTestCapabilityAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteTestCapabilityAsync

        [Fact]
        public async Task DeleteTestCapabilityAsync_WithValidKeys_DeletesAndReturnsTrue()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/testcapability?testCode=") &&
                url.Contains(Uri.EscapeDataString("TC1")) && url.Contains(Uri.EscapeDataString("WG1"))))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Code = "NOT_FOUND" }] };
            var mappedDto = new ApiResponseDto<bool> { Success = false, Errors = [new ApiErrorDto { Code = "NOT_FOUND" }], Meta = new ApiMetaDto() };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            var result = await _client.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.False(result.Success);
        }

        #endregion                     

        #region GetPagedTestCapabilityByPortfolioAsync

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithValidParams_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [new TestCapabilityRes { TestCode = "TC1", WorkGroup = "WG1" }] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/portfolio") && url.Contains("PP1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/portfolio")));
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WithNullPortfolio_OmitsPortfolioQueryParam()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/portfolio") && !url.Contains("portfolio=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedTestCapabilityByPortfolioAsync(query, null);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<TestCapabilityRes>> { Success = false, Errors = [new ApiError { Code = "ERR" }] };
            var mappedDto = new ApiResponseDto<List<TestCapabilityDto>> { Success = false, Errors = [new ApiErrorDto { Code = "ERR" }], Meta = new ApiMetaDto() };

            _http.GetAsync<List<TestCapabilityRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestCapabilityDto>>>(apiResponse).Returns(mappedDto);

            var result = await _client.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedWgTestCapabilitiesWithDescriptionAsync

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_WithValidParamsAndWorkGroup_ReturnsMappedList()
        {
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "TestCode",
                Descending = true,
                Filter = "{\"TestCode\":\"TC\"}"
            };

            var apiResponse = new ApiResponse<List<WgTestCapabilitiesWithDescriptionRes>>
            {
                Success = true,
                Data =
                [
                    new WgTestCapabilitiesWithDescriptionRes { WorkGroup = "WG1", TestCode = "TC001", ItemDescription = "Item 1" },
                    new WgTestCapabilitiesWithDescriptionRes { WorkGroup = "WG1", TestCode = "TC002", ItemDescription = "Item 2" }
                ],
                Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 12 }
            };

            var expectedDto = ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>.SuccessResponse(
                [
                    new WgTestCapabilitiesWithDescriptionDto { WorkGroup = "WG1", TestCode = "TC001", ItemDescription = "Item 1" },
                    new WgTestCapabilitiesWithDescriptionDto { WorkGroup = "WG1", TestCode = "TC002", ItemDescription = "Item 2" }
                ],
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 12 });

            _http.GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/wg-test-capabilities")
                && url.Contains("workGroup=WG1")
                && url.Contains("Page=2")
                && url.Contains("PageSize=5")))
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
            Assert.Equal("TC001", result.Data.First().TestCode);
            Assert.Equal("Item 1", result.Data.First().ItemDescription);
            Assert.Equal(2, result.Pagination!.PageNumber);
            Assert.Equal(5, result.Pagination.PageSize);
            Assert.Equal(3, result.Pagination.TotalPages);
            Assert.Equal(12, result.Pagination.TotalRecords);

            await _http.Received(1).GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/wg-test-capabilities")));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_WithNullOrWhitespaceWorkGroup_OmitsWorkGroupQueryParam(string? workGroup)
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgTestCapabilitiesWithDescriptionRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>.SuccessResponse([]);

            _http.GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/wg-test-capabilities")
                && !url.Contains("workGroup=")))
                .Returns(apiResponse);

            _mapper.Map<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, workGroup!);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/testcapability/paged/wg-test-capabilities")
                && !url.Contains("workGroup=")));
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgTestCapabilitiesWithDescriptionRes>>
            {
                Success = false,
                Errors = [new ApiError { Code = "ERR", Message = "Failure" }],
                Meta = new ApiMeta { CorrelationId = "corr-1" }
            };

            var mappedFailure = new ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "ERR", Message = "Failure" }],
                Meta = new ApiMetaDto { CorrelationId = "corr-1" }
            };

            _http.GetAsync<List<WgTestCapabilitiesWithDescriptionRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgTestCapabilitiesWithDescriptionDto>>>(apiResponse).Returns(mappedFailure);

            var result = await _client.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("ERR", result.Errors!.First().Code);
            Assert.Equal("corr-1", result.Meta.CorrelationId);
        }

        #endregion
    }
}
