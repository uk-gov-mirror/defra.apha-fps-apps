using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectStaffReplanServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectStaffReplanServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        // ── GetProjectStaffReplanAsync Tests ──────────────────────────────────

        #region GetProjectStaffReplanAsync

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var views = new List<ProjectStaffReplanView>
            {
                new() { WorkGroup = workgroup, WgGrade = "WG01", GradeCode = "GC01", Name = "Smith, John",  PlannedHours = 10.0, ParentProject = "PP001", Program = "P001" },
                new() { WorkGroup = workgroup, WgGrade = "WG01", GradeCode = "GC01", Name = "Jones, Alice", PlannedHours = 8.0,  ParentProject = "PP002", Program = "P001" }
            };
            var pagedData = new PagedData<ProjectStaffReplanView>(
                views, new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            var expectedDtos = new List<ProjectStaffReplanDto>
            {
                new() { WorkGroup = workgroup, WgGrade = "WG01", Name = "Smith, John",  PlannedHours = 10.0, ParentProject = "PP001", Program = "P001" },
                new() { WorkGroup = workgroup, WgGrade = "WG01", Name = "Jones, Alice", PlannedHours = 8.0,  ParentProject = "PP002", Program = "P001" }
            };
            var expectedResult = new PaginatedResult<ProjectStaffReplanDto>(
                expectedDtos, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectStaffReplanAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().WorkGroup.Should().Be(workgroup);
            result.Data.First().Name.Should().Be("Smith, John");
            await _mockRepository.Received(1).GetProjectStaffReplanAsync(paginationParams, workgroup);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var pagedData = new PagedData<ProjectStaffReplanView>(
                [], new PaginationData { TotalRecords = 0 });
            var expectedResult = new PaginatedResult<ProjectStaffReplanDto>(
                [], new PaginationDto { TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectStaffReplanAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            await _mockRepository.Received(1).GetProjectStaffReplanAsync(paginationParams, workgroup);
        }

        [Theory]
        [InlineData("WorkGroupA")]
        [InlineData("WG-Budget-001")]
        [InlineData("QA-Group")]
        public async Task GetProjectStaffReplanAsync_WithVariousWorkgroups_PassesWorkgroupToRepository(string workgroup)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var pagedData = new PagedData<ProjectStaffReplanView>([], new PaginationData());
            var expectedResult = new PaginatedResult<ProjectStaffReplanDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectStaffReplanAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetProjectStaffReplanAsync(paginationParams, workgroup);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_MapperIsCalledTwice_ForQueryAndResult()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ProjectStaffReplanView>([], new PaginationData());
            var expectedResult = new PaginatedResult<ProjectStaffReplanDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetProjectStaffReplanAsync(query, workgroup);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMultipleStaffRows_ReturnsMappedResultWithCorrectCount()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 5 };

            var views = Enumerable.Range(1, 5).Select(i => new ProjectStaffReplanView
            {
                WorkGroup = workgroup, WgGrade = $"WG0{i}", Name = $"Staff{i}, Name", PlannedHours = i * 2.0
            }).ToList();
            var pagedData = new PagedData<ProjectStaffReplanView>(
                views, new PaginationData { PageNumber = 1, PageSize = 5, TotalRecords = 5 });

            var dtos = views.Select(v => new ProjectStaffReplanDto { WorkGroup = v.WorkGroup, Name = v.Name, PlannedHours = v.PlannedHours }).ToList();
            var expectedResult = new PaginatedResult<ProjectStaffReplanDto>(
                dtos, new PaginationDto { PageNumber = 1, PageSize = 5, TotalRecords = 5 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffReplanDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectStaffReplanAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(5);
            result.PaginationData.TotalRecords.Should().Be(5);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectStaffReplanAsync(paginationParams, workgroup)
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetProjectStaffReplanAsync(query, workgroup));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion
    }
}
