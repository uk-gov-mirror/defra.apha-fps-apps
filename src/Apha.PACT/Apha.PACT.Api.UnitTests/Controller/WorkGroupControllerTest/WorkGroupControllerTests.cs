using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.WorkGroupControllerTest
{
    public class WorkGroupControllerTests
    {
        private readonly IWorkGroupService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupController _controller;

        public WorkGroupControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new WorkGroupController(_serviceMock, _mapperMock);
        }

        #region GetAll

        [Fact]
        public async Task GetAll_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var dtos = new List<WorkGroupDto> { new() { WorkGroupName = "WG1", ProfitCentre = "PC1" } };
            var mapped = new List<WorkGroupRes> { new() { WorkGroupName = "WG1", ProfitCentre = "PC1" } };

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetAllWorkGroupsAsync();
            _mapperMock.Received(1).Map<IEnumerable<WorkGroupRes>>(dtos);
        }

        [Fact]
        public async Task GetAll_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var dtos = new List<WorkGroupDto>();
            var mapped = new List<WorkGroupRes>();

            _serviceMock.GetAllWorkGroupsAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<WorkGroupRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<WorkGroupRes>>(okResult.Value, exactMatch: false);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }

        #endregion

        #region GetAllWorkGroupNamesAsync

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithData_ReturnsOkWithNames()
        {
            // Arrange
            var names = new List<string> { "WG1", "WG2" };

            _serviceMock.GetAllWorkGroupNamesAsync().Returns(names);

            // Act
            var result = await _controller.GetAllWorkGroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(names, okResult.Value);
            await _serviceMock.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupNamesAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetAllWorkGroupNamesAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupNamesAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllWorkGroupNamesAsync());
        }

        #endregion

        #region GetPagedWorkGroupTimeCodes

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto>
            {
                Data = [new() { PACTStaffID = "S1", TimeCode = "TC1" }]
            };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes>
            {
                Data = [new() { PACTStaffID = "S1", TimeCode = "TC1" }]
            };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG1", 3).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG1", 3);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupTimeCodeAsync(query, "WG1", 3);
            _mapperMock.Received(1).Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_NullWorkGroupAndMonth_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG2", 1).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG2", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupTimeCodeAsync(query, "WG2", 1);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, "WG2", 1).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupTimeCodes(query, "WG2", 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<WorkGroupTimeCodeRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task GetPagedWorkGroupTimeCodes_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWorkGroupTimeCodeAsync(query, Arg.Any<string>(), Arg.Any<int>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedWorkGroupTimeCodes(query, "WG1", 1));
        }

        #endregion

        #region GetPagedWorkGroupValidTimeCodes

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto>
            {
                Data = [new() { TimeCode = "TC1", ParentProject = "P001", WorkGroup = "WG1" }]
            };
            var mapped = new PaginationRes<WorkGroupValidTimeCodeRes>
            {
                Data = [new() { TimeCode = "TC1", ParentProject = "P001", WorkGroup = "WG1" }]
            };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupValidTimeCodeAsync(query, "WG1");
            _mapperMock.Received(1).Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupValidTimeCodeRes> { Data = [] };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<WorkGroupValidTimeCodeRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedWorkGroupValidTimeCodes_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupValidTimeCodeDto> { Data = [] };

            _serviceMock.GetWorkGroupValidTimeCodeAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupValidTimeCodeRes>>(serviceResult)
                       .Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetPagedWorkGroupValidTimeCodes(query, "WG1"));
        }

        #endregion

        #region GetWgSummarisedStaffTimeUsage

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto
            {
                Rows = [new() { ParentProject = "PP1", JobCode = "JC1" }],
                Summary = new WgSummarisedStaffTimeUsageSummaryDto { GrandTotalTime = 100.0 },
                HrsPaid = 120.0
            };
            var mapped = new WgSummarisedStaffTimeUsageRes
            {
                Rows = [new() { ParentProject = "PP1", JobCode = "JC1" }],
                Summary = new WgSummarisedStaffTimeUsageSummaryRes { GrandTotalTime = 100.0 },
                HrsPaid = 120.0
            };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG1");
            _mapperMock.Received(1).Map<WgSummarisedStaffTimeUsageRes>(serviceResult);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_EmptyRows_ReturnsOkWithEmptyRowsCollection()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto { Rows = [], HrsPaid = 0 };
            var mapped = new WgSummarisedStaffTimeUsageRes { Rows = [], HrsPaid = 0 };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<WgSummarisedStaffTimeUsageRes>(okResult.Value);
            Assert.Empty(returnValue.Rows);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_PassesQueryAndWorkGroupToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "JobCode" };
            var serviceResult = new WgSummarisedStaffTimeUsageDto();
            var mapped = new WgSummarisedStaffTimeUsageRes();

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG_ALPHA").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            await _controller.GetWgSummarisedStaffTimeUsage(query, "WG_ALPHA");

            // Assert
            await _serviceMock.Received(1).GetWgSummarisedStaffTimeUsageAsync(query, "WG_ALPHA");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_PassesServiceResultToMapper()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto { HrsPaid = 240.0 };
            var mapped = new WgSummarisedStaffTimeUsageRes { HrsPaid = 240.0 };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            _mapperMock.Received(1).Map<WgSummarisedStaffTimeUsageRes>(serviceResult);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_ReturnedDtoContainsHrsPaid()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto { HrsPaid = 180.0 };
            var mapped = new WgSummarisedStaffTimeUsageRes { HrsPaid = 180.0 };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<WgSummarisedStaffTimeUsageRes>(okResult.Value);
            Assert.Equal(180.0, returnValue.HrsPaid);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_ReturnedDtoContainsSummary()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto
            {
                Summary = new WgSummarisedStaffTimeUsageSummaryDto
                {
                    GrandTotalTime = 200.0,
                    StandardHoursPerMonth = 10.0,
                    GrandTotalPercentAllocated = 75.0
                }
            };
            var mapped = new WgSummarisedStaffTimeUsageRes
            {
                Summary = new WgSummarisedStaffTimeUsageSummaryRes
                {
                    GrandTotalTime = 200.0,
                    StandardHoursPerMonth = 10.0,
                    GrandTotalPercentAllocated = 75.0
                }
            };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<WgSummarisedStaffTimeUsageRes>(okResult.Value);
            Assert.Equal(200.0, returnValue.Summary.GrandTotalTime);
            Assert.Equal(10.0, returnValue.Summary.StandardHoursPerMonth);
            Assert.Equal(75.0, returnValue.Summary.GrandTotalPercentAllocated);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetWgSummarisedStaffTimeUsage(query, "WG1"));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_ServiceThrowsBusinessValidation_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new InvalidOperationException("Validation error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetWgSummarisedStaffTimeUsage(query, "WG1"));

            _mapperMock.DidNotReceiveWithAnyArgs().Map<WgSummarisedStaffTimeUsageRes>(default!);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto();

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult)
                       .Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(
                () => _controller.GetWgSummarisedStaffTimeUsage(query, "WG1"));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_ReturnsOkStatusCode()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto();
            var mapped = new WgSummarisedStaffTimeUsageRes();

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsage_MultipleRows_AllRowsReturnedInResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new WgSummarisedStaffTimeUsageDto
            {
                Rows =
                [
                    new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 },
                    new() { ParentProject = "PP1", JobCode = "JC2", April = 5.0  },
                    new() { ParentProject = "PP2", JobCode = "JC1", April = 8.0  }
                ]
            };
            var mapped = new WgSummarisedStaffTimeUsageRes
            {
                Rows =
                [
                    new() { ParentProject = "PP1", JobCode = "JC1", April = 10.0 },
                    new() { ParentProject = "PP1", JobCode = "JC2", April = 5.0  },
                    new() { ParentProject = "PP2", JobCode = "JC1", April = 8.0  }
                ]
            };

            _serviceMock.GetWgSummarisedStaffTimeUsageAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<WgSummarisedStaffTimeUsageRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWgSummarisedStaffTimeUsage(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<WgSummarisedStaffTimeUsageRes>(okResult.Value);
            Assert.Equal(3, returnValue.Rows.Count());
        }

        #endregion

        #region GetPagedSummarisedWorkgroupTime

        [Fact]
        public async Task GetPagedSummarisedWorkgroupTime_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new SummarisedWgTimeViewDto
            {
                Rows = [new() { ParentProject = "PP1" }]
            };
            var mapped = new SummarisedWgTimePivotRes
            {
                Rows = [new() { ParentProject = "PP1" }]
            };

            _serviceMock.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<SummarisedWgTimePivotRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedSummarisedWorkgroupTime(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1");
            _mapperMock.Received(1).Map<SummarisedWgTimePivotRes>(serviceResult);
        }

        [Fact]
        public async Task GetPagedSummarisedWorkgroupTime_EmptyRows_ReturnsOkWithEmptyRowsCollection()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new SummarisedWgTimeViewDto { Rows = [] };
            var mapped = new SummarisedWgTimePivotRes { Rows = [] };

            _serviceMock.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<SummarisedWgTimePivotRes>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetPagedSummarisedWorkgroupTime(query, "WG1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<SummarisedWgTimePivotRes>(okResult.Value);
            Assert.Empty(returnValue.Rows);
        }

        [Fact]
        public async Task GetPagedSummarisedWorkgroupTime_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetSummarisedWorkgroupTimeSummaryAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedSummarisedWorkgroupTime(query, "WG1"));
        }

        [Fact]
        public async Task GetPagedSummarisedWorkgroupTime_MapperThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new SummarisedWgTimeViewDto();

            _serviceMock.GetSummarisedWorkgroupTimeSummaryAsync(query, "WG1").Returns(serviceResult);
            _mapperMock.Map<SummarisedWgTimePivotRes>(serviceResult)
                       .Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetPagedSummarisedWorkgroupTime(query, "WG1"));
        }

        #endregion

        #region GetWorkGroupsByProfitCentre

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupDto>
            {
                Data = [new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }]
            };
            var mapped = new PaginationRes<WorkGroupRes>
            {
                Data = [new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }]
            };

            _serviceMock.GetWorkGroupsByProfitCentreAsync(query, "PC1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentre(query, "PC1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupsByProfitCentreAsync(query, "PC1");
            _mapperMock.Received(1).Map<PaginationRes<WorkGroupRes>>(serviceResult);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupDto> { Data = [] };
            var mapped = new PaginationRes<WorkGroupRes> { Data = [] };

            _serviceMock.GetWorkGroupsByProfitCentreAsync(query, "PC1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentre(query, "PC1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<WorkGroupRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentre_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetWorkGroupsByProfitCentreAsync(query, Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetWorkGroupsByProfitCentre(query, "PC1"));
        }

        #endregion

        #region GetWorkGroupsByProfitCentreForBudgetAsync

        [Fact]
        public async Task GetWorkGroupsByProfitCentreForBudgetAsync_WithData_ReturnsOkWithMappedResult()
        {
            // Arrange
            var serviceResult = new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", UserEmail = "a@b.com" }
            };
            var mapped = new List<WorkGroupViewRes>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", UserEmail = "a@b.com" }
            };

            _serviceMock.GetWorkGroupsByProfitCentreForBudgetAsync("PC1").Returns(serviceResult);
            _mapperMock.Map<List<WorkGroupViewRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentreForBudgetAsync("PC1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).GetWorkGroupsByProfitCentreForBudgetAsync("PC1");
            _mapperMock.Received(1).Map<List<WorkGroupViewRes>>(serviceResult);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreForBudgetAsync_EmptyList_ReturnsOkWithEmptyCollection()
        {
            // Arrange
            var serviceResult = new List<WorkGroupViewDto>();
            var mapped = new List<WorkGroupViewRes>();

            _serviceMock.GetWorkGroupsByProfitCentreForBudgetAsync("PC1").Returns(serviceResult);
            _mapperMock.Map<List<WorkGroupViewRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetWorkGroupsByProfitCentreForBudgetAsync("PC1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<WorkGroupViewRes>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreForBudgetAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetWorkGroupsByProfitCentreForBudgetAsync(Arg.Any<string>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetWorkGroupsByProfitCentreForBudgetAsync("PC1"));
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroups

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_ValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "PC1", SendEmail = 1 };

            _serviceMock.SetSendEmailForProfitCentreWorkGroupsAsync("PC1", 1).Returns(true);

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync("PC1", 1);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_NullProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = null, SendEmail = 1 };

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ProfitCentre is required.", badRequest.Value);
            await _serviceMock.DidNotReceiveWithAnyArgs().SetSendEmailForProfitCentreWorkGroupsAsync(default!, default);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WhitespaceProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "   ", SendEmail = 1 };

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceiveWithAnyArgs().SetSendEmailForProfitCentreWorkGroupsAsync(default!, default);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "PC1", SendEmail = 0 };

            _serviceMock.SetSendEmailForProfitCentreWorkGroupsAsync("PC1", 0).Returns(false);

            // Act
            var result = await _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { ProfitCentre = "PC1", SendEmail = 1 };

            _serviceMock.SetSendEmailForProfitCentreWorkGroupsAsync("PC1", 1)
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SetSendEmailForProfitCentreWorkGroupsAsync(request));
        }

        #endregion

        #region SetSendEmailForAllWorkGroups

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_FlagZero_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { SendEmail = 0 };

            _serviceMock.SetSendEmailForAllWorkGroupsAsync(0).Returns(true);

            // Act
            var result = await _controller.SetSendEmailForAllWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).SetSendEmailForAllWorkGroupsAsync(0);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { SendEmail = 0 };

            _serviceMock.SetSendEmailForAllWorkGroupsAsync(0).Returns(false);

            // Act
            var result = await _controller.SetSendEmailForAllWorkGroupsAsync(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateSendEmailFlagReq { SendEmail = 0 };

            _serviceMock.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SetSendEmailForAllWorkGroupsAsync(request));
        }

        #endregion

        #region UpdateWorkGroupEmail

        [Fact]
        public async Task UpdateWorkGroupEmail_ValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG1", SendEmail = 1, EmailRecipient = "test@example.com" };

            _serviceMock.UpdateWorkGroupEmailAsync("WG1", 1, "test@example.com").Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG1", request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateWorkGroupEmailAsync("WG1", 1, "test@example.com");
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_EmptyWorkGroupName_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG1", SendEmail = 1 };

            // Act
            var result = await _controller.UpdateWorkGroupEmail("", request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceiveWithAnyArgs().UpdateWorkGroupEmailAsync(default!, default, default);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_RouteAndBodyNameMismatch_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG_DIFFERENT", SendEmail = 1 };

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG1", request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("WorkGroupName in the request body does not match the query parameter.", badRequest.Value);
            await _serviceMock.DidNotReceiveWithAnyArgs().UpdateWorkGroupEmailAsync(default!, default, default);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_EmptyBodyWorkGroupName_CallsServiceWithRouteName()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "", SendEmail = 1, EmailRecipient = null };

            _serviceMock.UpdateWorkGroupEmailAsync("WG1", 1, null).Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG1", request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateWorkGroupEmailAsync("WG1", 1, null);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_NullEmailRecipient_ReturnsOk()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG1", SendEmail = 0, EmailRecipient = null };

            _serviceMock.UpdateWorkGroupEmailAsync("WG1", 0, null).Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG1", request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "WG1", SendEmail = 1 };

            _serviceMock.UpdateWorkGroupEmailAsync("WG1", Arg.Any<short>(), Arg.Any<string?>())
                        .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateWorkGroupEmail("WG1", request));
        }

        [Fact]
        public async Task UpdateWorkGroupEmail_RouteAndBodyNameMatchCaseInsensitive_ReturnsOk()
        {
            // Arrange
            var request = new UpdateWorkGroupEmailReq { WorkGroupName = "wg1", SendEmail = 1, EmailRecipient = "a@b.com" };

            _serviceMock.UpdateWorkGroupEmailAsync("WG1", 1, "a@b.com").Returns(true);

            // Act
            var result = await _controller.UpdateWorkGroupEmail("WG1", request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).UpdateWorkGroupEmailAsync("WG1", 1, "a@b.com");
        }

        #endregion
    }
}
