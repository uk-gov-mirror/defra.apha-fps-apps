using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests.Services.TestorProductServiceTest
{
    public class TestFeePlanServiceTests
    {
        private readonly ITestorProductRepository _repo;
        private readonly ITestCapabilityRepository _testCapabilityRepo;
        private readonly IMapper _mapper;
        private readonly TestorProductService _sut;

        public TestFeePlanServiceTests()
        {
            _repo = Substitute.For<ITestorProductRepository>();
            _testCapabilityRepo = Substitute.For<ITestCapabilityRepository>();
            _mapper = Substitute.For<IMapper>();
            _sut = new TestorProductService(_repo, _testCapabilityRepo, _mapper);
        }

        #region GetTestSnapshotPagedAsync

        [Fact]
        public async Task GetTestSnapshotPagedAsync_ValidQuery_ReturnsMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var entity = new TestFeePlanView { TestCode = "T001", Project = "JOB001", TestFee = 250d };
            var pagedData = new PagedData<TestFeePlanView>([entity], new PaginationData { TotalRecords = 1 });
            var expected = new PaginatedResult<TestFeePlanDto>
            {
                Data = [new TestFeePlanDto { TestCode = "T001", Project = "JOB001", TestFee = 250d }]
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repo.GetTestSnapshotPagedAsync(parameters).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestFeePlanDto>>(pagedData).Returns(expected);

            var result = await _sut.GetTestSnapshotPagedAsync(query);

            Assert.NotNull(result);
            Assert.Single(result.Data!);
            Assert.Equal("T001", result.Data!.First().TestCode);
            await _repo.Received(1).GetTestSnapshotPagedAsync(parameters);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_EmptyResult_ReturnsMappedEmptyResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>();
            var pagedData = new PagedData<TestFeePlanView>([], new PaginationData());
            var expected = new PaginatedResult<TestFeePlanDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repo.GetTestSnapshotPagedAsync(parameters).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestFeePlanDto>>(pagedData).Returns(expected);

            var result = await _sut.GetTestSnapshotPagedAsync(query);

            Assert.Empty(result.Data!);
        }

        #endregion
    }
}
