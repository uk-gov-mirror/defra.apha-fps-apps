using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsCostCentreApiClientTest
{
    public class FpsCostCentreApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsCostCentreApiClient _client;

        public FpsCostCentreApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsCostCentreApiClient(_http, _mapper);
        }

        private static ApiResponse<List<CostCentreWorkgroupRes>> BuildWorkgroupResponse(bool success = true) =>
            new()
            {
                Success = success,
                Data    = success ? new List<CostCentreWorkgroupRes> { new() { CostCentre = 100, ProfitCentre = "PC01" } } : null
            };

        private static ApiResponse<List<CostCentreRes>> BuildPagedResponse(bool success = true) =>
            new()
            {
                Success    = success,
                Data       = success ? new List<CostCentreRes> { new() { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 } } : null,
                Pagination = success ? new Apha.Common.Contracts.Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 } : null
            };

        private static ApiResponse<CostCentreRes> BuildSingleResponse(bool success = true) =>
            new()
            {
                Success = success,
                Data    = success ? new CostCentreRes { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 } : null
            };

        private static ApiResponse<bool?> BuildDeleteResponse(bool success = true) =>
            new()
            {
                Success = success,
                Data    = success ? true : null
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsCostCentreApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsCostCentreApiClient(_http, null!));
        }

        #endregion

        #region GetAllCostCentresAsync Tests

        [Fact]
        public async Task GetAllCostCentresAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = BuildWorkgroupResponse(true);
            var expectedDto  = ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(
                new List<CostCentreWorkgroupDto> { new() { ProfitCentre = "PC01" } });

            _http.GetAsync<List<CostCentreWorkgroupRes>>(Arg.Is<string>(url =>
                    url.Contains("api/v1/costcentre")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllCostCentresAsync();

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = BuildWorkgroupResponse(false);
            var mappedDto    = new ApiResponseDto<List<CostCentreWorkgroupDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Not found" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<CostCentreWorkgroupRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllCostCentresAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            _http.GetAsync<List<CostCentreWorkgroupRes>>(Arg.Any<string>())
                .Throws(new Exception("Network error"));

            // Act
            var result = await _client.GetAllCostCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetAllCostCentresPagedAsync Tests

        [Fact]
        public async Task GetAllCostCentresPagedAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = BuildPagedResponse(true);
            var expectedDto  = ApiResponseDto<List<CostCentreDto>>.SuccessResponse(
                new List<CostCentreDto> { new() { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 } });

            _http.GetAsync<List<CostCentreRes>>(Arg.Is<string>(url =>
                    url.Contains("api/v1/costcentre/paged")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            _mapper.Received(1).Map<ApiResponseDto<List<CostCentreDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = BuildPagedResponse(false);
            var mappedDto    = new ApiResponseDto<List<CostCentreDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<CostCentreRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreDto>>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<CostCentreRes>>(Arg.Any<string>())
                .Throws(new Exception("Timeout"));

            // Act
            var result = await _client.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetCostCentreByIdAsync Tests

        [Fact]
        public async Task GetCostCentreByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = BuildSingleResponse(true);
            var expectedDto  = ApiResponseDto<CostCentreDto>.SuccessResponse(
                new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 });

            _http.GetAsync<CostCentreRes>(Arg.Is<string>(url =>
                    url.Contains("api/v1/costcentre/") && url.Contains("100")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCostCentreByIdAsync(100.0);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(100.0, result.Data!.CostCentreNo);
            _mapper.Received(1).Map<ApiResponseDto<CostCentreDto>>(httpResponse);
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = BuildSingleResponse(false);
            var mappedDto    = new ApiResponseDto<CostCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<CostCentreRes>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetCostCentreByIdAsync(999.0);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            _http.GetAsync<CostCentreRes>(Arg.Any<string>())
                .Throws(new Exception("Connection refused"));

            // Act
            var result = await _client.GetCostCentreByIdAsync(100.0);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_UsesInvariantCultureFormatting_InUrl()
        {
            // Arrange — double with decimal part to verify culture-invariant formatting
            const double costCentreNo = 100.5;
            var httpResponse = BuildSingleResponse(true);
            var expectedDto  = ApiResponseDto<CostCentreDto>.SuccessResponse(new CostCentreDto());

            _http.GetAsync<CostCentreRes>(Arg.Is<string>(url =>
                    url.Contains("100.5")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetCostCentreByIdAsync(costCentreNo);

            // Assert — verifies the invariant-culture URL was called
            await _http.Received(1).GetAsync<CostCentreRes>(
                Arg.Is<string>(url => url.Contains("100.5")));
        }

        #endregion

        #region CreateCostCentreAsync Tests

        [Fact]
        public async Task CreateCostCentreAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto          = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 };
            var req          = new CostCentreReq { CostCentreNo = 100.0, ProfitCentre = "PC01" };
            var httpResponse = BuildSingleResponse(true);
            var expectedDto  = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PostAsync<CostCentreReq, CostCentreRes>(
                Arg.Is<string>(url => url.Contains("api/v1/costcentre")), req)
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateCostCentreAsync(dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<CostCentreReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<CostCentreDto>>(httpResponse);
        }

        [Fact]
        public async Task CreateCostCentreAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto  = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 };
            var req  = new CostCentreReq { CostCentreNo = 100.0, ProfitCentre = "PC01" };
            var httpResponse = new ApiResponse<CostCentreRes> { Success = false };
            var mappedDto    = new ApiResponseDto<CostCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "CONFLICT" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PostAsync<CostCentreReq, CostCentreRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateCostCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateCostCentreAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 };
            var req = new CostCentreReq { CostCentreNo = 100.0, ProfitCentre = "PC01" };

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PostAsync<CostCentreReq, CostCentreRes>(Arg.Any<string>(), Arg.Any<CostCentreReq>())
                .Throws(new Exception("POST failed"));

            // Act
            var result = await _client.CreateCostCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateCostCentreAsync Tests

        [Fact]
        public async Task UpdateCostCentreAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const double costCentreNo = 100.0;
            var dto          = new CostCentreDto { CostCentreNo = costCentreNo, ProfitCentre = "PC02", FpsYear = 2024 };
            var req          = new CostCentreReq { CostCentreNo = costCentreNo, ProfitCentre = "PC02" };
            var httpResponse = BuildSingleResponse(true);
            var expectedDto  = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PutAsync<CostCentreReq, CostCentreRes>(
                Arg.Is<string>(url => url.Contains("100")), req)
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateCostCentreAsync(costCentreNo, dto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<CostCentreReq>(dto);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto  = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC02", FpsYear = 2024 };
            var req  = new CostCentreReq { CostCentreNo = 100.0, ProfitCentre = "PC02" };
            var httpResponse = new ApiResponse<CostCentreRes> { Success = false };
            var mappedDto    = new ApiResponseDto<CostCentreDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PutAsync<CostCentreReq, CostCentreRes>(Arg.Any<string>(), req).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateCostCentreAsync(100.0, dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC02", FpsYear = 2024 };
            var req = new CostCentreReq { CostCentreNo = 100.0, ProfitCentre = "PC02" };

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PutAsync<CostCentreReq, CostCentreRes>(Arg.Any<string>(), Arg.Any<CostCentreReq>())
                .Throws(new Exception("PUT failed"));

            // Act
            var result = await _client.UpdateCostCentreAsync(100.0, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task UpdateCostCentreAsync_UsesInvariantCultureFormatting_InUrl()
        {
            // Arrange — double with decimal part to verify culture-invariant formatting
            const double costCentreNo = 100.5;
            var dto = new CostCentreDto { CostCentreNo = costCentreNo, ProfitCentre = "PC01" };
            var req = new CostCentreReq { CostCentreNo = costCentreNo, ProfitCentre = "PC01" };
            var httpResponse = BuildSingleResponse(true);
            var expectedDto  = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mapper.Map<CostCentreReq>(dto).Returns(req);
            _http.PutAsync<CostCentreReq, CostCentreRes>(
                Arg.Is<string>(url => url.Contains("100.5")), req)
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<CostCentreDto>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.UpdateCostCentreAsync(costCentreNo, dto);

            // Assert
            await _http.Received(1).PutAsync<CostCentreReq, CostCentreRes>(
                Arg.Is<string>(url => url.Contains("100.5")), req);
        }

        #endregion

        #region DeleteCostCentreAsync Tests

        [Fact]
        public async Task DeleteCostCentreAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = BuildDeleteResponse(true);
            var expectedDto  = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("100")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteCostCentreAsync(100.0);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(httpResponse);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = BuildDeleteResponse(false);
            var mappedDto    = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteCostCentreAsync(999.0);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_HttpThrowsException_ReturnsFailureResponse()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .Throws(new Exception("DELETE failed"));

            // Act
            var result = await _client.DeleteCostCentreAsync(100.0);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task DeleteCostCentreAsync_UsesInvariantCultureFormatting_InUrl()
        {
            // Arrange
            const double costCentreNo = 100.5;
            var httpResponse = BuildDeleteResponse(true);
            var expectedDto  = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("100.5")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.DeleteCostCentreAsync(costCentreNo);

            // Assert
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("100.5")));
        }

        #endregion
    }
}
