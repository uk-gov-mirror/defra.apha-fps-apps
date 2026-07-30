using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkgroupMaintenanceServiceTest
{
    public class WorkgroupMaintenanceServiceTests
    {
        private readonly IPactApiClient _fpsClient;
        private readonly IPactWorkGroupApiClient _fpsWorkgroupApiClient;
        private readonly WorkgroupMaintenanceService _service;

        public WorkgroupMaintenanceServiceTests()
        {
            _fpsClient             = Substitute.For<IPactApiClient>();
            _fpsWorkgroupApiClient = Substitute.For<IPactWorkGroupApiClient>();
            // wire aggregate client → sub-client (IPactApiClient.PactWorkGroup property)
            _fpsClient.PactWorkGroup.Returns(_fpsWorkgroupApiClient);
            _service = new WorkgroupMaintenanceService(_fpsClient);
        }

        // TRANSFORMENGINE: static helpers — minimal valid response wrappers
        private static WorkGroupDto BuildDto(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static ApiResponseDto<WorkGroupDto> BuildSuccessResponse(string name = "WG001") =>
            new() { Success = true, Data = BuildDto(name) };

        private static ApiResponseDto<WorkGroupDto> BuildFailureResponse() =>
            new()
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } }
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupMaintenanceService(null!));
        }

        #endregion

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success    = true,
                Data       = new List<WorkGroupDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _fpsWorkgroupApiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsEmptyPage_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success    = true,
                Data       = new List<WorkGroupDto>(),
                Pagination = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API error", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByWorkGroupNameAsync Tests

        [Fact]
        public async Task GetByWorkGroupNameAsync_ApiClientReturnsSuccess_ReturnsDelegatedResult()
        {
            // Arrange
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.GetByWorkGroupNameAsync("WG001").Returns(expected);

            // Act
            var result = await _service.GetByWorkGroupNameAsync("WG001");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _fpsWorkgroupApiClient.Received(1).GetByWorkGroupNameAsync("WG001");
        }

        [Fact]
        public async Task GetByWorkGroupNameAsync_ApiClientReturnsFailure_ReturnsDelegatedFailure()
        {
            // Arrange
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.GetByWorkGroupNameAsync("NOTEXIST").Returns(expected);

            // Act
            var result = await _service.GetByWorkGroupNameAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.UpdateAsync("WG001", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync("WG001", dto);

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).UpdateAsync("WG001", dto);
        }

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.UpdateAsync("WG001", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync("WG001", dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_VerifyRenamePathPassesOriginalKeyToApiClient()
        {
            // Arrange
            var dto      = BuildDto("WG_RENAMED");
            var expected = BuildSuccessResponse("WG_RENAMED");
            _fpsWorkgroupApiClient.UpdateAsync("WG_ORIGINAL", dto).Returns(expected);

            // Act
            await _service.UpdateAsync("WG_ORIGINAL", dto);

            // Assert
            await _fpsWorkgroupApiClient.Received(1).UpdateAsync("WG_ORIGINAL", dto);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<bool> { Success = true };
            _fpsWorkgroupApiClient.DeleteAsync("WG001").Returns(expected);

            // Act
            var result = await _service.DeleteAsync("WG001");

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).DeleteAsync("WG001");
        }

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to delete", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.DeleteAsync("WG001").Returns(expected);

            // Act
            var result = await _service.DeleteAsync("WG001");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<string>>
            {
                Success = true,
                Data    = new List<string> { "PC01", "PC02" }
            };
            _fpsWorkgroupApiClient.GetProfitCentresAsync().Returns(expected);

            // Act
            var result = await _service.GetProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkgroupApiClient.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load profit centres", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetProfitCentresAsync().Returns(expected);

            // Act
            var result = await _service.GetProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetOwnersAsync Tests

        [Fact]
        public async Task GetOwnersAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<OwnerDto>>
            {
                Success = true,
                Data    = new List<OwnerDto> { new() { Name = "Alice Smith" } }
            };
            _fpsWorkgroupApiClient.GetOwnersAsync().Returns(expected);

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWorkgroupApiClient.Received(1).GetOwnersAsync();
        }

        [Fact]
        public async Task GetOwnersAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<OwnerDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load owners", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetOwnersAsync().Returns(expected);

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<double?>>
            {
                Success = true,
                Data    = new List<double?> { 100.0, 200.0 }
            };
            _fpsWorkgroupApiClient.GetCostCentresAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetCostCentresAsync("PC01");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkgroupApiClient.Received(1).GetCostCentresAsync("PC01");
        }

        [Fact]
        public async Task GetCostCentresAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<double?>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load cost centres", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetCostCentresAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetCostCentresAsync("PC01");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
