using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProfitCentreApiClient _fpsProfitCentreApiClient;
        private readonly ProfitCentreService _sut;

        public ProfitCentreServiceTests()
        {
            _fpsClient                = Substitute.For<IFpsApiClient>();
            _fpsProfitCentreApiClient = Substitute.For<IFpsProfitCentreApiClient>();
            _fpsClient.FpsProfitCentre.Returns(_fpsProfitCentreApiClient);
            _sut = new ProfitCentreService(_fpsClient);
        }

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithSuccessResponse_ReturnsProfitCentreList()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres);

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProfitCentreApiClient.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>());

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithSuccessResponse_ReturnsEnumerable()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreDto>>.SuccessResponse(dtos);

            _fpsProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _fpsProfitCentreApiClient.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetAllProfitCentresAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsApiResponse()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos       = new List<ProfitCentreDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(dtos, pagination);

            _fpsProfitCentreApiClient.GetAllProfitCentresPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.NotNull(result.Pagination);
            await _fpsProfitCentreApiClient.Received(1).GetAllProfitCentresPagedAsync(query);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_PropagatesApiErrors()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetAllProfitCentresPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllProfitCentresPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_PassesFilterAndSortParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "ProfitCentreId", Descending = true,
                Filter = "{\"ProfitCentreId\":\"PC\"}"
            };
            var apiResponse = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetAllProfitCentresPagedAsync(query).Returns(apiResponse);

            // Act
            await _sut.GetAllProfitCentresPagedAsync(query);

            // Assert
            await _fpsProfitCentreApiClient.Received(1).GetAllProfitCentresPagedAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 && q.PageSize == 5 &&
                    q.SortBy == "ProfitCentreId" && q.Descending == true));
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsApiResponse()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _fpsProfitCentreApiClient.GetProfitCentreByIdAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data!.ProfitCentreId);
            await _fpsProfitCentreApiClient.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_PropagatesNotFoundError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetProfitCentreByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _fpsProfitCentreApiClient.CreateProfitCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateProfitCentreAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PC01", result.Data!.ProfitCentreId);
            await _fpsProfitCentreApiClient.Received(1).CreateProfitCentreAsync(dto);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.CreateProfitCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateProfitCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto         = BuildDto("PC01");
            var apiResponse = ApiResponseDto<ProfitCentreDto>.SuccessResponse(dto);

            _fpsProfitCentreApiClient.UpdateProfitCentreAsync("PC01", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsProfitCentreApiClient.Received(1).UpdateProfitCentreAsync("PC01", dto);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProfitCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.UpdateProfitCentreAsync("PC01", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateProfitCentreAsync("PC01", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProfitCentreApiClient.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteProfitCentreAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProfitCentreApiClient.Received(1).DeleteProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "In use", Code = "IN_USE" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.DeleteProfitCentreAsync("PC01").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteProfitCentreAsync("PC01");

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsProfitCentreApiClient.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProfitCentreApiClient.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPagedProfitCenterCostSummaryAsync Tests

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithSuccessResponse_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1000m },
                new() { ProfitCentre = "PC02", Cost = 2000m }
            };
            var pagination = new PaginationDto { TotalRecords = 2, PageNumber = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData, pagination);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result?.Pagination?.TotalRecords);
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(query, 0.0);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthNumber_PassesMonthNumberToApiClient()
        {
            // Arrange
            const double monthNumber = 3.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1500m }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            Assert.True(result.Success);
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<double>(m => m == monthNumber));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithZeroMonthNumber_PassesZeroToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<double>(m => m == 0.0));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
       }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_PassesPaginationParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "Cost",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 5 &&
                    q.SortBy == "Cost" &&
                    q.Descending == true),
                Arg.Any<double>());
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMultiplePages_ReturnsPaginatedData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 2 };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC03", Cost = 3000m },
                new() { ProfitCentre = "PC04", Cost = 4000m }
            };
            var pagination = new PaginationDto { TotalRecords = 10, PageNumber = 2, PageSize = 2 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData, pagination);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(10, result?.Pagination?.TotalRecords);
            Assert.Equal(2, result?.Pagination?.PageNumber);
            Assert.Equal(2, result?.Pagination?.PageSize);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthZero_PassesZeroToApiClient()
        {
            // Arrange
            const double monthNumber = 0.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            Assert.True(result.Success);
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<double>(m => m == 0.0));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMaxMonthNumber_PassesToApiClient()
        {
            // Arrange
            const double monthNumber = 12.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse([]);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, monthNumber);

            // Assert
            Assert.True(result.Success);
            await _fpsProfitCentreApiClient.Received(1).GetPagedProfitCenterCostSummaryAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<double>(m => m == 12.0));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_ReturnsDataWithCorrectCostValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var costData = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1234.56m },
                new() { ProfitCentre = "PC02", Cost = 7890.12m }
            };
            var expectedResponse = ApiResponseDto<List<ProfitCentreCostDto>>.SuccessResponse(costData);

            _fpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync(query, 0.0).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedProfitCenterCostSummaryAsync(query, 0.0);

            // Assert
            Assert.True(result.Success);
            var dataList = result.Data!.ToList();
            Assert.Equal(1234.56m, dataList[0].Cost);
            Assert.Equal(7890.12m, dataList[1].Cost);
        }

        #endregion

        #region GetPagedWgStaffPlanAsync Tests

        private static WgStaffPlanViewDto BuildWgStaffPlanDto(string workGroup = "WG001", string name = "Staff One") =>
            new()
            {
                WorkGroup = workGroup,
                GradeCode = "G1",
                Name = name,
                Manager = "Manager01",
                Program = "PROG01",
                JobCode = "JOB001",
                ProjectStatus = "Active",
                PlannedHours = 40.0,
                Fee = 1000m
            };

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithSuccessResponse_ReturnsStaffPlanList()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffPlanData = new List<WgStaffPlanViewDto>
            {
                BuildWgStaffPlanDto(workGroup, "Staff One"),
                BuildWgStaffPlanDto(workGroup, "Staff Two")
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(staffPlanData, pagination);

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination.TotalRecords);
            await _fpsProfitCentreApiClient.Received(1).GetPagedWgStaffPlanAsync(query, workGroup);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "Failed to retrieve staff plan", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve staff plan", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_PassesCorrectWorkGroupParameter()
        {
            // Arrange
            const string workGroup = "WG-SPECIAL-001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _fpsProfitCentreApiClient.Received(1).GetPagedWgStaffPlanAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<string>(wg => wg == workGroup));
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_PassesCorrectQueryParameters()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                SortBy = "Name",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(new List<WgStaffPlanViewDto>());

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            await _fpsProfitCentreApiClient.Received(1).GetPagedWgStaffPlanAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 &&
                    q.PageSize == 20 &&
                    q.SortBy == "Name" &&
                    q.Descending == true),
                Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_ReturnsDataWithCorrectStaffDetails()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var staffPlanData = new List<WgStaffPlanViewDto>
            {
                new()
                {
                    WorkGroup = workGroup,
                    GradeCode = "G1",
                    Name = "John Doe",
                    Manager = "Manager01",
                    Program = "PROG01",
                    JobCode = "JOB001",
                    ProjectStatus = "Active",
                    PlannedHours = 40.0,
                    Fee = 1500.00m
                },
                new()
                {
                    WorkGroup = workGroup,
                    GradeCode = "G2",
                    Name = "Jane Smith",
                    Manager = "Manager02",
                    Program = "PROG02",
                    JobCode = "JOB002",
                    ProjectStatus = "Pending",
                    PlannedHours = 35.5,
                    Fee = 1250.50m
                }
            };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(staffPlanData);

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.True(result.Success);
            var dataList = result.Data!.ToList();
            Assert.Equal(2, dataList.Count);
            Assert.Equal("John Doe", dataList[0].Name);
            Assert.Equal("G1", dataList[0].GradeCode);
            Assert.Equal(40.0, dataList[0].PlannedHours);
            Assert.Equal(1500.00m, dataList[0].Fee);
            Assert.Equal("Jane Smith", dataList[1].Name);
            Assert.Equal("G2", dataList[1].GradeCode);
            Assert.Equal(35.5, dataList[1].PlannedHours);
            Assert.Equal(1250.50m, dataList[1].Fee);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_WithPaginationData_ReturnsPaginationDetails()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var staffPlanData = new List<WgStaffPlanViewDto> { BuildWgStaffPlanDto(workGroup) };
            var pagination = new PaginationDto
            {
                PageNumber = 2,
                PageSize = 5,
                TotalRecords = 42,
                TotalPages = 9
            };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.SuccessResponse(staffPlanData, pagination);

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination.PageNumber);
            Assert.Equal(5, result.Pagination.PageSize);
            Assert.Equal(42, result.Pagination.TotalRecords);
            Assert.Equal(9, result.Pagination.TotalPages);
        }

        [Fact]
        public async Task GetPagedWgStaffPlanAsync_PropagatesMultipleApiErrors()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "Error 1", Code = "ERROR_1" },
                new() { Message = "Error 2", Code = "ERROR_2" }
            };
            var expectedResponse = ApiResponseDto<List<WgStaffPlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProfitCentreApiClient.GetPagedWgStaffPlanAsync(query, workGroup).Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedWgStaffPlanAsync(query, workGroup);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal("Error 1", result.Errors[0].Message);
            Assert.Equal("Error 2", result.Errors[1].Message);
        }

        #endregion

    }
}
