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

namespace Apha.PACT.Api.UnitTests.Controller.TimeCodeValidControllerTest
{
    public class TimeCodeValidControllerTests
    {
        private readonly ITimeCodeValidService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TimeCodeValidController _controller;

        public TimeCodeValidControllerTests()
        {
            _serviceMock = Substitute.For<ITimeCodeValidService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new TimeCodeValidController(_serviceMock, _mapperMock);
        }

        #region GetPagedByProjectAndTestCode

        [Fact]
        public async Task GetPagedByProjectAndTestCode_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" }
            };
            var serviceResult = new PaginatedResult<TimeCodeValidDto>(dtos, new PaginationDto { TotalRecords = 1 });
            var expectedResponse = new PaginationRes<TimeCodeValidRes>
            {
                Data = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1" } }
            };

            _serviceMock.GetPagedByProjectAndTestCodeAsync(query, "PRJ1", "TST1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCodeValidRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPagedByProjectAndTestCode(query, "PRJ1", "TST1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPagedByProjectAndTestCode_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetPagedByProjectAndTestCodeAsync(query, "PRJ1", "TST1")
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetPagedByProjectAndTestCode(query, "PRJ1", "TST1"));
        }

        #endregion

        #region GetByJobCode

        [Fact]
        public async Task GetByJobCode_HappyPath_ReturnsOk()
        {
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" } };
            var mapped = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" } };

            _serviceMock.GetByJobCodeAsync("JC1", "PRJ1").Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            var result = await _controller.GetByJobCode("JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetByJobCode_ServiceThrows_PropagatesException()
        {
            _serviceMock.GetByJobCodeAsync("JC1", "PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetByJobCode("JC1", "PRJ1"));
        }

        #endregion

        #region GetPaged

        [Fact]
        public async Task GetPaged_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<TimeCodeValidDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<TimeCodeValidRes>
            {
                Data = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedTimeCodesAsync(query, "JC1", "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCodeValidRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullFilters_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TimeCodeValidDto>(Enumerable.Empty<TimeCodeValidDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<TimeCodeValidRes>();

            _serviceMock.GetPagedTimeCodesAsync(query, null, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TimeCodeValidRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetTimeCodeValidsByWorkGroupAsync

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithValidWorkGroup_ReturnsOk()
        {
            var dtos = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" }
            };
            var mapped = new List<TimeCodeValidRes>
            {
                new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" }
            };

            _serviceMock.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            var result = await _controller.GetTimeCodeValidsByWorkGroupAsync("WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ok.Value);
        }

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WhenServiceThrows_PropagatesException()
        {
            _serviceMock.GetTimeCodeValidsByWorkGroupAsync("WG1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTimeCodeValidsByWorkGroupAsync("WG1"));
        }

        #endregion

        #region GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync

        [Fact]
        public async Task GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync_WithValidInput_ReturnsOk()
        {
            var projects = new List<string> { "PRJ1", "PRJ2" };
            _serviceMock.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(projects);

            var result = await _controller.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projects, ok.Value);
        }

        [Fact]
        public async Task GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync_WhenServiceThrows_PropagatesException()
        {
            _serviceMock.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1")
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1"));
        }

        #endregion

        #region GetAllDistinctTimeCodesAsync

        [Fact]
        public async Task GetAllDistinctTimeCodesAsync_WithData_ReturnsOk()
        {
            var timeCodes = new List<string> { "TC1", "TC2" };
            _serviceMock.GetAllDistinctTimeCodesAsync().Returns(timeCodes);

            var result = await _controller.GetAllDistinctTimeCodesAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(timeCodes, ok.Value);
        }

        #endregion

        #region GetAllDistinctProjectsAsync

        [Fact]
        public async Task GetAllDistinctProjectsAsync_WithData_ReturnsOk()
        {
            var projects = new List<string> { "PRJ1", "PRJ2" };
            _serviceMock.GetAllDistinctProjectsAsync().Returns(projects);

            var result = await _controller.GetAllDistinctProjectsAsync();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projects, ok.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_HappyPath_ReturnsOk()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _serviceMock.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(dto);
            _mapperMock.Map<TimeCodeValidRes>(dto).Returns(mapped);

            var result = await _controller.GetById("WG1", "TC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ReturnsNotFound()
        {
            _serviceMock.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns((TimeCodeValidDto?)null);

            var result = await _controller.GetById("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_HappyPath_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var createdDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).Returns(createdDto);
            _mapperMock.Map<TimeCodeValidRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Create_WithTestCodeAndPortfolio_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var createdDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).Returns(createdDto);
            _mapperMock.Map<TimeCodeValidRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Create_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).ThrowsAsync(new InvalidOperationException("Must fill in Testcode and Portfolio, or Jobcode"));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(req));
            Assert.Equal("Must fill in Testcode and Portfolio, or Jobcode", ex.Message);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.CreateTimeCodeValidAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_HappyPath_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var updatedDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).Returns(updatedDto);
            _mapperMock.Map<TimeCodeValidRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_WithTestCodeAndPortfolio_ReturnsOk()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var updatedDto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var mapped = new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).Returns(updatedDto);
            _mapperMock.Map<TimeCodeValidRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrowsInvalidOperationException_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).ThrowsAsync(new InvalidOperationException("Not a valid jobcode."));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Update(req));
            Assert.Equal("Not a valid jobcode.", ex.Message);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new TimeCodeValidReq { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mapperMock.Map<TimeCodeValidDto>(req).Returns(dto);
            _serviceMock.UpdateTimeCodeValidAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(true);

            var result = await _controller.Delete("WG1", "TC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.Delete("WG_MISSING", "TC_MISSING", "PRJ1"));
        }

        #endregion

        #region DeleteAllByJobCode

        [Fact]
        public async Task DeleteAllByJobCode_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteAllByJobCodeAsync("JC1", "PRJ1").Returns(true);

            var result = await _controller.DeleteAllByJobCode("JC1", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteAllByJobCode_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAllByJobCode("JC_MISSING", "PRJ1"));
        }

        #endregion

        #region CopyWorkGroup

        [Fact]
        public async Task CopyWorkGroup_HappyPath_ReturnsOk()
        {
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };
            var mapped = new List<TimeCodeValidRes> { new TimeCodeValidRes { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };

            _serviceMock.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            var result = await _controller.CopyWorkGroup("JC_SRC", "JC_TGT", "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CopyWorkGroup_ServiceThrows_PropagatesException()
        {
            _serviceMock.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CopyWorkGroup("JC_SRC", "JC_TGT", "PRJ1"));
        }

        #endregion

        #region DeleteBulk

        [Fact]
        public async Task DeleteBulk_HappyPath_ReturnsOk()
        {
            // Arrange
            var request = new BulkDeleteTimeCodeReq
            {
                ParentProject = "PRJ1",
                Items = [new TimeCodeKeyItem { WorkGroup = "WG1", TimeCode = "TC1" }]
            };
            _serviceMock
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    "PRJ1")
                .Returns(true);

            // Act
            var result = await _controller.DeleteBulk(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteBulkAsync(
                Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                "PRJ1");
        }

        [Fact]
        public async Task DeleteBulk_WithEmptyItems_ReturnsOk()
        {
            // Arrange — empty items list; service still returns true
            var request = new BulkDeleteTimeCodeReq { ParentProject = "PRJ1", Items = [] };
            _serviceMock
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    "PRJ1")
                .Returns(true);

            // Act
            var result = await _controller.DeleteBulk(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteBulk_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new BulkDeleteTimeCodeReq
            {
                ParentProject = "PRJ1",
                Items = [new TimeCodeKeyItem { WorkGroup = "WG1", TimeCode = "TC1" }]
            };
            _serviceMock
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteBulk(request));
        }

        #endregion

        #region CopyBulkWorkGroups

        [Fact]
        public async Task CopyBulkWorkGroups_HappyPath_ReturnsOk()
        {
            // Arrange
            var request = new BulkCopyWorkGroupReq
            {
                ParentProject = "PRJ1",
                SourceJobCode = "JC_SRC",
                TargetJobCode = "JC_TGT",
                WorkGroups = ["WG1", "WG2"]
            };
            var dtos = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_TGT" }
            };
            var mapped = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1" }
            };

            _serviceMock
                .CopySelectedWorkGroupsAsync(
                    Arg.Any<IEnumerable<string>>(),
                    "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(dtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.CopyBulkWorkGroups(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
            await _serviceMock.Received(1).CopySelectedWorkGroupsAsync(
                Arg.Any<IEnumerable<string>>(),
                "JC_SRC", "JC_TGT", "PRJ1");
        }

        [Fact]
        public async Task CopyBulkWorkGroups_WithEmptyWorkGroups_ReturnsOkWithEmptyList()
        {
            // Arrange — no work groups selected; service returns empty collection
            var request = new BulkCopyWorkGroupReq
            {
                ParentProject = "PRJ1",
                SourceJobCode = "JC_SRC",
                TargetJobCode = "JC_TGT",
                WorkGroups = []
            };
            var emptyDtos = Enumerable.Empty<TimeCodeValidDto>();
            var emptyMapped = Enumerable.Empty<TimeCodeValidRes>();

            _serviceMock
                .CopySelectedWorkGroupsAsync(
                    Arg.Any<IEnumerable<string>>(),
                    "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(emptyDtos);
            _mapperMock.Map<IEnumerable<TimeCodeValidRes>>(emptyDtos).Returns(emptyMapped);

            // Act
            var result = await _controller.CopyBulkWorkGroups(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Empty((IEnumerable<TimeCodeValidRes>)okResult.Value!);
        }

        [Fact]
        public async Task CopyBulkWorkGroups_ServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new BulkCopyWorkGroupReq
            {
                ParentProject = "PRJ1",
                SourceJobCode = "JC_SRC",
                TargetJobCode = "JC_TGT",
                WorkGroups = ["WG1"]
            };
            _serviceMock
                .CopySelectedWorkGroupsAsync(
                    Arg.Any<IEnumerable<string>>(),
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CopyBulkWorkGroups(request));
        }

        #endregion
    }
}
