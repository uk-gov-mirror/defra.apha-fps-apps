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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProfitCentreApiClientTest
{
    public class FpsProfitCentreApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProfitCentreApiClient _client;

        public FpsProfitCentreApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProfitCentreApiClient(_http, _mapper);
        }

        private static ProfitCentreRes BuildRes(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new() { Success = false, Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } } };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var resList     = new List<ProfitCentreRes> { BuildRes("PC01"), BuildRes("PC02") };
            var apiResponse = SuccessApiResponse(resList);
            var dtoList     = new List<ProfitCentreDto> { BuildDto("PC01"), BuildDto("PC02") };
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<List<ProfitCentreRes>>();
            var mappedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = SuccessApiResponse(new List<ProfitCentreRes>());
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_UsesCorrectEndpoint()
        {
            // Arrange
            var apiResponse = SuccessApiResponse(new List<ProfitCentreRes>());
            var expectedDto = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentresAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreRes>>(
                Arg.Is<string>(url => url.Contains("profitcentres")));
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsMappedEnumerable()
        {
            // Arrange
            var resList = new List<ProfitCentreRes> { new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" } };
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreRes>> { Success = true, Data = resList };
            var dtoList = new List<ProfitCentreDto> { new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" } };
            var expectedDto = ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(dtoList);

            _http.GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<IEnumerable<ProfitCentreRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<ProfitCentreDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList     = new List<ProfitCentreRes> { BuildRes() };
            var apiResponse = SuccessApiResponse(resList);
            var expected    = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(
                new List<ProfitCentreDto> { BuildDto() },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Is<string>(u => u.Contains("paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<ProfitCentreRes>>();
            var mappedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(BuildDto("PC01"));

            _http.GetAsync<ProfitCentreRes>(Arg.Is<string>(u => u.Contains("PC01"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data!.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<ProfitCentreRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentreByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" };
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.CreateProfitCentreAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = BuildDto();
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01" };
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateProfitCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" };
            var res         = BuildRes("PC01");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Is<string>(u => u.Contains("PC01")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = BuildDto();
            var req         = new ProfitCentreReq { ProfitCentreId = "PC01" };
            var apiResponse = FailureApiResponse<ProfitCentreRes>();
            var mappedResponse = new ApiResponseDto<ProfitCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<ProfitCentreReq>(dto).Returns(req);
            _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProfitCentreDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected    = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains("PC01"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.DeleteProfitCentreAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<bool?>();
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteProfitCentreAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>());
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Update failed", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateProfitCentreSettingsReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPagedProfitCenterCostSummaryAsync Tests

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithoutMonthNumber_ReturnsSuccess()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<ProfitCentreCostRes>
            {
                new() { ProfitCentre = "PC01", Cost = 1000m },
                new() { ProfitCentre = "PC02", Cost = 2000m }
            };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 }
            };
            var dtoList = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1000m },
                new() { ProfitCentre = "PC02", Cost = 2000m }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(dtoList,
                new PaginationDto { TotalRecords = 2, PageNumber = 1, PageSize = 10 });

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result?.Pagination?.TotalRecords);
            Assert.Equal(1, result?.Pagination?.PageNumber);
            Assert.Equal(10, result?.Pagination?.PageSize);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthNumber_AppendsQueryParameter()
        {
            // Arrange
            const double monthNumber = 5.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(new List<ProfitCentreCostDto>());

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<ProfitCentreCostRes>>(
                Arg.Is<string>(url => url.Contains($"monthNumber={monthNumber}")));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProfitCentreCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithNullPagination_UsesQueryParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes> { new() { ProfitCentre = "PC01", Cost = 1000m } },
                Pagination = null
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(
                new List<ProfitCentreCostDto> { new() { ProfitCentre = "PC01", Cost = 1000m } },
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 });

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result?.Pagination?.PageNumber);
            Assert.Equal(5, result?.Pagination?.PageSize);
            Assert.Equal(0, result?.Pagination?.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(new List<ProfitCentreCostDto>(),
                new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 });

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.Equal(0, result?.Pagination?.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMultiplePages_ReturnsPaginatedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 2 };
            var resList = new List<ProfitCentreCostRes>
            {
                new() { ProfitCentre = "PC03", Cost = 3000m },
                new() { ProfitCentre = "PC04", Cost = 4000m }
            };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 2, PageSize = 2, TotalPages = 5, TotalRecords = 10 }
            };
            var dtoList = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC03", Cost = 3000m },
                new() { ProfitCentre = "PC04", Cost = 4000m }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(dtoList,
                new PaginationDto { TotalRecords = 10, PageNumber = 2, PageSize = 2 });

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count());
            Assert.Equal(10, result?.Pagination?.TotalRecords);
            Assert.Equal(2, result?.Pagination?.PageNumber);
            Assert.Equal(2, result?.Pagination?.PageSize);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithNullData_CreatesEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = null,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = new ApiResponseDto<List<ProfitCentreCostDto>>
            {
                Success = true,
                Data = []
            };

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_PassesSortingParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Cost",
                Descending = true
            };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(new List<ProfitCentreCostDto>());

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreCostRes>>(
                Arg.Is<string>(url => url.Contains("SortBy=Cost") && url.Contains("Descending=True")));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthZero_AppendsZeroQueryParameter()
        {
            // Arrange
            const double monthNumber = 0.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(new List<ProfitCentreCostDto>());

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreCostRes>>(
                Arg.Is<string>(url => url.Contains("monthNumber=0")));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMaxMonthNumber_AppendsQueryParameter()
        {
            // Arrange
            const short monthNumber = 12;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreCostRes>>
            {
                Success = true,
                Data = new List<ProfitCentreCostRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(new List<ProfitCentreCostDto>());

            _http.GetAsync<List<ProfitCentreCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            await _http.Received(1).GetAsync<List<ProfitCentreCostRes>>(
                Arg.Is<string>(url => url.Contains("monthNumber=12")));
        }

        #endregion

        #region GetPagedWgStaffPlanAsync Tests

        private static WgStaffPlanViewRes BuildWgStaffPlanRes(string workGroup = "WG001", string name = "Staff One") =>
            new()
            {
                WorkGroup = workGroup,
                GradeCode = "G1",
                Name = name,
                Manager = "Manager01",
                Program = "PROG01",
                JobCode = "JOB001",
                ProjectStatus = "Active",
                PlannedHours = 40.0,
                Fee = 1000m
            };

        private static WgStaffPlanViewDto BuildWgStaffPlanDto(string workGroup = "WG001", string name = "Staff One") =>
            new()
            {
                WorkGroup = workGroup,
                GradeCode = "G1",
                Name = name,
                Manager = "Manager01",
                Program = "PROG01",
                JobCode = "JOB001",
                ProjectStatus = "Active",
                PlannedHours = 40.0,
                Fee = 1000m
            };

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WgStaffPlanViewRes>
            {
                BuildWgStaffPlanRes(workGroup, "Staff One"),
                BuildWgStaffPlanRes(workGroup, "Staff Two")
            };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 }
            };
            var dtoList = new List<WgStaffPlanViewDto>
            {
                BuildWgStaffPlanDto(workGroup, "Staff One"),
                BuildWgStaffPlanDto(workGroup, "Staff Two")
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(dtoList,
                new PaginationDto { TotalRecords = 2, PageNumber = 1, PageSize = 10 });

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result?.Pagination?.TotalRecords);
            await _http.Received(1).GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_AppendsWorkGroupParameter()
        {
            // Arrange
            const string workGroup = "WG-TEST-001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0 }
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _http.Received(1).GetAsync<List<WgStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains($"workGroup={Uri.EscapeDataString(workGroup)}")));
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithSpecialCharactersInWorkGroup_EscapesCorrectly()
        {
            // Arrange
            const string workGroup = "WG&001/TEST";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewRes>()
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _http.Received(1).GetAsync<List<WgStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains("workGroup=")));
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_UsesQueryStringHelper()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                SortBy = "Name",
                Descending = true
            };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewRes>()
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _http.Received(1).GetAsync<List<WgStaffPlanViewRes>>(
                Arg.Is<string>(url => 
                    (url.Contains("page=2") || url.Contains("Page=2")) && 
                    (url.Contains("pageSize=20") || url.Contains("PageSize=20"))));
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WhenExceptionThrown_ReturnsFailureResponse()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>())
                 .Returns<ApiResponse<List<WgStaffPlanViewRes>>>(x => throw new Exception("Network error"));

            // Act
            var result = await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("Failed to retrieve WG staff plan", result.Errors![0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithNullPagination_ReturnsSuccess()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WgStaffPlanViewRes> { BuildWgStaffPlanRes(workGroup) };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = resList,
                Pagination = null
            };
            var dtoList = new List<WgStaffPlanViewDto> { BuildWgStaffPlanDto(workGroup) };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(dtoList,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_VerifiesCorrectEndpointUsed()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WgStaffPlanViewRes>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewRes>()
            };
            var mappedDto = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _http.GetAsync<List<WgStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WgStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _http.Received(1).GetAsync<List<WgStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains("wgstaffplan") || url.Contains("WgStaffPlan")));
        }

        #endregion
    }
}
