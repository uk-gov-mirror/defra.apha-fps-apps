using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectStaffPlanDetailsApiClientTest
{
    public class FpsProjectStaffPlanDetailsApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectStaffPlanDetailsApiClient _client;

        public FpsProjectStaffPlanDetailsApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectStaffPlanDetailsApiClient(_http, _mapper);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        private static List<ProjectStaffPlanDetailsViewRes> BuildResList() =>
        [
            new() { ProfitCentre = "Wildlife", Program = "AH0032", Name = "E_WILDLIFE, General",
                    PlannedHours = 25344, ChargeRate = 53.34m, Cost = 1351848.96m, WorkGroup = "Wildlife", GradeCode = "E" },
            new() { ProfitCentre = "SIU", Program = "ED1044", Name = "C_SVCA, General",
                    PlannedHours = 12000, ChargeRate = 69.92m, Cost = 839040.00m, WorkGroup = "SVCA", GradeCode = "C" }
        ];

        private static ApiResponse<List<ProjectStaffPlanDetailsViewRes>> BuildSuccessApiResponse(
            List<ProjectStaffPlanDetailsViewRes>? data = null,
            int totalRecords = 2) =>
            new()
            {
                Success    = true,
                Data       = data ?? BuildResList(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = totalRecords }
            };

        private static ApiResponse<List<ProjectStaffPlanDetailsViewRes>> BuildFailureApiResponse() =>
            new()
            {
                Success = false,
                Errors  = [new ApiError { Code = "API_ERROR", Message = "API error" }]
            };

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse(
                [
                    new() { ProfitCentre = "Wildlife", Program = "AH0032" },
                    new() { ProfitCentre = "SIU", Program = "ED1044" }
                ],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_CallsHttpGetWithCorrectEndpoint()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectStaffPlanDetailsViewRes>>(
                Arg.Is<string>(url => url.Contains(FpsApiEndpoints.GetPagedProjectStaffPlanDetails)));
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_CallsMapperWithApiResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_UrlContainsPaginationParameters()
        {
            // Arrange
            var query       = DefaultQuery(page: 2, pageSize: 25);
            var apiResponse = BuildSuccessApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectStaffPlanDetailsViewRes>>(
                Arg.Is<string>(url => url.Contains("Page=2") || url.Contains("page=2")));
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildSuccessApiResponse(data: [], totalRecords: 0);
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse([]);

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetPagedAsync — Failure path

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildFailureApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.FailureResponse(
                [new ApiErrorDto { Code = "API_ERROR", Message = "API error" }],
                new ApiMetaDto());

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_DataIsNull()
        {
            // Arrange
            var query       = DefaultQuery();
            var apiResponse = BuildFailureApiResponse();
            var mappedDto   = ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.FailureResponse([], new ApiMetaDto());

            _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        #endregion
    }
}
