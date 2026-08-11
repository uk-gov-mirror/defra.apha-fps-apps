using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IProjectRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectService _sut;

        public ProjectServiceTests()
        {
            _mockRepository = Substitute.For<IProjectRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectService(_mockRepository, _mockMapper);
        }

        #region GetAllProjectsAsync

        [Fact]
        public async Task GetAllProjectsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var projectEntities = new List<ProjectView>
            {
                new ProjectView { ParentProject = "PROJ001", ProjectTitle = "FMD Survey",    ProjectStatus = "Active",   Disease = "FMD",  Contract = "CON001" },
                new ProjectView { ParentProject = "PROJ002", ProjectTitle = "TB Eradication", ProjectStatus = "Active",  Disease = "TB",   Contract = "CON002" }
            };

            var expectedDtos = new List<ProjectDto>
            {
                new ProjectDto { ParentProject = "PROJ001", ProjectTitle = "FMD Survey",     ProjectStatus = "Active",  Disease = "FMD", Contract = "CON001" },
                new ProjectDto { ParentProject = "PROJ002", ProjectTitle = "TB Eradication",  ProjectStatus = "Active",  Disease = "TB",  Contract = "CON002" }
            };

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(projectEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(projectEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ParentProject.Should().Be("PROJ001");
            result.First().ProjectTitle.Should().Be("FMD Survey");

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(projectEntities);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<ProjectView>();
            var emptyDtos = new List<ProjectDto>();

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<ProjectView>>(null!));

            _mockMapper.Map<IEnumerable<ProjectDto>>(null)
                .Returns((IEnumerable<ProjectDto>?)null);

            // Act
            var result = await _sut.GetAllProjectsAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(null);
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectsAsync()
                .Returns(Task.FromException<IEnumerable<ProjectView>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllProjectsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ProjectDto>>(Arg.Any<IEnumerable<Project>>());
        }

        #endregion

        #region GetAllProjectsForAllUsersAsync

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PROJ001", ProjectTitle = "FMD Survey", ProjectStatus = "Active", Disease = "FMD", Contract = "CON001", Customer = "DEFRA", Program = "P001", IncomeAccountCode = "IAC01" },
                new() { ParentProject = "PROJ002", ProjectTitle = "TB Eradication", ProjectStatus = "Active", Disease = "TB", Contract = "CON002", Customer = "APHA", Program = "P002", IncomeAccountCode = "IAC02" }
            };

            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PROJ001", ProjectTitle = "FMD Survey" },
                new() { ParentProject = "PROJ002", ProjectTitle = "TB Eradication" }
            };

            _mockRepository.GetAllProjectsForAllUsersAsync()
                .Returns(Task.FromResult<IEnumerable<Project>>(projectEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(projectEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllProjectsForAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ParentProject.Should().Be("PROJ001");

            await _mockRepository.Received(1).GetAllProjectsForAllUsersAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(projectEntities);
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<Project>();
            var emptyDtos = new List<ProjectDto>();

            _mockRepository.GetAllProjectsForAllUsersAsync()
                .Returns(Task.FromResult<IEnumerable<Project>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ProjectDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllProjectsForAllUsersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllProjectsForAllUsersAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllProjectsForAllUsersAsync()
                .Returns(Task.FromException<IEnumerable<Project>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllProjectsForAllUsersAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAllProjectsForAllUsersAsync();
        }

        #endregion

        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_WithValidParentProject_ReturnsMappedDto()
        {
            // Arrange
            var parentProject = "PROJ001";

            var projectEntity = new Project
            {
                ParentProject = parentProject,
                ProjectTitle = "FMD Survey",
                ProjectStatus = "Active",
                Disease = "FMD",
                Contract = "CON001"
            };

            var expectedDto = new ProjectDto
            {
                ParentProject = parentProject,
                ProjectTitle = "FMD Survey",
                ProjectStatus = "Active",
                Disease = "FMD",
                Contract = "CON001"
            };

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(projectEntity));

            _mockMapper.Map<ProjectDto>(projectEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PROJ001");
            result.ProjectTitle.Should().Be("FMD Survey");
            result.ProjectStatus.Should().Be("Active");

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(projectEntity);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var parentProject = "PROJ999";

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Theory]        
        [InlineData("")]
        public async Task GetProjectByIdAsync_WithNullOrEmptyId_ReturnsNull(string parentProject)
        {
            // Arrange
            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(null));

            _mockMapper.Map<ProjectDto>(Arg.Any<Project?>())
                .Returns((ProjectDto?)null);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var parentProject = "PROJ001";

            var projectEntity = new Project
            {
                ParentProject = parentProject,
                ProjectTitle  = "FMD Survey",
                ProjectStatus = "Active",
                Disease       = "FMD",
                Contract      = "CON001"
            };

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromResult<Project?>(projectEntity));

            _mockMapper.Map<ProjectDto>(projectEntity)
                .Returns((ProjectDto?)null);

            // Act
            var result = await _sut.GetProjectByIdAsync(parentProject);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.Received(1).Map<ProjectDto>(projectEntity);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PROJ001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetProjectByIdAsync(parentProject)
                .Returns(Task.FromException<Project?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectByIdAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetProjectByIdAsync(parentProject);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region GetProjectsByProgramAsync

        [Fact]
        public async Task GetProjectsByProgramAsync_CallsRepositoryWithMappedParameters_AndReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "parentproject" };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0 }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<Project>(projectEntities, paginationData);
            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<ProjectDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<Project>(
                Enumerable.Empty<Project>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectsByProgramAsync(paginationParams, programNo)
                .Returns(Task.FromException<PagedData<Project>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectsByProgramAsync(query, programNo)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<Project>>());
        }

        #endregion

        #region GetAllPactProjectsAsync

        [Fact]
        public async Task GetAllPactProjectsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var pactEntities = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Eradication" }
            };
            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Eradication" }
            };

            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<PactProjectView>>(pactEntities));
            _mockMapper.Map<IEnumerable<ProjectDto>>(pactEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllPactProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().ParentProject.Should().Be("PP001");
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ProjectDto>>(pactEntities);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<PactProjectView>();
            var emptyDtos = new List<ProjectDto>();

            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromResult<IEnumerable<PactProjectView>>(emptyEntities));
            _mockMapper.Map<IEnumerable<ProjectDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllPactProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllPactProjectsAsync()
                .Returns(Task.FromException<IEnumerable<PactProjectView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllPactProjectsAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAllPactProjectsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ProjectDto>>(Arg.Any<IEnumerable<PactProjectView>>());
        }

        #endregion

        #region GetPagedProjectsAsync

        [Fact]
        public async Task GetPagedProjectsAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<Project>(projectEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<Project>(
                Enumerable.Empty<Project>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsAsync(paginationParams)
                .Returns(Task.FromException<PagedData<Project>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedProjectsAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedProjectsAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<Project>>());
        }

        #endregion

        #region GetPagedProjectSnapshotDataAsync

        [Fact]
        public async Task GetPagedProjectSnapshotDataAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<Project>(projectEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectSnapshotDataAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedProjectSnapshotDataAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectSnapshotDataAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedProjectSnapshotDataAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<Project>(
                Enumerable.Empty<Project>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectSnapshotDataAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedProjectSnapshotDataAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedProjectSnapshotDataAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedProjectSnapshotDataAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectSnapshotDataAsync(paginationParams)
                .Returns(Task.FromException<PagedData<Project>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedProjectSnapshotDataAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedProjectSnapshotDataAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<Project>>());
        }

        #endregion

        #region GetPagedProjectsByUserAsync

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var viewEntities = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", UserEmail = "test@example.com" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<ProjectView>(viewEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsByUserAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedProjectsByUserAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectsByUserAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<ProjectView>(
                Enumerable.Empty<ProjectView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsByUserAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedProjectsByUserAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedProjectsByUserAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedProjectsByUserAsync(paginationParams)
                .Returns(Task.FromException<PagedData<ProjectView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedProjectsByUserAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedProjectsByUserAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<ProjectView>>());
        }

        #endregion

        #region GetPagedPactProjectsAsync

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var pactEntities = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Alpha" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<PactProjectView>(pactEntities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedPactProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<PactProjectView>(
                Enumerable.Empty<PactProjectView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedPactProjectsAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsAsync(paginationParams)
                .Returns(Task.FromException<PagedData<PactProjectView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedPactProjectsAsync(query)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedPactProjectsAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<PactProjectView>>());
        }

        #endregion

        #region GetPagedPactProjectsByProgramAsync

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_CallsRepositoryWithMappedParameters_AndReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "parentproject" };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var projectEntities = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1 },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0 }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<PactProjectView>(projectEntities, paginationData);
            var expectedDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<ProjectDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsByProgramAsync(paginationParams, programNo).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedPactProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectDto>>(pagedData);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<PactProjectView>(
                Enumerable.Empty<PactProjectView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsByProgramAsync(paginationParams, programNo).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetPagedPactProjectsByProgramAsync(paginationParams, programNo);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetPagedPactProjectsByProgramAsync(paginationParams, programNo)
                .Returns(Task.FromException<PagedData<PactProjectView>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedPactProjectsByProgramAsync(query, programNo)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPagedPactProjectsByProgramAsync(paginationParams, programNo);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectDto>>(Arg.Any<PagedData<PactProjectView>>());
        }

        #endregion

        #region CreateProjectAsync

        [Fact]
        public async Task CreateProjectAsync_WithValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var createdEntity = new Project { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.CreateProjectAsync(projectEntity).Returns(createdEntity);
            _mockMapper.Map<ProjectDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.CreateProjectAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).CreateProjectAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(createdEntity);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.CreateProjectAsync(projectEntity)
                .Returns(Task.FromException<Project>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.CreateProjectAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).CreateProjectAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region UpdateProjectAsync

        [Fact]
        public async Task UpdateProjectAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var updatedEntity = new Project { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P001", Customer = "DEFRA", ProjectStatus = "Closed" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", ProjectStatus = "Closed" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdateProjectAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateProjectAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.ParentProject.Should().Be("PP001");
            result.ProjectStatus.Should().Be("Closed");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdateProjectAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdateProjectAsync(projectEntity)
                .Returns(Task.FromException<Project>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateProjectAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdateProjectAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region UpdatePactProjectDetailsAsync

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var updatedEntity = new Project { ParentProject = "PP001", ProjectTitle = "PACT Update", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Update" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdatePactProjectDetailsAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result!.ParentProject.Should().Be("PP001");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity).Returns((Project?)null);

            // Act
            var result = await _sut.UpdatePactProjectDetailsAsync(inputDto);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdatePactProjectDetailsAsync(projectEntity)
                .Returns(Task.FromException<Project?>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdatePactProjectDetailsAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdatePactProjectDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region UpdatePactPortfolioDetailsAsync

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Portfolio Update", Program = "P002", Manager = "Manager A", Finished = 1, Comments = "Done", BudgetCvl = 500m, TransferIncome = 600m };
            var projectEntity = new Project { ParentProject = "PP001", ProjectTitle = "Portfolio Update", Program = "P002", Manager = "Manager A", Finished = 1, Comments = "Done", BudgetCvl = 500m, TransferIncome = 600m };
            var updatedEntity = new Project { ParentProject = "PP001", ProjectTitle = "Portfolio Update", Program = "P002", Manager = "Manager A", Finished = 1, Comments = "Done", BudgetCvl = 500m, TransferIncome = 600m };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Portfolio Update" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactPortfolioDetailsAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdatePactPortfolioDetailsAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result!.ParentProject.Should().Be("PP001");
            result.ProjectTitle.Should().Be("Portfolio Update");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdatePactPortfolioDetailsAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP999", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactPortfolioDetailsAsync(projectEntity).Returns((Project?)null);

            // Act
            var result = await _sut.UpdatePactPortfolioDetailsAsync(inputDto);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).UpdatePactPortfolioDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };
            var projectEntity = new Project { ParentProject = "PP001", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdatePactPortfolioDetailsAsync(projectEntity)
                .Returns(Task.FromException<Project?>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdatePactPortfolioDetailsAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdatePactPortfolioDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region DeleteProjectAsync

        [Fact]
        public async Task DeleteProjectAsync_WithExistingProject_ReturnsTrue()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject).Returns(true);

            // Act
            var result = await _sut.DeleteProjectAsync(parentProject);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistingProject_ReturnsFalse()
        {
            // Arrange
            var parentProject = "PP999";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject).Returns(false);

            // Act
            var result = await _sut.DeleteProjectAsync(parentProject);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenProjectHasAssociatedJobCodes_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAsync(parentProject)
            );

            exception.Errors.Should().HaveCount(1);
            exception.Errors[0].Code.Should().Be("PROJECT_HAS_ASSOCIATIONS");
            exception.Errors[0].Message.Should().Contain(parentProject);
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.DidNotReceive().DeleteProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var parentProject = "PP001";
            _mockRepository.HasAssociatedJobCodesAsync(parentProject).Returns(false);
            _mockRepository.DeleteProjectAsync(parentProject)
                .Returns(Task.FromException<bool>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.DeleteProjectAsync(parentProject)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).HasAssociatedJobCodesAsync(parentProject);
            await _mockRepository.Received(1).DeleteProjectAsync(parentProject);
        }

        #endregion

        #region CheckProjectExistsAsync

        [Fact]
        public async Task CheckProjectExistsAsync_WhenProjectExists_ReturnsTrue()
        {
            _mockRepository.CheckProjectExistsAsync("PP001").Returns(true);

            var result = await _sut.CheckProjectExistsAsync("PP001");

            result.Should().BeTrue();
            await _mockRepository.Received(1).CheckProjectExistsAsync("PP001");
        }

        [Fact]
        public async Task CheckProjectExistsAsync_WhenProjectDoesNotExist_ReturnsFalse()
        {
            _mockRepository.CheckProjectExistsAsync("NOPE").Returns(false);

            var result = await _sut.CheckProjectExistsAsync("NOPE");

            result.Should().BeFalse();
            await _mockRepository.Received(1).CheckProjectExistsAsync("NOPE");
        }

        [Fact]
        public async Task CheckProjectExistsAsync_WhenNullCode_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CheckProjectExistsAsync(null!));
        }

        [Fact]
        public async Task CheckProjectExistsAsync_WhenRepositoryThrows_PropagatesException()
        {
            _mockRepository.CheckProjectExistsAsync("PP001")
                .Returns(Task.FromException<bool>(new Exception("Database connection failed")));

            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.CheckProjectExistsAsync("PP001"));

            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region CheckProjectExistsInFarmFileAsync

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_WhenExists_ReturnsTrue()
        {
            _mockRepository.CheckProjectExistsInFarmFileAsync("PP001").Returns(true);

            var result = await _sut.CheckProjectExistsInFarmFileAsync("PP001");

            result.Should().BeTrue();
            await _mockRepository.Received(1).CheckProjectExistsInFarmFileAsync("PP001");
        }

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_WhenNotExists_ReturnsFalse()
        {
            _mockRepository.CheckProjectExistsInFarmFileAsync("PP001").Returns(false);

            var result = await _sut.CheckProjectExistsInFarmFileAsync("PP001");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_WhenNullCode_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.CheckProjectExistsInFarmFileAsync(null!));
        }

        #endregion

        #region ChangeProjectCodeAsync

        [Fact]
        public async Task ChangeProjectCodeAsync_WithValidCodes_CallsRepository()
        {
            _mockRepository.CheckProjectExistsAsync("OLD1").Returns(true);
            _mockRepository.CheckProjectExistsAsync("NEW1").Returns(false);
            _mockRepository.CheckProjectExistsInFarmFileAsync("OLD1").Returns(false);

            await _sut.ChangeProjectCodeAsync("OLD1", "NEW1");

            await _mockRepository.Received(1).ChangeProjectCodeAsync("OLD1", "NEW1");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenOldCodeEmpty_ThrowsBusinessValidationErrorException()
        {
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.ChangeProjectCodeAsync("", "NEW1"));

            exception.Errors.Should().Contain(e => e.Code == "OLD_CODE_REQUIRED");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenNewCodeEmpty_ThrowsBusinessValidationErrorException()
        {
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.ChangeProjectCodeAsync("OLD1", ""));

            exception.Errors.Should().Contain(e => e.Code == "NEW_CODE_REQUIRED");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenOldCodeNotFound_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.CheckProjectExistsAsync("OLD1").Returns(false);
            _mockRepository.CheckProjectExistsAsync("NEW1").Returns(false);
            _mockRepository.CheckProjectExistsInFarmFileAsync("OLD1").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.ChangeProjectCodeAsync("OLD1", "NEW1"));

            exception.Errors.Should().Contain(e => e.Code == "OLD_CODE_NOT_FOUND");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenNewCodeAlreadyExists_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.CheckProjectExistsAsync("OLD1").Returns(true);
            _mockRepository.CheckProjectExistsAsync("NEW1").Returns(true);
            _mockRepository.CheckProjectExistsInFarmFileAsync("OLD1").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.ChangeProjectCodeAsync("OLD1", "NEW1"));

            exception.Errors.Should().Contain(e => e.Code == "CODE_ALREADY_EXISTS");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenFarmFileDataExists_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.CheckProjectExistsAsync("OLD1").Returns(true);
            _mockRepository.CheckProjectExistsAsync("NEW1").Returns(false);
            _mockRepository.CheckProjectExistsInFarmFileAsync("OLD1").Returns(true);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.ChangeProjectCodeAsync("OLD1", "NEW1"));

            exception.Errors.Should().Contain(e => e.Code == "FARM_FILE_DATA_EXISTS");
        }

        #endregion

        #region DeleteProjectAndChildrenAsync

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WithValidProject_CallsRepository()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            await _sut.DeleteProjectAndChildrenAsync("PP001");

            await _mockRepository.Received(1).DeleteProjectAndChildrenAsync("PP001");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenEmptyCode_ThrowsBusinessValidationErrorException()
        {
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync(""));

            exception.Errors.Should().Contain(e => e.Code == "PARENT_PROJECT_REQUIRED");
            await _mockRepository.DidNotReceive().DeleteProjectAndChildrenAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenHasPlannedTests_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(true);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().Contain(e => e.Code == "HAS_PLANNED_TESTS");
            await _mockRepository.DidNotReceive().DeleteProjectAndChildrenAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenHasMonthlyOutput_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(true);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().Contain(e => e.Code == "HAS_MONTHLY_OUTPUT");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenHasMonthlyTime_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(true);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().Contain(e => e.Code == "HAS_MONTHLY_TIME");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenHasInvoices_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(true);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().Contain(e => e.Code == "HAS_INVOICES");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenHasSubcontracts_ThrowsBusinessValidationErrorException()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(false);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(true);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().Contain(e => e.Code == "HAS_SUBCONTRACTS");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenMultipleBlockers_ThrowsWithAllErrors()
        {
            _mockRepository.HasPlannedTestsAsync("PP001").Returns(true);
            _mockRepository.HasMonthlyOutputAsync("PP001").Returns(true);
            _mockRepository.HasMonthlyTimeAsync("PP001").Returns(false);
            _mockRepository.HasProjectInvoicesAsync("PP001").Returns(false);
            _mockRepository.HasProjectSubcontractsAsync("PP001").Returns(false);

            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.DeleteProjectAndChildrenAsync("PP001"));

            exception.Errors.Should().HaveCount(2);
            exception.Errors.Should().Contain(e => e.Code == "HAS_PLANNED_TESTS");
            exception.Errors.Should().Contain(e => e.Code == "HAS_MONTHLY_OUTPUT");
            await _mockRepository.DidNotReceive().DeleteProjectAndChildrenAsync(Arg.Any<string>());
        }

        #endregion

        #region UpdateFpsPortfolioDetailsAsync

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var inputDto = new ProjectDto
            {
                ParentProject = "PP001", ProjectTitle = "FPS Portfolio Update",
                Program = "P002", Manager = "Manager A",
                Disease = "FMD", ProjectStatus = "Active",
                TransferIncome = 500m, CustIncome = 600m, Profit = 150m,
                Contract = "C001", Customer = "DEFRA"
            };
            var projectEntity = new Project
            {
                ParentProject = "PP001", ProjectTitle = "FPS Portfolio Update",
                Program = "P002", Manager = "Manager A",
                Disease = "FMD", ProjectStatus = "Active",
                TransferIncome = 500m, CustIncome = 600m, Profit = 150m,
                Contract = "C001", Customer = "DEFRA"
            };
            var updatedEntity = new Project
            {
                ParentProject = "PP001", ProjectTitle = "FPS Portfolio Update",
                Program = "P002", Manager = "Manager A",
                Disease = "FMD", ProjectStatus = "Active",
                TransferIncome = 500m, CustIncome = 600m, Profit = 150m,
                Contract = "C001", Customer = "DEFRA"
            };
            var expectedDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "FPS Portfolio Update" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(inputDto.Program).Returns(true);
            _mockRepository.UpdateFpsPortfolioDetailsAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateFpsPortfolioDetailsAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result!.ParentProject.Should().Be("PP001");
            result.ProjectTitle.Should().Be("FPS Portfolio Update");
            _mockMapper.Received(1).Map<Project>(inputDto);
            await _mockRepository.Received(1).UpdateFpsPortfolioDetailsAsync(projectEntity);
            _mockMapper.Received(1).Map<ProjectDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WhenProjectNotFound_ReturnsNull()
        {
            // Arrange
            var inputDto = new ProjectDto
            {
                ParentProject = "PP999", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active"
            };
            var projectEntity = new Project
            {
                ParentProject = "PP999", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active"
            };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdateFpsPortfolioDetailsAsync(projectEntity).Returns((Project?)null);

            // Act
            var result = await _sut.UpdateFpsPortfolioDetailsAsync(inputDto);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).UpdateFpsPortfolioDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WhenProgramDoesNotExist_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var inputDto = new ProjectDto
            {
                ParentProject = "PP001", Program = "INVALID",
                Customer = "DEFRA", ProjectStatus = "Active"
            };

            _mockRepository.CheckProgramExistsAsync("INVALID").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateFpsPortfolioDetailsAsync(inputDto));

            exception.Errors.Should().ContainSingle(e => e.Code == "PROGRAM_NOT_FOUND");
            await _mockRepository.DidNotReceive().UpdateFpsPortfolioDetailsAsync(Arg.Any<Project>());
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WhenProgramIsNullOrEmpty_SkipsProgramCheck()
        {
            // Arrange — null/empty program bypasses the FK guard
            var inputDto = new ProjectDto
            {
                ParentProject = "PP001", Program = null!,
                Customer = "DEFRA", ProjectStatus = "Active"
            };
            var projectEntity = new Project { ParentProject = "PP001", Customer = "DEFRA", ProjectStatus = "Active" };
            var updatedEntity = new Project { ParentProject = "PP001" };
            var expectedDto   = new ProjectDto { ParentProject = "PP001" };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.UpdateFpsPortfolioDetailsAsync(projectEntity).Returns(updatedEntity);
            _mockMapper.Map<ProjectDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateFpsPortfolioDetailsAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.DidNotReceive().CheckProgramExistsAsync(Arg.Any<string>());
            await _mockRepository.Received(1).UpdateFpsPortfolioDetailsAsync(projectEntity);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new ProjectDto
            {
                ParentProject = "PP001", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active"
            };
            var projectEntity = new Project
            {
                ParentProject = "PP001", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active"
            };

            _mockMapper.Map<Project>(inputDto).Returns(projectEntity);
            _mockRepository.CheckProgramExistsAsync(Arg.Any<string>()).Returns(true);
            _mockRepository.UpdateFpsPortfolioDetailsAsync(projectEntity)
                .Returns(Task.FromException<Project?>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateFpsPortfolioDetailsAsync(inputDto));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdateFpsPortfolioDetailsAsync(projectEntity);
            _mockMapper.DidNotReceive().Map<ProjectDto>(Arg.Any<Project>());
        }

        #endregion

        #region GetPagedProjectSpecificQueryAsync

        [Fact]
        public async Task GetPagedProjectSpecificQueryAsync_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paramMapped = new PaginationParameters<string>(page: 1, pageSize: 10);
            var pagedData = new PagedData<ProjectSpecificQueryItem>(
                new List<ProjectSpecificQueryItem> { new() { ParentProject = "PP001", Account = "ACC1" } },
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var expected = new PaginatedResult<ProjectSpecificQueryDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paramMapped);
            _mockRepository.GetPagedProjectSpecificQueryAsync(paramMapped).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSpecificQueryDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedProjectSpecificQueryAsync(query);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).GetPagedProjectSpecificQueryAsync(paramMapped);
        }

        #endregion

        #region GetProjectExceptionalCostsPagedAsync

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var entities = new List<ProjectExceptionalCostView>
            {
                new() { Directorate = "DIR1", Programme = "P001", Project = "PP001", AccountCat = "ACC1", ItemCost = 100m }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<ProjectExceptionalCostView>(entities, paginationData);
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<ProjectExceptionalCostViewDto>(
                new List<ProjectExceptionalCostViewDto> { new() { Directorate = "DIR1", Project = "PP001" } }, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectExceptionalCostsPagedAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectExceptionalCostViewDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().Project.Should().Be("PP001");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetProjectExceptionalCostsPagedAsync(paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<ProjectExceptionalCostViewDto>>(pagedData);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithEmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<ProjectExceptionalCostView>(
                Enumerable.Empty<ProjectExceptionalCostView>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ProjectExceptionalCostViewDto>(
                Enumerable.Empty<ProjectExceptionalCostViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectExceptionalCostsPagedAsync(paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ProjectExceptionalCostViewDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            await _mockRepository.Received(1).GetProjectExceptionalCostsPagedAsync(paginationParams);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetProjectExceptionalCostsPagedAsync(paginationParams)
                .Returns(Task.FromException<PagedData<ProjectExceptionalCostView>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetProjectExceptionalCostsPagedAsync(query));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetProjectExceptionalCostsPagedAsync(paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<ProjectExceptionalCostViewDto>>(
                Arg.Any<PagedData<ProjectExceptionalCostView>>());
        }

        #endregion
    }
}
