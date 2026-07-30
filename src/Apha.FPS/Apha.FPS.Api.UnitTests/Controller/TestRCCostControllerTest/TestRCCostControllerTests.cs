using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.TestRCCostControllerTest
{
    public class TestRCCostControllerTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRCCostService _service;
        private readonly IMapper _mapper;
        private readonly TestRCCostController _controller;

        public TestRCCostControllerTests()
        {
            _service = Substitute.For<ITestRCCostService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestRCCostController(_service, _mapper);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRCCostDto>
            {
                Data = new List<TestRCCostDto> { CreateTestDto(), CreateTestDto() },
                PaginationData = new PaginationDto { TotalRecords = 2 }
            };
            var mappedRes = new PaginationRes<TestRCCostRes>
            {
                Data = new List<TestRCCostRes> { CreateTestRes(), CreateTestRes() },
                PaginationData = new Pagination { TotalRecords = 2 }
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, DefaultTestCode).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRCCostRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestRCCostRes>>(okResult.Value);
            Assert.Equal(2, data.Data.Count());
        }

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRCCostDto>
            {
                Data = new List<TestRCCostDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            var mappedRes = new PaginationRes<TestRCCostRes>
            {
                Data = new List<TestRCCostRes>(),
                PaginationData = new Pagination { TotalRecords = 0 }
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, DefaultTestCode).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRCCostRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestRCCostRes>>(okResult.Value);
            Assert.Empty(data.Data);
        }

        [Fact]
        public async Task GetByTestCodeAsync_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRCCostDto>
            {
                Data = new List<TestRCCostDto>(),
                PaginationData = new PaginationDto()
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, "ALPHA").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRCCostRes>>(serviceResult)
                .Returns(new PaginationRes<TestRCCostRes> { Data = new List<TestRCCostRes>(), PaginationData = new Pagination() });

            // Act
            await _controller.GetByTestCodeAsync("ALPHA", query);

            // Assert
            await _service.Received(1).GetPagedByTestCodeAsync(queryParams, "ALPHA");
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ExistingRecord_ReturnsOkWithMappedRecord()
        {
            // Arrange
            var dto = CreateTestDto();
            var res = CreateTestRes();

            _service.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre).Returns(dto);
            _mapper.Map<TestRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRCCostRes>(okResult.Value);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ReturnsOkWithEmptyRecord()
        {
            // Arrange
            _service.GetByKeyAsync("NOTEXIST", "PC999").Returns((TestRCCostDto?)null);

            // Act
            var result = await _controller.GetByKeyAsync("NOTEXIST", "PC999");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRCCostRes>(okResult.Value);
        }

        #endregion

        #region Helper Methods

        private static TestRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        private static TestRCCostRes CreateTestRes() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        #endregion
    }
}
