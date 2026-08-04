using Apha.Common.Contracts;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsAnimalApiClientTest
{
    public class FpsAnimalApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsAnimalApiClient _client;

        public FpsAnimalApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsAnimalApiClient(_http, _mapper);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new()
            {
                Success = false,
                Errors = [new ApiError { Message = "Error", Code = "ERROR" }]
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsAnimalApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsAnimalApiClient(_http, null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged) Tests

        [Fact]
        public async Task GetAllAnimalsAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var dtos = new List<AnimalDto> { BuildDto() };
            var apiResponse = SuccessApiResponse<IEnumerable<AnimalDto>>(dtos);
            var expected = ApiResponseDto<IEnumerable<AnimalDto>>.SuccessResponse(dtos);

            _http.GetAsync<IEnumerable<AnimalDto>>(FpsApiEndpoints.GetAllAnimalMasters).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<AnimalDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetAllAnimalsAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<IEnumerable<AnimalDto>>(FpsApiEndpoints.GetAllAnimalMasters);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<IEnumerable<AnimalDto>>();
            var failDto = new ApiResponseDto<IEnumerable<AnimalDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<AnimalDto>>(FpsApiEndpoints.GetAllAnimalMasters).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<AnimalDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetAllAnimalsAsync();

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region GetAllAnimalsAsync (paged) Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<AnimalDto> { BuildDto() };
            var apiResponse = SuccessApiResponse(dtos);
            var expected = ApiResponseDto<List<AnimalDto>>.SuccessResponse(dtos,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<AnimalDto>>(Arg.Is<string>(u => u.Contains("animal/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetAllAnimalsAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<AnimalDto>>();
            var failDto = new ApiResponseDto<List<AnimalDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalDto>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetAllAnimalsAsync(query);

            Assert.False(result.Success);
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse<AnimalDto>(dto);
            var expected = ApiResponseDto<AnimalDto?>.SuccessResponse(dto);

            _http.GetAsync<AnimalDto>(Arg.Is<string>(u => u.Contains("animal/CATTLE")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto?>>(apiResponse).Returns(expected);

            var result = await _client.GetAnimalByIdAsync("CATTLE");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("CATTLE", result.Data!.AnimalType);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<AnimalDto>();
            var failDto = new ApiResponseDto<AnimalDto?>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "404" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<AnimalDto>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto?>>(apiResponse).Returns(failDto);

            var result = await _client.GetAnimalByIdAsync("NOTEXIST");

            Assert.False(result.Success);
        }

        #endregion

        #region AddAnimalAsync Tests

        [Fact]
        public async Task AddAnimalAsync_WithSuccessResponse_ReturnsCreatedDto()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse(dto);
            var expected = ApiResponseDto<AnimalDto>.SuccessResponse(dto);

            _http.PostAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.CreateAnimalMaster, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto>>(apiResponse).Returns(expected);

            var result = await _client.AddAnimalAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("CATTLE", result.Data!.AnimalType);
            await _http.Received(1).PostAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.CreateAnimalMaster, dto);
        }

        [Fact]
        public async Task AddAnimalAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = BuildDto();
            var apiResponse = FailureApiResponse<AnimalDto>();
            var failDto = new ApiResponseDto<AnimalDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Validation error", Code = "400" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<AnimalDto, AnimalDto>(Arg.Any<string>(), Arg.Any<AnimalDto>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto>>(apiResponse).Returns(failDto);

            var result = await _client.AddAnimalAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAnimalAsync Tests

        [Fact]
        public async Task UpdateAnimalAsync_WithSuccessResponse_ReturnsUpdatedDto()
        {
            var dto = BuildDto();
            var apiResponse = SuccessApiResponse(dto);
            var expected = ApiResponseDto<AnimalDto>.SuccessResponse(dto);

            _http.PutAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.UpdateAnimalMaster, dto).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto>>(apiResponse).Returns(expected);

            var result = await _client.UpdateAnimalAsync(dto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.UpdateAnimalMaster, dto);
        }

        [Fact]
        public async Task UpdateAnimalAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var dto = BuildDto();
            var apiResponse = FailureApiResponse<AnimalDto>();
            var failDto = new ApiResponseDto<AnimalDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "404" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<AnimalDto, AnimalDto>(Arg.Any<string>(), Arg.Any<AnimalDto>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<AnimalDto>>(apiResponse).Returns(failDto);

            var result = await _client.UpdateAnimalAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAnimalAsync Tests

        [Fact]
        public async Task DeleteAnimalAsync_WithSuccessResponse_ReturnsTrue()
        {
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains("animal/CATTLE"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            var result = await _client.DeleteAnimalAsync("CATTLE");

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteAnimalAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = FailureApiResponse<bool?>();
            var failDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "404" }],
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failDto);

            var result = await _client.DeleteAnimalAsync("NOTEXIST");

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
        public async Task GetAnimalSnapshotAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<AnimalSnapshotViewDto> { BuildSnapshotDto() };
            var apiResponse = SuccessApiResponse(dtos);
            var expected = ApiResponseDto<List<AnimalSnapshotViewDto>>.SuccessResponse(dtos,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<AnimalSnapshotViewDto>>(Arg.Is<string>(u => u.Contains(FpsApiEndpoints.GetAnimalSnapshot)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalSnapshotViewDto>>>(apiResponse).Returns(expected);

            var result = await _client.GetAnimalSnapshotAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<AnimalSnapshotViewDto>>(
                Arg.Is<string>(u => u.Contains(FpsApiEndpoints.GetAnimalSnapshot)));
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<AnimalSnapshotViewDto>>();
            var failDto = new ApiResponseDto<List<AnimalSnapshotViewDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalSnapshotViewDto>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AnimalSnapshotViewDto>>>(apiResponse).Returns(failDto);

            var result = await _client.GetAnimalSnapshotAsync(query);

            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion
    }
}
