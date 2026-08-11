using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.MonthlyTimeServiceTest
{
    public class MonthlyTimeServiceTests
    {
        private readonly IMonthlyTimeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ICalenderMonthRepository _mockCalenderMonthRepository;
        private readonly IWorkGroupRepository _mockWorkGroupRepository;
        private readonly ITimeCodeValidRepository _mockTimeCodeValidRepository;
        private readonly MonthlyTimeService _sut;

        public MonthlyTimeServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyTimeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockCalenderMonthRepository = Substitute.For<ICalenderMonthRepository>();
            _mockWorkGroupRepository = Substitute.For<IWorkGroupRepository>();
            _mockTimeCodeValidRepository = Substitute.For<ITimeCodeValidRepository>();
            _sut = new MonthlyTimeService(
                _mockRepository,
                _mockMapper,
                _mockCalenderMonthRepository,
                _mockWorkGroupRepository,
                _mockTimeCodeValidRepository);
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new() { Page = page, PageSize = pageSize };

        private static PaginationParameters<string> DefaultPaginationParameters(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static PagedData<MonthlyTimeLog> BuildPagedData(
            IEnumerable<MonthlyTimeLog> items,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = items.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PagedData<MonthlyTimeLog>(
                list.AsReadOnly(),
                new PaginationData
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        private static PaginatedResult<MonthlyTimeLogDto> BuildPaginatedResult(
            IEnumerable<MonthlyTimeLogDto> dtos,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = dtos.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PaginatedResult<MonthlyTimeLogDto>(
                list,
                new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        // ── SearchAsync — happy path ────────────────────────────────────────────

        #region SearchAsync — happy path

        [Fact]
        public async Task SearchAsync_WithNoFilters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var entities = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, PactStaffId = "S001", TimeCode = "TC1", WorkGroup = "WG1" },
                new() { SequenceNo = 2, PactStaffId = "S002", TimeCode = "TC2", WorkGroup = "WG2" }
            };
            var dtos = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, PactStaffId = "S001", TimeCode = "TC1", WorkGroup = "WG1" },
                new() { SequenceNo = 2, PactStaffId = "S002", TimeCode = "TC2", WorkGroup = "WG2" }
            };
            var pagedData = BuildPagedData(entities);
            var expectedResult = BuildPaginatedResult(dtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 1);
            var filterDto = new MonthlyTimeLogFilterDto
            {
                WorkGroup = "WG1", TimeCode = "TC1", PactStaffId = "S001",
                ParentProject = "PP1", DateImported = dateImported, Month = 6,
                UserId = "USER1", InsertDelete = "I"
            };
            var coreFilter = new MonthlyTimeLogFilter
            {
                WorkGroup = "WG1", TimeCode = "TC1", PactStaffId = "S001",
                ParentProject = "PP1", DateImported = dateImported, Month = 6,
                UserId = "USER1", InsertDelete = "I"
            };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        #endregion

        #region SearchAsync — mapper verification

        [Fact]
        public async Task SearchAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 5);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 5);
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task SearchAsync_MapsFilterDtoToCoreFilter()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var coreFilter = new MonthlyTimeLogFilter { WorkGroup = "WG1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

            // Assert
            _mockMapper.Received(1).Map<MonthlyTimeLogFilter>(filterDto);
        }

        [Fact]
        public async Task SearchAsync_MapsPagedDataToPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.SearchAsync(query, filterDto);

            // Assert
            _mockMapper.Received(1).Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData);
        }

        #endregion

        #region SearchAsync — empty results

        [Fact]
        public async Task SearchAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region SearchAsync — pagination metadata

        [Fact]
        public async Task SearchAsync_ReturnsPaginationMetadataFromMappedResult()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 3);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 3);
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();
            var entities = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 4 },
                new() { SequenceNo = 5 },
                new() { SequenceNo = 6 }
            };
            var dtos = entities.Select(e => new MonthlyTimeLogDto { SequenceNo = e.SequenceNo }).ToList();
            var pagedData = BuildPagedData(entities, page: 2, pageSize: 3, totalRecords: 10);
            var expectedResult = BuildPaginatedResult(dtos, page: 2, pageSize: 3, totalRecords: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(10);
            result.PaginationData.TotalPages.Should().Be(4);
        }

        #endregion

        #region SearchAsync — individual filter delegation

        [Fact]
        public async Task SearchAsync_WorkGroupFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var coreFilter = new MonthlyTimeLogFilter { WorkGroup = "WG1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_TimeCodeFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { TimeCode = "TC1" };
            var coreFilter = new MonthlyTimeLogFilter { TimeCode = "TC1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_PactStaffIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { PactStaffId = "S001" };
            var coreFilter = new MonthlyTimeLogFilter { PactStaffId = "S001" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_ParentProjectFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { ParentProject = "PP1" };
            var coreFilter = new MonthlyTimeLogFilter { ParentProject = "PP1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_DateImportedFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 3, 15);
            var filterDto = new MonthlyTimeLogFilterDto { DateImported = dateImported };
            var coreFilter = new MonthlyTimeLogFilter { DateImported = dateImported };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_MonthFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { Month = 6 };
            var coreFilter = new MonthlyTimeLogFilter { Month = 6 };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_UserIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { UserId = "USER1" };
            var coreFilter = new MonthlyTimeLogFilter { UserId = "USER1" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        [Fact]
        public async Task SearchAsync_InsertDeleteFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto { InsertDelete = "D" };
            var coreFilter = new MonthlyTimeLogFilter { InsertDelete = "D" };
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(paginationParams, coreFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyTimeLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.SearchAsync(query, filterDto);

            // Assert
            await _mockRepository.Received(1).SearchAsync(paginationParams, coreFilter);
        }

        #endregion

        #region SearchAsync — exception propagation

        [Fact]
        public async Task SearchAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var filterDto = new MonthlyTimeLogFilterDto();
            var coreFilter = new MonthlyTimeLogFilter();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockMapper.Map<MonthlyTimeLogFilter>(filterDto).Returns(coreFilter);
            _mockRepository.SearchAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<MonthlyTimeLogFilter>())
                .ThrowsAsync(new InvalidOperationException("Repository failure"));

            // Act
            var act = async () => await _sut.SearchAsync(query, filterDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Repository failure");
        }

        #endregion

        #region Live and Staging Operations

        [Fact]
        public async Task SearchLiveAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<MonthlyTimeStaff>([], new PaginationData());
            var expected = new PaginatedResult<MonthlyTimeDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchLiveAsync(paginationParams, "WG1", "TC1", "S1", "PP1", 6).Returns(paged);
            _mockMapper.Map<PaginatedResult<MonthlyTimeDto>>(paged).Returns(expected);

            var result = await _sut.SearchLiveAsync(query, "WG1", "TC1", "S1", "PP1", 6);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchLiveAsync(paginationParams, "WG1", "TC1", "S1", "PP1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new MonthlyTime { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1" };
            var dto = new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1" };

            _mockRepository.GetLiveByKeyAsync("S1", "TC1", 6, "PP1").Returns(entity);
            _mockMapper.Map<MonthlyTimeDto>(entity).Returns(dto);

            var result = await _sut.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            result.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetLiveByKeyAsync("S1", "TC1", 6, "PP1").Returns((MonthlyTime?)null);

            var result = await _sut.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLiveAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteLiveAsync("S1", "TC1", 6, "PP1").Returns(true);

            var result = await _sut.DeleteLiveAsync("S1", "TC1", 6, "PP1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteLiveAsync("S1", "TC1", 6, "PP1");
        }

        [Fact]
        public async Task SearchStagingAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<StagingMonthlyTime>([], new PaginationData());
            var expected = new PaginatedResult<StagingMonthlyTimeDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchStagingAsync(paginationParams, "user1", false).Returns(paged);
            _mockMapper.Map<PaginatedResult<StagingMonthlyTimeDto>>(paged).Returns(expected);

            var result = await _sut.SearchStagingAsync(query, "user1", false);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchStagingAsync(paginationParams, "user1", false);
        }

        #endregion
    }
}
