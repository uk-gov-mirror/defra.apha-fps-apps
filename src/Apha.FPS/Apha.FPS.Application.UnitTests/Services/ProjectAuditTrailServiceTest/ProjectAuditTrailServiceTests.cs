using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProjectAuditTrailServiceTest
{
    public class ProjectAuditTrailServiceTests
    {
        private const string TestProject = "PROJ001";

        private readonly IProjectAuditTrailRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectAuditTrailService _service;

        public ProjectAuditTrailServiceTests()
        {
            _repository = Substitute.For<IProjectAuditTrailRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new ProjectAuditTrailService(_repository, _mapper);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_ValidParams_DelegatesToRepositoryAndMapsResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.ProjectLog>
            {
                Data = new List<Apha.FPS.Core.Entities.ProjectLog> { new() },
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 1 }
            };
            var expected = new PaginatedResult<ProjectLogDto>
            {
                Data = new List<ProjectLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetProjectLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<ProjectLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetProjectLogsAsync(paginationParams, TestProject, null, null);
        }

        [Fact]
        public async Task GetProjectLogsAsync_RepositoryReturnsEmpty_ReturnsMappedEmpty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.ProjectLog>
            {
                Data = new List<Apha.FPS.Core.Entities.ProjectLog>(),
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 0 }
            };
            var expected = new PaginatedResult<ProjectLogDto>
            {
                Data = new List<ProjectLogDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetProjectLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<ProjectLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectLogsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GetProjectLogsAsync(null!, TestProject, null, null));
        }

        [Fact]
        public async Task GetProjectLogsAsync_NullOrEmptyParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetProjectLogsAsync(query, "", null, null));
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateTime(2024, 1, 1);
            var toDate = new DateTime(2024, 12, 31);

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetProjectLogsAsync(paginationParams, TestProject, fromDate, toDate)
                .Returns(new PagedData<Apha.FPS.Core.Entities.ProjectLog>
                {
                    Data = new List<Apha.FPS.Core.Entities.ProjectLog>(),
                    PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
                });
            _mapper.Map<PaginatedResult<ProjectLogDto>>(Arg.Any<PagedData<Apha.FPS.Core.Entities.ProjectLog>>())
                .Returns(new PaginatedResult<ProjectLogDto> { Data = new List<ProjectLogDto>(), PaginationData = new PaginationDto() });

            // Act
            await _service.GetProjectLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _repository.Received(1).GetProjectLogsAsync(paginationParams, TestProject, fromDate, toDate);
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_ValidParams_DelegatesToRepositoryAndMapsResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.StaffJobLog>
            {
                Data = new List<Apha.FPS.Core.Entities.StaffJobLog> { new() },
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 1 }
            };
            var expected = new PaginatedResult<StaffJobLogDto>
            {
                Data = new List<StaffJobLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetStaffJobLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<StaffJobLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetStaffJobLogsAsync(paginationParams, TestProject, null, null);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GetStaffJobLogsAsync(null!, TestProject, null, null));
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_EmptyParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetStaffJobLogsAsync(query, "", null, null));
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_RepositoryReturnsEmpty_ReturnsMappedEmpty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.StaffJobLog>
            {
                Data = new List<Apha.FPS.Core.Entities.StaffJobLog>(),
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 0 }
            };
            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetStaffJobLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<StaffJobLogDto>>(repoData)
                .Returns(new PaginatedResult<StaffJobLogDto> { Data = new List<StaffJobLogDto>(), PaginationData = new PaginationDto() });

            // Act
            var result = await _service.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WhitespaceParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetStaffJobLogsAsync(query, "   ", null, null));
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var query          = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var fromDate       = new DateTime(2024, 1, 1);
            var toDate         = new DateTime(2024, 12, 31);

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetStaffJobLogsAsync(paginationParams, TestProject, fromDate, toDate)
                .Returns(new PagedData<Apha.FPS.Core.Entities.StaffJobLog>
                {
                    Data           = new List<Apha.FPS.Core.Entities.StaffJobLog>(),
                    PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
                });
            _mapper.Map<PaginatedResult<StaffJobLogDto>>(Arg.Any<PagedData<Apha.FPS.Core.Entities.StaffJobLog>>())
                .Returns(new PaginatedResult<StaffJobLogDto> { Data = new List<StaffJobLogDto>(), PaginationData = new PaginationDto() });

            // Act
            await _service.GetStaffJobLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _repository.Received(1).GetStaffJobLogsAsync(paginationParams, TestProject, fromDate, toDate);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_ValidParams_DelegatesToRepositoryAndMapsResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.TestRequirementLog>
            {
                Data = new List<Apha.FPS.Core.Entities.TestRequirementLog> { new() },
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 1 }
            };
            var expected = new PaginatedResult<TestRequirementLogDto>
            {
                Data = new List<TestRequirementLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetTestRequirementLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<TestRequirementLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetTestRequirementLogsAsync(paginationParams, TestProject, null, null);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_EmptyParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetTestRequirementLogsAsync(query, "   ", null, null));
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_RepositoryReturnsEmpty_ReturnsMappedEmpty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.TestRequirementLog>
            {
                Data = new List<Apha.FPS.Core.Entities.TestRequirementLog>(),
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
            };
            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetTestRequirementLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<TestRequirementLogDto>>(repoData)
                .Returns(new PaginatedResult<TestRequirementLogDto> { Data = new List<TestRequirementLogDto>(), PaginationData = new PaginationDto() });

            // Act
            var result = await _service.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GetTestRequirementLogsAsync(null!, TestProject, null, null));
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var query          = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var fromDate       = new DateTime(2024, 1, 1);
            var toDate         = new DateTime(2024, 12, 31);

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetTestRequirementLogsAsync(paginationParams, TestProject, fromDate, toDate)
                .Returns(new PagedData<Apha.FPS.Core.Entities.TestRequirementLog>
                {
                    Data           = new List<Apha.FPS.Core.Entities.TestRequirementLog>(),
                    PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
                });
            _mapper.Map<PaginatedResult<TestRequirementLogDto>>(Arg.Any<PagedData<Apha.FPS.Core.Entities.TestRequirementLog>>())
                .Returns(new PaginatedResult<TestRequirementLogDto> { Data = new List<TestRequirementLogDto>(), PaginationData = new PaginationDto() });

            // Act
            await _service.GetTestRequirementLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _repository.Received(1).GetTestRequirementLogsAsync(paginationParams, TestProject, fromDate, toDate);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ValidParams_DelegatesToRepositoryAndMapsResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.AnimalRequestLog>
            {
                Data = new List<Apha.FPS.Core.Entities.AnimalRequestLog> { new() },
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 1 }
            };
            var expected = new PaginatedResult<AnimalRequestLogDto>
            {
                Data = new List<AnimalRequestLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAnimalRequestLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<AnimalRequestLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetAnimalRequestLogsAsync(paginationParams, TestProject, null, null);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_EmptyParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetAnimalRequestLogsAsync(query, "", null, null));
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_RepositoryReturnsEmpty_ReturnsMappedEmpty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.AnimalRequestLog>
            {
                Data = new List<Apha.FPS.Core.Entities.AnimalRequestLog>(),
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
            };
            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAnimalRequestLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<AnimalRequestLogDto>>(repoData)
                .Returns(new PaginatedResult<AnimalRequestLogDto> { Data = new List<AnimalRequestLogDto>(), PaginationData = new PaginationDto() });

            // Act
            var result = await _service.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GetAnimalRequestLogsAsync(null!, TestProject, null, null));
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WhitespaceParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetAnimalRequestLogsAsync(query, "   ", null, null));
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var query          = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var fromDate       = new DateTime(2024, 1, 1);
            var toDate         = new DateTime(2024, 12, 31);

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAnimalRequestLogsAsync(paginationParams, TestProject, fromDate, toDate)
                .Returns(new PagedData<Apha.FPS.Core.Entities.AnimalRequestLog>
                {
                    Data           = new List<Apha.FPS.Core.Entities.AnimalRequestLog>(),
                    PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
                });
            _mapper.Map<PaginatedResult<AnimalRequestLogDto>>(Arg.Any<PagedData<Apha.FPS.Core.Entities.AnimalRequestLog>>())
                .Returns(new PaginatedResult<AnimalRequestLogDto> { Data = new List<AnimalRequestLogDto>(), PaginationData = new PaginationDto() });

            // Act
            await _service.GetAnimalRequestLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _repository.Received(1).GetAnimalRequestLogsAsync(paginationParams, TestProject, fromDate, toDate);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ValidParams_DelegatesToRepositoryAndMapsResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.AdditionalCostLog>
            {
                Data = new List<Apha.FPS.Core.Entities.AdditionalCostLog> { new() },
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData { TotalRecords = 1 }
            };
            var expected = new PaginatedResult<AdditionalCostLogDto>
            {
                Data = new List<AdditionalCostLogDto> { new() },
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAdditionalCostLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<AdditionalCostLogDto>>(repoData).Returns(expected);

            // Act
            var result = await _service.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetAdditionalCostLogsAsync(paginationParams, TestProject, null, null);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_EmptyParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetAdditionalCostLogsAsync(query, "", null, null));
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_RepositoryReturnsEmpty_ReturnsMappedEmpty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoData = new PagedData<Apha.FPS.Core.Entities.AdditionalCostLog>
            {
                Data = new List<Apha.FPS.Core.Entities.AdditionalCostLog>(),
                PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
            };
            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAdditionalCostLogsAsync(paginationParams, TestProject, null, null).Returns(repoData);
            _mapper.Map<PaginatedResult<AdditionalCostLogDto>>(repoData)
                .Returns(new PaginatedResult<AdditionalCostLogDto> { Data = new List<AdditionalCostLogDto>(), PaginationData = new PaginationDto() });

            // Act
            var result = await _service.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GetAdditionalCostLogsAsync(null!, TestProject, null, null));
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WhitespaceParentProject_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.GetAdditionalCostLogsAsync(query, "   ", null, null));
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithDateRange_PassesDateRangeToRepository()
        {
            // Arrange
            var query          = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var fromDate       = new DateTime(2024, 1, 1);
            var toDate         = new DateTime(2024, 12, 31);

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAdditionalCostLogsAsync(paginationParams, TestProject, fromDate, toDate)
                .Returns(new PagedData<Apha.FPS.Core.Entities.AdditionalCostLog>
                {
                    Data           = new List<Apha.FPS.Core.Entities.AdditionalCostLog>(),
                    PaginationData = new Apha.FPS.Core.Pagination.PaginationData()
                });
            _mapper.Map<PaginatedResult<AdditionalCostLogDto>>(Arg.Any<PagedData<Apha.FPS.Core.Entities.AdditionalCostLog>>())
                .Returns(new PaginatedResult<AdditionalCostLogDto> { Data = new List<AdditionalCostLogDto>(), PaginationData = new PaginationDto() });

            // Act
            await _service.GetAdditionalCostLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            await _repository.Received(1).GetAdditionalCostLogsAsync(paginationParams, TestProject, fromDate, toDate);
        }

        #endregion
    }
}
