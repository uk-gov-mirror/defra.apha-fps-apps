using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectAuditTrailControllerTest
{
    public class ProjectAuditTrailControllerTests
    {
        private const string TestProject = "PROJ001";

        private readonly IProjectAuditTrailService _service;
        private readonly IMapper _mapper;
        private readonly ProjectAuditTrailController _controller;

        public ProjectAuditTrailControllerTests()
        {
            _service = Substitute.For<IProjectAuditTrailService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectAuditTrailController(_service, _mapper);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_ValidProjectNoDateRange_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectLogDto>
            {
                Data = new List<ProjectLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedResponse = new PaginationRes<ProjectLogRes>
            {
                Data = new List<ProjectLogRes> { new() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetProjectLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetProjectLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<ProjectLogRes>>(ok.Value);
            Assert.Single(data.Data);
        }

        [Fact]
        public async Task GetProjectLogsAsync_EmptyProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetProjectLogsAsync(query, ""));
        }

        [Fact]
        public async Task GetProjectLogsAsync_WhitespaceProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetProjectLogsAsync(query, "   "));
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithDateRange_PassesConvertedDatesToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 1, 1);
            var toDate = new DateOnly(2024, 12, 31);
            var expectedFrom = fromDate.ToDateTime(TimeOnly.MinValue);
            var expectedTo = toDate.ToDateTime(TimeOnly.MaxValue);

            var serviceResult = new PaginatedResult<ProjectLogDto>
            {
                Data = new List<ProjectLogDto>(),
                PaginationData = new PaginationDto()
            };
            _service.GetProjectLogsAsync(query, TestProject, expectedFrom, expectedTo)
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectLogRes>>(serviceResult)
                .Returns(new PaginationRes<ProjectLogRes> { Data = new List<ProjectLogRes>(), PaginationData = new Pagination() });

            // Act
            var result = await _controller.GetProjectLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _service.Received(1).GetProjectLogsAsync(query, TestProject, expectedFrom, expectedTo);
        }

        [Fact]
        public async Task GetProjectLogsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectLogDto>
            {
                Data = new List<ProjectLogDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            var mappedResponse = new PaginationRes<ProjectLogRes>
            {
                Data = new List<ProjectLogRes>(),
                PaginationData = new Pagination { TotalRecords = 0 }
            };

            _service.GetProjectLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetProjectLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<ProjectLogRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_ValidProjectNoDateRange_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<StaffJobLogDto>
            {
                Data = new List<StaffJobLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedResponse = new PaginationRes<StaffJobLogRes>
            {
                Data = new List<StaffJobLogRes> { new() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetStaffJobLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<StaffJobLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PaginationRes<StaffJobLogRes>>(ok.Value);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_EmptyProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetStaffJobLogsAsync(query, ""));
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithDateRange_PassesConvertedDatesToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 3, 1);
            var toDate = new DateOnly(2024, 3, 31);
            var expectedFrom = fromDate.ToDateTime(TimeOnly.MinValue);
            var expectedTo = toDate.ToDateTime(TimeOnly.MaxValue);

            _service.GetStaffJobLogsAsync(query, TestProject, expectedFrom, expectedTo)
                .Returns(new PaginatedResult<StaffJobLogDto> { Data = new List<StaffJobLogDto>(), PaginationData = new PaginationDto() });
            _mapper.Map<PaginationRes<StaffJobLogRes>>(Arg.Any<PaginatedResult<StaffJobLogDto>>())
                .Returns(new PaginationRes<StaffJobLogRes> { Data = new List<StaffJobLogRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetStaffJobLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _service.Received(1).GetStaffJobLogsAsync(query, TestProject, expectedFrom, expectedTo);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<StaffJobLogDto>
            {
                Data = new List<StaffJobLogDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            _service.GetStaffJobLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<StaffJobLogRes>>(serviceResult)
                .Returns(new PaginationRes<StaffJobLogRes> { Data = new List<StaffJobLogRes>(), PaginationData = new Pagination() });

            // Act
            var result = await _controller.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<StaffJobLogRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_ValidProjectNoDateRange_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementLogDto>
            {
                Data = new List<TestRequirementLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedResponse = new PaginationRes<TestRequirementLogRes>
            {
                Data = new List<TestRequirementLogRes> { new() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetTestRequirementLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PaginationRes<TestRequirementLogRes>>(ok.Value);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_EmptyProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetTestRequirementLogsAsync(query, ""));
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithDateRange_PassesConvertedDatesToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 6, 1);
            var toDate = new DateOnly(2024, 6, 30);

            _service.GetTestRequirementLogsAsync(query, TestProject,
                    fromDate.ToDateTime(TimeOnly.MinValue),
                    toDate.ToDateTime(TimeOnly.MaxValue))
                .Returns(new PaginatedResult<TestRequirementLogDto> { Data = new List<TestRequirementLogDto>(), PaginationData = new PaginationDto() });
            _mapper.Map<PaginationRes<TestRequirementLogRes>>(Arg.Any<PaginatedResult<TestRequirementLogDto>>())
                .Returns(new PaginationRes<TestRequirementLogRes> { Data = new List<TestRequirementLogRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetTestRequirementLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _service.Received(1).GetTestRequirementLogsAsync(
                query, TestProject,
                fromDate.ToDateTime(TimeOnly.MinValue),
                toDate.ToDateTime(TimeOnly.MaxValue));
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementLogDto>
            {
                Data = new List<TestRequirementLogDto>(),
                PaginationData = new PaginationDto()
            };
            _service.GetTestRequirementLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementLogRes>>(serviceResult)
                .Returns(new PaginationRes<TestRequirementLogRes> { Data = new List<TestRequirementLogRes>(), PaginationData = new Pagination() });

            // Act
            var result = await _controller.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestRequirementLogRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ValidProjectNoDateRange_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AnimalRequestLogDto>
            {
                Data = new List<AnimalRequestLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedResponse = new PaginationRes<AnimalRequestLogRes>
            {
                Data = new List<AnimalRequestLogRes> { new() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetAnimalRequestLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<AnimalRequestLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PaginationRes<AnimalRequestLogRes>>(ok.Value);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_EmptyProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetAnimalRequestLogsAsync(query, ""));
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithDateRange_PassesConvertedDatesToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 9, 1);
            var toDate = new DateOnly(2024, 9, 30);

            _service.GetAnimalRequestLogsAsync(query, TestProject,
                    fromDate.ToDateTime(TimeOnly.MinValue),
                    toDate.ToDateTime(TimeOnly.MaxValue))
                .Returns(new PaginatedResult<AnimalRequestLogDto> { Data = new List<AnimalRequestLogDto>(), PaginationData = new PaginationDto() });
            _mapper.Map<PaginationRes<AnimalRequestLogRes>>(Arg.Any<PaginatedResult<AnimalRequestLogDto>>())
                .Returns(new PaginationRes<AnimalRequestLogRes> { Data = new List<AnimalRequestLogRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetAnimalRequestLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _service.Received(1).GetAnimalRequestLogsAsync(
                query, TestProject,
                fromDate.ToDateTime(TimeOnly.MinValue),
                toDate.ToDateTime(TimeOnly.MaxValue));
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AnimalRequestLogDto>
            {
                Data = new List<AnimalRequestLogDto>(),
                PaginationData = new PaginationDto()
            };
            _service.GetAnimalRequestLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<AnimalRequestLogRes>>(serviceResult)
                .Returns(new PaginationRes<AnimalRequestLogRes> { Data = new List<AnimalRequestLogRes>(), PaginationData = new Pagination() });

            // Act
            var result = await _controller.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<AnimalRequestLogRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ValidProjectNoDateRange_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AdditionalCostLogDto>
            {
                Data = new List<AdditionalCostLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };
            var mappedResponse = new PaginationRes<AdditionalCostLogRes>
            {
                Data = new List<AdditionalCostLogRes> { new() },
                PaginationData = new Pagination { TotalRecords = 1 }
            };

            _service.GetAdditionalCostLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<AdditionalCostLogRes>>(serviceResult).Returns(mappedResponse);

            // Act
            var result = await _controller.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PaginationRes<AdditionalCostLogRes>>(ok.Value);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_EmptyProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _controller.GetAdditionalCostLogsAsync(query, ""));
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithDateRange_PassesConvertedDatesToService()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 11, 1);
            var toDate = new DateOnly(2024, 11, 30);

            _service.GetAdditionalCostLogsAsync(query, TestProject,
                    fromDate.ToDateTime(TimeOnly.MinValue),
                    toDate.ToDateTime(TimeOnly.MaxValue))
                .Returns(new PaginatedResult<AdditionalCostLogDto> { Data = new List<AdditionalCostLogDto>(), PaginationData = new PaginationDto() });
            _mapper.Map<PaginationRes<AdditionalCostLogRes>>(Arg.Any<PaginatedResult<AdditionalCostLogDto>>())
                .Returns(new PaginationRes<AdditionalCostLogRes> { Data = new List<AdditionalCostLogRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetAdditionalCostLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _service.Received(1).GetAdditionalCostLogsAsync(
                query, TestProject,
                fromDate.ToDateTime(TimeOnly.MinValue),
                toDate.ToDateTime(TimeOnly.MaxValue));
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<AdditionalCostLogDto>
            {
                Data = new List<AdditionalCostLogDto>(),
                PaginationData = new PaginationDto()
            };
            _service.GetAdditionalCostLogsAsync(query, TestProject, null, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<AdditionalCostLogRes>>(serviceResult)
                .Returns(new PaginationRes<AdditionalCostLogRes> { Data = new List<AdditionalCostLogRes>(), PaginationData = new Pagination() });

            // Act
            var result = await _controller.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<AdditionalCostLogRes>>(ok.Value);
            Assert.Empty(data.Data);
        }

        #endregion
    }
}
