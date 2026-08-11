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

namespace Apha.PACT.Application.UnitTests.Services.MonthlyOutputServiceTest
{
    public class MonthlyOutputServiceTests
    {
        private readonly IMonthlyOutputRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ICalenderMonthRepository _mockCalenderMonthRepository;
        private readonly IWorkGroupRepository _mockWorkGroupRepository;
        private readonly ITestCapabilityRepository _mockTestCapabilityRepository;
        private readonly ITestRequirementRepository _mockTestRequirementRepository;
        private readonly MonthlyOutputService _sut;

        public MonthlyOutputServiceTests()
        {
            _mockRepository = Substitute.For<IMonthlyOutputRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockCalenderMonthRepository = Substitute.For<ICalenderMonthRepository>();
            _mockWorkGroupRepository = Substitute.For<IWorkGroupRepository>();
            _mockTestCapabilityRepository = Substitute.For<ITestCapabilityRepository>();
            _mockTestRequirementRepository = Substitute.For<ITestRequirementRepository>();
            _sut = new MonthlyOutputService(
                _mockRepository,
                _mockMapper,
                _mockCalenderMonthRepository,
                _mockWorkGroupRepository,
                _mockTestCapabilityRepository,
                _mockTestRequirementRepository);
        }

        // ── helpers ────────────────────────────────────────────────────────────

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new() { Page = page, PageSize = pageSize };

        private static PaginationParameters<string> DefaultPaginationParameters(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static PagedData<MonthlyOutputLog> BuildPagedData(
            IEnumerable<MonthlyOutputLog> items,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = items.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PagedData<MonthlyOutputLog>(
                list.AsReadOnly(),
                new PaginationData
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        private static PaginatedResult<MonthlyOutputLogDto> BuildPaginatedResult(
            IEnumerable<MonthlyOutputLogDto> dtos,
            int page = 1, int pageSize = 10, int totalRecords = 0)
        {
            var list = dtos.ToList();
            var total = totalRecords > 0 ? totalRecords : list.Count;
            return new PaginatedResult<MonthlyOutputLogDto>(
                list,
                new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = total,
                    TotalPages = (int)Math.Ceiling((double)total / pageSize)
                });
        }

        // ── GetMonthlyOutputLogAsync — happy path ───────────────────────────────

        #region GetMonthlyOutputLogAsync — happy path

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WithNoFilters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var entities = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A" },
                new() { SequenceNo = 2, WorkGroup = "WG2", TestCode = "TC2", Buyer = "BUYER_B" }
            };
            var dtos = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A" },
                new() { SequenceNo = 2, WorkGroup = "WG2", TestCode = "TC2", Buyer = "BUYER_B" }
            };
            var pagedData = BuildPagedData(entities);
            var expectedResult = BuildPaginatedResult(dtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WithAllFilters_PassesFiltersToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 1);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I");

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, "WG1", "TC1", "BUYER_A", dateImported, 6, "SP001", "I");
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 5);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 5);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MapsPagedDataToPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — empty results

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetMonthlyOutputLogAsync — pagination metadata

        [Fact]
        public async Task GetMonthlyOutputLogAsync_ReturnsPaginationMetadataFromMappedResult()
        {
            // Arrange
            var query = DefaultQuery(page: 2, pageSize: 3);
            var paginationParams = DefaultPaginationParameters(page: 2, pageSize: 3);
            var entities = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 4 },
                new() { SequenceNo = 5 },
                new() { SequenceNo = 6 }
            };
            var dtos = entities.Select(e => new MonthlyOutputLogDto { SequenceNo = e.SequenceNo }).ToList();
            var pagedData = BuildPagedData(entities, page: 2, pageSize: 3, totalRecords: 10);
            var expectedResult = BuildPaginatedResult(dtos, page: 2, pageSize: 3, totalRecords: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(10);
            result.PaginationData.TotalPages.Should().Be(4);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — individual filter delegation

        [Fact]
        public async Task GetMonthlyOutputLogAsync_WorkGroupFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, "WG1", null, null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, "WG1", null, null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, "WG1", null, null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_TestCodeFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, "TC1", null, null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, "TC1", null, null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, "TC1", null, null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_BuyerFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, "BUYER_A", null, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, "BUYER_A", null, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, "BUYER_A", null, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_DateImportedFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var dateImported = new DateTime(2024, 6, 15);
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, dateImported, null, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, dateImported, null, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, dateImported, null, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_MonthFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, 6, null, null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, 6, null, null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, 6, null, null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_UserIdFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, "SP001", null)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, "SP001", null);

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, "SP001", null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_InsertDeleteFilter_DelegatesCorrectlyToRepository()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var pagedData = BuildPagedData([]);
            var expectedResult = BuildPaginatedResult([]);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, "I")
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<MonthlyOutputLogDto>>(pagedData).Returns(expectedResult);

            // Act
            await _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, "I");

            // Assert
            await _mockRepository.Received(1)
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, "I");
        }

        #endregion

        #region GetMonthlyOutputLogAsync — exception handling

        [Fact]
        public async Task GetMonthlyOutputLogAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository
                .GetMonthlyOutputLogAsync(paginationParams, null, null, null, null, null, null, null)
                .ThrowsAsync(new Exception("DB error"));

            // Act
            Func<Task> act = () => _sut.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        #endregion

        #region Live and Staging Operations

        [Fact]
        public async Task SearchLiveAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<MonthlyOutput>([], new PaginationData());
            var expected = new PaginatedResult<MonthlyOutputDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchLiveAsync(paginationParams, "WG1", "TC1", "B1", 6).Returns(paged);
            _mockMapper.Map<PaginatedResult<MonthlyOutputDto>>(paged).Returns(expected);

            var result = await _sut.SearchLiveAsync(query, "WG1", "TC1", "B1", 6);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchLiveAsync(paginationParams, "WG1", "TC1", "B1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityExists_ReturnsMappedDto()
        {
            var entity = new MonthlyOutput { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6 };
            var dto = new MonthlyOutputDto { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6 };

            _mockRepository.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns(entity);
            _mockMapper.Map<MonthlyOutputDto>(entity).Returns(dto);

            var result = await _sut.GetLiveByKeyAsync("TC1", "B1", 6, "WG1");

            result.Should().BeEquivalentTo(dto);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenEntityMissing_ReturnsNull()
        {
            _mockRepository.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns((MonthlyOutput?)null);

            var result = await _sut.GetLiveByKeyAsync("TC1", "B1", 6, "WG1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteLiveAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteLiveAsync("TC1", "B1", 6, "WG1").Returns(true);

            var result = await _sut.DeleteLiveAsync("TC1", "B1", 6, "WG1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteLiveAsync("TC1", "B1", 6, "WG1");
        }

        [Fact]
        public async Task SearchStagingAsync_WithFilters_MapsAndDelegatesToRepository()
        {
            var query = DefaultQuery();
            var paginationParams = DefaultPaginationParameters();
            var paged = new PagedData<StagingMonthlyOutput>([], new PaginationData());
            var expected = new PaginatedResult<StagingMonthlyOutputDto>([], new PaginationDto());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.SearchStagingAsync(paginationParams, "user1", true).Returns(paged);
            _mockMapper.Map<PaginatedResult<StagingMonthlyOutputDto>>(paged).Returns(expected);

            var result = await _sut.SearchStagingAsync(query, "user1", true);

            result.Should().BeSameAs(expected);
            await _mockRepository.Received(1).SearchStagingAsync(paginationParams, "user1", true);
        }

        #endregion
    }
}
