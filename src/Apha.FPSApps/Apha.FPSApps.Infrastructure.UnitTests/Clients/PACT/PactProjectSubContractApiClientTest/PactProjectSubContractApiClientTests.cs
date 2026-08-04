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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectSubContractApiClientTest
{
    public class PactProjectSubContractApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectSubContractApiClient _client;

        public PactProjectSubContractApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectSubContractApiClient(_http, _mapper);
        }

        #region GetPagedProjectSubContractsAsync Tests

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var project = "PP001";
            var subContractList = new List<ProjectSubContractRes>
            {
                new() { SubContCounter = 1, Project = project, Amount = 300.00m },
                new() { SubContCounter = 2, Project = project, Amount = 600.00m }
            };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>>
            {
                Success = true,
                Data = subContractList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                new List<ProjectSubContractDto>
                {
                    new() { SubContCounter = 1, Project = project },
                    new() { SubContCounter = 2, Project = project }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithNullProject_ReturnsEmptySuccessWithoutCallingApi()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            await _http.DidNotReceive().GetAsync<List<ProjectSubContractRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectSubContractDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectSubContractsAsync(query, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectSubContractsManualAsync Tests

        [Fact]
        public async Task GetPagedProjectSubContractsManualAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var project = "PP001";
            var subContractList = new List<ProjectSubContractRes>
            {
                new() { SubContCounter = 1, Project = project, Amount = 300.00m },
                new() { SubContCounter = 2, Project = project, Amount = 600.00m }
            };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>>
            {
                Success = true,
                Data = subContractList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse(
                new List<ProjectSubContractDto>
                {
                    new() { SubContCounter = 1, Project = project },
                    new() { SubContCounter = 2, Project = project }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsManualAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract") && url.Contains("project=PP001")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsManualAsync_WithNullProject_CallsApiUsingBaseUrl()
        {
            // Arrange — unlike GetPagedProjectSubContractsAsync, the manual variant always calls the API
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract") && !url.Contains("project=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsManualAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract") && !url.Contains("project=")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsManualAsync_WithWhitespaceProject_CallsApiUsingBaseUrl()
        {
            // Arrange — whitespace project is treated the same as null: no project param in URL
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract") && !url.Contains("project=")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectSubContractsManualAsync(query, "   ");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProjectSubContractRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract") && !url.Contains("project=")));
        }

        [Fact]
        public async Task GetPagedProjectSubContractsManualAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectSubContractRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectSubContractDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectSubContractRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectSubContractsManualAsync(query, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithProject_IncludesProjectInUrl()
        {
            // Arrange
            var project = "PP001";
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 2000.00m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(2000.00m);

            _http.GetAsync<decimal?>(Arg.Is<string>(url =>
                url.Contains("api/v1/projectsubcontract/total") && url.Contains("project=PP001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAmountAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2000.00m, result.Data);
            await _http.Received(1).GetAsync<decimal?>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/total") && url.Contains("project=PP001")));
        }

        [Fact]
        public async Task GetTotalAmountAsync_WithNullProject_ReturnsZeroSuccessWithoutCallingApi()
        {
            // Arrange — no HTTP setup needed; the implementation short-circuits for null/whitespace project

            // Act
            var result = await _client.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data);
            await _http.DidNotReceive().GetAsync<decimal?>(Arg.Any<string>());
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedSubContract()
        {
            // Arrange
            var subContCounter = 1;
            var subContractRes = new ProjectSubContractRes { SubContCounter = subContCounter, Project = "PP001", Amount = 400.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(
                new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001" }
            );

            _http.GetAsync<ProjectSubContractRes>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(subContCounter, result.Data?.SubContCounter);
            await _http.Received(1).GetAsync<ProjectSubContractRes>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}");
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectSubContractRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidSubContract_ReturnsMappedCreatedSubContract()
        {
            // Arrange
            var subContractDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PP001", Supplier = "Supplier A", Amount = 500.00m };
            var subContractReq = new ProjectSubContractReq { Project = "PP001", Supplier = "Supplier A", Amount = 500.00m };
            var subContractRes = new ProjectSubContractRes { SubContCounter = 1, Project = "PP001", Amount = 500.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(subContractDto);

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>("api/v1/projectsubcontract", subContractReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.SubContCounter);
            await _http.Received(1).PostAsync<ProjectSubContractReq, ProjectSubContractRes>("api/v1/projectsubcontract", subContractReq);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var subContractDto = new ProjectSubContractDto { Project = "PP001" };
            var subContractReq = new ProjectSubContractReq { Project = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>(Arg.Any<string>(), Arg.Any<ProjectSubContractReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidSubContract_ReturnsMappedUpdatedSubContract()
        {
            // Arrange
            var subContCounter = 1;
            var subContractDto = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001", Amount = 800.00m };
            var subContractReq = new ProjectSubContractReq { Project = "PP001", Amount = 800.00m };
            var subContractRes = new ProjectSubContractRes { SubContCounter = subContCounter, Project = "PP001", Amount = 800.00m };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = true, Data = subContractRes };
            var expectedDto = ApiResponseDto<ProjectSubContractDto>.SuccessResponse(subContractDto);

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}", Arg.Any<ProjectSubContractReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(subContCounter, subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(800.00m, result.Data?.Amount);
            await _http.Received(1).PutAsync<ProjectSubContractReq, ProjectSubContractRes>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}", Arg.Any<ProjectSubContractReq>());
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var subContCounter = 9999;
            var subContractDto = new ProjectSubContractDto { SubContCounter = subContCounter, Project = "PP001" };
            var subContractReq = new ProjectSubContractReq { Project = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectSubContractRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectSubContractDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectSubContractReq>(subContractDto).Returns(subContractReq);
            _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>(Arg.Any<string>(), Arg.Any<ProjectSubContractReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync(subContCounter, subContractDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetMonthlySubContractsSummaryAsync Tests

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsSuccess_ReturnsMappedDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlySubContractsPivotRes
            {
                Months = [1, 2, 3],
                Rows = [new MonthlySubContractsSummaryItemRes { Program = "ADMIN", ParentProject = "AH" }],
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new MonthlySubContractsPivotDto
            {
                Months = [1, 2, 3],
                Rows = [new MonthlySubContractsSummaryItemDto { Program = "ADMIN", ParentProject = "AH" }],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(
                    Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")))
                .Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(pivotRes).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Months.Count);
            Assert.Single(result.Data.Rows);
            await _http.Received(1).GetAsync<MonthlySubContractsPivotRes>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")));
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WithQueryParameters_IncludesQueryStringInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "program", Descending = true };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = new MonthlySubContractsPivotRes() };
            var expectedDto = new MonthlySubContractsPivotDto();

            _http.GetAsync<MonthlySubContractsPivotRes>(
                    Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")))
                .Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(Arg.Any<MonthlySubContractsPivotRes>()).Returns(expectedDto);

            // Act
            await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            await _http.Received(1).GetAsync<MonthlySubContractsPivotRes>(
                Arg.Is<string>(url => url.Contains("api/v1/projectsubcontract/monthly-summary")));
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsSuccess_MapsResponseData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotRes = new MonthlySubContractsPivotRes();
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = true, Data = pivotRes };
            var expectedDto = new MonthlySubContractsPivotDto();

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<MonthlySubContractsPivotDto>(pivotRes).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<MonthlySubContractsPivotDto>(pivotRes);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<MonthlySubContractsPivotDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlySubContractsPivotDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhenApiReturnsFailure_DoesNotMapResponseData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<MonthlySubContractsPivotRes> { Success = false, Errors = [] };
            var mappedFailure = new ApiResponseDto<MonthlySubContractsPivotDto>
            {
                Success = false,
                Errors = [],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlySubContractsPivotRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlySubContractsPivotDto>>(apiResponse).Returns(mappedFailure);

            // Act
            await _client.GetMonthlySubContractsSummaryAsync(query);

            // Assert
            _mapper.DidNotReceive().Map<MonthlySubContractsPivotDto>(Arg.Any<MonthlySubContractsPivotRes>());
        }

        #endregion

        #region Failed SubContract RMS Tests

        [Fact]
        public async Task GetFailedSubContractRmsAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<SubContractRmsImportRowRes>>
            {
                Success = true,
                Data = [new SubContractRmsImportRowRes { Id = 1, Project = "P1" }]
            };
            var expectedDto = ApiResponseDto<List<SubContractRmsImportRowDto>>.SuccessResponse(
                [new SubContractRmsImportRowDto { Id = 1, Project = "P1" }]);

            _http.GetAsync<List<SubContractRmsImportRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SubContractRmsImportRowDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetFailedSubContractRmsAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<SubContractRmsImportRowRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<SubContractRmsImportRowDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetFailedSubContractRmsAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<SubContractRmsImportRowRes>>
            {
                Success = false,
                Errors = [new ApiError { Code = "API_ERROR", Message = "Failed" }]
            };
            var mappedFailure = new ApiResponseDto<List<SubContractRmsImportRowDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "API_ERROR", Message = "Failed" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<SubContractRmsImportRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SubContractRmsImportRowDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetFailedSubContractRmsAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetFailedSubContractRmsByIdAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var id = 42;
            var apiRow = new SubContractRmsImportRowRes { Id = id, Project = "P42" };
            var mappedRow = new SubContractRmsImportRowDto { Id = id, Project = "P42" };
            var apiResponse = new ApiResponse<SubContractRmsImportRowRes> { Success = true, Data = apiRow };

            _http.GetAsync<SubContractRmsImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<SubContractRmsImportRowDto>(apiRow).Returns(mappedRow);

            // Act
            var result = await _client.GetFailedSubContractRmsByIdAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(id, result.Data!.Id);
            _mapper.Received(1).Map<SubContractRmsImportRowDto>(apiRow);
            await _http.Received(1).GetAsync<SubContractRmsImportRowRes>(Arg.Is<string>(u => u.Contains(id.ToString())));
        }

        [Fact]
        public async Task GetFailedSubContractRmsByIdAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<SubContractRmsImportRowRes>
            {
                Success = false,
                Errors = [new ApiError { Code = "NOT_FOUND", Message = "Not found" }]
            };
            var mappedFailure = new ApiResponseDto<SubContractRmsImportRowDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "NOT_FOUND", Message = "Not found" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<SubContractRmsImportRowRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SubContractRmsImportRowDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetFailedSubContractRmsByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task SaveFailedSubContractRmsAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var id = 7;
            var dto = new SubContractRmsImportRowDto { Id = id, Project = "P7", Month = "1" };
            var req = new SubContractRmsImportRowReq { Project = "P7", Month = "1" };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mappedSuccess = ApiResponseDto<bool>.SuccessResponse(true);

            _mapper.Map<SubContractRmsImportRowReq>(dto).Returns(req);
            _http.PutAsync<SubContractRmsImportRowReq, bool?>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedSuccess);

            // Act
            var result = await _client.SaveFailedSubContractRmsAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            _mapper.Received(1).Map<SubContractRmsImportRowReq>(dto);
            await _http.Received(1).PutAsync<SubContractRmsImportRowReq, bool?>(Arg.Is<string>(u => u.Contains(id.ToString())), req);
        }

        [Fact]
        public async Task SaveFailedSubContractRmsAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SubContractRmsImportRowDto { Id = 1, Project = "P1" };
            var req = new SubContractRmsImportRowReq { Project = "P1" };
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = [new ApiError { Code = "VALIDATION", Message = "Invalid" }]
            };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "VALIDATION", Message = "Invalid" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<SubContractRmsImportRowReq>(dto).Returns(req);
            _http.PutAsync<SubContractRmsImportRowReq, bool?>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.SaveFailedSubContractRmsAsync(1, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByIdAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var id = 12;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mappedSuccess = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedSuccess);

            // Act
            var result = await _client.DeleteFailedSubContractRmsByIdAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains(id.ToString())));
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByIdAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = [new ApiError { Code = "NOT_FOUND", Message = "Not found" }]
            };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "NOT_FOUND", Message = "Not found" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.DeleteFailedSubContractRmsByIdAsync(999);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task ImportSubContractRmsAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var requestDto = new SubContractRmsImportReqDto
            {
                FileName = "subcontract.xlsx",
                Rows = [new SubContractRmsImportRowDto { Project = "P10" }]
            };
            var request = new SubContractRmsImportReq
            {
                FileName = "subcontract.xlsx",
                Rows = [new SubContractRmsImportRowReq { Project = "P10" }]
            };
            var responseData = new SubContractRmsImportRes { PassedCount = 1, FailedCount = 0, Message = "Done" };
            var apiResponse = new ApiResponse<SubContractRmsImportRes> { Success = true, Data = responseData };
            var mappedData = new SubContractRmsImportResultDto { PassedCount = 1, FailedCount = 0, Message = "Done" };

            _mapper.Map<SubContractRmsImportReq>(requestDto).Returns(request);
            _http.PostAsync<SubContractRmsImportReq, SubContractRmsImportRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<SubContractRmsImportResultDto>(responseData).Returns(mappedData);

            // Act
            var result = await _client.ImportSubContractRmsAsync(requestDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
            _mapper.Received(1).Map<SubContractRmsImportReq>(requestDto);
            _mapper.Received(1).Map<SubContractRmsImportResultDto>(responseData);
            await _http.Received(1).PostAsync<SubContractRmsImportReq, SubContractRmsImportRes>(Arg.Any<string>(), request);
        }

        [Fact]
        public async Task ImportSubContractRmsAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var requestDto = new SubContractRmsImportReqDto { FileName = "subcontract.xlsx" };
            var request = new SubContractRmsImportReq { FileName = "subcontract.xlsx" };
            var apiResponse = new ApiResponse<SubContractRmsImportRes>
            {
                Success = false,
                Errors = [new ApiError { Code = "IMPORT_FAILED", Message = "Failed" }]
            };
            var mappedFailure = new ApiResponseDto<SubContractRmsImportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "IMPORT_FAILED", Message = "Failed" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<SubContractRmsImportReq>(requestDto).Returns(request);
            _http.PostAsync<SubContractRmsImportReq, SubContractRmsImportRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SubContractRmsImportResultDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.ImportSubContractRmsAsync(requestDto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByUserAsync_ApiReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var mappedSuccess = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedSuccess);

            // Act
            var result = await _client.DeleteFailedSubContractRmsByUserAsync();

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByUserAsync_ApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = [new ApiError { Code = "API_ERROR", Message = "Failed" }]
            };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Code = "API_ERROR", Message = "Failed" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.DeleteFailedSubContractRmsByUserAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var subContCounter = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(subContCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/projectsubcontract/subcontract/id?id={subContCounter}");
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAsync(9999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
