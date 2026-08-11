using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectManagerControllerTest
{
    public class ProjectManagerControllerTests
    {
        private readonly IProjectManagerService _service;
        private readonly IMapper _mapper;
        private readonly ProjectManagerController _controller;

        public ProjectManagerControllerTests()
        {
            _service    = Substitute.For<IProjectManagerService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new ProjectManagerController(_service, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static ProjectManagerDto MakeDto(string name = "J. Smith") =>
            new ProjectManagerDto { ProjectManager = name, Email = "j.smith@apha.gov.uk", LoginEmail = "j.smith@login.apha.gov.uk", Disable = false };

        private static ProjectManagerRes MakeRes(string name = "J. Smith") =>
            new ProjectManagerRes { ProjectManager = name, Email = "j.smith@apha.gov.uk", LoginEmail = "j.smith@login.apha.gov.uk", Disable = false };

        private static ProjectManagerReq MakeReq(string name = "J. Smith") =>
            new ProjectManagerReq { ProjectManager = name, Email = "j.smith@apha.gov.uk", LoginEmail = "j.smith@login.apha.gov.uk", Disable = false };

        // ── GetAll ────────────────────────────────────────────────────────────────

        #region GetAll

        [Fact]
        public async Task GetAll_ServiceReturnsData_ReturnsOkWithMappedList()
        {
            // Arrange
            var pagedDto = new PaginatedResult<ProjectManagerDto>
            {
                Data = new List<ProjectManagerDto> { MakeDto("Smith"), MakeDto("Jones") },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 }
            };
            var pagedRes = new PaginationRes<ProjectManagerRes>
            {
                Data = new List<ProjectManagerRes> { MakeRes("Smith"), MakeRes("Jones") }
            };
            _service.GetPagedProjectManagersAsync(null).Returns(pagedDto);
            _mapper.Map<PaginationRes<ProjectManagerRes>>(pagedDto).Returns(pagedRes);

            // Act
            var result = await _controller.GetPagedProjectManagers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginationRes<ProjectManagerRes>>(ok.Value);
            Assert.Equal(2, returned.Data.Count());
            await _service.Received(1).GetPagedProjectManagersAsync(null);
        }

        [Fact]
        public async Task GetAll_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var pagedDto = new PaginatedResult<ProjectManagerDto>();
            var pagedRes = new PaginationRes<ProjectManagerRes>();
            _service.GetPagedProjectManagersAsync(null).Returns(pagedDto);
            _mapper.Map<PaginationRes<ProjectManagerRes>>(pagedDto).Returns(pagedRes);

            // Act
            var result = await _controller.GetPagedProjectManagers();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PaginationRes<ProjectManagerRes>>(ok.Value);
            Assert.Empty(returned.Data);
        }

        [Fact]
        public async Task GetAll_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetPagedProjectManagersAsync(null).ThrowsAsync(new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedProjectManagers());
        }

        #endregion

        // ── GetById ───────────────────────────────────────────────────────────────

        #region GetById

        [Fact]
        public async Task GetById_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string name = "J. Smith";
            var dto = MakeDto(name);
            var res = MakeRes(name);
            _service.GetProjectManagerByNameAsync(name).Returns(dto);
            _mapper.Map<ProjectManagerRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetProjectManagerByName(name);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
            await _service.Received(1).GetProjectManagerByNameAsync(name);
        }

        [Fact]
        public async Task GetById_ServiceReturnsNull_ReturnsNotFound()
        {
            // Arrange
            _service.GetProjectManagerByNameAsync(Arg.Any<string>()).Returns((ProjectManagerDto?)null);

            // Act
            var result = await _controller.GetProjectManagerByName("Unknown");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_DecodesNameBeforeServiceCall()
        {
            // Arrange — URL-encoded space %20 should be decoded
            const string encoded = "J.%20Smith";
            const string decoded = "J. Smith";
            _service.GetProjectManagerByNameAsync(decoded).Returns(MakeDto(decoded));
            _mapper.Map<ProjectManagerRes>(Arg.Any<ProjectManagerDto>()).Returns(MakeRes(decoded));

            // Act
            await _controller.GetProjectManagerByName(encoded);

            // Assert
            await _service.Received(1).GetProjectManagerByNameAsync(decoded);
        }

        #endregion

        // ── Create ────────────────────────────────────────────────────────────────

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtActionWithMappedResult()
        {
            // Arrange
            var req     = MakeReq("New Manager");
            var dto     = MakeDto("New Manager");
            var created = MakeDto("New Manager");
            var res     = MakeRes("New Manager");
            _mapper.Map<ProjectManagerDto>(req).Returns(dto);
            _service.CreateProjectManagerAsync(dto).Returns(created);
            _mapper.Map<ProjectManagerRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateProjectManager(req);

            // Assert
            var created201 = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ProjectManagerController.GetProjectManagerByName), created201.ActionName);
            Assert.Equal(res, created201.Value);
        }

        [Fact]
        public async Task Create_DuplicateName_PropagatesInvalidOperationException()
        {
            // Arrange
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(MakeDto());
            _service.CreateProjectManagerAsync(Arg.Any<ProjectManagerDto>()).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateProjectManager(MakeReq()));
        }

        #endregion

        // ── Update ────────────────────────────────────────────────────────────────

        #region Update

        [Fact]
        public async Task Update_ServiceReturnsDto_ReturnsOkWithMappedResult()
        {
            // Arrange
            const string name = "J. Smith";
            var dto     = MakeDto("");
            var updated = MakeDto(name);
            var res     = MakeRes(name);
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(dto);
            _service.UpdateProjectManagerAsync(Arg.Any<ProjectManagerDto>()).Returns(updated);
            _mapper.Map<ProjectManagerRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateProjectManager(name, MakeReq(name));

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task Update_SetsRouteNameOnDtoBeforeCallingService()
        {
            // Arrange — mapper returns empty name; controller sets dto.ProjectManager = route name
            const string routeName = "J. Smith";
            var dto = new ProjectManagerDto { ProjectManager = "" };
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(dto);
            _service.UpdateProjectManagerAsync(Arg.Any<ProjectManagerDto>()).Returns(MakeDto(routeName));
            _mapper.Map<ProjectManagerRes>(Arg.Any<ProjectManagerDto>()).Returns(MakeRes(routeName));

            // Act
            await _controller.UpdateProjectManager(routeName, MakeReq(""));

            // Assert
            await _service.Received(1).UpdateProjectManagerAsync(
                Arg.Is<ProjectManagerDto>(d => d.ProjectManager == routeName));
        }

        [Fact]
        public async Task Update_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _mapper.Map<ProjectManagerDto>(Arg.Any<ProjectManagerReq>()).Returns(MakeDto());
            _service.UpdateProjectManagerAsync(Arg.Any<ProjectManagerDto>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateProjectManager("Unknown", MakeReq("Unknown")));
        }

        #endregion

        // ── Delete ────────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ServiceCompletes_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteProjectManagerAsync("J. Smith").Returns(true);

            // Act
            var result = await _controller.DeleteProjectManager("J.%20Smith");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.True(Assert.IsType<bool>(ok.Value));
            await _service.Received(1).DeleteProjectManagerAsync("J. Smith");
        }

        [Fact]
        public async Task Delete_DecodesNameBeforeServiceCall()
        {
            // Arrange
            const string encoded = "J.%20Smith";
            const string decoded = "J. Smith";
            _service.DeleteProjectManagerAsync(Arg.Any<string>()).Returns(true);

            // Act
            await _controller.DeleteProjectManager(encoded);

            // Assert
            await _service.Received(1).DeleteProjectManagerAsync(decoded);
        }

        [Fact]
        public async Task Delete_ServiceThrowsKeyNotFoundException_PropagatesException()
        {
            // Arrange
            _service.DeleteProjectManagerAsync(Arg.Any<string>()).ThrowsAsync(new KeyNotFoundException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteProjectManager("Unknown"));
        }

        #endregion
    }
}
