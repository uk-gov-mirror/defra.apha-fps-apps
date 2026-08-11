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

namespace Apha.PACT.Application.UnitTests.Services.TestCapabilityServiceTest
{
    public class TestCapabilityServiceTests
    {
        private readonly ITestCapabilityRepository _testCapabilityRepo;
        private readonly ITestRequirementRepository _testReqmtRepo;
        private readonly ITestorProductRepository _testorProductRepo;
        private readonly IMonthlyOutputRepository _monthlyOutputRepo;
        private readonly IMapper _mapper;
        private readonly TestCapabilityService _sut;

        public TestCapabilityServiceTests()
        {
            _testCapabilityRepo = Substitute.For<ITestCapabilityRepository>();
            _testReqmtRepo = Substitute.For<ITestRequirementRepository>();
            _testorProductRepo = Substitute.For<ITestorProductRepository>();
            _monthlyOutputRepo = Substitute.For<IMonthlyOutputRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new TestCapabilityService(
                _testCapabilityRepo, _testReqmtRepo, _testorProductRepo, _monthlyOutputRepo, _mapper);
        }

        #region GetPagedTestCapabilityByPortfolioAsync

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_ValidQuery_ReturnsMappedResultWithDescriptions()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var pagedData = new PagedData<TestCapability>([entity], new PaginationData { TotalRecords = 1 });
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = [dto] };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "Test Description" };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Should().Be(pagedResult);
            Assert.Equal("Test Description", result.Data!.First().ItemDescription);
            Assert.Equal(42.50m, result.Data!.First().UnitCost);
            await _testCapabilityRepo.Received(1).GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1");
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_UnitCost_AlwaysSourcedFromProductPrice()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var pagedData = new PagedData<TestCapability>([entity], new PaginationData { TotalRecords = 1 });
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = 99.99m };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = [dto] };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "Test Description" };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            // Unit Cost is always taken from the TestorProduct master, overriding any value on the capability row.
            Assert.Equal(42.50m, result.Data!.First().UnitCost);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_ZeroUnitCost_FallsBackToProductPrice()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var pagedData = new PagedData<TestCapability>([entity], new PaginationData { TotalRecords = 1 });
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = 0m };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = [dto] };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "Test Description" };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            Assert.Equal(42.50m, result.Data!.First().UnitCost);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_NullUnitCost_FallsBackToProductPrice()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var pagedData = new PagedData<TestCapability>([entity], new PaginationData { TotalRecords = 1 });
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = null };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = [dto] };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "Test Description" };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            Assert.Equal(42.50m, result.Data!.First().UnitCost);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_EmptyData_DoesNotCallDescriptions()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Should().Be(pagedResult);
            await _testorProductRepo.DidNotReceive().GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>());
        }

        #endregion

        #region AddTestCapabilityAsync — ValidateRequiredFields paths

        [Fact]
        public async Task AddTestCapabilityAsync_MissingTestCode_ThrowsArgumentException()
        {
            var dto = new TestCapabilityDto { TestCode = "", WorkGroup = "WG1", PlanPortfolio = "PP1" };

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().AddAsync(Arg.Any<TestCapability>());
        }

        [Fact]
        public async Task AddTestCapabilityAsync_MissingPlanPortfolio_ThrowsArgumentException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.AddTestCapabilityAsync(dto));
        }

        #endregion

        #region UpdateTestCapabilityAsync — ValidateRequiredFields paths

        [Fact]
        public async Task UpdateTestCapabilityAsync_MissingWorkGroup_ThrowsArgumentException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "", PlanPortfolio = "PP1" };

            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().UpdateAsync(Arg.Any<TestCapability>());
        }

        #endregion

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, "WG1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByWorkGroupAsync(query, "WG1");

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByWorkGroupAsync(mappedParams, "WG1");
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_NullWorkGroup_PassesNullToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, null).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByWorkGroupAsync(query, null);

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByWorkGroupAsync(mappedParams, null);
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            var mappedParams = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByWorkGroupAsync(mappedParams, null).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedByWorkGroupAsync(query, null));
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TestCapability>([], new PaginationData());
            var expected = new PaginatedResult<TestCapabilityDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedByTestCodeAsync(mappedParams, "TC1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedByTestCodeAsync(query, "TC1");

            result.Should().Be(expected);
            await _testCapabilityRepo.Received(1).GetPagedByTestCodeAsync(mappedParams, "TC1");
        }

        #endregion

        #region GetTestCapabilityByIdAsync

        [Fact]
        public async Task GetTestCapabilityByIdAsync_RecordFound_ReturnsMappedDto()
        {
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(dto);

            var result = await _sut.GetTestCapabilityByIdAsync("TC1", "WG1");

            result.Should().Be(dto);
            await _testCapabilityRepo.Received(1).GetByIdAsync("TC1", "WG1");
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_RecordNotFound_ReturnsNull()
        {
            _testCapabilityRepo.GetByIdAsync("MISSING", "WG1").Returns((TestCapability?)null);

            var result = await _sut.GetTestCapabilityByIdAsync("MISSING", "WG1");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_ZeroUnitCost_FallsBackToProductPrice()
        {
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = 0m };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(dto);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.Equal(42.50m, result!.UnitCost);
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_NullUnitCost_FallsBackToProductPrice()
        {
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = null };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(dto);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.Equal(42.50m, result!.UnitCost);
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_UnitCost_AlwaysSourcedFromProductPrice()
        {
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", UnitCost = 99.99m };
            var unitPrices = new Dictionary<string, decimal?> { ["TC1"] = 42.50m };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(dto);
            _testorProductRepo.GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(unitPrices);

            var result = await _sut.GetTestCapabilityByIdAsync("TC1", "WG1");

            // Unit Cost is always taken from the TestorProduct master, overriding any value on the capability row.
            Assert.Equal(42.50m, result!.UnitCost);
            await _testorProductRepo.Received(1).GetUnitPricesByCodesAsync(Arg.Any<IEnumerable<string>>());
        }

        #endregion

        #region AddTestCapabilityAsync

        [Fact]
        public async Task AddTestCapabilityAsync_NoDuplicate_CreatesAndReturnsMappedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var created = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns((TestCapability?)null);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.AddAsync(entity).Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(created);

            var result = await _sut.AddTestCapabilityAsync(dto);

            result.Should().Be(created);
            await _testCapabilityRepo.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddTestCapabilityAsync_DuplicateExists_ThrowsInvalidOperationException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().AddAsync(Arg.Any<TestCapability>());
        }

        #endregion

        #region UpdateTestCapabilityAsync

        [Fact]
        public async Task UpdateTestCapabilityAsync_ValidUpdate_ReturnsUpdatedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.UpdateAsync(entity, "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(updated);

            var result = await _sut.UpdateTestCapabilityAsync(dto);

            result.Should().Be(updated);
            await _testCapabilityRepo.Received(1).UpdateAsync(entity, "WG1");
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WithUnitCost_PersistsToTestorProductMaster()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", UnitCost = 75.25m };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.UpdateAsync(entity, "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(updated);

            await _sut.UpdateTestCapabilityAsync(dto);

            // Unit Cost must be written to the TestorProduct master so it reflects for all rows of the Test Code.
            await _testorProductRepo.Received(1).UpdateUnitPriceByCodeAsync("TC1", 75.25m);
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WithoutUnitCost_DoesNotUpdateTestorProductMaster()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", UnitCost = null };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.UpdateAsync(entity, "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(updated);

            await _sut.UpdateTestCapabilityAsync(dto);

            await _testorProductRepo.DidNotReceive().UpdateUnitPriceByCodeAsync(Arg.Any<string>(), Arg.Any<decimal?>());
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_WorkGroupChanged_UsesOriginalWorkGroupForLookup()
        {
            var dto = new TestCapabilityDto
            {
                TestCode = "TC1",
                WorkGroup = "WG2",
                OriginalWorkGroup = "WG1",
                PlanPortfolio = "PP1"
            };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };
            var entity = new TestCapability { TestCode = "TC1", WorkGroup = "WG2" };
            var updated = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG2" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _mapper.Map<TestCapability>(dto).Returns(entity);
            _testCapabilityRepo.UpdateAsync(entity, "WG1").Returns(entity);
            _mapper.Map<TestCapabilityDto>(entity).Returns(updated);

            var result = await _sut.UpdateTestCapabilityAsync(dto);

            result.Should().Be(updated);
            await _testCapabilityRepo.Received(1).GetByIdAsync("TC1", "WG1");
            await _testCapabilityRepo.Received(1).UpdateAsync(entity, "WG1");
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            var dto = new TestCapabilityDto { TestCode = "MISSING", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            _testCapabilityRepo.GetByIdAsync("MISSING", "WG1").Returns((TestCapability?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateTestCapabilityAsync(dto));
        }

        [Fact]
        public async Task UpdateTestCapabilityAsync_HasDependentReqmts_ThrowsInvalidOperationException()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var existing = new TestCapability { TestCode = "TC1", WorkGroup = "WG1" };

            _testCapabilityRepo.GetByIdAsync("TC1", "WG1").Returns(existing);
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestCapabilityAsync(dto));
            await _testCapabilityRepo.DidNotReceive().UpdateAsync(Arg.Any<TestCapability>());
        }

        #endregion

        #region DeleteTestCapabilityAsync

        [Fact]
        public async Task DeleteTestCapabilityAsync_NoReqmtsDependency_DeletesAndReturnsTrue()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _monthlyOutputRepo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1").Returns(false);
            _testCapabilityRepo.DeleteAsync("TC1", "WG1").Returns(true);

            var result = await _sut.DeleteTestCapabilityAsync("TC1", "WG1");

            result.Should().BeTrue();
            await _testCapabilityRepo.Received(1).DeleteAsync("TC1", "WG1");
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_HasReqmtsDependency_ThrowsInvalidOperationException()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteTestCapabilityAsync("TC1", "WG1"));
            await _testCapabilityRepo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_HasReqmtsDependency_DoesNotCheckMonthlyOutputs()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteTestCapabilityAsync("TC1", "WG1"));
            await _monthlyOutputRepo.DidNotReceive().ExistsByTestCodeAndWorkGroupAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_HasMonthlyOutputDependency_ThrowsInvalidOperationException()
        {
            _testReqmtRepo.ExistsByTestBuyerCodeAsync("TC1WG1").Returns(false);
            _monthlyOutputRepo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1").Returns(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteTestCapabilityAsync("TC1", "WG1"));
            await _testCapabilityRepo.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region GetPagedWgTestCapabilitiesWithDescriptionAsync

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_ValidInput_ReturnsMappedPaginatedResultWithDtoProperties()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "TestCode", Descending = true, Filter = "{\"TestCode\":\"TC\"}" };
            var mappedParams = new PaginationParameters<string> { Page = 2, PageSize = 5, SortBy = "TestCode", Descending = true, Filter = "{\"TestCode\":\"TC\"}" };

            var entities = new List<WgTestCapabilitiesWithDescription>
            {
                new() { WorkGroup = "WG1", TestCode = "TC001", ItemDescription = "Item 1" },
                new() { WorkGroup = "WG1", TestCode = "TC002", ItemDescription = "Item 2" }
            };
            var pagedData = new PagedData<WgTestCapabilitiesWithDescription>(entities, new PaginationData
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 3,
                TotalRecords = 12
            });

            var expected = new PaginatedResult<WgTestCapabilitiesWithDescriptionDto>
            {
                Data = new List<WgTestCapabilitiesWithDescriptionDto>
                {
                    new() { WorkGroup = "WG1", TestCode = "TC001", ItemDescription = "Item 1" },
                    new() { WorkGroup = "WG1", TestCode = "TC002", ItemDescription = "Item 2" }
                },
                PaginationData = new PaginationDto
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 12
                }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedWgTestCapabilitiesWithDescriptionAsync(mappedParams, "WG1").Returns(pagedData);
            _mapper.Map<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>>(pagedData).Returns(expected);

            var result = await _sut.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            result.Should().Be(expected);
            result.Data.Should().HaveCount(2);
            result.Data!.First().WorkGroup.Should().Be("WG1");
            result.Data.First().TestCode.Should().Be("TC001");
            result.Data.First().ItemDescription.Should().Be("Item 1");
            result.PaginationData.PageNumber.Should().Be(2);
            result.PaginationData.PageSize.Should().Be(5);
            result.PaginationData.TotalPages.Should().Be(3);
            result.PaginationData.TotalRecords.Should().Be(12);

            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _testCapabilityRepo.Received(1).GetPagedWgTestCapabilitiesWithDescriptionAsync(mappedParams, "WG1");
            _mapper.Received(1).Map<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>>(pagedData);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_InvalidWorkGroup_ThrowsBusinessValidationErrorException(string? workGroup)
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, workGroup!));

            ex.Errors.Should().ContainSingle();
            ex.Errors[0].Code.Should().Be("WORKGROUP_REQUIRED");
            ex.Errors[0].Message.Should().Be("Work Group is required");

            await _testCapabilityRepo.DidNotReceive()
                .GetPagedWgTestCapabilitiesWithDescriptionAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region GetPagedTestCapabilityByPortfolioAsync — ItemDescription Filter and Sort

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_FilterByItemDescription_ReturnsOnlyMatchingRecords()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":"Alpha"}"""
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC3", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 3 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" },
         new() { TestCode = "TC3", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Alpha Test",
                ["TC2"] = "Beta Test",
                ["TC3"] = "Alpha Beta"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Should().HaveCount(2);
            result.Data.Should().OnlyContain(d => d.ItemDescription!.Contains("Alpha", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_FilterByItemDescription_CaseInsensitive()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":"alpha"}"""
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 1 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "ALPHA Test" };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Should().HaveCount(1);
            result.Data.First().ItemDescription.Should().Be("ALPHA Test");
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_FilterByItemDescription_NoMatch_ReturnsEmpty()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":"NoMatch"}"""
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 1 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?> { ["TC1"] = "Alpha Test" };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByItemDescriptionAscending_ReturnsSortedData()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "ItemDescription",
                Descending = false
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC3", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 3 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" },
         new() { TestCode = "TC3", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Charlie",
                ["TC2"] = "Alpha",
                ["TC3"] = "Bravo"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Select(d => d.ItemDescription).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_SortByItemDescriptionDescending_ReturnsSortedData()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "ItemDescription",
                Descending = true
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC3", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 3 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" },
         new() { TestCode = "TC3", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Charlie",
                ["TC2"] = "Alpha",
                ["TC3"] = "Bravo"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Select(d => d.ItemDescription).Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_NoItemDescriptionFilterOrSort_DataUnchanged()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"WorkGroup":"WG1"}""",
                SortBy = "TestCode"
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 2 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Alpha",
                ["TC2"] = "Beta"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            result.Data.Should().HaveCount(2);
            result.Data.First().ItemDescription.Should().Be("Alpha");
            result.Data.Last().ItemDescription.Should().Be("Beta");
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_FilterAndSortByItemDescription_AppliesBoth()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":"a"}""",
                SortBy = "ItemDescription",
                Descending = false
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC3", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 3 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" },
         new() { TestCode = "TC3", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Charlie",
                ["TC2"] = "Alpha",
                ["TC3"] = "Bravo"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

            // All contain "a" (case-insensitive): Alpha, Bravo, Charlie
            result.Data.Should().HaveCount(3);
            result.Data.Select(d => d.ItemDescription).Should().BeInAscendingOrder();
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolioAsync_EmptyItemDescriptionFilter_DoesNotFilter()
        {
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = """{"ItemDescription":""}"""
            };
            var mappedParams = new PaginationParameters<string>();
            var entities = new List<TestCapability>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" },
         new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1" }
     };
            var pagedData = new PagedData<TestCapability>(entities, new PaginationData { TotalRecords = 2 });
            var dtos = new List<TestCapabilityDto>
     {
         new() { TestCode = "TC1", WorkGroup = "WG1" },
         new() { TestCode = "TC2", WorkGroup = "WG1" }
     };
            var pagedResult = new PaginatedResult<TestCapabilityDto> { Data = dtos };
            var descriptions = new Dictionary<string, string?>
            {
                ["TC1"] = "Alpha",
                ["TC2"] = "Beta"
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _testCapabilityRepo.GetPagedTestCapabilityByPortfolioAsync(mappedParams, "PP1").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData).Returns(pagedResult);
            _testorProductRepo.GetDescriptionsByCodesAsync(Arg.Any<IEnumerable<string>>()).Returns(descriptions);

            var result = await _sut.GetPagedTestCapabilityByPortfolioAsync(query, "PP1");

                    result.Data.Should().HaveCount(2);
                    }

                    #endregion

                    #region BuildTestPlanSummaryAsync

                    [Fact]
                    public async Task BuildTestPlanSummaryAsync_CallsRepository()
                    {
                        // Act
                        await _sut.BuildTestPlanSummaryAsync();

                        // Assert
                        await _testCapabilityRepo.Received(1).BuildTestPlanSummaryAsync();
                    }

                    [Fact]
                    public async Task BuildTestPlanSummaryAsync_DoesNotThrow()
                    {
                        _testCapabilityRepo.BuildTestPlanSummaryAsync().Returns(Task.CompletedTask);

                        var ex = await Record.ExceptionAsync(() => _sut.BuildTestPlanSummaryAsync());

                        Assert.Null(ex);
                    }

                    #endregion

                    #region GetPagedTestPlanCrossTabAsync

                    [Fact]
                    public async Task GetPagedTestPlanCrossTabAsync_MapsQueryAndCallsRepository()
                    {
                        // Arrange
                        var query        = new QueryParameters<string> { Page = 1, PageSize = 20 };
                        var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 20 };
                        var repoResult   = new CrossTabPagedResult
                        {
                            Columns    = ["testcode", "PROG01"],
                            Rows       = [new() { ["testcode"] = "PT001", ["PROG01"] = "200" }],
                            TotalCount = 1,
                            Page       = 1,
                            PageSize   = 20
                        };

                        _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
                        _testCapabilityRepo.GetPagedTestPlanCrossTabAsync(mappedParams).Returns(repoResult);

                        // Act
                        var result = await _sut.GetPagedTestPlanCrossTabAsync(query);

                        // Assert
                        await _testCapabilityRepo.Received(1).GetPagedTestPlanCrossTabAsync(mappedParams);
                        result.Should().NotBeNull();
                    }

                    [Fact]
                    public async Task GetPagedTestPlanCrossTabAsync_MapsResultToDto_ColumnsMatch()
                    {
                        // Arrange
                        var query        = new QueryParameters<string> { Page = 1, PageSize = 20 };
                        var mappedParams = new PaginationParameters<string>();
                        var repoResult   = new CrossTabPagedResult
                        {
                            Columns    = ["testcode", "shortdescription", "PROG01"],
                            Rows       = [],
                            TotalCount = 0,
                            Page       = 1,
                            PageSize   = 20
                        };

                        _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
                        _testCapabilityRepo.GetPagedTestPlanCrossTabAsync(mappedParams).Returns(repoResult);

                        // Act
                        var result = await _sut.GetPagedTestPlanCrossTabAsync(query);

                        // Assert
                        result.Columns.Should().BeEquivalentTo(["testcode", "shortdescription", "PROG01"]);
                    }

                    [Fact]
                    public async Task GetPagedTestPlanCrossTabAsync_MapsResultToDto_RowsMatch()
                    {
                        // Arrange
                        var query        = new QueryParameters<string> { Page = 1, PageSize = 20 };
                        var mappedParams = new PaginationParameters<string>();
                        var rows         = new List<Dictionary<string, string?>>
                        {
                            new() { ["testcode"] = "PT001", ["PROG01"] = "200" },
                            new() { ["testcode"] = "PT002", ["PROG01"] = "50"  }
                        };
                        var repoResult = new CrossTabPagedResult
                        {
                            Columns    = ["testcode", "PROG01"],
                            Rows       = rows,
                            TotalCount = 2,
                            Page       = 1,
                            PageSize   = 20
                        };

                        _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
                        _testCapabilityRepo.GetPagedTestPlanCrossTabAsync(mappedParams).Returns(repoResult);

                        // Act
                        var result = await _sut.GetPagedTestPlanCrossTabAsync(query);

                        // Assert
                        result.Rows.Should().HaveCount(2);
                        result.Rows[0]["testcode"].Should().Be("PT001");
                    }

                    [Fact]
                    public async Task GetPagedTestPlanCrossTabAsync_MapsResultToDto_PaginationMatch()
                    {
                        // Arrange
                        var query        = new QueryParameters<string> { Page = 3, PageSize = 10 };
                        var mappedParams = new PaginationParameters<string>();
                        var repoResult   = new CrossTabPagedResult
                        {
                            Columns    = ["testcode"],
                            Rows       = [],
                            TotalCount = 250,
                            Page       = 3,
                            PageSize   = 10
                        };

                        _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
                        _testCapabilityRepo.GetPagedTestPlanCrossTabAsync(mappedParams).Returns(repoResult);

                        // Act
                        var result = await _sut.GetPagedTestPlanCrossTabAsync(query);

                        // Assert
                        result.TotalCount.Should().Be(250);
                        result.Page.Should().Be(3);
                        result.PageSize.Should().Be(10);
                    }

                    [Fact]
                    public async Task GetPagedTestPlanCrossTabAsync_EmptyRepositoryResult_ReturnsEmptyDto()
                    {
                        // Arrange
                        var query        = new QueryParameters<string> { Page = 1, PageSize = 20 };
                        var mappedParams = new PaginationParameters<string>();
                        var repoResult   = new CrossTabPagedResult
                        {
                            Columns    = [],
                            Rows       = [],
                            TotalCount = 0,
                            Page       = 1,
                            PageSize   = 20
                        };

                        _mapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
                        _testCapabilityRepo.GetPagedTestPlanCrossTabAsync(mappedParams).Returns(repoResult);

                        // Act
                        var result = await _sut.GetPagedTestPlanCrossTabAsync(query);

                        // Assert
                        result.Columns.Should().BeEmpty();
                        result.Rows.Should().BeEmpty();
                        result.TotalCount.Should().Be(0);
                    }

                    #endregion


                }
            }
