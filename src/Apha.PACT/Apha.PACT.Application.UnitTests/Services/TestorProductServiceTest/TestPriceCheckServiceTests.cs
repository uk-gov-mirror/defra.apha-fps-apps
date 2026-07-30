using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.TestorProductServiceTest
{
    public class TestPriceCheckServiceTests
    {
        private readonly ITestorProductRepository _repo;
        private readonly ITestCapabilityRepository _testCapabilityRepo;
        private readonly IMapper _mapper;
        private readonly TestorProductService _sut;

        public TestPriceCheckServiceTests()
        {
            _repo = Substitute.For<ITestorProductRepository>();
            _testCapabilityRepo = Substitute.For<ITestCapabilityRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new TestorProductService(_repo, _testCapabilityRepo, _mapper);
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_ValidQuery_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var entity = new TestPriceCheckView { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m };
            var pagedData = new PagedData<TestPriceCheckView>([entity], new PaginationData { TotalRecords = 1 });
            var expected = new PaginatedResult<TestPriceCheckDto>
            {
                Data = [new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m }]
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repo.GetTestPriceCheckPagedAsync(parameters, "all", null).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestPriceCheckDto>>(pagedData).Returns(expected);

            var result = await _sut.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.NotNull(result);
            Assert.Single(result.Data!);
            Assert.Equal("T001", result.Data!.First().TestCode);
            await _repo.Received(1).GetTestPriceCheckPagedAsync(parameters, "all", null);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_WithOwnerFilter_PassesOwnerToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<TestPriceCheckView>([], new PaginationData());
            var expected = new PaginatedResult<TestPriceCheckDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repo.GetTestPriceCheckPagedAsync(parameters, "zero", "AB").Returns(pagedData);
            _mapper.Map<PaginatedResult<TestPriceCheckDto>>(pagedData).Returns(expected);

            await _sut.GetTestPriceCheckPagedAsync(query, "zero", "AB");

            await _repo.Received(1).GetTestPriceCheckPagedAsync(parameters, "zero", "AB");
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_EmptyResult_ReturnsMappedEmptyResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<TestPriceCheckView>([], new PaginationData());
            var expected = new PaginatedResult<TestPriceCheckDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repo.GetTestPriceCheckPagedAsync(parameters, "all", null).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestPriceCheckDto>>(pagedData).Returns(expected);

            var result = await _sut.GetTestPriceCheckPagedAsync(query, "all", null);

            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsMappedDto()
        {
            var entity = new TestPriceCheckView { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m, NormalPrice = 50m };
            var expected = new TestPriceCheckDto { TestCode = "T001", JobCode = "JOB001", TestPrice = 50m, NormalPrice = 50m };

            _repo.GetTestPriceCheckByKeyAsync("T001", "JOB001").Returns(entity);
            _mapper.Map<TestPriceCheckDto>(entity).Returns(expected);

            var result = await _sut.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            Assert.Equal("T001",   result.TestCode);
            Assert.Equal("JOB001", result.JobCode);
            Assert.Equal(50m,      result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_NonExistentKey_ReturnsNull()
        {
            _repo.GetTestPriceCheckByKeyAsync("MISSING", "MISSING").Returns((TestPriceCheckView?)null);

            var result = await _sut.GetTestPriceCheckByKeyAsync("MISSING", "MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_RepositoryThrows_PropagatesException()
        {
            _repo.GetTestPriceCheckByKeyAsync("T001", "JOB001")
                .ThrowsAsync(new InvalidOperationException("DB error"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetTestPriceCheckByKeyAsync("T001", "JOB001"));
        }

        #endregion

        #region UpdateTestPriceCheckAsync

        [Fact]
        public async Task UpdateTestPriceCheckAsync_ValidInput_CallsRepositoryWithExtractedFields()
        {
            var dto = new TestPriceCheckDto
            {
                IsDefraProject = -1,
                TestPrice      = 75m,
                DefraUnitPrice = 120m
            };
            _repo.UpdateTestPriceCheckAsync("T001", "JOB001", -1, 75m, 120m).Returns(true);

            var result = await _sut.UpdateTestPriceCheckAsync("T001", "JOB001", dto);

            Assert.True(result);
            await _repo.Received(1).UpdateTestPriceCheckAsync("T001", "JOB001", -1, 75m, 120m);
        }

        [Fact]
        public async Task UpdateTestPriceCheckAsync_NullPrices_PassesNullToRepository()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = null, DefraUnitPrice = null };
            _repo.UpdateTestPriceCheckAsync("T001", "JOB001", 0, null, null).Returns(true);

            var result = await _sut.UpdateTestPriceCheckAsync("T001", "JOB001", dto);

            Assert.True(result);
            await _repo.Received(1).UpdateTestPriceCheckAsync("T001", "JOB001", 0, null, null);
        }

        [Fact]
        public async Task UpdateTestPriceCheckAsync_RepositoryReturnsFalse_ReturnsFalse()
        {
            var dto = new TestPriceCheckDto { IsDefraProject = 0, TestPrice = 50m, DefraUnitPrice = 80m };
            _repo.UpdateTestPriceCheckAsync("T001", "JOB001", 0, 50m, 80m).Returns(false);

            var result = await _sut.UpdateTestPriceCheckAsync("T001", "JOB001", dto);

            Assert.False(result);
        }

        #endregion
    }
}
