using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.AnimalPlanServiceTest
{
    public class AnimalPlanServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsAnimalPlanApiClient _fpsAnimalPlanApiClient;
        private readonly AnimalPlanService _animalPlanService;

        public AnimalPlanServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsAnimalPlanApiClient = Substitute.For<IFpsAnimalPlanApiClient>();
            _fpsClient.FpsAnimalPlan.Returns(_fpsAnimalPlanApiClient);
            _animalPlanService = new AnimalPlanService(_fpsClient);
        }

        #region GetAllAnimalCostAsync Tests

        [Fact]
        public async Task GetAllAnimalCostAsync_WithSuccessResponse_ReturnsAnimalCostList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var animalCosts = new List<AnimalCostViewDto>
            {
                new() { IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode, NumberOfDays = 5, NumberOfAnimals = 10, DailyRate = 25m },
                new() { IndCounter = 2, AnimalType = "Sheep",  JobCode = jobCode, NumberOfDays = 3, NumberOfAnimals = 20, DailyRate = 15m }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                animalCosts, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsAnimalPlanApiClient.GetAllAnimalCostAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAllAnimalCostAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsAnimalPlanApiClient.Received(1).GetAllAnimalCostAsync(query, jobCode);
        }

        [Fact]
        public async Task GetAllAnimalCostAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _fpsAnimalPlanApiClient.GetAllAnimalCostAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAllAnimalCostAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllAnimalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsAnimalPlanApiClient.GetAllAnimalCostAsync(query, jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAllAnimalCostAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAnimalCostByAnimalTypeAsync Tests

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithSuccessResponse_ReturnsAnimalCostList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalType = "Cattle";
            var animalCosts = new List<AnimalCostViewDto>
            {
                new() { IndCounter = 1, AnimalType = "Cattle", JobCode = "JOB001", NumberOfDays = 5, NumberOfAnimals = 10, DailyRate = 25m },
                new() { IndCounter = 2, AnimalType = "Cattle", JobCode = "JOB002", NumberOfDays = 3, NumberOfAnimals = 20, DailyRate = 25m }
            };
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                animalCosts, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsAnimalPlanApiClient.GetAnimalCostByAnimalTypeAsync(query, animalType).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsAnimalPlanApiClient.Received(1).GetAnimalCostByAnimalTypeAsync(query, animalType);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalType = "Cattle";
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                new List<AnimalCostViewDto>(), new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _fpsAnimalPlanApiClient.GetAnimalCostByAnimalTypeAsync(query, animalType).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var animalType = "Cattle";
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AnimalCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsAnimalPlanApiClient.GetAnimalCostByAnimalTypeAsync(query, animalType).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAnimalLookupAsync Tests

        [Fact]
        public async Task GetAnimalLookupAsync_ReturnsAnimalList()
        {
            // Arrange
            var animals = new List<AnimalDto>
            {
                new() { AnimalType = "Cattle", DailyRate = 25m },
                new() { AnimalType = "Sheep",  DailyRate = 15m }
            };
            var expectedResponse = ApiResponseDto<List<AnimalDto>>.SuccessResponse(animals);

            _fpsAnimalPlanApiClient.GetAnimalLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsAnimalPlanApiClient.Received(1).GetAnimalLookupAsync();
        }

        [Fact]
        public async Task GetAnimalLookupAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>());
            _fpsAnimalPlanApiClient.GetAnimalLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAnimalLookupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AnimalDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.GetAnimalLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAnimalRateAsync Tests

        [Fact]
        public async Task GetAnimalRateAsync_WithValidAnimalType_ReturnsRate()
        {
            // Arrange
            var animalType = "Cattle";
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<decimal?>.SuccessResponse(25.50m);
            _fpsAnimalPlanApiClient.GetAnimalRateAsync(animalType, jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalRateAsync(animalType, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(25.50m, result.Data);
            await _fpsAnimalPlanApiClient.Received(1).GetAnimalRateAsync(animalType, jobCode);
        }

        [Fact]
        public async Task GetAnimalRateAsync_WhenRateIsNull_ReturnsSuccessWithNullData()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<decimal?>.SuccessResponse(null);
            _fpsAnimalPlanApiClient.GetAnimalRateAsync("Unknown", jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalRateAsync("Unknown", jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAnimalRateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<decimal?>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.GetAnimalRateAsync("Cattle", jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalRateAsync("Cattle", jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetTotalAnimalCostAsync Tests

        [Fact]
        public async Task GetTotalAnimalCostAsync_WithValidJobCode_ReturnsTotalCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(1250.75m);
            _fpsAnimalPlanApiClient.GetTotalAnimalCostAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetTotalAnimalCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1250.75m, result.Data);
            await _fpsAnimalPlanApiClient.Received(1).GetTotalAnimalCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.GetTotalAnimalCostAsync("JOB001").Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetTotalAnimalCostAsync("JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAnimalCostViewByIdAsync Tests

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WithValidId_ReturnsAnimalCostView()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";
            var animalCostView = new AnimalCostViewDto
            {
                IndCounter = 1, AnimalType = "Cattle", JobCode = jobCode,
                NumberOfDays = 5, NumberOfAnimals = 10, DailyRate = 25m, AnimalCost = 1250m
            };
            var expectedResponse = ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(animalCostView);
            _fpsAnimalPlanApiClient.GetAnimalCostViewByIdAsync(indCounter, jobCode).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.IndCounter);
            await _fpsAnimalPlanApiClient.Received(1).GetAnimalCostViewByIdAsync(indCounter, jobCode);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WhenNotFound_ReturnsSuccessWithNull()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(null);
            _fpsAnimalPlanApiClient.GetAnimalCostViewByIdAsync(999, "JOB001").Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostViewByIdAsync(999, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<AnimalCostViewDto?>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.GetAnimalCostViewByIdAsync(1, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAnimalCostAsync Tests

        [Fact]
        public async Task CreateAnimalCostAsync_WithValidRequest_ReturnsCreatedDto()
        {
            // Arrange
            var animalRequest = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 5, NumberOfAnimals = 10 };
            var expectedResponse = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequest);
            _fpsAnimalPlanApiClient.CreateAnimalCostAsync(animalRequest).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.CreateAnimalCostAsync(animalRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("JOB001", result.Data?.JobCode);
            await _fpsAnimalPlanApiClient.Received(1).CreateAnimalCostAsync(animalRequest);
        }

        [Fact]
        public async Task CreateAnimalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var animalRequest = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "Cattle" };
            var errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<AnimalRequestDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.CreateAnimalCostAsync(animalRequest).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.CreateAnimalCostAsync(animalRequest);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateAnimalCostAsync Tests

        [Fact]
        public async Task UpdateAnimalCostAsync_WithValidRequest_ReturnsUpdatedDto()
        {
            // Arrange
            var animalRequest = new AnimalRequestDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle", NumberOfDays = 7, NumberOfAnimals = 12 };
            var expectedResponse = ApiResponseDto<AnimalRequestDto>.SuccessResponse(animalRequest);
            _fpsAnimalPlanApiClient.UpdateAnimalCostAsync(animalRequest).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.UpdateAnimalCostAsync(animalRequest);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.IndCounter);
            await _fpsAnimalPlanApiClient.Received(1).UpdateAnimalCostAsync(animalRequest);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var animalRequest = new AnimalRequestDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "Cattle" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<AnimalRequestDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.UpdateAnimalCostAsync(animalRequest).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.UpdateAnimalCostAsync(animalRequest);

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
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsAnimalPlanApiClient.DeleteAnimalCostAsync(indCounter).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.DeleteAnimalCostAsync(indCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsAnimalPlanApiClient.Received(1).DeleteAnimalCostAsync(indCounter);
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsAnimalPlanApiClient.DeleteAnimalCostAsync(999).Returns(expectedResponse);

            // Act
            var result = await _animalPlanService.DeleteAnimalCostAsync(999);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
