using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestListApiClientTest
{
    public class PactTestorProductApiClientTests
    {
        private readonly IPactHttpExecutor _httpExecutor;
        private readonly IMapper _mapper;
        private readonly PactTestorProductApiClient _client;

        public PactTestorProductApiClientTests()
        {
            _httpExecutor = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            SetupMapper();
            _client = new PactTestorProductApiClient(_httpExecutor, _mapper);
        }

        private void SetupMapper()
        {
            // Map TestOrProductRes to TestOrProductDto
            _mapper.Map<TestorProductDto>(Arg.Any<TestorProductRes>())
                .Returns(callInfo =>
                {
                    var res = callInfo.ArgAt<TestorProductRes>(0);
                    if (res == null) return null;
                    return new TestorProductDto
                    {
                        ItemCode = res.ItemCode,
                        ItemDescription = res.ItemDescription,
                        TestManager = res.TestManager,
                        JobStatus = res.JobStatus,
                        UnitPriceVla = res.UnitPriceVla,
                        PriceAhvg = res.PriceAhvg,
                        Owner = res.Owner,
                        ChargeMethod = res.ChargeMethod,
                        ShortDescription = res.ShortDescription,
                        DefraUnitPrice = res.DefraUnitPrice,
                        FpsYear = res.FpsYear
                    };
                });

            // Map TestOrProductDto to TestOrProductReq
            _mapper.Map<TestorProductReq>(Arg.Any<TestorProductDto>())
                .Returns(callInfo =>
                {
                    var dto = callInfo.ArgAt<TestorProductDto>(0);
                    if (dto == null) return null;
                    return new TestorProductReq
                    {
                        ItemCode = dto.ItemCode,
                        ItemDescription = dto.ItemDescription,
                        TestManager = dto.TestManager,
                        JobStatus = dto.JobStatus,
                        UnitPriceVla = dto.UnitPriceVla,
                        PriceAhvg = dto.PriceAhvg,
                        Owner = dto.Owner,
                        ChargeMethod = dto.ChargeMethod,
                        ShortDescription = dto.ShortDescription,
                        DefraUnitPrice = dto.DefraUnitPrice,
                        FpsYear = dto.FpsYear
                    };
                });

            // Map List<TestOrProductRes> to List<TestOrProductDto>
            _mapper.Map<List<TestorProductDto>>(Arg.Any<IEnumerable<TestorProductRes>>())
                .Returns(callInfo =>
                {
                    var resList = callInfo.ArgAt<IEnumerable<TestorProductRes>>(0);
                    if (resList == null) return new List<TestorProductDto>();
                    return resList.Select(res => new TestorProductDto
                    {
                        ItemCode = res.ItemCode,
                        ItemDescription = res.ItemDescription,
                        TestManager = res.TestManager,
                        JobStatus = res.JobStatus,
                        UnitPriceVla = res.UnitPriceVla,
                        PriceAhvg = res.PriceAhvg,
                        Owner = res.Owner,
                        ChargeMethod = res.ChargeMethod,
                        ShortDescription = res.ShortDescription,
                        DefraUnitPrice = res.DefraUnitPrice,
                        FpsYear = res.FpsYear
                    }).ToList();
                });

            // Map ApiResponse<List<TestorProductRes>> to ApiResponseDto<List<TestorProductDto>>
            _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(Arg.Any<ApiResponse<List<TestorProductRes>>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<List<TestorProductRes>>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());

                    var dtoList = response.Data.Select(res => new TestorProductDto
                    {
                        ItemCode = res.ItemCode,
                        ItemDescription = res.ItemDescription,
                        TestManager = res.TestManager,
                        JobStatus = res.JobStatus,
                        UnitPriceVla = res.UnitPriceVla,
                        PriceAhvg = res.PriceAhvg,
                        Owner = res.Owner,
                        ChargeMethod = res.ChargeMethod,
                        ShortDescription = res.ShortDescription,
                        DefraUnitPrice = res.DefraUnitPrice,
                        FpsYear = res.FpsYear
                    }).ToList();

                    return ApiResponseDto<List<TestorProductDto>>.SuccessResponse(dtoList);
                });

            // Map ApiResponse<TestorProductRes> to ApiResponseDto<TestorProductDto>
            _mapper.Map<ApiResponseDto<TestorProductDto>>(Arg.Any<ApiResponse<TestorProductRes>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<TestorProductRes>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<TestorProductDto>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());

                    var dto = new TestorProductDto
                    {
                        ItemCode = response.Data.ItemCode,
                        ItemDescription = response.Data.ItemDescription,
                        TestManager = response.Data.TestManager,
                        JobStatus = response.Data.JobStatus,
                        UnitPriceVla = response.Data.UnitPriceVla,
                        PriceAhvg = response.Data.PriceAhvg,
                        Owner = response.Data.Owner,
                        ChargeMethod = response.Data.ChargeMethod,
                        ShortDescription = response.Data.ShortDescription,
                        DefraUnitPrice = response.Data.DefraUnitPrice,
                        FpsYear = response.Data.FpsYear
                    };

                    return ApiResponseDto<TestorProductDto>.SuccessResponse(dto);
                });

            // Map ApiResponse<bool> to ApiResponseDto<bool>
            _mapper.Map<ApiResponseDto<bool>>(Arg.Any<ApiResponse<bool>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<bool>>(0);
                    if (response == null || !response.Success)
                        return ApiResponseDto<bool>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    return ApiResponseDto<bool>.SuccessResponse(response.Data);
                });

            // Map ApiResponse<bool?> to ApiResponseDto<bool>
            _mapper.Map<ApiResponseDto<bool>>(Arg.Any<ApiResponse<bool?>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<bool?>>(0);
                    if (response == null || !response.Success)
                        return ApiResponseDto<bool>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    return ApiResponseDto<bool>.SuccessResponse(response.Data ?? false);
                });

            // Map ApiResponse<List<string>> to ApiResponseDto<List<string>>
            _mapper.Map<ApiResponseDto<List<string>>>(Arg.Any<ApiResponse<List<string>>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<List<string>>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<List<string>>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());
                    return ApiResponseDto<List<string>>.SuccessResponse(response.Data);
                });
        }

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithValidQuery_ReturnsListOfDtos()
        {
            // Arrange
            var query = new Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var testItems = new List<TestorProductRes>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" },
                new() { ItemCode = "T002", ItemDescription = "Test Two" }
            };
            var httpResponse = new ApiResponse<List<TestorProductRes>>
            {
                Success = true,
                Data = testItems
            };
            _httpExecutor.GetAsync<List<TestorProductRes>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestorProductRes>>
            {
                Success = true,
                Data = new List<TestorProductRes>()
            };
            _httpExecutor.GetAsync<List<TestorProductRes>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WhenHttpFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<TestorProductRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "HTTP Error", Code = "HTTP_ERROR" } }
            };
            _httpExecutor.GetAsync<List<TestorProductRes>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_WithValidItemCode_ReturnsDto()
        {
            // Arrange
            var itemCode = "T001";
            var testorProduct = new TestorProductRes{ItemCode = itemCode, ItemDescription = "Test Product" };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = true,
                Data = testorProduct
            };
            _httpExecutor.GetAsync<TestorProductRes>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetTestOrProductByIdAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(itemCode, result.Data?.ItemCode);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_WithNonExistentItemCode_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            _httpExecutor.GetAsync<TestorProductRes>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetTestOrProductByIdAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_WithValidDto_ReturnsCreatedDto()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "T001", ItemDescription = "New Test" };
            var createdRes = new TestorProductRes { ItemCode = "T001", ItemDescription = "New Test" };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = true,
                Data = createdRes
            };
            _httpExecutor.PostAsync<TestorProductReq, TestorProductRes>(Arg.Any<string>(), Arg.Any<TestorProductReq>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.CreateTestOrProductAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("T001", result.Data?.ItemCode);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_WhenValidationFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new TestorProductDto { ItemCode = "T001" };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } }
            };
            _httpExecutor.PostAsync<TestorProductReq, TestorProductRes>(Arg.Any<string>(), Arg.Any<TestorProductReq>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.CreateTestOrProductAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestOrProductAsync

        [Fact]
        public async Task UpdateTestOrProductAsync_WithValidItemCodeAndDto_ReturnsUpdatedDto()
        {
            // Arrange
            var itemCode = "T001";
            var dto = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Updated Test" };
            var updatedRes = new TestorProductRes { ItemCode = itemCode, ItemDescription = "Updated Test" };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = true,
                Data = updatedRes
            };
            _httpExecutor.PutAsync<TestorProductReq, TestorProductRes>(Arg.Any<string>(), Arg.Any<TestorProductReq>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.UpdateTestOrProductAsync(itemCode, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(itemCode, result.Data?.ItemCode);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_WithNonExistentItemCode_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var dto = new TestorProductDto { ItemCode = itemCode };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            _httpExecutor.PutAsync<TestorProductReq, TestorProductRes>(Arg.Any<string>(), Arg.Any<TestorProductReq>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.UpdateTestOrProductAsync(itemCode, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_WhenValidationFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var dto = new TestorProductDto { ItemCode = itemCode };
            var httpResponse = new ApiResponse<TestorProductRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } }
            };
            _httpExecutor.PutAsync<TestorProductReq, TestorProductRes>(Arg.Any<string>(), Arg.Any<TestorProductReq>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.UpdateTestOrProductAsync(itemCode, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_WithValidItemCode_ReturnsSuccessTrue()
        {
            // Arrange
            var itemCode = "T001";
            var httpResponse = new ApiResponse<bool?>
            {
                Success = true,
                Data = true
            };
            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WithNonExistentItemCode_ReturnsSuccessFalse()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var httpResponse = new ApiResponse<bool?>
            {
                Success = true,
                Data = false
            };
            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WhenHttpFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var httpResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } }
            };
            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_WithValidRequest_ReturnsOwnersList()
        {
            // Arrange
            var owners = new List<string> { "AB", "CD", "EF" };
            var httpResponse = new ApiResponse<List<string>>
            {
                Success = true,
                Data = owners
            };
            _httpExecutor.GetAsync<List<string>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
        }

        [Fact]
        public async Task GetOwnersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<string>>
            {
                Success = true,
                Data = new List<string>()
            };
            _httpExecutor.GetAsync<List<string>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetOwnersAsync_WhenHttpFails_ReturnsFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Service error", Code = "SERVICE_ERROR" } }
            };
            _httpExecutor.GetAsync<List<string>>(Arg.Any<string>())
                .Returns(Task.FromResult(httpResponse));

            // Act
            var result = await _client.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
