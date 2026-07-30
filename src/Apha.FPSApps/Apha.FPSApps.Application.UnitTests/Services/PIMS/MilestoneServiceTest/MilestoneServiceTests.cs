using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.MilestoneServiceTest
{
    public class MilestoneServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsMilestoneApiClient _pimsMilestoneApiClient;
        private readonly MilestoneService _sut;

        public MilestoneServiceTests()
        {
            _pimsApiClient          = Substitute.For<IPimsApiClient>();
            _pimsMilestoneApiClient = Substitute.For<IPimsMilestoneApiClient>();
            _pimsApiClient.PimsMilestone.Returns(_pimsMilestoneApiClient);
            _sut = new MilestoneService(_pimsApiClient);
        }

        private static List<ApiErrorDto> OneError(string message = "Error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        #region GetAllMilestonesAsync

        [Fact]
        public async Task GetAllMilestonesAsync_WithSuccessResponse_ReturnsMilestoneList()
        {
            // Arrange
            const string project = "PP001";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1" },
                new() { Project = project, Number = "M2" }
            };
            var expected = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetAllMilestonesAsync(parameters, project).Returns(expected);

            // Act
            var result = await _sut.GetAllMilestonesAsync(parameters, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("M1", result.Data[0].Number);
            await _pimsMilestoneApiClient.Received(1).GetAllMilestonesAsync(parameters, project);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected   = ApiResponseDto<List<MilestoneDto>>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetAllMilestonesAsync(parameters, project).Returns(expected);

            // Act
            var result = await _sut.GetAllMilestonesAsync(parameters, project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_PassesCorrectParametersToClient()
        {
            // Arrange
            const string project = "PP123";
            var parameters = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expected   = ApiResponseDto<List<MilestoneDto>>.SuccessResponse([]);

            _pimsMilestoneApiClient.GetAllMilestonesAsync(parameters, project).Returns(expected);

            // Act
            await _sut.GetAllMilestonesAsync(parameters, project);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetAllMilestonesAsync(
                Arg.Is<QueryParameters<string>>(p => p.Page == 2 && p.PageSize == 5),
                Arg.Is<string>(p => p == project));
        }

        #endregion

        #region GetMilestoneAsync

        [Fact]
        public async Task GetMilestoneAsync_WithSuccessResponse_ReturnsMilestone()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var data     = new MilestoneDto { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetMilestoneAsync(project, number).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(project, result.Data.Project);
            Assert.Equal(number,  result.Data.Number);
            await _pimsMilestoneApiClient.Received(1).GetMilestoneAsync(project, number);
        }

        [Fact]
        public async Task GetMilestoneAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "UNKNOWN";
            var expected = ApiResponseDto<MilestoneDto>.FailureResponse(OneError("Milestone not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetMilestoneAsync(project, number).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetMilestoneAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            const string number  = "M5";
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = number });

            _pimsMilestoneApiClient.GetMilestoneAsync(project, number).Returns(expected);

            // Act
            await _sut.GetMilestoneAsync(project, number);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetMilestoneAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<string>(n => n == number));
        }

        #endregion

        #region SaveMilestoneAsync

        [Fact]
        public async Task SaveMilestoneAsync_WithSuccessResponse_ReturnsSavedMilestone()
        {
            // Arrange
            const string project = "PP001";
            var dto = new MilestoneDto { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(30) };
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.SaveMilestoneAsync(project, dto).Returns(expected);

            // Act
            var result = await _sut.SaveMilestoneAsync(project, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("M1", result.Data.Number);
            await _pimsMilestoneApiClient.Received(1).SaveMilestoneAsync(project, dto);
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var dto      = new MilestoneDto { Project = project, Number = "M1" };
            var expected = ApiResponseDto<MilestoneDto>.FailureResponse(OneError("Validation error", "VALIDATION_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.SaveMilestoneAsync(project, dto).Returns(expected);

            // Act
            var result = await _sut.SaveMilestoneAsync(project, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task SaveMilestoneAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            var dto      = new MilestoneDto { Project = project, Number = "M3", DateDue = DateTime.Today.AddDays(10) };
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.SaveMilestoneAsync(project, dto).Returns(expected);

            // Act
            await _sut.SaveMilestoneAsync(project, dto);

            // Assert
            await _pimsMilestoneApiClient.Received(1).SaveMilestoneAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<MilestoneDto>(d => d.Number == "M3"));
        }

        #endregion

        #region UpdateMilestoneAsync

        [Fact]
        public async Task UpdateMilestoneAsync_WithSuccessResponse_ReturnsUpdatedMilestone()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto      = new MilestoneDto { Project = project, Number = number, Description = "Updated" };
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.UpdateMilestoneAsync(project, number, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateMilestoneAsync(project, number, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated", result.Data.Description);
            await _pimsMilestoneApiClient.Received(1).UpdateMilestoneAsync(project, number, dto);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto      = new MilestoneDto { Project = project, Number = number };
            var expected = ApiResponseDto<MilestoneDto>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.UpdateMilestoneAsync(project, number, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateMilestoneAsync(project, number, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            const string number  = "M7";
            var dto      = new MilestoneDto { Project = project, Number = number, Description = "Test" };
            var expected = ApiResponseDto<MilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.UpdateMilestoneAsync(project, number, dto).Returns(expected);

            // Act
            await _sut.UpdateMilestoneAsync(project, number, dto);

            // Assert
            await _pimsMilestoneApiClient.Received(1).UpdateMilestoneAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<string>(n => n == number),
                Arg.Is<MilestoneDto>(d => d.Description == "Test"));
        }

        #endregion

        #region DeleteMilestoneAsync

        [Fact]
        public async Task DeleteMilestoneAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.DeleteMilestoneAsync(project, number).Returns(expected);

            // Act
            var result = await _sut.DeleteMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).DeleteMilestoneAsync(project, number);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "UNKNOWN";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.DeleteMilestoneAsync(project, number).Returns(expected);

            // Act
            var result = await _sut.DeleteMilestoneAsync(project, number);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            const string number  = "M9";
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.DeleteMilestoneAsync(project, number).Returns(expected);

            // Act
            await _sut.DeleteMilestoneAsync(project, number);

            // Assert
            await _pimsMilestoneApiClient.Received(1).DeleteMilestoneAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<string>(n => n == number));
        }

        #endregion

        #region UpdateFormRequiredAsync

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequiredAsync_WithSuccessResponse_ReturnsSuccess(bool formRequired)
        {
            // Arrange
            const string parent  = "PP001";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.UpdateFormRequiredAsync(parent, formRequired).Returns(expected);

            // Act
            var result = await _sut.UpdateFormRequiredAsync(parent, formRequired);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).UpdateFormRequiredAsync(parent, formRequired);
        }

        [Fact]
        public async Task UpdateFormRequiredAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string parent  = "PP001";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.UpdateFormRequiredAsync(parent, true).Returns(expected);

            // Act
            var result = await _sut.UpdateFormRequiredAsync(parent, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateFormRequiredAsync_PassesCorrectParameters()
        {
            // Arrange
            const string parent = "PP123";
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.UpdateFormRequiredAsync(parent, true).Returns(expected);

            // Act
            await _sut.UpdateFormRequiredAsync(parent, true);

            // Assert
            await _pimsMilestoneApiClient.Received(1).UpdateFormRequiredAsync(
                Arg.Is<string>(p => p == parent),
                Arg.Is<bool>(f => f == true));
        }

        #endregion

        #region GetMilestoneTypesAsync

        [Fact]
        public async Task GetMilestoneTypesAsync_WithSuccessResponseAndNoFilter_ReturnsAllTypes()
        {
            // Arrange
            var data = new List<MilestoneTypeDto>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };
            var expected = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetMilestoneTypesAsync(null).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _pimsMilestoneApiClient.Received(1).GetMilestoneTypesAsync(null);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_WithFilter_PassesFilterToClient()
        {
            // Arrange
            const string filter = "M";
            var data     = new List<MilestoneTypeDto> { new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' } };
            var expected = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetMilestoneTypesAsync(filter).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneTypesAsync(filter);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsMilestoneApiClient.Received(1).GetMilestoneTypesAsync(filter);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<MilestoneTypeDto>>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetMilestoneTypesAsync(null).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region GetAllMilestoneFormDatesAsync

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithSuccessResponse_ReturnsFormDatesList()
        {
            // Arrange
            const string parent    = "PP001";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<MilestoneFormDatesDto>
            {
                new() { Year = 2024, ParentProject = parent },
                new() { Year = 2023, ParentProject = parent }
            };
            var expected = ApiResponseDto<List<MilestoneFormDatesDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetAllMilestoneFormDatesAsync(parent, parameters).Returns(expected);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal((short)2024, result.Data[0].Year);
            await _pimsMilestoneApiClient.Received(1).GetAllMilestoneFormDatesAsync(parent, parameters);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string parent    = "PP001";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected   = ApiResponseDto<List<MilestoneFormDatesDto>>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetAllMilestoneFormDatesAsync(parent, parameters).Returns(expected);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_PassesCorrectParameters()
        {
            // Arrange
            const string parent    = "PP123";
            var parameters = new QueryParameters<string> { Page = 3, PageSize = 20 };
            var expected   = ApiResponseDto<List<MilestoneFormDatesDto>>.SuccessResponse([]);

            _pimsMilestoneApiClient.GetAllMilestoneFormDatesAsync(parent, parameters).Returns(expected);

            // Act
            await _sut.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetAllMilestoneFormDatesAsync(
                Arg.Is<string>(p => p == parent),
                Arg.Is<QueryParameters<string>>(q => q.Page == 3 && q.PageSize == 20));
        }

        #endregion

        #region GetMilestoneFormDatesAsync

        [Fact]
        public async Task GetMilestoneFormDatesAsync_WithSuccessResponse_ReturnsFormDates()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var data     = new MilestoneFormDatesDto { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var expected = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(year,   result.Data.Year);
            Assert.Equal(parent, result.Data.ParentProject);
            Assert.Equal(new DateTime(2024, 1, 31), result.Data.Jan);
            await _pimsMilestoneApiClient.Received(1).GetMilestoneFormDatesAsync(parent, year);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var expected = ApiResponseDto<MilestoneFormDatesDto>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_PassesCorrectParameters()
        {
            // Arrange
            const string parent = "PP123";
            const short  year   = 2023;
            var expected = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(new MilestoneFormDatesDto { Year = year, ParentProject = parent });

            _pimsMilestoneApiClient.GetMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            await _sut.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetMilestoneFormDatesAsync(
                Arg.Is<string>(p => p == parent),
                Arg.Is<short>(y => y == year));
        }

        #endregion

        #region SaveMilestoneFormDatesAsync

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_WithSuccessResponse_ReturnsSavedFormDates()
        {
            // Arrange
            const string parent = "PP001";
            var dto      = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var expected = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.SaveMilestoneFormDatesAsync(parent, dto).Returns(expected);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal((short)2024, result.Data.Year);
            Assert.Equal(parent, result.Data.ParentProject);
            await _pimsMilestoneApiClient.Received(1).SaveMilestoneFormDatesAsync(parent, dto);
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            var dto      = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent };
            var expected = ApiResponseDto<MilestoneFormDatesDto>.FailureResponse(OneError("Validation error", "VALIDATION_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.SaveMilestoneFormDatesAsync(parent, dto).Returns(expected);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_PassesCorrectParameters()
        {
            // Arrange
            const string parent = "PP123";
            var dto      = new MilestoneFormDatesDto { Year = 2025, ParentProject = parent, Feb = new DateTime(2025, 2, 28) };
            var expected = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.SaveMilestoneFormDatesAsync(parent, dto).Returns(expected);

            // Act
            await _sut.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            await _pimsMilestoneApiClient.Received(1).SaveMilestoneFormDatesAsync(
                Arg.Is<string>(p => p == parent),
                Arg.Is<MilestoneFormDatesDto>(d => d.Year == 2025 && d.Feb == new DateTime(2025, 2, 28)));
        }

        #endregion

        #region DeleteMilestoneFormDatesAsync

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.DeleteMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).DeleteMilestoneFormDatesAsync(parent, year);
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 9999;
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.DeleteMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_PassesCorrectParameters()
        {
            // Arrange
            const string parent = "PP123";
            const short  year   = 2022;
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.DeleteMilestoneFormDatesAsync(parent, year).Returns(expected);

            // Act
            await _sut.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            await _pimsMilestoneApiClient.Received(1).DeleteMilestoneFormDatesAsync(
                Arg.Is<string>(p => p == parent),
                Arg.Is<short>(y => y == year));
        }

        #endregion

        #region GetLogMilestonesAsync

        [Fact]
        public async Task GetLogMilestonesAsync_WithSuccessResponse_ReturnsLogMilestoneList()
        {
            // Arrange
            const string project     = "PP001";
            const string numberPart1 = "M";
            const string numberPart2 = "1";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<LogMilestoneDto>
            {
                new() { Project = project, Number = "M1", Description = "Log Entry 1" },
                new() { Project = project, Number = "M2", Description = "Log Entry 2" }
            };
            var expected = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2).Returns(expected);

            // Act
            var result = await _sut.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("M1", result.Data[0].Number);
            await _pimsMilestoneApiClient.Received(1).GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected   = ApiResponseDto<List<LogMilestoneDto>>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetLogMilestonesAsync(parameters, project, null, null).Returns(expected);

            // Act
            var result = await _sut.GetLogMilestonesAsync(parameters, project, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_PassesCorrectParametersToClient()
        {
            // Arrange
            const string project     = "PP123";
            const string numberPart1 = "M";
            const string numberPart2 = "5";
            var parameters = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expected   = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse([]);

            _pimsMilestoneApiClient.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2).Returns(expected);

            // Act
            await _sut.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetLogMilestonesAsync(
                Arg.Is<QueryParameters<string>>(p => p.Page == 2 && p.PageSize == 5),
                Arg.Is<string?>(p => p == project),
                Arg.Is<string?>(n => n == numberPart1),
                Arg.Is<string?>(n => n == numberPart2));
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WithNullOptionalParameters_PassesNullsToClient()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected   = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse([]);

            _pimsMilestoneApiClient.GetLogMilestonesAsync(parameters, null, null, null).Returns(expected);

            // Act
            var result = await _sut.GetLogMilestonesAsync(parameters, null, null, null);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).GetLogMilestonesAsync(
                parameters,
                Arg.Is<string?>(p => p == null),
                Arg.Is<string?>(n => n == null),
                Arg.Is<string?>(n => n == null));
        }

        #endregion

        #region GetAllStagingRowsAsync

        [Fact]
        public async Task GetAllStagingRowsAsync_WithSuccessResponse_ReturnsStagingRows()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<StagingMilestoneDto>
            {
                new() { Id = 1, Project = "PP001", Number = "M1" },
                new() { Id = 2, Project = "PP001", Number = "M2" }
            };
            var expected = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetAllStagingRowsAsync(parameters).Returns(expected);

            // Act
            var result = await _sut.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _pimsMilestoneApiClient.Received(1).GetAllStagingRowsAsync(parameters);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<StagingMilestoneDto>>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetAllStagingRowsAsync(parameters).Returns(expected);

            // Act
            var result = await _sut.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_PassesCorrectParametersToClient()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 3, PageSize = 25 };
            var expected = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse([]);

            _pimsMilestoneApiClient.GetAllStagingRowsAsync(parameters).Returns(expected);

            // Act
            await _sut.GetAllStagingRowsAsync(parameters);

            // Assert
            await _pimsMilestoneApiClient.Received(1).GetAllStagingRowsAsync(
                Arg.Is<QueryParameters<string>>(p => p.Page == 3 && p.PageSize == 25));
        }

        #endregion

        #region GetStagingRowsAsync

        [Fact]
        public async Task GetStagingRowsAsync_WithId_ReturnsRows()
        {
            // Arrange
            const int id = 1;
            var data = new List<StagingMilestoneDto> { new() { Id = id, Project = "PP001", Number = "M1" } };
            var expected = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(data);

            _pimsMilestoneApiClient.GetStagingRowsAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetStagingRowsAsync(id);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsMilestoneApiClient.Received(1).GetStagingRowsAsync(id);
        }

        [Fact]
        public async Task GetStagingRowsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 1;
            var expected = ApiResponseDto<List<StagingMilestoneDto>>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.GetStagingRowsAsync(id).Returns(expected);

            // Act
            var result = await _sut.GetStagingRowsAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion

        #region AddStagingRowAsync

        [Fact]
        public async Task AddStagingRowAsync_WithSuccessResponse_ReturnsRow()
        {
            // Arrange
            const int year = 2025;
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1", Description = "Added" };
            var expected = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.AddStagingRowAsync(dto, year).Returns(expected);

            // Act
            var result = await _sut.AddStagingRowAsync(dto, year);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Added", result.Data!.Description);
            await _pimsMilestoneApiClient.Received(1).AddStagingRowAsync(dto, year);
        }

        [Fact]
        public async Task AddStagingRowAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int year = 2025;
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            var expected = ApiResponseDto<StagingMilestoneDto>.FailureResponse(OneError("Validation error", "VALIDATION_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.AddStagingRowAsync(dto, year).Returns(expected);

            // Act
            var result = await _sut.AddStagingRowAsync(dto, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task AddStagingRowAsync_PassesCorrectParameters()
        {
            // Arrange
            const int year = 2026;
            var dto = new StagingMilestoneDto { Project = "PP123", Number = "M3" };
            var expected = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.AddStagingRowAsync(dto, year).Returns(expected);

            // Act
            await _sut.AddStagingRowAsync(dto, year);

            // Assert
            await _pimsMilestoneApiClient.Received(1).AddStagingRowAsync(
                Arg.Is<StagingMilestoneDto>(d => d.Project == "PP123" && d.Number == "M3"),
                Arg.Is<int>(y => y == year));
        }

        #endregion

        #region UpdateStagingRowAsync

        [Fact]
        public async Task UpdateStagingRowAsync_WithSuccessResponse_ReturnsUpdatedRow()
        {
            // Arrange
            const int id = 12;
            var dto = new StagingMilestoneDto { Id = id, Project = "PP001", Description = "Updated" };
            var expected = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.UpdateStagingRowAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateStagingRowAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated", result.Data!.Description);
            await _pimsMilestoneApiClient.Received(1).UpdateStagingRowAsync(id, dto);
        }

        [Fact]
        public async Task UpdateStagingRowAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 12;
            var dto = new StagingMilestoneDto { Id = id, Project = "PP001" };
            var expected = ApiResponseDto<StagingMilestoneDto>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.UpdateStagingRowAsync(id, dto).Returns(expected);

            // Act
            var result = await _sut.UpdateStagingRowAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateStagingRowAsync_PassesCorrectParameters()
        {
            // Arrange
            const int id = 33;
            var dto = new StagingMilestoneDto { Id = id, Project = "PP123", Number = "M7" };
            var expected = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(dto);

            _pimsMilestoneApiClient.UpdateStagingRowAsync(id, dto).Returns(expected);

            // Act
            await _sut.UpdateStagingRowAsync(id, dto);

            // Assert
            await _pimsMilestoneApiClient.Received(1).UpdateStagingRowAsync(
                Arg.Is<int>(i => i == id),
                Arg.Is<StagingMilestoneDto>(d => d.Project == "PP123" && d.Number == "M7"));
        }

        #endregion

        #region DeleteStagingRowAsync

        [Fact]
        public async Task DeleteStagingRowAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const int id = 10;
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.DeleteStagingRowAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteStagingRowAsync(id);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).DeleteStagingRowAsync(id);
        }

        [Fact]
        public async Task DeleteStagingRowAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 10;
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Not found", "NOT_FOUND"), new ApiMetaDto());

            _pimsMilestoneApiClient.DeleteStagingRowAsync(id).Returns(expected);

            // Act
            var result = await _sut.DeleteStagingRowAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteStagingRowAsync_PassesCorrectParameters()
        {
            // Arrange
            const int id = 55;
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.DeleteStagingRowAsync(id).Returns(expected);

            // Act
            await _sut.DeleteStagingRowAsync(id);

            // Assert
            await _pimsMilestoneApiClient.Received(1).DeleteStagingRowAsync(Arg.Is<int>(i => i == id));
        }

        #endregion

        #region ClearStagingAsync

        [Fact]
        public async Task ClearStagingAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.ClearStagingAsync(project).Returns(expected);

            // Act
            var result = await _sut.ClearStagingAsync(project);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).ClearStagingAsync(project);
        }

        [Fact]
        public async Task ClearStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.ClearStagingAsync(project).Returns(expected);

            // Act
            var result = await _sut.ClearStagingAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task ClearStagingAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.ClearStagingAsync(project).Returns(expected);

            // Act
            await _sut.ClearStagingAsync(project);

            // Assert
            await _pimsMilestoneApiClient.Received(1).ClearStagingAsync(Arg.Is<string>(p => p == project));
        }

        #endregion

        #region ValidateStagingAsync

        [Fact]
        public async Task ValidateStagingAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            const string typeId = "M";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.ValidateStagingAsync(project, typeId, true).Returns(expected);

            // Act
            var result = await _sut.ValidateStagingAsync(project, typeId, true);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).ValidateStagingAsync(project, typeId, true);
        }

        [Fact]
        public async Task ValidateStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Validation error", "VALIDATION_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.ValidateStagingAsync(project, null, false).Returns(expected);

            // Act
            var result = await _sut.ValidateStagingAsync(project, null, false);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task ValidateStagingAsync_WithNullTypeId_PassesNullToClient()
        {
            // Arrange
            const string project = "PP123";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.ValidateStagingAsync(project, null, false).Returns(expected);

            // Act
            await _sut.ValidateStagingAsync(project, null, false);

            // Assert
            await _pimsMilestoneApiClient.Received(1).ValidateStagingAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<string?>(t => t == null),
                Arg.Is<bool>(m => m == false));
        }

        #endregion

        #region ImportStagingAsync

        [Fact]
        public async Task ImportStagingAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.ImportStagingAsync(project).Returns(expected);

            // Act
            var result = await _sut.ImportStagingAsync(project);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).ImportStagingAsync(project);
        }

        [Fact]
        public async Task ImportStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.ImportStagingAsync(project).Returns(expected);

            // Act
            var result = await _sut.ImportStagingAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task ImportStagingAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.ImportStagingAsync(project).Returns(expected);

            // Act
            await _sut.ImportStagingAsync(project);

            // Assert
            await _pimsMilestoneApiClient.Received(1).ImportStagingAsync(Arg.Is<string>(p => p == project));
        }

        #endregion

        #region ImportWithOverwriteAsync

        [Fact]
        public async Task ImportWithOverwriteAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _pimsMilestoneApiClient.ImportWithOverwriteAsync(project).Returns(expected);

            // Act
            var result = await _sut.ImportWithOverwriteAsync(project);

            // Assert
            Assert.True(result.Success);
            await _pimsMilestoneApiClient.Received(1).ImportWithOverwriteAsync(project);
        }

        [Fact]
        public async Task ImportWithOverwriteAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var expected = ApiResponseDto<object>.FailureResponse(OneError("Server error", "SERVER_ERROR"), new ApiMetaDto());

            _pimsMilestoneApiClient.ImportWithOverwriteAsync(project).Returns(expected);

            // Act
            var result = await _sut.ImportWithOverwriteAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task ImportWithOverwriteAsync_PassesCorrectParameters()
        {
            // Arrange
            const string project = "PP123";
            var expected = ApiResponseDto<object>.SuccessResponse(new object());

            _pimsMilestoneApiClient.ImportWithOverwriteAsync(project).Returns(expected);

            // Act
            await _sut.ImportWithOverwriteAsync(project);

            // Assert
            await _pimsMilestoneApiClient.Received(1).ImportWithOverwriteAsync(Arg.Is<string>(p => p == project));
        }

        #endregion
    }
}
