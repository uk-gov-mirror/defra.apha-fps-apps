using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.TestListVlaServiceTest
{
    public class TestListVlaServiceTests
    {
        private const string DefaultItemCode = "TEST001";
        private const int DefaultFpsYear = 2025;

        private readonly IPactApiClient _pactClient;
        private readonly IPactTestorProductApiClient _testListVlaClient;
        private readonly TestListVlaService _service;

        public TestListVlaServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _testListVlaClient = Substitute.For<IPactTestorProductApiClient>();
            _pactClient.PactTestList.Returns(_testListVlaClient);
            _service = new TestListVlaService(_pactClient);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } },
                new PaginationDto { TotalRecords = 1 });
            _testListVlaClient.GetPagedTestOrProductsAsync(query).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _testListVlaClient.Received(1).GetPagedTestOrProductsAsync(query);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto>(), new PaginationDto { TotalRecords = 0 });
            _testListVlaClient.GetPagedTestOrProductsAsync(query).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "SERVER_ERROR", Message = "Server error" } },
                new ApiMetaDto());
            _testListVlaClient.GetPagedTestOrProductsAsync(query).Returns(expected);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            Assert.False(result.Success);
            await _testListVlaClient.Received(1).GetPagedTestOrProductsAsync(query);
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } });
            _testListVlaClient.GetAllTestorProductsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllByYearAsync();

            // Assert
            Assert.True(result.Success);
            await _testListVlaClient.Received(1).GetAllTestorProductsAsync();
        }

        [Fact]
        public async Task GetAllByYearAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.GetAllTestorProductsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllByYearAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var expected = ApiResponseDto<TestorProductDto>.SuccessResponse(dto);
            _testListVlaClient.GetTestOrProductByIdAsync(DefaultItemCode).Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(DefaultItemCode);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(DefaultItemCode, result.Data!.ItemCode);
            await _testListVlaClient.Received(1).GetTestOrProductByIdAsync(DefaultItemCode);
        }

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<TestorProductDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaClient.GetTestOrProductByIdAsync("NOTEXIST").Returns(expected);

            // Act
            var result = await _service.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
