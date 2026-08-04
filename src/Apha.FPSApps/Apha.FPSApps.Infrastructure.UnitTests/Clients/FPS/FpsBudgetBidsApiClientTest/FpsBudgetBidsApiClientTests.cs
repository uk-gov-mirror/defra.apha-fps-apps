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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsBudgetBidsApiClientTest
{
    public class FpsBudgetBidsApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsBudgetBidsApiClient _client;

        public FpsBudgetBidsApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsBudgetBidsApiClient(_http, _mapper);
        }

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithSuccessResponse_ReturnsBidViews()
        {
            // Arrange
            var res = new List<BidViewRes> { new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m } };
            var apiResponse = new ApiResponse<List<BidViewRes>> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m }
            });

            _http.GetAsync<List<BidViewRes>>(Arg.Is<string>(url => url.Contains("budgetbids")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BidViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<BidViewRes>> { Success = true, Data = new List<BidViewRes>() };
            var expectedDto = ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>());

            _http.GetAsync<List<BidViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BidViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetBidViewAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<BidViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<BidViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<BidViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BidViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetBidByIdAsync Tests

        [Fact]
        public async Task GetBidByIdAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var res = new BidRes { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var apiResponse = new ApiResponse<BidRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<BidDto>.SuccessResponse(new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m });

            _http.GetAsync<BidRes>(Arg.Is<string>(url => url.Contains("WG01") && url.Contains("ACC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ACC1", result.Data?.Account);
        }

        [Fact]
        public async Task GetBidByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<BidRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<BidDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<BidRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetBidByIdAsync("WG01", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region CreateBidAsync Tests

        [Fact]
        public async Task CreateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto  = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var bidReq  = new BidReq { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var bidRes  = new BidRes { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var apiResponse = new ApiResponse<BidRes> { Success = true, Data = bidRes };
            var expectedDto = ApiResponseDto<BidDto>.SuccessResponse(bidDto);

            _mapper.Map<BidReq>(bidDto).Returns(bidReq);
            _http.PostAsync<BidReq, BidRes>(Arg.Any<string>(), bidReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateBidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var bidReq = new BidReq { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var apiResponse = new ApiResponse<BidRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Already exists", Code = "CONFLICT" } }
            };
            var mappedResponse = new ApiResponseDto<BidDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<BidReq>(bidDto).Returns(bidReq);
            _http.PostAsync<BidReq, BidRes>(Arg.Any<string>(), bidReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateBidAsync Tests

        [Fact]
        public async Task UpdateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto  = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var bidReq  = new BidReq { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var bidRes  = new BidRes { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var apiResponse = new ApiResponse<BidRes> { Success = true, Data = bidRes };
            var expectedDto = ApiResponseDto<BidDto>.SuccessResponse(bidDto);

            _mapper.Map<BidReq>(bidDto).Returns(bidReq);
            _http.PutAsync<BidReq, BidRes>(Arg.Any<string>(), bidReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateBidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var bidReq = new BidReq { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var apiResponse = new ApiResponse<BidRes>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<BidDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<BidReq>(bidDto).Returns(bidReq);
            _http.PutAsync<BidReq, BidRes>(Arg.Any<string>(), bidReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteBidAsync Tests

        [Fact]
        public async Task DeleteBidAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1" };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("WG01") && url.Contains("ACC1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1" };
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsCategories()
        {
            // Arrange
            var res = new List<AccountCategoryRes> { new() { AccShortName = "ACC1" } };
            var apiResponse = new ApiResponse<List<AccountCategoryRes>> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto> { new() { AccShortName = "ACC1" } });

            _http.GetAsync<List<AccountCategoryRes>>(Arg.Is<string>(url => url.Contains("accounts")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AccountCategoryRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<AccountCategoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetGenericBidsPagedAsync Tests

        [Fact]
        public async Task GetGenericBidsPagedAsync_WithSuccessResponse_ReturnsMappedDtos()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var res = new List<GenericBidViewRes>
            {
                new() { ProfitCentre = "PC1", WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, AccountType = "TYPE1" }
            };
            var apiResponse = new ApiResponse<IEnumerable<GenericBidViewRes>> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(new List<GenericBidViewDto>
            {
                new() { ProfitCentre = "PC1", WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, AccountType = "TYPE1" }
            });

            _http.GetAsync<IEnumerable<GenericBidViewRes>>(Arg.Is<string>(url => url.Contains("generic")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("WG01", result.Data![0].WorkGroupName);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<IEnumerable<GenericBidViewRes>> { Success = true, Data = new List<GenericBidViewRes>() };
            var expectedDto = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(new List<GenericBidViewDto>());

            _http.GetAsync<IEnumerable<GenericBidViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<IEnumerable<GenericBidViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<GenericBidViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<GenericBidViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_PassesQueryParametersToUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "Account", Descending = true };
            var apiResponse = new ApiResponse<IEnumerable<GenericBidViewRes>> { Success = true, Data = new List<GenericBidViewRes>() };
            var expectedDto = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(new List<GenericBidViewDto>());

            _http.GetAsync<IEnumerable<GenericBidViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetGenericBidsPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<IEnumerable<GenericBidViewRes>>(
                Arg.Is<string>(url => url.Contains("Account") && url.Contains("2")));
        }

        #endregion
    }
}
