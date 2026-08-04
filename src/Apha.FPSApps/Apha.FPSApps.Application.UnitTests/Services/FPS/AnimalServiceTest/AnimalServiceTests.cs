using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.AnimalServiceTest
{
    public class AnimalServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsAnimalApiClient _mockApiClient;
        private readonly AnimalService _sut;

        public AnimalServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockApiClient = Substitute.For<IFpsAnimalApiClient>();
            _mockFpsClient.FpsAnimalMaster.Returns(_mockApiClient);
            _sut = new AnimalService(_mockFpsClient);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFpsClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimalService(null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged) Tests

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsApiResponse()
        {
            var dtos = new List<AnimalDto> { BuildDto() };
            var response = ApiResponseDto<IEnumerable<AnimalDto>>.SuccessResponse(dtos);
            _mockApiClient.GetAllAnimalsAsync().Returns(response);

            var result = await _sut.GetAllAnimalsAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockApiClient.Received(1).GetAllAnimalsAsync();
        }

        [Fact]
        public async Task GetAllAnimalsAsync_PropagatesApiErrors()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var response = ApiResponseDto<IEnumerable<AnimalDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAllAnimalsAsync().Returns(response);

            var result = await _sut.GetAllAnimalsAsync();

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAllAnimalsAsync (paged) Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsApiResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<AnimalDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var response = ApiResponseDto<List<AnimalDto>>.SuccessResponse(dtos, pagination);
            _mockApiClient.GetAllAnimalsAsync(query).Returns(response);

            var result = await _sut.GetAllAnimalsAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockApiClient.Received(1).GetAllAnimalsAsync(query);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_PropagatesApiErrors()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var response = ApiResponseDto<List<AnimalDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAllAnimalsAsync(query).Returns(response);

            var result = await _sut.GetAllAnimalsAsync(query);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_PassesFilterAndSortParameters()
        {
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "AnimalType", Descending = true,
                Filter = "{\"AnimalType\":\"CAT\"}"
            };
            var response = ApiResponseDto<List<AnimalDto>>.SuccessResponse([]);
            _mockApiClient.GetAllAnimalsAsync(query).Returns(response);

            await _sut.GetAllAnimalsAsync(query);

            await _mockApiClient.Received(1).GetAllAnimalsAsync(Arg.Is<QueryParameters<string>>(
                q => q.Page == 2 && q.PageSize == 5 && q.SortBy == "AnimalType" && q.Descending == true));
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsDto_WhenFound()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<AnimalDto?>.SuccessResponse(dto);
            _mockApiClient.GetAnimalByIdAsync("CATTLE").Returns(response);

            var result = await _sut.GetAnimalByIdAsync("CATTLE");

            Assert.True(result.Success);
            Assert.Equal("CATTLE", result.Data!.AnimalType);
            await _mockApiClient.Received(1).GetAnimalByIdAsync("CATTLE");
        }

        [Fact]
        public async Task GetAnimalByIdAsync_PropagatesApiErrors_WhenNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<AnimalDto?>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAnimalByIdAsync("NOTEXIST").Returns(response);

            var result = await _sut.GetAnimalByIdAsync("NOTEXIST");

            Assert.False(result.Success);
        }

        #endregion

        #region AddAnimalAsync Tests

        [Fact]
        public async Task AddAnimalAsync_ReturnsSuccessResponse_WhenCreated()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<AnimalDto>.SuccessResponse(dto);
            _mockApiClient.AddAnimalAsync(dto).Returns(response);

            var result = await _sut.AddAnimalAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("CATTLE", result.Data!.AnimalType);
            await _mockApiClient.Received(1).AddAnimalAsync(dto);
        }

        [Fact]
        public async Task AddAnimalAsync_PropagatesApiErrors()
        {
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "400" } };
            var response = ApiResponseDto<AnimalDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.AddAnimalAsync(dto).Returns(response);

            var result = await _sut.AddAnimalAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAnimalAsync Tests

        [Fact]
        public async Task UpdateAnimalAsync_ReturnsSuccessResponse_WhenUpdated()
        {
            var dto = BuildDto();
            var response = ApiResponseDto<AnimalDto>.SuccessResponse(dto);
            _mockApiClient.UpdateAnimalAsync(dto).Returns(response);

            var result = await _sut.UpdateAnimalAsync(dto);

            Assert.True(result.Success);
            await _mockApiClient.Received(1).UpdateAnimalAsync(dto);
        }

        [Fact]
        public async Task UpdateAnimalAsync_PropagatesApiErrors()
        {
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<AnimalDto>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.UpdateAnimalAsync(dto).Returns(response);

            var result = await _sut.UpdateAnimalAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAnimalAsync Tests

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsTrue_WhenDeleted()
        {
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _mockApiClient.DeleteAnimalAsync("CATTLE").Returns(response);

            var result = await _sut.DeleteAnimalAsync("CATTLE");

            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockApiClient.Received(1).DeleteAnimalAsync("CATTLE");
        }

        [Fact]
        public async Task DeleteAnimalAsync_PropagatesApiErrors_WhenNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "404" } };
            var response = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.DeleteAnimalAsync("NOTEXIST").Returns(response);

            var result = await _sut.DeleteAnimalAsync("NOTEXIST");

            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalSnapshotAsync Tests

        private static AnimalSnapshotViewDto BuildSnapshotDto(string animalType = "CATTLE") =>
            new()
            {
                Directorate = "Dir",
                Program = "PRG",
                Contract = "C1",
                Project = "P1",
                ProjectStatus = "Approved",
                Species = "Bovine",
                SecurityLevel = "L1",
                AnimalType = animalType,
                DailyRate = 50m,
                JobCode = "JOB001",
                NumberOfDays = 5,
                NumberOfAnimals = 3,
                Cost = 750m
            };

        [Fact]
        public async Task GetAnimalSnapshotAsync_ReturnsApiResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<AnimalSnapshotViewDto> { BuildSnapshotDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var response = ApiResponseDto<List<AnimalSnapshotViewDto>>.SuccessResponse(dtos, pagination);
            _mockApiClient.GetAnimalSnapshotAsync(query).Returns(response);

            var result = await _sut.GetAnimalSnapshotAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockApiClient.Received(1).GetAnimalSnapshotAsync(query);
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_PropagatesApiErrors()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var response = ApiResponseDto<List<AnimalSnapshotViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockApiClient.GetAnimalSnapshotAsync(query).Returns(response);

            var result = await _sut.GetAnimalSnapshotAsync(query);

            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_PassesFilterAndSortParameters()
        {
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "Cost", Descending = true,
                Filter = "{\"AnimalType\":\"CAT\"}"
            };
            var response = ApiResponseDto<List<AnimalSnapshotViewDto>>.SuccessResponse([]);
            _mockApiClient.GetAnimalSnapshotAsync(query).Returns(response);

            await _sut.GetAnimalSnapshotAsync(query);

            await _mockApiClient.Received(1).GetAnimalSnapshotAsync(Arg.Is<QueryParameters<string>>(
                q => q.Page == 2 && q.PageSize == 5 && q.SortBy == "Cost" && q.Descending == true));
        }

        #endregion
    }
}
