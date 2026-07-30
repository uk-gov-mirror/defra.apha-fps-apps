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

namespace Apha.PACT.Application.UnitTests.Services.TestRequirementServiceTest
{
    public class TestRequirementServiceTests
    {
        private readonly ITestRequirementRepository _testReqmtRepository;
        private readonly IProjectRepository         _projectRepository;
        private readonly IMapper                    _mapper;
        private readonly TestRequirementService     _sut;

        public TestRequirementServiceTests()
        {
            _testReqmtRepository = Substitute.For<ITestRequirementRepository>();
            _projectRepository   = Substitute.For<IProjectRepository>();
            _mapper              = Substitute.For<IMapper>();
            _sut                 = new TestRequirementService(_testReqmtRepository, _projectRepository, _mapper);
        }

        #region GetPagedTestReqmtAsync

        [Fact]
        public async Task GetPagedTestReqmtAsync_ValidQuery_ReturnsMappedResult()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped       = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestRequirementDetail>([], new PaginationData());
            var dtos         = new List<TestRequirementtDto>();
            var paginationDto = new PaginationDto();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPagedWithDetailsAsync(mapped, "PT0001").Returns(pagedData);
            _mapper.Map<List<TestRequirementtDto>>(pagedData.Data).Returns(dtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            var result = await _sut.GetPagedTestReqmtAsync(query, "PT0001");

            result.Should().NotBeNull();
            result.Data.Should().BeSameAs(dtos);
        }

