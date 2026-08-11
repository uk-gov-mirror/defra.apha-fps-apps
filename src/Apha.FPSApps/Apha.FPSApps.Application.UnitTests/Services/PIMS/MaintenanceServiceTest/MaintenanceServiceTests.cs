using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.MaintenanceServiceTest
{
    public class MaintenanceServiceTests
    {
        
        private readonly IPimsApiClient _pimsClient;
        private readonly IPimsReportApiClient _pimsReportApiClient;
        private readonly IPimsReportGroupApiClient _pimsReportGroupApiClient;
        private readonly IPimsReportGroupLinkApiClient _pimsReportGroupLinkApiClient;
        private readonly IPimsProjectManagerApiClient _pimsProjectManagerApiClient;
        private readonly IPimsProgramManagerLinkApiClient _pimsProgramManagerLinkApiClient;
        private readonly IPimsProfitCentreManagerLinkApiClient _pimsProfitCentreManagerLinkApiClient;
        private readonly IPimsSettingApiClient _pimsSettingApiClient;
        private readonly IPimsAccessUserApiClient _pimsAccessUserApiClient;
        private readonly IPimsAccessLevelApiClient _pimsAccessLevelApiClient;
        private readonly IPimsAccessUserLevelApiClient _pimsAccessUserLevelApiClient;
        private readonly IPimsAccessSystemApiClient _pimsAccessSystemApiClient;
        private readonly IPimsFrequencyApiClient _pimsFrequencyApiClient;
        private readonly IPimsReviewItemApiClient _pimsReviewItemApiClient;
        private readonly IPimsRadTrackProgApiClient _pimsRadTrackProgApiClient;
        private readonly IPimsRiskApiClient _pimsRiskApiClient;
        private readonly IPimsPublicationTypeApiClient _pimsPublicationTypeApiClient;
        private readonly MaintenanceService _service;

        public MaintenanceServiceTests()
        {
            _pimsClient                        = Substitute.For<IPimsApiClient>();
            _pimsReportApiClient               = Substitute.For<IPimsReportApiClient>();
            _pimsReportGroupApiClient          = Substitute.For<IPimsReportGroupApiClient>();
            _pimsReportGroupLinkApiClient      = Substitute.For<IPimsReportGroupLinkApiClient>();
            _pimsProjectManagerApiClient       = Substitute.For<IPimsProjectManagerApiClient>();
            _pimsProgramManagerLinkApiClient   = Substitute.For<IPimsProgramManagerLinkApiClient>();
            _pimsProfitCentreManagerLinkApiClient = Substitute.For<IPimsProfitCentreManagerLinkApiClient>();
            _pimsSettingApiClient              = Substitute.For<IPimsSettingApiClient>();
            _pimsAccessUserApiClient           = Substitute.For<IPimsAccessUserApiClient>();
            _pimsAccessLevelApiClient          = Substitute.For<IPimsAccessLevelApiClient>();
            _pimsAccessUserLevelApiClient      = Substitute.For<IPimsAccessUserLevelApiClient>();
            _pimsAccessSystemApiClient         = Substitute.For<IPimsAccessSystemApiClient>();
            _pimsFrequencyApiClient            = Substitute.For<IPimsFrequencyApiClient>();
            _pimsReviewItemApiClient           = Substitute.For<IPimsReviewItemApiClient>();
            _pimsRadTrackProgApiClient         = Substitute.For<IPimsRadTrackProgApiClient>();
            _pimsRiskApiClient                 = Substitute.For<IPimsRiskApiClient>();
            _pimsPublicationTypeApiClient      = Substitute.For<IPimsPublicationTypeApiClient>();

            
            _pimsClient.PimsReport.Returns(_pimsReportApiClient);
            _pimsClient.PimsReportGroup.Returns(_pimsReportGroupApiClient);
            _pimsClient.PimsReportGroupLink.Returns(_pimsReportGroupLinkApiClient);
            _pimsClient.PimsProjectManager.Returns(_pimsProjectManagerApiClient);
            _pimsClient.PimsProgramManagerLink.Returns(_pimsProgramManagerLinkApiClient);
            _pimsClient.PimsProfitCentreManagerLink.Returns(_pimsProfitCentreManagerLinkApiClient);
            _pimsClient.PimsSetting.Returns(_pimsSettingApiClient);
            _pimsClient.PimsAccessUser.Returns(_pimsAccessUserApiClient);
            _pimsClient.PimsAccessLevel.Returns(_pimsAccessLevelApiClient);
            _pimsClient.PimsAccessUserLevel.Returns(_pimsAccessUserLevelApiClient);
            _pimsClient.PimsAccessSystem.Returns(_pimsAccessSystemApiClient);
            _pimsClient.PimsFrequency.Returns(_pimsFrequencyApiClient);
            _pimsClient.PimsReviewItem.Returns(_pimsReviewItemApiClient);
            _pimsClient.PimsRadTrackProg.Returns(_pimsRadTrackProgApiClient);
            _pimsClient.PimsRisk.Returns(_pimsRiskApiClient);
            _pimsClient.PimsPublicationType.Returns(_pimsPublicationTypeApiClient);

            _service = new MaintenanceService(_pimsClient);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static List<ApiErrorDto> OneError(string code = "ERR", string message = "Error") =>
            new List<ApiErrorDto> { new ApiErrorDto { Code = code, Message = message } };

        private static ApiResponseDto<T> SuccessDto<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(OneError(), new ApiMetaDto());

        // ── Report surface ────────────────────────────────────────────────────────

        #region Report

        [Fact]
        public async Task GetAllReportsAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(new List<ReportDto> { new() { ReportName = "R1" } });
            _pimsReportApiClient.GetAllReportsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportApiClient.Received(1).GetAllReportsAsync();
        }

        [Fact]
        public async Task GetAllReportsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.GetAllReportsAsync().Returns(FailureDto<List<ReportDto>>());

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportByIdAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { Id = 5, ReportName = "R5" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.GetReportByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReportByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Id);
            await _pimsReportApiClient.Received(1).GetReportByIdAsync(5);
        }

        [Fact]
        public async Task GetReportByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.GetReportByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReportDto>());

            // Act
            var result = await _service.GetReportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { ReportName = "New", Type = "R" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.CreateReportAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).CreateReportAsync(dto);
        }

        [Fact]
        public async Task CreateReportAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReportDto { ReportName = "Bad" };
            _pimsReportApiClient.CreateReportAsync(dto).Returns(FailureDto<ReportDto>());

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { Id = 3, ReportName = "Updated", Type = "R" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.UpdateReportAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReportAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).UpdateReportAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportApiClient.DeleteReportAsync(7).Returns(expected);

            // Act
            var result = await _service.DeleteReportAsync(7);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).DeleteReportAsync(7);
        }

        [Fact]
        public async Task DeleteReportAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.DeleteReportAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReportGroup surface ───────────────────────────────────────────────────

        #region ReportGroup

        [Fact]
        public async Task GetAllReportGroupsAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupDto> { new() { GroupId = 1, Description = "Group A" } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupApiClient.GetAllReportGroupsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportGroupsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupApiClient.Received(1).GetAllReportGroupsAsync();
        }

        [Fact]
        public async Task GetAllReportGroupsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.GetAllReportGroupsAsync().Returns(FailureDto<List<ReportGroupDto>>());

            // Act
            var result = await _service.GetAllReportGroupsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportGroupByIdAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { GroupId = 2, Description = "Group B" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.GetReportGroupByIdAsync(2).Returns(expected);

            // Act
            var result = await _service.GetReportGroupByIdAsync(2);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.GroupId);
            await _pimsReportGroupApiClient.Received(1).GetReportGroupByIdAsync(2);
        }

        [Fact]
        public async Task GetReportGroupByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.GetReportGroupByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReportGroupDto>());

            // Act
            var result = await _service.GetReportGroupByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { Description = "New Group" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.CreateReportGroupAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportGroupAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).CreateReportGroupAsync(dto);
        }

        [Fact]
        public async Task UpdateReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { GroupId = 3, Description = "Updated Group" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.UpdateReportGroupAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReportGroupAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).UpdateReportGroupAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportGroupApiClient.DeleteReportGroupAsync(4).Returns(expected);

            // Act
            var result = await _service.DeleteReportGroupAsync(4);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).DeleteReportGroupAsync(4);
        }

        [Fact]
        public async Task DeleteReportGroupAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.DeleteReportGroupAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportGroupAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReportGroupLink surface ───────────────────────────────────────────────

        #region ReportGroupLink

        [Fact]
        public async Task GetAllReportGroupLinksAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupLinkDto> { new() { ReportId = 1, GroupId = 2 } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupLinkApiClient.GetAllReportGroupLinksAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportGroupLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupLinkApiClient.Received(1).GetAllReportGroupLinksAsync();
        }

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupLinkDto> { new() { ReportId = 5, GroupId = 1 } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupLinkApiClient.GetReportGroupLinksByReportIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReportGroupLinksByReportIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupLinkApiClient.Received(1).GetReportGroupLinksByReportIdAsync(5);
        }

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupLinkApiClient.GetReportGroupLinksByReportIdAsync(Arg.Any<int>()).Returns(FailureDto<List<ReportGroupLinkDto>>());

            // Act
            var result = await _service.GetReportGroupLinksByReportIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportGroupLinkByIdAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupLinkDto { ReportId = 3, GroupId = 7 };
            var expected = SuccessDto(dto);
            _pimsReportGroupLinkApiClient.GetReportGroupLinkByIdAsync(3, 7).Returns(expected);

            // Act
            var result = await _service.GetReportGroupLinkByIdAsync(3, 7);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.ReportId);
            Assert.Equal(7, result.Data.GroupId);
            await _pimsReportGroupLinkApiClient.Received(1).GetReportGroupLinkByIdAsync(3, 7);
        }

        [Fact]
        public async Task CreateReportGroupLinkAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupLinkDto { ReportId = 1, GroupId = 2 };
            var expected = SuccessDto(dto);
            _pimsReportGroupLinkApiClient.CreateReportGroupLinkAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportGroupLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupLinkApiClient.Received(1).CreateReportGroupLinkAsync(dto);
        }

        [Fact]
        public async Task DeleteReportGroupLinkAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportGroupLinkApiClient.DeleteReportGroupLinkAsync(2, 5).Returns(expected);

            // Act
            var result = await _service.DeleteReportGroupLinkAsync(2, 5);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupLinkApiClient.Received(1).DeleteReportGroupLinkAsync(2, 5);
        }

        [Fact]
        public async Task DeleteReportGroupLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupLinkApiClient.DeleteReportGroupLinkAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportGroupLinkAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProjectManager surface ────────────────────────────────────────────────

        #region ProjectManager

        [Fact]
        public async Task GetAllProjectManagersAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProjectManagerDto> { new() { Projectmanager = "Smith" } };
            var expected = SuccessDto(dtos);
            _pimsProjectManagerApiClient.GetAllProjectManagersAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProjectManagersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProjectManagerApiClient.Received(1).GetAllProjectManagersAsync();
        }

        [Fact]
        public async Task GetProjectManagerByIdAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "Smith, J." };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.GetProjectManagerByNameAsync("Smith, J.").Returns(expected);

            // Act
            var result = await _service.GetProjectManagerByIdAsync("Smith, J.");

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).GetProjectManagerByNameAsync("Smith, J.");
        }

        [Fact]
        public async Task GetProjectManagerByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProjectManagerApiClient.GetProjectManagerByNameAsync(Arg.Any<string>()).Returns(FailureDto<ProjectManagerDto>());

            // Act
            var result = await _service.GetProjectManagerByIdAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "New Manager" };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.CreateProjectManagerAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProjectManagerAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).CreateProjectManagerAsync(dto);
        }

        [Fact]
        public async Task UpdateProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "Smith, J." };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.UpdateProjectManagerAsync("Smith, J.", dto).Returns(expected);

            // Act
            var result = await _service.UpdateProjectManagerAsync("Smith, J.", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).UpdateProjectManagerAsync("Smith, J.", dto);
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProjectManagerApiClient.DeleteProjectManagerAsync("Smith, J.").Returns(expected);

            // Act
            var result = await _service.DeleteProjectManagerAsync("Smith, J.");

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).DeleteProjectManagerAsync("Smith, J.");
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProjectManagerApiClient.DeleteProjectManagerAsync(Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProjectManagerAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProgramManagerLink surface ────────────────────────────────────────────

        #region ProgramManagerLink

        [Fact]
        public async Task GetAllProgramManagerLinksAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProgramManagerLinkDto> { new() { Program = "RAD", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProgramManagerLinkApiClient.GetAllProgramManagerLinksAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProgramManagerLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProgramManagerLinkApiClient.Received(1).GetAllProgramManagerLinksAsync();
        }

        [Fact]
        public async Task GetProgramManagerLinksByProgramAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProgramManagerLinkDto> { new() { Program = "RAD", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProgramManagerLinkApiClient.GetByProgramAsync("RAD").Returns(expected);

            // Act
            var result = await _service.GetProgramManagerLinksByProgramAsync("RAD");

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).GetByProgramAsync("RAD");
        }

        [Fact]
        public async Task GetProgramManagerLinksByProgramAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProgramManagerLinkApiClient.GetByProgramAsync(Arg.Any<string>()).Returns(FailureDto<List<ProgramManagerLinkDto>>());

            // Act
            var result = await _service.GetProgramManagerLinksByProgramAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProgramManagerLinkByIdAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProgramManagerLinkDto { Program = "RAD", Manager = "Jones" };
            var expected = SuccessDto(dto);
            _pimsProgramManagerLinkApiClient.GetProgramManagerLinkByIdAsync("RAD", "Jones").Returns(expected);

            // Act
            var result = await _service.GetProgramManagerLinkByIdAsync("RAD", "Jones");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("RAD",   result.Data!.Program);
            Assert.Equal("Jones", result.Data.Manager);
            await _pimsProgramManagerLinkApiClient.Received(1).GetProgramManagerLinkByIdAsync("RAD", "Jones");
        }

        [Fact]
        public async Task CreateProgramManagerLinkAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProgramManagerLinkDto { Program = "RAD", Manager = "Smith" };
            var expected = SuccessDto(dto);
            _pimsProgramManagerLinkApiClient.CreateProgramManagerLinkAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProgramManagerLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).CreateProgramManagerLinkAsync(dto);
        }

        [Fact]
        public async Task DeleteProgramManagerLinkAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProgramManagerLinkApiClient.DeleteProgramManagerLinkAsync("RAD", "Jones").Returns(expected);

            // Act
            var result = await _service.DeleteProgramManagerLinkAsync("RAD", "Jones");

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).DeleteProgramManagerLinkAsync("RAD", "Jones");
        }

        [Fact]
        public async Task DeleteProgramManagerLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProgramManagerLinkApiClient.DeleteProgramManagerLinkAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProgramManagerLinkAsync("X", "Y");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProfitCentreManagerLink surface ───────────────────────────────────────

        #region ProfitCentreManagerLink

        [Fact]
        public async Task GetAllProfitCentreManagerLinksAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProfitCentreManagerLinkDto> { new() { ProfitCentre = "PC01", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProfitCentreManagerLinkApiClient.GetAllProfitCentreManagerLinksAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProfitCentreManagerLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetAllProfitCentreManagerLinksAsync();
        }

        [Fact]
        public async Task GetProfitCentreManagerLinksByProfitCentreAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProfitCentreManagerLinkDto> { new() { ProfitCentre = "PC01", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProfitCentreManagerLinkApiClient.GetByProfitCentreAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetProfitCentreManagerLinksByProfitCentreAsync("PC01");

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetByProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreManagerLinksByProfitCentreAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProfitCentreManagerLinkApiClient.GetByProfitCentreAsync(Arg.Any<string>()).Returns(FailureDto<List<ProfitCentreManagerLinkDto>>());

            // Act
            var result = await _service.GetProfitCentreManagerLinksByProfitCentreAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkByIdAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProfitCentreManagerLinkDto { ProfitCentre = "PC01", Manager = "Jones" };
            var expected = SuccessDto(dto);
            _pimsProfitCentreManagerLinkApiClient.GetProfitCentreManagerLinkByIdAsync("PC01", "Jones").Returns(expected);

            // Act
            var result = await _service.GetProfitCentreManagerLinkByIdAsync("PC01", "Jones");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PC01",  result.Data!.ProfitCentre);
            Assert.Equal("Jones", result.Data.Manager);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetProfitCentreManagerLinkByIdAsync("PC01", "Jones");
        }

        [Fact]
        public async Task CreateProfitCentreManagerLinkAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProfitCentreManagerLinkDto { ProfitCentre = "PC02", Manager = "Smith" };
            var expected = SuccessDto(dto);
            _pimsProfitCentreManagerLinkApiClient.CreateProfitCentreManagerLinkAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProfitCentreManagerLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).CreateProfitCentreManagerLinkAsync(dto);
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLinkAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProfitCentreManagerLinkApiClient.DeleteProfitCentreManagerLinkAsync("PC01", "Jones").Returns(expected);

            // Act
            var result = await _service.DeleteProfitCentreManagerLinkAsync("PC01", "Jones");

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).DeleteProfitCentreManagerLinkAsync("PC01", "Jones");
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProfitCentreManagerLinkApiClient.DeleteProfitCentreManagerLinkAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProfitCentreManagerLinkAsync("X", "Y");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── Setting surface ───────────────────────────────────────────────────────

        #region Setting

        [Fact]
        public async Task GetAllSettingsAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<SettingDto> { new() { Id = "WorkingHours" }, new() { Id = "TestSetting" } };
            var expected = SuccessDto(dtos);
            _pimsSettingApiClient.GetAllSettingsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllSettingsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsSettingApiClient.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetAllSettingsAsync().Returns(FailureDto<List<SettingDto>>());

            // Act
            var result = await _service.GetAllSettingsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<SettingDto> { new() { Id = "WorkingHours" } };
            var expected = SuccessDto(dtos);
            _pimsSettingApiClient.GetAllUserUpdateableSettingsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsSettingApiClient.Received(1).GetAllUserUpdateableSettingsAsync();
        }

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetAllUserUpdateableSettingsAsync().Returns(FailureDto<List<SettingDto>>());

            // Act
            var result = await _service.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetSettingByIdAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dto      = new SettingDto { Id = "WorkingHours", SettingValue = "7.4" };
            var expected = SuccessDto(dto);
            _pimsSettingApiClient.GetSettingByIdAsync("WorkingHours").Returns(expected);

            // Act
            var result = await _service.GetSettingByIdAsync("WorkingHours");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("WorkingHours", result.Data!.Id);
            await _pimsSettingApiClient.Received(1).GetSettingByIdAsync("WorkingHours");
        }

        [Fact]
        public async Task GetSettingByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetSettingByIdAsync(Arg.Any<string>()).Returns(FailureDto<SettingDto>());

            // Act
            var result = await _service.GetSettingByIdAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateSettingAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dto      = new SettingDto { Id = "WorkingHours" };
            var expected = SuccessDto(dto);
            _pimsSettingApiClient.UpdateSettingAsync("WorkingHours", dto).Returns(expected);

            // Act
            var result = await _service.UpdateSettingAsync("WorkingHours", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsSettingApiClient.Received(1).UpdateSettingAsync("WorkingHours", dto);
        }

        [Fact]
        public async Task UpdateSettingAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "WorkingHours" };
            _pimsSettingApiClient.UpdateSettingAsync(Arg.Any<string>(), Arg.Any<SettingDto>()).Returns(FailureDto<SettingDto>());

            // Act
            var result = await _service.UpdateSettingAsync("WorkingHours", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessUser surface ────────────────────────────────────────────────────

        #region AccessUser

        [Fact]
        public async Task GetAllAccessUsersAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserDto> { new() { SystemId = 1, NtLogin = "dom\\u1" } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessUsersAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAccessUsersBySystemIdAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserDto> { new() { SystemId = 2, NtLogin = "dom\\u1" } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserApiClient.GetBySystemIdAsync(2).Returns(expected);

            // Act
            var result = await _service.GetAccessUsersBySystemIdAsync(2);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetAccessUsersBySystemIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.GetBySystemIdAsync(Arg.Any<int>()).Returns(FailureDto<List<AccessUserDto>>());

            // Act
            var result = await _service.GetAccessUsersBySystemIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessUserByIdAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { SystemId = 1, NtLogin = "dom\\user" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.GetByIdAsync(1, "dom\\user").Returns(expected);

            // Act
            var result = await _service.GetAccessUserByIdAsync(1, "dom\\user");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1,            result.Data!.SystemId);
            Assert.Equal("dom\\user",  result.Data.NtLogin);
            await _pimsAccessUserApiClient.Received(1).GetByIdAsync(1, "dom\\user");
        }

        [Fact]
        public async Task GetAccessUserByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<AccessUserDto>());

            // Act
            var result = await _service.GetAccessUserByIdAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { SystemId = 1, NtLogin = "dom\\newuser" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { SystemId = 1, NtLogin = "dom\\user" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.UpdateAsync(1, "dom\\user", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccessUserAsync(1, "dom\\user", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).UpdateAsync(1, "dom\\user", dto);
        }

        [Fact]
        public async Task DeleteAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessUserApiClient.DeleteAsync(1, "dom\\user").Returns(expected);

            // Act
            var result = await _service.DeleteAccessUserAsync(1, "dom\\user");

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).DeleteAsync(1, "dom\\user");
        }

        [Fact]
        public async Task DeleteAccessUserAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessUserAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessLevelName surface ───────────────────────────────────────────────────

        #region AccessLevelName

        [Fact]
        public async Task GetAllAccessLevelsAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessLevelDto> { new() { SystemId = 1, AccessLevelId = 10, AccessLevelName = "Admin" } };
            var expected = SuccessDto(dtos);
            _pimsAccessLevelApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessLevelsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsAccessLevelApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAccessLevelsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.GetAllAsync().Returns(FailureDto<List<AccessLevelDto>>());

            // Act
            var result = await _service.GetAllAccessLevelsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessLevelsBySystemIdAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessLevelDto> { new() { SystemId = 1, AccessLevelId = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessLevelApiClient.GetBySystemIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessLevelsBySystemIdAsync(1);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).GetBySystemIdAsync(1);
        }

        [Fact]
        public async Task GetAccessLevelByIdAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { SystemId = 1, AccessLevelId = 10, AccessLevelName = "Admin" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.GetByIdAsync(1, 10).Returns(expected);

            // Act
            var result = await _service.GetAccessLevelByIdAsync(1, 10);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data!.AccessLevelId);
            await _pimsAccessLevelApiClient.Received(1).GetByIdAsync(1, 10);
        }

        [Fact]
        public async Task GetAccessLevelByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.GetByIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<AccessLevelDto>());

            // Act
            var result = await _service.GetAccessLevelByIdAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { SystemId = 1, AccessLevelId = 20, AccessLevelName = "ReadOnly" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessLevelAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { SystemId = 1, AccessLevelId = 10, AccessLevelName = "SuperAdmin" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.UpdateAsync(1, 10, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccessLevelAsync(1, 10, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).UpdateAsync(1, 10, dto);
        }

        [Fact]
        public async Task DeleteAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessLevelApiClient.DeleteAsync(1, 10).Returns(expected);

            // Act
            var result = await _service.DeleteAccessLevelAsync(1, 10);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).DeleteAsync(1, 10);
        }

        [Fact]
        public async Task DeleteAccessLevelAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessLevelAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessUserLevel surface ───────────────────────────────────────────────

        #region AccessUserLevel

        [Fact]
        public async Task GetAllAccessUserLevelsAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { SystemId = 1, NtLogin = "dom\\u1", AccessLevelId = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetBySystemIdAsync(Arg.Any<int>()).Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelsBySystemIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsAccessUserLevelApiClient.Received(1).GetBySystemIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetAccessUserLevelsBySystemIdAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { SystemId = 1, NtLogin = "dom\\u1", AccessLevelId = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetBySystemIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelsBySystemIdAsync(1);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).GetBySystemIdAsync(1);
        }

        [Fact]
        public async Task GetAccessUserLevelsByUserAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { SystemId = 1, NtLogin = "dom\\u1", AccessLevelId = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetByUserAsync(1, "dom\\u1").Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelsByUserAsync(1, "dom\\u1");

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).GetByUserAsync(1, "dom\\u1");
        }

        [Fact]
        public async Task GetAccessUserLevelsByUserAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserLevelApiClient.GetByUserAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<List<AccessUserLevelDto>>());

            // Act
            var result = await _service.GetAccessUserLevelsByUserAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessUserLevelByIdAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserLevelDto { SystemId = 1, NtLogin = "dom\\u1", AccessLevelId = 10 };
            var expected = SuccessDto(dto);
            _pimsAccessUserLevelApiClient.GetByIdAsync(1, "dom\\u1", 10).Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelByIdAsync(1, "dom\\u1", 10);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data!.AccessLevelId);
            await _pimsAccessUserLevelApiClient.Received(1).GetByIdAsync(1, "dom\\u1", 10);
        }

        [Fact]
        public async Task CreateAccessUserLevelAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserLevelDto { SystemId = 1, NtLogin = "dom\\newuser", AccessLevelId = 20 };
            var expected = SuccessDto(dto);
            _pimsAccessUserLevelApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessUserLevelAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task DeleteAccessUserLevelAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessUserLevelApiClient.DeleteAsync(1, "dom\\u1", 10).Returns(expected);

            // Act
            var result = await _service.DeleteAccessUserLevelAsync(1, "dom\\u1", 10);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).DeleteAsync(1, "dom\\u1", 10);
        }

        [Fact]
        public async Task DeleteAccessUserLevelAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserLevelApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessUserLevelAsync(99, "unknown", 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessSystem surface (read-only) ──────────────────────────────────────

        #region AccessSystem

        [Fact]
        public async Task GetAllAccessSystemsAsync_DelegatesToPimsAccessSystemClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessSystemDto> { new() { SystemId = 1, SystemName = "PIMS" }, new() { SystemId = 2, SystemName = "FPS" } };
            var expected = SuccessDto(dtos);
            _pimsAccessSystemApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessSystemsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsAccessSystemApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAccessSystemsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessSystemApiClient.GetAllAsync().Returns(FailureDto<List<AccessSystemDto>>());

            // Act
            var result = await _service.GetAllAccessSystemsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessSystemByIdAsync_DelegatesToPimsAccessSystemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessSystemDto { SystemId = 1, SystemName = "PIMS" };
            var expected = SuccessDto(dto);
            _pimsAccessSystemApiClient.GetByIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessSystemByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PIMS", result.Data!.SystemName);
            await _pimsAccessSystemApiClient.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetAccessSystemByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessSystemApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<AccessSystemDto>());

            // Act
            var result = await _service.GetAccessSystemByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── Frequency surface ─────────────────────────────────────────────────────

        #region Frequency

        [Fact]
        public async Task GetAllFrequenciesAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<FrequencyDto> { new() { Frequencyid = 1 } };
            var expected = SuccessDto(dtos);
            _pimsFrequencyApiClient.GetAllFrequenciesAsync().Returns(expected);

            // Act
            var result = await _service.GetAllFrequenciesAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).GetAllFrequenciesAsync();
        }

        [Fact]
        public async Task GetAllFrequenciesAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.GetAllFrequenciesAsync().Returns(FailureDto<List<FrequencyDto>>());

            // Act
            var result = await _service.GetAllFrequenciesAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { Frequencyid = 5, FrequencyValue = "Monthly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.GetFrequencyByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetFrequencyByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Frequencyid);
            await _pimsFrequencyApiClient.Received(1).GetFrequencyByIdAsync(5);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.GetFrequencyByIdAsync(Arg.Any<int>()).Returns(FailureDto<FrequencyDto>());

            // Act
            var result = await _service.GetFrequencyByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { FrequencyValue = "Weekly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.CreateFrequencyAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateFrequencyAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).CreateFrequencyAsync(dto);
        }

        [Fact]
        public async Task CreateFrequencyAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new FrequencyDto { FrequencyValue = "Bad" };
            _pimsFrequencyApiClient.CreateFrequencyAsync(dto).Returns(FailureDto<FrequencyDto>());

            // Act
            var result = await _service.CreateFrequencyAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { Frequencyid = 3, FrequencyValue = "Quarterly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.UpdateFrequencyAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateFrequencyAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).UpdateFrequencyAsync(3, dto);
        }

        [Fact]
        public async Task DeleteFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsFrequencyApiClient.DeleteFrequencyAsync(3).Returns(expected);

            // Act
            var result = await _service.DeleteFrequencyAsync(3);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).DeleteFrequencyAsync(3);
        }

        [Fact]
        public async Task DeleteFrequencyAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.DeleteFrequencyAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteFrequencyAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReviewItem surface ────────────────────────────────────────────────────

        #region ReviewItem

        [Fact]
        public async Task GetAllReviewItemsAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReviewItemDto> { new() { Itemid = 1, Item = "Item A" } };
            var expected = SuccessDto(dtos);
            _pimsReviewItemApiClient.GetAllReviewItemsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReviewItemsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReviewItemApiClient.Received(1).GetAllReviewItemsAsync();
        }

        [Fact]
        public async Task GetAllReviewItemsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.GetAllReviewItemsAsync().Returns(FailureDto<List<ReviewItemDto>>());

            // Act
            var result = await _service.GetAllReviewItemsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReviewItemByIdAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Itemid = 5, Item = "Item E" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.GetReviewItemByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReviewItemByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Itemid);
            await _pimsReviewItemApiClient.Received(1).GetReviewItemByIdAsync(5);
        }

        [Fact]
        public async Task GetReviewItemByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.GetReviewItemByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReviewItemDto>());

            // Act
            var result = await _service.GetReviewItemByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Item = "New Item" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.CreateReviewItemAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReviewItemAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).CreateReviewItemAsync(dto);
        }

        [Fact]
        public async Task CreateReviewItemAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReviewItemDto { Item = "Bad" };
            _pimsReviewItemApiClient.CreateReviewItemAsync(dto).Returns(FailureDto<ReviewItemDto>());

            // Act
            var result = await _service.CreateReviewItemAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Itemid = 3, Item = "Updated Item" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.UpdateReviewItemAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReviewItemAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).UpdateReviewItemAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReviewItemApiClient.DeleteReviewItemAsync(4).Returns(expected);

            // Act
            var result = await _service.DeleteReviewItemAsync(4);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).DeleteReviewItemAsync(4);
        }

        [Fact]
        public async Task DeleteReviewItemAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.DeleteReviewItemAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReviewItemAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── RadTrackProg surface ──────────────────────────────────────────────────

        #region RadTrackProg

        [Fact]
        public async Task GetAllRadTrackProgsAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<RadTrackProgDto> { new() { Program = "RAD1", Radtrackprog = true } };
            var expected = SuccessDto(dtos);
            _pimsRadTrackProgApiClient.GetAllRadTrackProgsAsync().Returns(expected);

            // Act
            var result = await _service.GetAllRadTrackProgsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsRadTrackProgApiClient.Received(1).GetAllRadTrackProgsAsync();
        }

        [Fact]
        public async Task GetAllRadTrackProgsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.GetAllRadTrackProgsAsync().Returns(FailureDto<List<RadTrackProgDto>>());

            // Act
            var result = await _service.GetAllRadTrackProgsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetRadTrackProgByIdAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD1", Radtrackprog = true, Publicationprefix = "RT" };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.GetRadTrackProgByProgramAsync("RAD1").Returns(expected);

            // Act
            var result = await _service.GetRadTrackProgByIdAsync("RAD1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("RAD1", result.Data!.Program);
            await _pimsRadTrackProgApiClient.Received(1).GetRadTrackProgByProgramAsync("RAD1");
        }

        [Fact]
        public async Task GetRadTrackProgByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.GetRadTrackProgByProgramAsync(Arg.Any<string>()).Returns(FailureDto<RadTrackProgDto>());

            // Act
            var result = await _service.GetRadTrackProgByIdAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD2", Radtrackprog = false };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.CreateRadTrackProgAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).CreateRadTrackProgAsync(dto);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "BAD" };
            _pimsRadTrackProgApiClient.CreateRadTrackProgAsync(dto).Returns(FailureDto<RadTrackProgDto>());

            // Act
            var result = await _service.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD1", Radtrackprog = true, Publicationprefix = "RT2" };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.UpdateRadTrackProgAsync("RAD1", dto).Returns(expected);

            // Act
            var result = await _service.UpdateRadTrackProgAsync("RAD1", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).UpdateRadTrackProgAsync("RAD1", dto);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsRadTrackProgApiClient.DeleteRadTrackProgAsync("RAD1").Returns(expected);

            // Act
            var result = await _service.DeleteRadTrackProgAsync("RAD1");

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).DeleteRadTrackProgAsync("RAD1");
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.DeleteRadTrackProgAsync(Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteRadTrackProgAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region AdditionalDelegateCoverage

        [Fact]
        public async Task GetPagedReportsAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ReportDto>(new List<ReportDto> { new() { Id = 1, ReportName = "R1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsReportApiClient.GetPagedReportsAsync(query).Returns(expected);

            var result = await _service.GetPagedReportsAsync(query);

            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).GetPagedReportsAsync(query);
        }

        [Fact]
        public async Task GetPagedReportGroupsAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ReportGroupDto>(new List<ReportGroupDto> { new() { GroupId = 2, ReportId = 5, Description = "G1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsReportGroupApiClient.GetPagedReportGroupsAsync(query, 5).Returns(expected);

            var result = await _service.GetPagedReportGroupsAsync(query, 5);

            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).GetPagedReportGroupsAsync(query, 5);
        }

        [Fact]
        public async Task GetReportGroupsByReportIdAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<ReportGroupDto> { new() { GroupId = 2, ReportId = 5, Description = "G1" } });
            _pimsReportGroupApiClient.GetReportGroupsByReportIdAsync(5).Returns(expected);

            var result = await _service.GetReportGroupsByReportIdAsync(5);

            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).GetReportGroupsByReportIdAsync(5);
        }

        [Fact]
        public async Task GetPagedProjectManagersAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ProjectManagerDto>(new List<ProjectManagerDto> { new() { Projectmanager = "pm1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsProjectManagerApiClient.GetPagedProjectManagersAsync(query).Returns(expected);

            var result = await _service.GetPagedProjectManagersAsync(query);

            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).GetPagedProjectManagersAsync(query);
        }

        [Fact]
        public async Task GetManagerNamesAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<string> { "pm1", "pm2" });
            _pimsProjectManagerApiClient.GetManagerNamesAsync().Returns(expected);

            var result = await _service.GetManagerNamesAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsProjectManagerApiClient.Received(1).GetManagerNamesAsync();
        }

        [Fact]
        public async Task GetPagedProgramManagerLinksByManagerAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ProgramManagerLinkDto>(new List<ProgramManagerLinkDto> { new() { Program = "P1", Manager = "pm1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsProgramManagerLinkApiClient.GetPagedByManagerAsync(query, "pm1").Returns(expected);

            var result = await _service.GetPagedProgramManagerLinksByManagerAsync(query, "pm1");

            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).GetPagedByManagerAsync(query, "pm1");
        }

        [Fact]
        public async Task GetProgramManagerLinksByManagerAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<ProgramManagerLinkDto> { new() { Program = "P1", Manager = "pm1" } });
            _pimsProgramManagerLinkApiClient.GetByManagerAsync("pm1").Returns(expected);

            var result = await _service.GetProgramManagerLinksByManagerAsync("pm1");

            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).GetByManagerAsync("pm1");
        }

        [Fact]
        public async Task GetProgramsAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<ProgramLookupDto> { new() { ProgramNo = "P1", LatestYear = 2025 } });
            _pimsProgramManagerLinkApiClient.GetProgramsAsync().Returns(expected);

            var result = await _service.GetProgramsAsync();

            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetPagedProfitCentreManagerLinksByManagerAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ProfitCentreManagerLinkDto>(new List<ProfitCentreManagerLinkDto> { new() { ProfitCentre = "PC1", Manager = "pm1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsProfitCentreManagerLinkApiClient.GetPagedByManagerAsync(query, "pm1").Returns(expected);

            var result = await _service.GetPagedProfitCentreManagerLinksByManagerAsync(query, "pm1");

            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetPagedByManagerAsync(query, "pm1");
        }

        [Fact]
        public async Task GetProfitCentresAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<ProfitCentreLookupDto> { new() { ProfitCentre = "PC1", LatestYear = 2025 } });
            _pimsProfitCentreManagerLinkApiClient.GetProfitCentresAsync().Returns(expected);

            var result = await _service.GetProfitCentresAsync();

            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentreManagerLinksByManagerAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<ProfitCentreManagerLinkDto> { new() { ProfitCentre = "PC1", Manager = "pm1" } });
            _pimsProfitCentreManagerLinkApiClient.GetByManagerAsync("pm1").Returns(expected);

            var result = await _service.GetProfitCentreManagerLinksByManagerAsync("pm1");

            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetByManagerAsync("pm1");
        }

        [Fact]
        public async Task GetPagedAccessUsersAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<AccessUserDto>(new List<AccessUserDto> { new() { SystemId = 1, NtLogin = "dom\\u1" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsAccessUserApiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAccessUsersAsync(query);

            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAccessUserLevelsAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<AccessUserLevelDto>(new List<AccessUserLevelDto> { new() { SystemId = 1, NtLogin = "dom\\u1", AccessLevelId = 1 } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsAccessUserLevelApiClient.GetPagedAsync(query).Returns(expected);

            var result = await _service.GetPagedAccessUserLevelsAsync(query);

            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedFrequenciesAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<FrequencyDto>(new List<FrequencyDto> { new() { Frequencyid = 1, FrequencyValue = "Monthly" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsFrequencyApiClient.GetPagedFrequenciesAsync(query).Returns(expected);

            var result = await _service.GetPagedFrequenciesAsync(query);

            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).GetPagedFrequenciesAsync(query);
        }

        [Fact]
        public async Task GetPagedReviewItemsAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<ReviewItemDto>(new List<ReviewItemDto> { new() { Itemid = 1, Item = "Item A" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsReviewItemApiClient.GetPagedReviewItemsAsync(query).Returns(expected);

            var result = await _service.GetPagedReviewItemsAsync(query);

            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).GetPagedReviewItemsAsync(query);
        }

        [Fact]
        public async Task GetPagedRadTrackProgsAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<RadTrackProgDto>(new List<RadTrackProgDto> { new() { Program = "RAD1", Radtrackprog = true } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsRadTrackProgApiClient.GetPagedRadTrackProgsAsync(query).Returns(expected);

            var result = await _service.GetPagedRadTrackProgsAsync(query);

            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).GetPagedRadTrackProgsAsync(query);
        }

        [Fact]
        public async Task GetRadTrackProgProgramsAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<string> { "RAD1", "RAD2" });
            _pimsRadTrackProgApiClient.GetAllProgramNamesAsync().Returns(expected);

            var result = await _service.GetRadTrackProgProgramsAsync();

            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsRadTrackProgApiClient.Received(1).GetAllProgramNamesAsync();
        }

        [Fact]
        public async Task GetAllRiskRatingsAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<RiskDto> { new() { Riskid = 1, Riskrating = "Low" } });
            _pimsRiskApiClient.GetAllRiskRatingsAsync().Returns(expected);

            var result = await _service.GetAllRiskRatingsAsync();

            Assert.True(result.Success);
            await _pimsRiskApiClient.Received(1).GetAllRiskRatingsAsync();
        }

        [Fact]
        public async Task GetPagedRiskRatingsAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<RiskDto>(new List<RiskDto> { new() { Riskid = 1, Riskrating = "Low" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsRiskApiClient.GetPagedRiskRatingsAsync(query).Returns(expected);

            var result = await _service.GetPagedRiskRatingsAsync(query);

            Assert.True(result.Success);
            await _pimsRiskApiClient.Received(1).GetPagedRiskRatingsAsync(query);
        }

        [Fact]
        public async Task GetRiskRatingByIdAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var dto = new RiskDto { Riskid = 3, Riskrating = "Medium" };
            var expected = SuccessDto(dto);
            _pimsRiskApiClient.GetRiskRatingByIdAsync(3).Returns(expected);

            var result = await _service.GetRiskRatingByIdAsync(3);

            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Riskid);
            await _pimsRiskApiClient.Received(1).GetRiskRatingByIdAsync(3);
        }

        [Fact]
        public async Task CreateRiskRatingAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var dto = new RiskDto { Riskid = 0, Riskrating = "High" };
            var expected = SuccessDto(dto);
            _pimsRiskApiClient.CreateRiskRatingAsync(dto).Returns(expected);

            var result = await _service.CreateRiskRatingAsync(dto);

            Assert.True(result.Success);
            await _pimsRiskApiClient.Received(1).CreateRiskRatingAsync(dto);
        }

        [Fact]
        public async Task UpdateRiskRatingAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var dto = new RiskDto { Riskid = 2, Riskrating = "Updated" };
            var expected = SuccessDto(dto);
            _pimsRiskApiClient.UpdateRiskRatingAsync(2, dto).Returns(expected);

            var result = await _service.UpdateRiskRatingAsync(2, dto);

            Assert.True(result.Success);
            await _pimsRiskApiClient.Received(1).UpdateRiskRatingAsync(2, dto);
        }

        [Fact]
        public async Task DeleteRiskRatingAsync_DelegatesToPimsRiskClient_ReturnsResult()
        {
            var expected = SuccessDto(true);
            _pimsRiskApiClient.DeleteRiskRatingAsync(2).Returns(expected);

            var result = await _service.DeleteRiskRatingAsync(2);

            Assert.True(result.Success);
            await _pimsRiskApiClient.Received(1).DeleteRiskRatingAsync(2);
        }

        [Fact]
        public async Task GetAllPublicationTypesAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var expected = SuccessDto(new List<PublicationTypeDto> { new() { Type = "A", Description = "Alpha" } });
            _pimsPublicationTypeApiClient.GetAllPublicationTypesAsync().Returns(expected);

            var result = await _service.GetAllPublicationTypesAsync();

            Assert.True(result.Success);
            await _pimsPublicationTypeApiClient.Received(1).GetAllPublicationTypesAsync();
        }

        [Fact]
        public async Task GetPagedPublicationTypesAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var page = new PaginatedResult<PublicationTypeDto>(new List<PublicationTypeDto> { new() { Type = "A", Description = "Alpha" } }, 1, 1, 10);
            var expected = SuccessDto(page);
            _pimsPublicationTypeApiClient.GetPagedPublicationTypesAsync(query).Returns(expected);

            var result = await _service.GetPagedPublicationTypesAsync(query);

            Assert.True(result.Success);
            await _pimsPublicationTypeApiClient.Received(1).GetPagedPublicationTypesAsync(query);
        }

        [Fact]
        public async Task GetPublicationTypeByCodeAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var dto = new PublicationTypeDto { Type = "A", Description = "Alpha" };
            var expected = SuccessDto(dto);
            _pimsPublicationTypeApiClient.GetPublicationTypeByCodeAsync("A").Returns(expected);

            var result = await _service.GetPublicationTypeByCodeAsync("A");

            Assert.True(result.Success);
            Assert.Equal("A", result.Data!.Type);
            await _pimsPublicationTypeApiClient.Received(1).GetPublicationTypeByCodeAsync("A");
        }

        [Fact]
        public async Task CreatePublicationTypeAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var dto = new PublicationTypeDto { Type = "B", Description = "Beta" };
            var expected = SuccessDto(dto);
            _pimsPublicationTypeApiClient.CreatePublicationTypeAsync(dto).Returns(expected);

            var result = await _service.CreatePublicationTypeAsync(dto);

            Assert.True(result.Success);
            await _pimsPublicationTypeApiClient.Received(1).CreatePublicationTypeAsync(dto);
        }

        [Fact]
        public async Task UpdatePublicationTypeAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var dto = new PublicationTypeDto { Type = "A", Description = "Updated" };
            var expected = SuccessDto(dto);
            _pimsPublicationTypeApiClient.UpdatePublicationTypeAsync("A", dto).Returns(expected);

            var result = await _service.UpdatePublicationTypeAsync("A", dto);

            Assert.True(result.Success);
            await _pimsPublicationTypeApiClient.Received(1).UpdatePublicationTypeAsync("A", dto);
        }

        [Fact]
        public async Task DeletePublicationTypeAsync_DelegatesToPimsPublicationTypeClient_ReturnsResult()
        {
            var expected = SuccessDto(true);
            _pimsPublicationTypeApiClient.DeletePublicationTypeAsync("A").Returns(expected);

            var result = await _service.DeletePublicationTypeAsync("A");

            Assert.True(result.Success);
            await _pimsPublicationTypeApiClient.Received(1).DeletePublicationTypeAsync("A");
        }

        #endregion
    }
}
