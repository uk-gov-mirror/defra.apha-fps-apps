using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.BudgetBidsServiceTest
{
    public class BudgetBidsServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsBudgetBidsApiClient _fpsBudgetBidsApiClient;
        private readonly BudgetBidsService _sut;

        public BudgetBidsServiceTests()
        {
            _fpsClient             = Substitute.For<IFpsApiClient>();
            _fpsBudgetBidsApiClient = Substitute.For<IFpsBudgetBidsApiClient>();
            _fpsClient.FpsBudgetBids.Returns(_fpsBudgetBidsApiClient);
            _sut = new BudgetBidsService(_fpsClient);
        }

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithSuccessResponse_ReturnsBidViews()
        {
            // Arrange
            var bidList = new List<BidViewDto> { new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m } };
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(bidList);
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsBudgetBidsApiClient.Received(1).GetBidViewAsync("WG01");
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>());
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetBidViewAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

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
            var dto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(dto);
            _fpsBudgetBidsApiClient.GetBidByIdAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ACC1", result.Data?.Account);
        }

        [Fact]
        public async Task GetBidByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetBidByIdAsync("WG01", "NOTEXIST").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateBidAsync Tests

        [Fact]
        public async Task CreateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(bidDto);
            _fpsBudgetBidsApiClient.CreateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).CreateBidAsync(bidDto);
        }

        [Fact]
        public async Task CreateBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.CreateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateBidAsync Tests

        [Fact]
        public async Task UpdateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(bidDto);
            _fpsBudgetBidsApiClient.UpdateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).UpdateBidAsync(bidDto);
        }

        [Fact]
        public async Task UpdateBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.UpdateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateBidAsync(bidDto);

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
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsBudgetBidsApiClient.DeleteBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).DeleteBidAsync(bidDto);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.DeleteBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFpsClient_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new BudgetBidsService(null!));
            Assert.Equal("fpsClient", ex.ParamName);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsCategories()
        {
            // Arrange
            var categories = new List<AccountCategoryDto> { new() { AccShortName = "ACC1" } };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories);
            _fpsBudgetBidsApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsBudgetBidsApiClient.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetBidViewPagedAsync Tests

        [Fact]
        public async Task GetBidViewPagedAsync_WithData_ReturnsPagedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var bidList = new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m },
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m }
            };
            var allResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(bidList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(allResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
            Assert.Equal(1, result.Pagination.PageNumber);
            Assert.Equal(10, result.Pagination.PageSize);
            await _fpsBudgetBidsApiClient.Received(1).GetBidViewPagedAsync(query, "WG01");
        }

        [Fact]
        public async Task GetBidViewPagedAsync_EmptyList_ReturnsPagedSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var allResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse([], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(allResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            Assert.NotNull(result.Pagination);
            Assert.Equal(0, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetBidViewPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var failResponse = ApiResponseDto<List<BidViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(failResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).GetBidViewPagedAsync(query, "WG01");
        }

        [Fact]
        public async Task GetBidViewPagedAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            var pagedList = new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m }
            };
            var pagedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(pagedList, new PaginationDto { PageNumber = 2, PageSize = 1, TotalRecords = 2, TotalPages = 2 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(pagedResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("ACC2", result.Data![0].Account);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
            Assert.Equal(2, result.Pagination.PageNumber);
            Assert.Equal(1, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetBidViewPagedAsync_AppliesFilter_ReturnsFilteredResults()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"Account":"ACC1"}"""
            };
            var filteredList = new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m }
            };
            var filteredResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(filteredList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(filteredResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("ACC1", result.Data![0].Account);
            Assert.Equal(1, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetBidViewPagedAsync_AppliesSortAscending_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "Account", Descending = false };
            var sortedList = new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m },
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m }
            };
            var sortedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(sortedList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(sortedResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("ACC1", result.Data![0].Account);
            Assert.Equal("ACC2", result.Data![1].Account);
        }

        [Fact]
        public async Task GetBidViewPagedAsync_AppliesSortDescending_ReturnsSortedResults()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "Account", Descending = true };
            var sortedList = new List<BidViewDto>
            {
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m },
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m }
            };
            var sortedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(sortedList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsBudgetBidsApiClient.GetBidViewPagedAsync(query, "WG01").Returns(sortedResponse);

            // Act
            var result = await _sut.GetBidViewPagedAsync(query, "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("ACC2", result.Data![0].Account);
            Assert.Equal("ACC1", result.Data![1].Account);
        }

        #endregion

        #region GetGenericBidsPagedAsync Tests

        [Fact]
        public async Task GetGenericBidsPagedAsync_WithData_ReturnsPagedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var bidList = new List<GenericBidViewDto>
            {
                new() { ProfitCentre = "PC1", WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, AccountType = "TYPE1" },
                new() { ProfitCentre = "PC1", WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m, AccountType = "TYPE2" }
            };
            var expectedResponse = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(
                bidList, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 });
            _fpsBudgetBidsApiClient.GetGenericBidsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
            await _fpsBudgetBidsApiClient.Received(1).GetGenericBidsPagedAsync(query);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(
                new List<GenericBidViewDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            _fpsBudgetBidsApiClient.GetGenericBidsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<GenericBidViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetGenericBidsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsBudgetBidsApiClient.Received(1).GetGenericBidsPagedAsync(query);
        }

        #endregion
    }
}