        [Fact]
        public async Task GetPagedTestReqmtAsync_RepositoryThrows_PropagatesException()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPagedWithDetailsAsync(mapped, "PT0001")
                                 .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedTestReqmtAsync(query, "PT0001"));
        }

        #endregion

        #region GetPagedTestReqmtByProjectAsync

        [Fact]
        public async Task GetPagedTestReqmtByProjectAsync_ValidQuery_ReturnsMappedResult()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped       = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestRequirementDetail>([], new PaginationData());
            var dtos         = new List<TestRequirementtDto>();
            var paginationDto = new PaginationDto();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPagedByProjectAsync(mapped, "PROJ01").Returns(pagedData);
            _mapper.Map<List<TestRequirementtDto>>(pagedData.Data).Returns(dtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            var result = await _sut.GetPagedTestReqmtByProjectAsync(query, "PROJ01");

            result.Should().NotBeNull();
        }

        #endregion

        #region GetAllTestReqmtForExportAsync

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_ValidInput_ReturnsMappedDtos()
        {
            var details = new List<TestRequirementDetail>();
            var dtos    = new List<TestRequirementtDto>();

            _testReqmtRepository.GetAllForExportAsync("PT0001", null).Returns(details);
            _mapper.Map<IEnumerable<TestRequirementtDto>>(details).Returns(dtos);

            var result = await _sut.GetAllTestReqmtForExportAsync("PT0001", null);

            result.Should().BeSameAs(dtos);
        }

        #endregion

        #region GetTestReqmtByIdAsync

        [Fact]
        public async Task GetTestReqmtByIdAsync_Found_ReturnsMappedDto()
        {
            var detail = new TestRequirementDetail { TestCode = "PT0001", Buyer = "SV3300" };
            var dto    = new TestRequirementtDto();

            _testReqmtRepository.GetDetailByIdAsync("PT0001", "SV3300").Returns(detail);
            _mapper.Map<TestRequirementtDto>(detail).Returns(dto);

            var result = await _sut.GetTestReqmtByIdAsync("PT0001", "SV3300");

            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetTestReqmtByIdAsync_NotFound_ReturnsNull()
        {
            _testReqmtRepository.GetDetailByIdAsync("PT9999", "SV0000")
                                 .Returns((TestRequirementDetail?)null);

            var result = await _sut.GetTestReqmtByIdAsync("PT9999", "SV0000");

            result.Should().BeNull();
        }

        #endregion

        #region GetTestReqmtPricingAsync

        [Fact]
        public async Task GetTestReqmtPricingAsync_Found_ReturnsMappedDto()
        {
            var detail = new TestRequirementDetail();
            var dto    = new TestRequirementtDto();

            _testReqmtRepository.GetPricingAsync("PT0001", null).Returns(detail);
            _mapper.Map<TestRequirementtDto>(detail).Returns(dto);

            var result = await _sut.GetTestReqmtPricingAsync("PT0001");

            result.Should().Be(dto);
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_NotFound_ReturnsNull()
        {
            _testReqmtRepository.GetPricingAsync("PT9999", null)
                                 .Returns((TestRequirementDetail?)null);

            var result = await _sut.GetTestReqmtPricingAsync("PT9999");

            result.Should().BeNull();
        }

        #endregion

        #region GetPagedBySupplierTestCodeAsync

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ValidQuery_ReturnsMappedResult()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped   = new PaginationParameters<string>();
            var pagedData = new PagedData<TestSupplierView>([], new PaginationData());
            var expected  = new PaginatedResult<TestSupplierViewDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPagedBySupplierTestCodeAsync(mapped, "PT0001", false).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestSupplierViewDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedBySupplierTestCodeAsync(query, "PT0001", false);

            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NullQuery_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.GetPagedBySupplierTestCodeAsync(null!, "PT0001", false));
        }

        #endregion

        #region AddTestReqmtAsync

        [Fact]
        public async Task AddTestReqmtAsync_ValidDto_ReturnsCreatedDto()
        {
            var dto     = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300", ProjectBuyerCode = "PB01", TestBuyerCode = "TB01" };
            var entity  = new TestRequirement();
            var created = new TestRequirement();
            var result  = new TestRequirementtDto();

            _projectRepository.ExistsAsync("PB01").Returns(true);
            _testReqmtRepository.ExistsByTestBuyerCodeAsync("TB01").Returns(true);
            _testReqmtRepository.ExistsAsync("PT0001", "SV3300").Returns(false);
            _mapper.Map<TestRequirement>(dto).Returns(entity);
            _testReqmtRepository.AddAsync(entity).Returns(created);
            _mapper.Map<TestRequirementtDto>(created).Returns(result);

            var outcome = await _sut.AddTestReqmtAsync(dto);

            outcome.Should().Be(result);
        }

        [Fact]
        public async Task AddTestReqmtAsync_BothFieldsEmpty_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "", Buyer = "" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
        }

        [Fact]
        public async Task AddTestReqmtAsync_InvalidProject_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300", ProjectBuyerCode = "PB_BAD" };
            _projectRepository.ExistsAsync("PB_BAD").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
        }

        [Fact]
        public async Task AddTestReqmtAsync_DuplicateRecord_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300" };
            _testReqmtRepository.ExistsAsync("PT0001", "SV3300").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestReqmtAsync(dto));
        }

        #endregion

        #region UpdateTestReqmtAsync

        [Fact]
        public async Task UpdateTestReqmtAsync_BothBuyerCodesNull_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_InvalidTestBuyerCode_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300", TestBuyerCode = "TB_BAD" };
            _testReqmtRepository.ExistsByTestBuyerCodeAsync("TB_BAD").Returns(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        [Fact]
        public async Task UpdateTestReqmtAsync_MonthlyOutputExists_ThrowsInvalidOperationException()
        {
            var dto = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300", TestBuyerCode = "TB01" };
            _testReqmtRepository.ExistsByTestBuyerCodeAsync("TB01").Returns(true);
            _testReqmtRepository.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("PT0001", "SV3300").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmtAsync(dto));
        }

        #endregion

        #region DeleteTestReqmtAsync

        [Fact]
        public async Task DeleteTestReqmtAsync_ExistingRecord_ReturnsTrue()
        {
            _testReqmtRepository.DeleteAsync("PT0001", "SV3300").Returns(true);

            var result = await _sut.DeleteTestReqmtAsync("PT0001", "SV3300");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteTestReqmtAsync_NotFound_ReturnsFalse()
        {
            _testReqmtRepository.DeleteAsync("PT9999", "SV0000").Returns(false);

            var result = await _sut.DeleteTestReqmtAsync("PT9999", "SV0000");

            result.Should().BeFalse();
        }

        #endregion

        #region GetPlannedTestsByWorkgroupAsync

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_ValidQuery_ReturnsMappedResult()
        {
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped   = new PaginationParameters<string>();
            var pagedData = new PagedData<TestReqBreakdownView>([], new PaginationData());
            var expected  = new PaginatedResult<TestReqBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPlannedTestsByWorkgroupAsync(mapped).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestReqBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPlannedTestsByWorkgroupAsync(query);

            result.Should().Be(expected);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_RepositoryThrows_PropagatesException()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mapped = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(mapped);
            _testReqmtRepository.GetPlannedTestsByWorkgroupAsync(mapped)
                                 .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPlannedTestsByWorkgroupAsync(query));
        }

        #endregion

        #region GetActualsTestsWithPlannedDataByWorkgroupAsync

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_ValidQuery_MapsAndCallsRepository()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            result.Should().Be(expected);
            await _testReqmtRepository.Received(1)
                .GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithItems_ReturnsMappedDtos()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Month = 4, PCPrice = 159m, PCCost = 319m },
                new() { TestCode = "PT0049", Buyer = "SB4600", Month = 4, PCPrice = 313m, PCCost = 313m }
            };
            var pagedData = new PagedData<TestActualBreakdownView>(views, new PaginationData());
            var dtos      = new List<TestActualBreakdownDto> { new() { TestCode = "PT0047" }, new() { TestCode = "PT0049" } };
            var expected  = new PaginatedResult<TestActualBreakdownDto>(dtos, new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_EmptyRepository_ReturnsEmpty()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_RepositoryThrows_PropagatesException()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams)
                                 .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query));
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_MapsQueryExactlyOnce()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_MapsResultExactlyOnce()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            _mapper.Received(1).Map<PaginatedResult<TestActualBreakdownDto>>(pagedData);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithSortingQuery_PassesMappedParams()
        {
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var pagedData    = new PagedData<TestActualBreakdownView>([], new PaginationData());
            var expected     = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(mappedParams).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData).Returns(expected);

            var result = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            result.Should().Be(expected);
        }

        #endregion
    }
}
