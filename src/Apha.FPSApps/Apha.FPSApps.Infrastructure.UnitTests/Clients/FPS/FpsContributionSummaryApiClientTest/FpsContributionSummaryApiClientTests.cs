using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTimeSellerPcApiClientTest
{
    public class FpsContributionSummaryApiClientTests
    {
        private readonly IFpsHttpExecutor         _http;
        private readonly IMapper                  _mapper;
        private readonly FpsContributionSummaryApiClient _client;

        public FpsContributionSummaryApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsContributionSummaryApiClient(_http, _mapper);
        }

        private static ApiResponse<List<ContributionSummaryRowRes>> MakeRowsApiResponse(bool success = true)
            => new()
            {
                Success = success,
                Data    = success ? new List<ContributionSummaryRowRes>
                {
                    new() { WorkGroup = "WG1", WgGrade = "G1", Fec = 500m },
                    new() { WorkGroup = "WG2", WgGrade = "G2", Fec = 750m }
                } : null,
                Errors = success ? null : new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };

        private static ApiResponse<ContributionSummaryTotalsRes> MakeTotalsApiResponse(bool success = true)
            => new()
            {
                Success = success,
                Data    = success ? new ContributionSummaryTotalsRes { SellingPc = "ENV", TotalFec = 1250m } : null,
                Errors  = success ? null : new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };

        #region GetRowsAsync

        [Fact]
        public async Task GetRowsAsync_WhenApiSucceeds_ReturnsMappedDtos()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeRowsApiResponse();
            var expectedDto = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(
                new List<ContributionSummaryRowDto>
                {
                    new() { WorkGroup = "WG1" },
                    new() { WorkGroup = "WG2" }
                });

            _http.GetAsync<List<ContributionSummaryRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetRowsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ContributionSummaryRowRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetRowsAsync_UsesCorrectUrl()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeRowsApiResponse();
            var dto         = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(new List<ContributionSummaryRowDto>());

            _http.GetAsync<List<ContributionSummaryRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(Arg.Any<ApiResponse<List<ContributionSummaryRowRes>>>()).Returns(dto);

            // Act
            await _client.GetRowsAsync(sellingPc);

            // Assert
            await _http.Received(1).GetAsync<List<ContributionSummaryRowRes>>(
                Arg.Is<string>(url => url.Contains("timeseller")
                                   && url.Contains(sellingPc)
                                   && url.Contains("rows")));
        }

        [Fact]
        public async Task GetRowsAsync_WhenApiReturnsFail_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse    = MakeRowsApiResponse(success: false);
            var mappedResponse = new ApiResponseDto<List<ContributionSummaryRowDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ContributionSummaryRowRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryRowDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetRowsAsync("ENV");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WhenApiSucceeds_ReturnsMappedDto()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeTotalsApiResponse();
            var expectedDto = ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(
                new ContributionSummaryTotalsDto { SellingPc = sellingPc, TotalFec = 1250m });

            _http.GetAsync<ContributionSummaryTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryTotalsDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(sellingPc, result.Data?.SellingPc);
            await _http.Received(1).GetAsync<ContributionSummaryTotalsRes>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<ContributionSummaryTotalsDto>>(apiResponse);
        }

        [Fact]
        public async Task GetTotalsAsync_UsesCorrectUrl()
        {
            // Arrange
            var sellingPc   = "ENV";
            var apiResponse = MakeTotalsApiResponse();
            var dto         = ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(
                new ContributionSummaryTotalsDto { SellingPc = sellingPc });

            _http.GetAsync<ContributionSummaryTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryTotalsDto>>(Arg.Any<ApiResponse<ContributionSummaryTotalsRes>>()).Returns(dto);

            // Act
            await _client.GetTotalsAsync(sellingPc);

            // Assert
            await _http.Received(1).GetAsync<ContributionSummaryTotalsRes>(
                Arg.Is<string>(url => url.Contains("timeseller")
                                   && url.Contains(sellingPc)
                                   && url.Contains("totals")));
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiReturnsFail_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse    = MakeTotalsApiResponse(success: false);
            var mappedResponse = new ApiResponseDto<ContributionSummaryTotalsDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<ContributionSummaryTotalsRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryTotalsDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalsAsync("ENV");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
