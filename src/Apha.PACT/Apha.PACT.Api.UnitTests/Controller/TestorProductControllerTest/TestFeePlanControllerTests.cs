using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TestorProductControllerTest
{
    public class TestFeePlanControllerTests
    {
        private readonly ITestorProductService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly TestorProductController _controller;

        public TestFeePlanControllerTests()
        {
            _serviceMock = Substitute.For<ITestorProductService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new TestorProductController(_serviceMock, _mapperMock);
        }

        #region GetTestSnapshotPaged

        [Fact]
        public async Task GetTestSnapshotPaged_ValidQuery_ReturnsOkWithMappedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestFeePlanDto>(
                [new TestFeePlanDto { TestCode = "T001", Project = "JOB001" }],
                new PaginationDto { TotalRecords = 1 });
            var mapped = new PaginationRes<TestFeePlanRes>
            {
                Data = [new TestFeePlanRes { TestCode = "T001", Project = "JOB001" }]
            };

            _serviceMock.GetTestSnapshotPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestFeePlanRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetTestSnapshotPaged(query);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<TestFeePlanRes>>(ok.Value);
            Assert.Single(response.Data!);
            Assert.Equal("T001", response.Data!.First().TestCode);
        }

        [Fact]
        public async Task GetTestSnapshotPaged_PassesQueryToService()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestFeePlanDto>([], new PaginationDto());
            _serviceMock.GetTestSnapshotPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestFeePlanRes>>(serviceResult)
                .Returns(new PaginationRes<TestFeePlanRes>());

            await _controller.GetTestSnapshotPaged(query);

            await _serviceMock.Received(1).GetTestSnapshotPagedAsync(query);
        }

        [Fact]
        public async Task GetTestSnapshotPaged_EmptyResult_ReturnsOkWithEmptyData()
        {
            var query = new QueryParameters<string>();
            var serviceResult = new PaginatedResult<TestFeePlanDto>([], new PaginationDto());
            _serviceMock.GetTestSnapshotPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<TestFeePlanRes>>(serviceResult)
                .Returns(new PaginationRes<TestFeePlanRes>());

            var result = await _controller.GetTestSnapshotPaged(query);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetTestSnapshotPaged_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.GetTestSnapshotPagedAsync(query)
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetTestSnapshotPaged(query));
        }

        #endregion
    }
}
