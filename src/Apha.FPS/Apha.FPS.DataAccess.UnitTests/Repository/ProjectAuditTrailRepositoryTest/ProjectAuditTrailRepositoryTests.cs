using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectAuditTrailRepositoryTest
{
    public class ProjectAuditTrailRepositoryTests
    {
        private const int DefaultFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";
        private const string TestProject = "PROJ001";
        private const string TestJobCode = "JOB001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static ProjectAuditTrailRepository CreateRepository(
            IEnumerable<ProjectLog>? projectLogs = null,
            IEnumerable<StaffJobLog>? staffJobLogs = null,
            IEnumerable<TestRequirementLog>? testRequirementLogs = null,
            IEnumerable<AnimalRequestLog>? animalRequestLogs = null,
            IEnumerable<AdditionalCostLog>? additionalCostLogs = null,
            IEnumerable<JobCode>? jobCodes = null,
            IEnumerable<StaffGeneralView>? staffGeneralViews = null,
            IEnumerable<User>? users = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (projectLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectLogs);
                mockContext.Setup(x => x.ProjectLogs).Returns(mockSet.Object);
            }

            if (staffJobLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(staffJobLogs);
                mockContext.Setup(x => x.StaffJobLogs).Returns(mockSet.Object);

                // GetStaffJobLogsAsync enriches results with staff names via StaffGeneralViews;
                // always configure it (defaulting to empty) so the lookup doesn't hit an unmocked DbSet.
                var staffGeneralMockSet = RepositoryTestHelper.CreateMockDbSet(staffGeneralViews ?? Enumerable.Empty<StaffGeneralView>());
                mockContext.Setup(x => x.StaffGeneralViews).Returns(staffGeneralMockSet.Object);
            }

            if (testRequirementLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirementLogs);
                mockContext.Setup(x => x.TestRequirementLogs).Returns(mockSet.Object);
            }

            if (animalRequestLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animalRequestLogs);
                mockContext.Setup(x => x.AnimalRequestLogs).Returns(mockSet.Object);
            }

            if (additionalCostLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(additionalCostLogs);
                mockContext.Setup(x => x.AdditionalCostLogs).Returns(mockSet.Object);

                // GetAdditionalCostLogsAsync resolves legacy, non-email UserId values via a Users lookup;
                // always configure it (defaulting to empty) so the lookup doesn't hit an unmocked DbSet.
                var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users ?? Enumerable.Empty<User>());
                mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);
            }

            if (jobCodes != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);
                mockContext.Setup(x => x.JobCodes).Returns(mockSet.Object);
            }

            return new ProjectAuditTrailRepository(mockContext.Object);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_WithMatchingParentProject_ReturnsPagedData()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithNonMatchingParentProject_ReturnsEmpty()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = "OTHER", ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithFromDateFilter_ExcludesEarlierRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 1);
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Old", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "New", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 7, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, cutoff, null);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithToDateFilter_ExcludesLaterRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 30);
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Early", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 3, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Late", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, cutoff);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithSearchFilter_ReturnsMatchingRecords()
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = "INSERT", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = "DELETE", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "INSERT" };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_ResolvesStaffNameFromStaffGeneralView()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear }
            };
            var staffGeneralViews = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Jane Doe", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes, staffGeneralViews: staffGeneralViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Equal("Jane Doe", Assert.Single(result.Data).Name);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_NoMatchingStaffGeneralView_LeavesNameNull()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes, staffGeneralViews: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Null(Assert.Single(result.Data).Name);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange — job code has a different parent project
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "OTHERPROJ" }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithFromDateFilter_ExcludesEarlierRecords()
        {
            // Arrange
            var cutoff = new DateTime(2024, 6, 1);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8,
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4,
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, cutoff, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC001", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = "TC002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "DIFFERENT" }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC001", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var fromDate = new DateTime(2024, 4, 1);
            var toDate = new DateTime(2024, 9, 30);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 2, 1) },    // before range
                new() { SequenceNo = 2, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 6, 1) },    // in range
                new() { SequenceNo = 3, JobCode = TestJobCode, FpsYear = DefaultFpsYear,
                        DateTime = new DateTime(2024, 11, 1) }    // after range
            };
            var repo = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "WRONGPROJECT" }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithSearchFilter_ReturnsMatchingByAnimalType()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "rat" };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithMatchingJobCode_ReturnsPagedData()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002",
                        Description = "Equipment", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_NoMatchingJobCode_ReturnsEmpty()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = "NOPROJECT" }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithSearchFilter_ReturnsMatchingByDescription()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001",
                        Description = "Lab Supplies", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002",
                        Description = "Equipment Rental", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "Lab" };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithDateRange_FiltersCorrectly()
        {
            // Arrange
            var fromDate = new DateTime(2024, 3, 1);
            var toDate = new DateTime(2024, 8, 31);
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = TestJobCode, ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC001", Description = "A",
                        ItemCost = 50m, DateTime = new DateTime(2024, 1, 15), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC002", Description = "B",
                        ItemCost = 75m, DateTime = new DateTime(2024, 5, 15), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, JobCode = TestJobCode, Account = "ACC003", Description = "C",
                        ItemCost = 90m, DateTime = new DateTime(2024, 10, 15), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        // ── GetProjectLogsAsync – additional search and sorting ───────────────────────

        #region GetProjectLogsAsync - additional

        [Fact]
        public async Task GetProjectLogsAsync_WithSearchFilter_ReturnsMatchingByUserId()
        {
            // Arrange – InsertDelete is null to force UserId branch evaluation
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = null, UserId = "adminuser",   FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta",  Program = "P1",
                        Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = null, UserId = "regularuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "admin" };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("parentproject",  false)]
        [InlineData("parentproject",  true)]
        [InlineData("projecttitle",   false)]
        [InlineData("projecttitle",   true)]
        [InlineData("program",        false)]
        [InlineData("program",        true)]
        [InlineData("jobcode",        false)]
        [InlineData("jobcode",        true)]
        [InlineData("date_time",      false)]
        [InlineData("date_time",      true)]
        [InlineData("insert_delete",  false)]
        [InlineData("insert_delete",  true)]
        [InlineData("user_id",        false)]
        [InlineData("user_id",        true)]
        public async Task GetProjectLogsAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            // Arrange
            var logs = new List<ProjectLog>
            {
                new() { SequenceNo = 1, ParentProject = TestProject, ProjectTitle = "Alpha", Program = "PA",
                        JobCode = "JA", Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "K1",
                        InsertDelete = "I", UserId = "user1", DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, ParentProject = TestProject, ProjectTitle = "Beta",  Program = "PB",
                        JobCode = "JB", Customer = "C2", ProjectStatus = "Active", Disease = "D2", Contract = "K2",
                        InsertDelete = "D", UserId = "user2", DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(projectLogs: logs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetProjectLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        // ── GetStaffJobLogsAsync – additional filters, search and sorting ─────────────

        #region GetStaffJobLogsAsync - additional

        [Fact]
        public async Task GetStaffJobLogsAsync_WithToDateFilter_ExcludesLaterRecords()
        {
            // Arrange
            var cutoff   = new DateTime(2024, 6, 30);
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8,
                        DateTime = new DateTime(2024, 3, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4,
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, cutoff);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithSearchFilter_ReturnsMatchingByJobCode()
        {
            // Arrange – two distinct job codes so only one matches the search term
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JOB_ALPHA", ParentProject = TestProject },
                new() { JobCodeId = "JOB_BETA",  ParentProject = TestProject }
            };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = "JOB_ALPHA", StaffId = "S001", PlannedHours = 8,  FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = "JOB_BETA",  StaffId = "S002", PlannedHours = 4,  FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "alpha" };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_WithSearchFilter_ReturnsMatchingByUserId()
        {
            // Arrange – JobCode doesn't match to force UserId branch evaluation
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8,
                        UserId = "adminuser",   FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 4,
                        UserId = "regularuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "admin" };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("staffid",       false)]
        [InlineData("staffid",       true)]
        [InlineData("jobcode",       false)]
        [InlineData("jobcode",       true)]
        [InlineData("plannedhours",  false)]
        [InlineData("plannedhours",  true)]
        [InlineData("date_time",     false)]
        [InlineData("date_time",     true)]
        [InlineData("insert_delete", false)]
        [InlineData("insert_delete", true)]
        [InlineData("user_id",       false)]
        [InlineData("user_id",       true)]
        public async Task GetStaffJobLogsAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<StaffJobLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, StaffId = "S001", PlannedHours = 8,
                        InsertDelete = "I", UserId = "user1", DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, StaffId = "S002", PlannedHours = 16,
                        InsertDelete = "D", UserId = "user2", DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(staffJobLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetStaffJobLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        // ── GetTestRequirementLogsAsync – additional search and sorting ───────────────

        #region GetTestRequirementLogsAsync - additional

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithSearchFilter_ReturnsMatchingByTestCode()
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC_ALPHA", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = "TC_BETA",  FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "alpha" };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithSearchFilter_ReturnsMatchingByBuyer()
        {
            // Arrange – TestCode is null to force Buyer branch evaluation
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = null, Buyer = "BuyerAlpha", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = null, Buyer = "BuyerBeta",  FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "alpha" };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_WithSearchFilter_ReturnsMatchingByUserId()
        {
            // Arrange – TestCode and Buyer are null to force UserId branch evaluation
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = null, Buyer = null,
                        UserId = "adminuser",   FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = null, Buyer = null,
                        UserId = "regularuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "admin" };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("testcode",         false)]
        [InlineData("testcode",         true)]
        [InlineData("buyer",            false)]
        [InlineData("buyer",            true)]
        [InlineData("unitprice",        false)]
        [InlineData("unitprice",        true)]
        [InlineData("norequired",       false)]
        [InlineData("norequired",       true)]
        [InlineData("projectbuyercode", false)]
        [InlineData("projectbuyercode", true)]
        [InlineData("testbuyercode",    false)]
        [InlineData("testbuyercode",    true)]
        [InlineData("active",           false)]
        [InlineData("active",           true)]
        [InlineData("date_time",        false)]
        [InlineData("date_time",        true)]
        [InlineData("insert_delete",    false)]
        [InlineData("insert_delete",    true)]
        [InlineData("user_id",          false)]
        [InlineData("user_id",          true)]
        public async Task GetTestRequirementLogsAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<TestRequirementLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, TestCode = "TC1", Buyer = "BA",
                        UnitPrice = 10d, NoRequired = 5,  ProjectBuyerCode = "PB1", TestBuyerCode = "TB1",
                        Active = 0, InsertDelete = "I", UserId = "user1",
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, TestCode = "TC2", Buyer = "BB",
                        UnitPrice = 20d, NoRequired = 10, ProjectBuyerCode = "PB2", TestBuyerCode = "TB2",
                        Active = 1, InsertDelete = "D", UserId = "user2",
                        DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(testRequirementLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetTestRequirementLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync – additional filters, search and sorting ────────

        #region GetAnimalRequestLogsAsync - additional

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithFromDateFilter_ExcludesEarlierRecords()
        {
            // Arrange
            var cutoff   = new DateTime(2024, 6, 1);
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5,
                        DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10,
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, cutoff, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithToDateFilter_ExcludesLaterRecords()
        {
            // Arrange
            var cutoff   = new DateTime(2024, 6, 30);
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5,
                        DateTime = new DateTime(2024, 3, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10,
                        DateTime = new DateTime(2024, 9, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, cutoff);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_WithSearchFilter_ReturnsMatchingByUserId()
        {
            // Arrange – JobCode/AnimalType don't match to force UserId branch evaluation
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 10, NumberOfAnimals = 5,
                        UserId = "adminuser",   FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 20, NumberOfAnimals = 10,
                        UserId = "regularuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "admin" };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("jobcode",         false)]
        [InlineData("jobcode",         true)]
        [InlineData("animaltype",      false)]
        [InlineData("animaltype",      true)]
        [InlineData("numberofdays",    false)]
        [InlineData("numberofdays",    true)]
        [InlineData("numberofanimals", false)]
        [InlineData("numberofanimals", true)]
        [InlineData("date_time",       false)]
        [InlineData("date_time",       true)]
        [InlineData("insert_delete",   false)]
        [InlineData("insert_delete",   true)]
        [InlineData("user_id",         false)]
        [InlineData("user_id",         true)]
        public async Task GetAnimalRequestLogsAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AnimalRequestLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, AnimalType = "Rat",
                        NumberOfDays = 5,  NumberOfAnimals = 10,
                        InsertDelete = "I", UserId = "user1", DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, AnimalType = "Mouse",
                        NumberOfDays = 10, NumberOfAnimals = 20,
                        InsertDelete = "D", UserId = "user2", DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(animalRequestLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetAnimalRequestLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync – additional search and sorting ────────────────

        #region GetAdditionalCostLogsAsync - additional

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithSearchFilter_ReturnsMatchingByAccount()
        {
            // Arrange – tests the Account branch of the search predicate
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC_ALPHA",
                        Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC_BETA",
                        Description = "Desc2", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "alpha" };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithSearchFilter_ReturnsMatchingByJobCode()
        {
            // Arrange – two distinct job codes so only one matches; tests the JobCode branch
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JOB_ALPHA", ParentProject = TestProject },
                new() { JobCodeId = "JOB_BETA",  ParentProject = TestProject }
            };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = "JOB_ALPHA", Account = "ACC1",
                        Description = "Desc1", ItemCost = 100m, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = "JOB_BETA",  Account = "ACC2",
                        Description = "Desc2", ItemCost = 200m, FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "alpha" };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_WithSearchFilter_ReturnsMatchingByUserId()
        {
            // Arrange – Account/Description don't match to force UserId branch evaluation
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1",
                        Description = "Desc1", ItemCost = 100m, UserId = "adminuser",   FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC2",
                        Description = "Desc2", ItemCost = 200m, UserId = "regularuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "admin" };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Single(result.Data);
        }

        [Theory]
        [InlineData("jobcode",       false)]
        [InlineData("jobcode",       true)]
        [InlineData("account",       false)]
        [InlineData("account",       true)]
        [InlineData("description",   false)]
        [InlineData("description",   true)]
        [InlineData("itemcost",      false)]
        [InlineData("itemcost",      true)]
        [InlineData("freq",          false)]
        [InlineData("freq",          true)]
        [InlineData("supplier",      false)]
        [InlineData("supplier",      true)]
        [InlineData("date_time",     false)]
        [InlineData("date_time",     true)]
        [InlineData("insert_delete", false)]
        [InlineData("insert_delete", true)]
        [InlineData("user_id",       false)]
        [InlineData("user_id",       true)]
        public async Task GetAdditionalCostLogsAsync_Sorting_ReturnsResults(string sortBy, bool descending)
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "DescA",
                        ItemCost = 100m, Freq = "M", Supplier = "SupA",
                        InsertDelete = "I", UserId = "user1", DateTime = new DateTime(2024, 1, 1), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC2", Description = "DescB",
                        ItemCost = 200m, Freq = "A", Supplier = "SupB",
                        InsertDelete = "D", UserId = "user2", DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        // ── GetAdditionalCostLogsAsync – UserId → email resolution ──────────────────

        [Fact]
        public async Task GetAdditionalCostLogsAsync_UserIdIsAlreadyEmail_LeavesValueUnchanged()
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "Desc1",
                        ItemCost = 100m, UserId = "already.email@example.com", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Equal("already.email@example.com", result.Data.Single().UserId);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_UserIdMatchesUsername_ResolvesToEmail()
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "Desc1",
                        ItemCost = 100m, UserId = "jbloggs", FpsYear = DefaultFpsYear }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "jbloggs", UserEmail = "j.bloggs@example.com" }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes, users: users);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Equal("j.bloggs@example.com", result.Data.Single().UserId);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_UserIdMatchesDt2Username_ResolvesToEmail()
        {
            // Arrange
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "Desc1",
                        ItemCost = 100m, UserId = "DOMAIN\\jbloggs", FpsYear = DefaultFpsYear }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Dt2Username = "DOMAIN\\jbloggs", UserEmail = "j.bloggs@example.com" }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes, users: users);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Equal("j.bloggs@example.com", result.Data.Single().UserId);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_UserIdHasNoMatchingUser_LeavesLegacyValueUnchanged()
        {
            // Arrange – simulates a legacy/orphaned user_id with no fps.tblusers match
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "Desc1",
                        ItemCost = 100m, UserId = "orphanuser", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes, users: new List<User>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.Equal("orphanuser", result.Data.Single().UserId);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_MixedUserIdValues_AllResolveToEmailWherePossible()
        {
            // Arrange – reproduces the reported bug: some rows already have email, others a raw username
            var jobCodes = new List<JobCode> { new() { JobCodeId = TestJobCode, ParentProject = TestProject } };
            var logs = new List<AdditionalCostLog>
            {
                new() { SequenceNo = 1, JobCode = TestJobCode, Account = "ACC1", Description = "Desc1",
                        ItemCost = 100m, UserId = "already@example.com", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, JobCode = TestJobCode, Account = "ACC2", Description = "Desc2",
                        ItemCost = 200m, UserId = "jbloggs", FpsYear = DefaultFpsYear }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "jbloggs", UserEmail = "j.bloggs@example.com" }
            };
            var repo  = CreateRepository(additionalCostLogs: logs, jobCodes: jobCodes, users: users);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalCostLogsAsync(query, TestProject, null, null);

            // Assert
            Assert.All(result.Data, item =>
            {
                Assert.NotNull(item.UserId);
                Assert.Contains('@', item.UserId);
            });
        }

        #endregion
    }
}
