using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.ProjectSubContractServiceTest
{
    public class ProjectSubContractServiceTests
    {
        private readonly IProjectSubContractRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectSubContractService _sut;

        public ProjectSubContractServiceTests()
        {
            _mockRepository = Substitute.For<IProjectSubContractRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProjectSubContractService(_mockRepository, _mockMapper);
        }

        #region GetPagedProjectSubContractsAsync

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedProjectSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedProjectSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedProjectSubContractsAsync(mappedParams, null);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync("PRJ1").Returns(2500.00m);

            var result = await _sut.GetTotalAmountAsync("PRJ1");

            result.Should().Be(2500.00m);
            await _mockRepository.Received(1).GetTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullProject_ReturnsTotalAmount()
        {
            _mockRepository.GetTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalAmountAsync(null);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMappedDto()
        {
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1" };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };

            _mockRepository.GetByIdAsync(1).Returns(entity);
            _mockMapper.Map<ProjectSubContractDto>(entity).Returns(dto);

            var result = await _sut.GetByIdAsync(1);

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetByIdAsync(99).Returns((ProjectSubContract?)null);

            var result = await _sut.GetByIdAsync(99);

            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var created = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<ProjectSubContractDto>(created).Returns(expected);

            var result = await _sut.CreateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "", Month = 1.0, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = null, Amount = 500m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task CreateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { Project = "PRJ1", Month = 1.0, Amount = 500m };
            var entity = new ProjectSubContract { Project = "PRJ1", Month = 1.0, Amount = 500m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var updated = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var expected = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).Returns(updated);
            _mockMapper.Map<ProjectSubContractDto>(updated).Returns(expected);

            var result = await _sut.UpdateAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<ProjectSubContract>(dto);
            await _mockRepository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "", Month = 1.0, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = null, Amount = 750m };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "MONTH_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = null };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));

            ex.Errors.Should().ContainSingle(e => e.Code == "AMOUNT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", Month = 1.0, Amount = 750m };

            _mockMapper.Map<ProjectSubContract>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
        }

        #endregion

        #region GetFpsProjectSubContractsAsync

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<ProjectSubContract> { new ProjectSubContract { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals" } };
            var pagedData = new PagedData<ProjectSubContract>(entities, new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetFpsProjectSubContractsAsync(query, "PRJ1");

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetFpsProjectSubContractsAsync(mappedParams, "PRJ1");
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_NullProject_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<ProjectSubContract>(new List<ProjectSubContract>(), new PaginationData());
            var pagedResult = new PaginatedResult<ProjectSubContractDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetFpsProjectSubContractsAsync(query, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetFpsProjectSubContractsAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFpsProjectSubContractsAsync(mappedParams, "PRJ1").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetFpsProjectSubContractsAsync(query, "PRJ1"));
        }

        #endregion

        #region GetFpsProjectSubContractTotalAmountAsync

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_ValidProject_ReturnsTotalAmount()
        {
            _mockRepository.GetFpsProjectSubContractTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _sut.GetFpsProjectSubContractTotalAmountAsync("PRJ1");

            result.Should().Be(1500.00m);
            await _mockRepository.Received(1).GetFpsProjectSubContractTotalAmountAsync("PRJ1");
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_NullProject_ReturnsTotalAmount()
        {
            _mockRepository.GetFpsProjectSubContractTotalAmountAsync(null).Returns(0m);

            var result = await _sut.GetFpsProjectSubContractTotalAmountAsync(null);

            result.Should().Be(0m);
            await _mockRepository.Received(1).GetFpsProjectSubContractTotalAmountAsync(null);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            _mockRepository.DeleteAsync(1).Returns(true);

            var result = await _sut.DeleteAsync(1);

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAsync(99).Returns(false);

            var result = await _sut.DeleteAsync(99);

            result.Should().BeFalse();
        }

        #endregion

        #region GetMonthlySubContractsSummaryAsync

        private static MonthlySubContractsSummary MakeSummary(string program, string parentProject, double month, decimal? amount = null)
            => new() { FpsYear = 2024, Program = program, ParentProject = parentProject, Month = month, MonthlyAmount = amount };

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_ValidQuery_CallsRepositoryWithMappedParameters()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            await _sut.GetMonthlySubContractsSummaryAsync(query);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetMonthlySubContractsSummaryAsync(mappedParams);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_EmptyData_ReturnsEmptyPivot()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().BeEmpty();
            result.Months.Should().BeEmpty();
            result.Pagination.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SingleRow_ReturnsCorrectPivotRow()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("ADMIN", "AH", 2, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(1);
            result.Rows[0].Program.Should().Be("ADMIN");
            result.Rows[0].ParentProject.Should().Be("AH");
            result.Rows[0].MonthlyAmounts[1].Should().Be(100m);
            result.Rows[0].MonthlyAmounts[2].Should().Be(200m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_MultipleGroups_GroupsByProgramAndParentProject()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH",  1, 100m),
                MakeSummary("ADMIN", "AH",  2, 200m),
                MakeSummary("BETA",  "ZZ",  1, 300m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(2);
            result.Pagination.TotalRecords.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_DiscoverMonths_ReturnsDistinctOrderedMonths()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 3, 300m),
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("BETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Months.Should().BeInAscendingOrder();
            result.Months.Should().Equal(1, 3);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NullMonthlyAmount_TreatedAsZero()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, null)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(0m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_Pagination_ReturnsCorrectPage()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("BETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows.Should().HaveCount(1);
            result.Pagination.PageNumber.Should().Be(2);
            result.Pagination.PageSize.Should().Be(1);
            result.Pagination.TotalRecords.Should().Be(2);
            result.Pagination.TotalPages.Should().Be(2);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_PageLessThanOne_DefaultsToPageOne()
        {
            var query = new QueryParameters<string> { Page = 0, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Pagination.PageNumber.Should().Be(1);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_PageSizeLessThanOne_DefaultsToTen()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 0 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns([]);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Pagination.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NoSortBy_SortsByProgramThenParentProject()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ZETA",  "BB", 1, 300m),
                MakeSummary("ADMIN", "CC", 1, 100m),
                MakeSummary("ADMIN", "AA", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].Program.Should().Be("ADMIN");
            result.Rows[0].ParentProject.Should().Be("AA");
            result.Rows[1].Program.Should().Be("ADMIN");
            result.Rows[1].ParentProject.Should().Be("CC");
            result.Rows[2].Program.Should().Be("ZETA");
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SortByProgramDescending_SortsCorrectly()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "program", Descending = true };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ADMIN", "AH", 1, 100m),
                MakeSummary("ZETA",  "ZZ", 1, 200m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].Program.Should().Be("ZETA");
            result.Rows[1].Program.Should().Be("ADMIN");
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_SortByMonthColumn_SortsByMonthlyAmount()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "M1", Descending = false };
            var mappedParams = new PaginationParameters<string>();
            var data = new List<MonthlySubContractsSummary>
            {
                MakeSummary("ZETA",  "ZZ", 1, 500m),
                MakeSummary("ADMIN", "AH", 1, 100m)
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams).Returns(data);

            var result = await _sut.GetMonthlySubContractsSummaryAsync(query);

            result.Rows[0].MonthlyAmounts[1].Should().Be(100m);
            result.Rows[1].MonthlyAmounts[1].Should().Be(500m);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetMonthlySubContractsSummaryAsync(mappedParams)
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetMonthlySubContractsSummaryAsync(query));
        }

        #endregion

        #region FailedSubContractRms

        [Fact]
        public async Task GetFailedSubContractRmsAsync_ValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<SubContractRmsImportRow>([], new PaginationData());
            var expected = new PaginatedResult<SubContractRmsImportRowDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetFailedSubContractRmsAsync(mappedParams, "user1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<SubContractRmsImportRowDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetFailedSubContractRmsAsync(query, "user1");

            // Assert
            result.Should().Be(expected);
            await _mockRepository.Received(1).GetFailedSubContractRmsAsync(mappedParams, "user1");
        }

        [Fact]
        public async Task GetFailedSubContractRmsByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            var entity = new ProjectSubcontractStaging { Id = 5, Project = "PRJ5" };
            var dto = new SubContractRmsImportRowDto { Id = 5, Project = "PRJ5" };

            _mockRepository.GetFailedSubContractRmsByIdAsync(5, "user2").Returns(entity);
            _mockMapper.Map<SubContractRmsImportRowDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetFailedSubContractRmsByIdAsync(5, "user2");

            // Assert
            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetFailedSubContractRmsByIdAsync_EntityMissing_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetFailedSubContractRmsByIdAsync(99, "user3").Returns((ProjectSubcontractStaging?)null);

            // Act
            var result = await _sut.GetFailedSubContractRmsByIdAsync(99, "user3");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SaveFailedSubContractRmsAsync_WhenValidationFails_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new SubContractRmsImportRowDto
            {
                Project = "UNKNOWN_PROJECT",
                Month = "13",
                Amount = "-1",
                SupplierNumber = "-2",
                DailyRate = "-3",
                AnimalDays = "-4"
            };

            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2025);

            // Act
            Func<Task> act = () => _sut.SaveFailedSubContractRmsAsync(1, dto, "user4");

            // Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(act);
            ex.Errors.Should().NotBeEmpty();
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<ProjectSubContract>());
            await _mockRepository.DidNotReceive().DeleteFailedSubContractRmsByIdAsync(Arg.Any<int>(), Arg.Any<string>());
        }

        [Fact]
        public async Task SaveFailedSubContractRmsAsync_WhenValidationPasses_CreatesSubContractAndDeletesFailedRow()
        {
            // Arrange
            var dto = new SubContractRmsImportRowDto
            {
                Project = "PRJ1",
                TestJob = "TJ1",
                Month = "4",
                Amount = "100.50",
                WorkGroup = "WG1",
                AcctCode = "AC1",
                Supplier = "SupplierA",
                Description = "Desc",
                SupplierNumber = "10",
                DailyRate = "50.25",
                AnimalDays = "2"
            };

            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2026);
            _mockRepository.CreateAsync(Arg.Any<ProjectSubContract>()).Returns(new ProjectSubContract());
            _mockRepository.DeleteFailedSubContractRmsByIdAsync(7, "user5").Returns(true);

            // Act
            var result = await _sut.SaveFailedSubContractRmsAsync(7, dto, "user5");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).CreateAsync(Arg.Is<ProjectSubContract>(x =>
                x.Project == "PRJ1" &&
                x.Month == 4 &&
                x.Amount == 100.50m &&
                x.SupplierNumber == 10 &&
                x.DailyRate == 50.25m &&
                x.AnimalDays == 2 &&
                x.FpsYear == 2026));
            await _mockRepository.Received(1).DeleteFailedSubContractRmsByIdAsync(7, "user5");
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByIdAsync_RepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteFailedSubContractRmsByIdAsync(8, "user6").Returns(true);

            // Act
            var result = await _sut.DeleteFailedSubContractRmsByIdAsync(8, "user6");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByUserAsync_RepositoryReturnsDeletedCount_ReturnsCount()
        {
            // Arrange
            _mockRepository.DeleteFailedSubContractRmsByUserAsync("user7").Returns(3);

            // Act
            var result = await _sut.DeleteFailedSubContractRmsByUserAsync("user7");

            // Assert
            result.Should().Be(3);
        }

        [Fact]
        public async Task ImportSubContractRmsAsync_WithMixedValidInvalidRows_ReturnsCountsAndMessage()
        {
            // Arrange
            var request = new SubContractRmsImportDto
            {
                FileName = "rms-import.xlsx",
                Rows =
                [
                    new SubContractRmsImportRowDto
                    {
                        Project = "PRJ1", Month = "4", Amount = "100", SupplierNumber = "1", DailyRate = "10", AnimalDays = "2"
                    },
                    new SubContractRmsImportRowDto
                    {
                        Project = "UNKNOWN", Month = "14", Amount = "abc"
                    }
                ]
            };

            _mockRepository.GetValidProjectsAsync().Returns(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "PRJ1" });
            _mockRepository.GetCurrentFpsYear().Returns(2027);
            _mockRepository.ImportSubContractRmsAsync(Arg.Any<List<ProjectSubContract>>(), Arg.Any<List<ProjectSubcontractStaging>>())
                .Returns(new SubContractRmsImportResult { PassedCount = 1, FailedCount = 1 });

            // Act
            var result = await _sut.ImportSubContractRmsAsync(request, "user8");

            // Assert
            result.PassedCount.Should().Be(1);
            result.FailedCount.Should().Be(1);
            result.Message.Should().Contain("1 out of 2 records successfully validated and is now live");

            await _mockRepository.Received(1).ImportSubContractRmsAsync(
                Arg.Is<List<ProjectSubContract>>(p => p.Count == 1 && p[0].Project == "PRJ1" && p[0].FpsYear == 2027),
                Arg.Is<List<ProjectSubcontractStaging>>(f => f.Count == 1 && f[0].Project == "UNKNOWN" && f[0].ImportedBy == "user8" && f[0].Filename == "rms-import.xlsx"));
        }

        #endregion
    }
}
