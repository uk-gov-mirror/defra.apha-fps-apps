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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsAnimalPlanApiClientTest
{
    public class FpsAnimalPlanApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsAnimalPlanApiClient _client;

        public FpsAnimalPlanApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsAnimalPlanApiClient(_http, _mapper);
        }

        #region GetAllAnimalCostAsync Tests

        [Fact]
        public async Task GetAllAnimalCostAsync_WithSuccessResponse_ReturnsMappedAnimalCostList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var resList = new List<AnimalCostViewRes>
            {
                new() { IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode, NumberOfDays = 5 },
                new() { IndCounter = 2, AnimalType = "Sheep",  JobCode = jobCode, NumberOfDays = 3 }
            };
            var apiResponse = new ApiResponse<List<AnimalCostViewRes>>
            {
                Success = true, Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>
                {
                    new() { IndCounter = 1, AnimalType = "Cattle" },
                    new() { IndCounter = 2, AnimalType = "Sheep" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<AnimalCostViewRes>>(
                    Arg.Is<string>(url => url.Contains("api/v1/animalrequest") && url.Contains($"jobCode={jobCode}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllAnimalCostAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AnimalCostViewRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/animalrequest") && url.Contains($"jobCode={jobCode}")));
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllAnimalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AnimalCostViewRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<List<AnimalCostViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllAnimalCostAsync(new QueryParameters<string>(), "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalCostByAnimalTypeAsync Tests

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithSuccessResponse_ReturnsMappedAnimalCostList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalType = "Cattle";
            var resList = new List<AnimalCostViewRes>
            {
                new() { IndCounter = 1, AnimalType = "Cattle", JobCode = "JOB001", NumberOfDays = 5 },
                new() { IndCounter = 2, AnimalType = "Cattle", JobCode = "JOB002", NumberOfDays = 3 }
            };
            var apiResponse = new ApiResponse<List<AnimalCostViewRes>>
            {
                Success = true, Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>
                {
                    new() { IndCounter = 1, AnimalType = "Cattle" },
                    new() { IndCounter = 2, AnimalType = "Cattle" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<AnimalCostViewRes>>(
                    Arg.Is<string>(url => url.Contains("api/v1/animalrequest/byanimaltype") && url.Contains($"animalType={animalType}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AnimalCostViewRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/animalrequest/byanimaltype") && url.Contains($"animalType={animalType}")));
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithoutAnimalType_DoesNotAppendAnimalTypeQuery()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<AnimalCostViewRes>>
            {
                Success = true, Data = new List<AnimalCostViewRes>()
            };
            var expectedDto = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(new List<AnimalCostViewDto>());

            _http.GetAsync<List<AnimalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalCostByAnimalTypeAsync(query, "");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<AnimalCostViewRes>>(
                Arg.Is<string>(url => !url.Contains("animalType=")));
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AnimalCostViewRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<List<AnimalCostViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAnimalCostByAnimalTypeAsync(new QueryParameters<string>(), "Cattle");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalLookupAsync Tests

        [Fact]
        public async Task GetAnimalLookupAsync_WithSuccessResponse_ReturnsMappedAnimalList()
        {
            // Arrange
            var resList = new List<AnimalRes>
            {
                new() { AnimalType = "Cattle", DailyRate = 25m },
                new() { AnimalType = "Sheep",  DailyRate = 15m }
            };
            var apiResponse = new ApiResponse<List<AnimalRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<AnimalDto>>.SuccessResponse(
                new List<AnimalDto>
                {
                    new() { AnimalType = "Cattle", DailyRate = 25m },
                    new() { AnimalType = "Sheep",  DailyRate = 15m }
                }
            );

            _http.GetAsync<List<AnimalRes>>("api/v1/animalrequest/lookup").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<AnimalRes>>("api/v1/animalrequest/lookup");
        }

        [Fact]
        public async Task GetAnimalLookupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<AnimalRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<AnimalDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalRes>>("api/v1/animalrequest/lookup").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAnimalLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalRateAsync Tests

        [Fact]
        public async Task GetAnimalRateAsync_WithValidAnimalType_ReturnsMappedRate()
        {
            // Arrange
            var animalType = "Cattle";
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 25.50m };
            var expectedDto = ApiResponseDto<decimal?>.SuccessResponse(25.50m);

            _http.GetAsync<decimal?>($"api/v1/animalrequest/rate?animalType={animalType}&jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal?>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalRateAsync(animalType, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(25.50m, result.Data);
        }

        [Fact]
        public async Task GetAnimalRateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var animalType = "Cattle";
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<decimal?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal?>($"api/v1/animalrequest/rate?animalType={animalType}&jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal?>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAnimalRateAsync(animalType, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAnimalCostAsync Tests

        [Fact]
        public async Task CreateAnimalCostAsync_WithValidRequest_ReturnsMappedCreatedDto()
        {
            // Arrange
            var animalRequestDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 5, NumberOfAnimals = 10 };
            var animalRequestReq = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 5, NumberOfAnimals = 10 };
            var apiResponse = new ApiResponse<AnimalRequestRes>
            {
                Success = true, Data = new AnimalRequestRes { JobCode = "JOB001", AnimalType = "Cattle" }
            };
            var expectedDto = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequestDto);

            _mapper.Map<AnimalRequestReq>(animalRequestDto).Returns(animalRequestReq);
            _http.PostAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalRequestDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAnimalCostAsync(animalRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("JOB001", result.Data?.JobCode);
            await _http.Received(1).PostAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq);
        }

        [Fact]
        public async Task CreateAnimalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var animalRequestDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle" };
            var animalRequestReq = new AnimalRequestReq { JobCode = "JOB001", AnimalType = "Cattle" };
            var apiResponse = new ApiResponse<AnimalRequestRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<AnimalRequestDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<AnimalRequestReq>(animalRequestDto).Returns(animalRequestReq);
            _http.PostAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalRequestDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAnimalCostAsync(animalRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAnimalCostAsync Tests

        [Fact]
        public async Task UpdateAnimalCostAsync_WithValidRequest_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var animalRequestDto = new AnimalRequestDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 7 };
            var animalRequestReq = new AnimalRequestReq { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 7 };
            var apiResponse = new ApiResponse<AnimalRequestRes>
            {
                Success = true, Data = new AnimalRequestRes { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle" }
            };
            var expectedDto = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequestDto);

            _mapper.Map<AnimalRequestReq>(animalRequestDto).Returns(animalRequestReq);
            _http.PutAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalRequestDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAnimalCostAsync(animalRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.IndCounter);
            await _http.Received(1).PutAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var animalRequestDto = new AnimalRequestDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle" };
            var animalRequestReq = new AnimalRequestReq { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle" };
            var apiResponse = new ApiResponse<AnimalRequestRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<AnimalRequestDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<AnimalRequestReq>(animalRequestDto).Returns(animalRequestReq);
            _http.PutAsync<AnimalRequestReq, AnimalRequestRes>("api/v1/animalrequest", animalRequestReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalRequestDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAnimalCostAsync(animalRequestDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAnimalCostAsync Tests

        [Fact]
        public async Task DeleteAnimalCostAsync_WithValidIndCounter_ReturnsSuccess()
        {
            // Arrange
            var indCounter = 1;
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/animalrequest?indCounter={indCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAnimalCostAsync(indCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/animalrequest?indCounter={indCounter}");
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var indCounter = 999;
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>($"api/v1/animalrequest?indCounter={indCounter}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAnimalCostAsync(indCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTotalAnimalCostAsync Tests

        [Fact]
        public async Task GetTotalAnimalCostAsync_WithValidJobCode_ReturnsMappedTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 1250.75m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(1250.75m);

            _http.GetAsync<decimal>($"api/v1/animalrequest/totalanimalcost?jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalAnimalCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1250.75m, result.Data);
            await _http.Received(1).GetAsync<decimal>($"api/v1/animalrequest/totalanimalcost?jobCode={jobCode}");
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
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

            _http.GetAsync<decimal>($"api/v1/animalrequest/totalanimalcost?jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalAnimalCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalCostViewByIdAsync Tests

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WithSuccessAndData_ReturnsMappedDto()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";
            var costViewRes = new AnimalCostViewRes
            {
                IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode, NumberOfDays = 5
            };
            var apiResponse = new ApiResponse<AnimalCostViewRes> { Success = true, Data = costViewRes };
            var expectedDto = new AnimalCostViewDto { IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode };

            _http.GetAsync<AnimalCostViewRes>($"api/v1/animalrequest/view?indCounter={indCounter}&jobCode={jobCode}")
                .Returns(apiResponse);
            _mapper.Map<AnimalCostViewDto>(costViewRes).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IndCounter);
            _mapper.Received(1).Map<AnimalCostViewDto>(costViewRes);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WithSuccessAndNullData_ReturnsSuccessWithNull()
        {
            // Arrange
            var indCounter = 999;
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<AnimalCostViewRes> { Success = true, Data = null };

            _http.GetAsync<AnimalCostViewRes>($"api/v1/animalrequest/view?indCounter={indCounter}&jobCode={jobCode}")
                .Returns(apiResponse);

            // Act
            var result = await _client.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
            _mapper.DidNotReceive().Map<AnimalCostViewDto>(Arg.Any<AnimalCostViewRes>());
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<AnimalCostViewRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<AnimalCostViewDto?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<AnimalCostViewRes>($"api/v1/animalrequest/view?indCounter={indCounter}&jobCode={jobCode}")
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalCostViewDto?>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
