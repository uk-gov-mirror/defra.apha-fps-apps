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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsAdditionalCostApiClientTest
{
    public class FpsAdditionalCostApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsAdditionalCostApiClient _client;

        public FpsAdditionalCostApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsAdditionalCostApiClient(_http, _mapper);
        }

        private static AdditionalCostRes BuildRes(string jobCode = "JOB001") =>
            new() { JobCode = jobCode, Account = "ACC001", Description = "Test Cost", ItemCost = 100m };

        private static AdditionalCostDto BuildDto(string jobCode = "JOB001") =>
            new() { JobCode = jobCode, Account = "ACC001", Description = "Test Cost", OriginalDescription = "Test Cost", ItemCost = 100m };

        #region GetAdditionalCostsAsync Tests

        [Fact]
        public async Task GetAdditionalCostsAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var resList = new List<AdditionalCostRes>
            {
                new() { JobCode = jobCode, Account = "ACC001", Description = "Cost A" },
                new() { JobCode = jobCode, Account = "ACC002", Description = "Cost B" }
            };
            var apiResponse = new ApiResponse<List<AdditionalCostRes>>
            {
                Success = true, Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<AdditionalCostDto>>.SuccessResponse(
                new List<AdditionalCostDto>
                {
                    new() { JobCode = jobCode, Account = "ACC001", Description = "Cost A" },
                    new() { JobCode = jobCode, Account = "ACC002", Description = "Cost B" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<AdditionalCostRes>>(
                    Arg.Is<string>(url => url.Contains("api/v1/additionalcost") && url.Contains($"jobCode={jobCode}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAdditionalCostsAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AdditionalCostRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/additionalcost") && url.Contains($"jobCode={jobCode}")));
            _mapper.Received(1).Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAdditionalCostsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AdditionalCostRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<List<AdditionalCostDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAdditionalCostsAsync(new QueryParameters<string>(), "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTotalItemCostAsync Tests

        [Fact]
        public async Task GetTotalItemCostAsync_WithSuccessResponse_ReturnsTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 300m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(300m);

            _http.GetAsync<decimal>(Arg.Is<string>(url => url.Contains("totalitemcost") && url.Contains($"jobCode={jobCode}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalItemCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(300m, result.Data);
            _mapper.Received(1).Map<ApiResponseDto<decimal>>(apiResponse);
        }

        [Fact]
        public async Task GetTotalItemCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalItemCostAsync("JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsMappedCategories()
        {
            // Arrange
            var resList = new List<AccountCategoryRes>
            {
                new() { AccShortName = "ACC001", AccountDescription = "Travel" },
                new() { AccShortName = "ACC002", AccountDescription = "Equipment" }
            };
            var apiResponse = new ApiResponse<List<AccountCategoryRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto>
                {
                    new() { AccShortName = "ACC001", AccountDescription = "Travel" },
                    new() { AccShortName = "ACC002", AccountDescription = "Equipment" }
                });

            _http.GetAsync<List<AccountCategoryRes>>("api/v1/additionalcost/accountcategories").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AccountCategoryRes>>("api/v1/additionalcost/accountcategories");
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AccountCategoryRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<AccountCategoryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
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

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidKeys_ReturnsMappedAdditionalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var account = "ACC001";
            var description = "Test Cost";
            var res = BuildRes(jobCode);
            var apiResponse = new ApiResponse<AdditionalCostRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<AdditionalCostDto>.SuccessResponse(BuildDto(jobCode));

            _http.GetAsync<AdditionalCostRes>(
                    Arg.Is<string>(url => url.Contains(jobCode) && url.Contains(account) && url.Contains(description)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(jobCode, account, description);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCode, result.Data?.JobCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<AdditionalCostRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<AdditionalCostDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<AdditionalCostRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync("JOB001", "ACC001", "Missing");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAdditionalCostAsync Tests

        [Fact]
        public async Task CreateAdditionalCostAsync_WithValidDto_ReturnsMappedResult()
        {
            // Arrange
            var dto = BuildDto();
            var req = new AdditionalCostReq { JobCode = dto.JobCode, Account = dto.Account, Description = dto.Description };
            var res = BuildRes();
            var apiResponse = new ApiResponse<AdditionalCostRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);

            _mapper.Map<AdditionalCostReq>(dto).Returns(req);
            _http.PostAsync<AdditionalCostReq, AdditionalCostRes>("api/v1/additionalcost", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mapper.Received(1).Map<AdditionalCostReq>(dto);
            await _http.Received(1).PostAsync<AdditionalCostReq, AdditionalCostRes>("api/v1/additionalcost", req);
        }

        [Fact]
        public async Task CreateAdditionalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = new AdditionalCostReq();
            var apiResponse = new ApiResponse<AdditionalCostRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Create failed", Code = "CREATE_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<AdditionalCostDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Create failed", Code = "CREATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<AdditionalCostReq>(dto).Returns(req);
            _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAdditionalCostAsync Tests

        [Fact]
        public async Task UpdateAdditionalCostAsync_WithValidDto_ReturnsMappedResult()
        {
            // Arrange
            var dto = BuildDto();
            var req = new AdditionalCostReq { JobCode = dto.JobCode, Account = dto.Account, Description = dto.Description };
            var res = BuildRes();
            var apiResponse = new ApiResponse<AdditionalCostRes> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto);

            _mapper.Map<AdditionalCostReq>(dto).Returns(req);
            _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(FpsApiEndpoints.UpdateAdditionalCost, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mapper.Received(1).Map<AdditionalCostReq>(dto);
            await _http.Received(1).PutAsync<AdditionalCostReq, AdditionalCostRes>(FpsApiEndpoints.UpdateAdditionalCost, req);
        }

        [Fact]
        public async Task UpdateAdditionalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = new AdditionalCostReq();
            var apiResponse = new ApiResponse<AdditionalCostRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<AdditionalCostDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<AdditionalCostReq>(dto).Returns(req);
            _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAdditionalCostAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAdditionalCostAsync Tests

        [Fact]
        public async Task DeleteAdditionalCostAsync_WithValidKeys_ReturnsMappedResult()
        {
            // Arrange
            var additionalCost = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC001", Description = "Test Cost" };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url =>
                    url.Contains($"jobCode={additionalCost.JobCode}") && url.Contains($"account={additionalCost.Account}") && url.Contains($"description={additionalCost.Description}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAdditionalCostAsync(additionalCost);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResponse);
        }

        [Fact]
        public async Task DeleteAdditionalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAdditionalCostAsync(new AdditionalCostDto { JobCode = "JOB001", Account = "ACC001", Description = "Test Cost" });

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
