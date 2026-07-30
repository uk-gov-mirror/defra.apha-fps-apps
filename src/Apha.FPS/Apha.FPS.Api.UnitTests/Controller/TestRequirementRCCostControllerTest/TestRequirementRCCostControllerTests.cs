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

namespace Apha.FPS.Api.UnitTests.Controller.TestRequirementRCCostControllerTest
{
    public class TestRequirementRCCostControllerTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRequirementRCCostService _service;
        private readonly IMapper _mapper;
        private readonly TestRequirementRCCostController _controller;

        public TestRequirementRCCostControllerTests()
        {
            _service = Substitute.For<ITestRequirementRCCostService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestRequirementRCCostController(_service, _mapper);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementRCCostDto>
            {
                Data = new List<TestRequirementRCCostDto> { CreateTestDto(), CreateTestDto() },
                PaginationData = new PaginationDto { TotalRecords = 2 }
            };
            var mappedRes = new PaginationRes<TestRequirementRCCostRes>
            {
                Data = new List<TestRequirementRCCostRes> { CreateTestRes(), CreateTestRes() },
                PaginationData = new Pagination { TotalRecords = 2 }
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, DefaultTestCode).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementRCCostRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestRequirementRCCostRes>>(okResult.Value);
            Assert.Equal(2, data.Data.Count());
        }

        [Fact]
        public async Task GetByTestCodeAsync_ServiceReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementRCCostDto>
            {
                Data = new List<TestRequirementRCCostDto>(),
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };
            var mappedRes = new PaginationRes<TestRequirementRCCostRes>
            {
                Data = new List<TestRequirementRCCostRes>(),
                PaginationData = new Pagination { TotalRecords = 0 }
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, DefaultTestCode).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementRCCostRes>>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetByTestCodeAsync(DefaultTestCode, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsType<PaginationRes<TestRequirementRCCostRes>>(okResult.Value);
            Assert.Empty(data.Data);
        }

        [Fact]
        public async Task GetByTestCodeAsync_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestRequirementRCCostDto>
            {
                Data = new List<TestRequirementRCCostDto>(),
                PaginationData = new PaginationDto()
            };

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _service.GetPagedByTestCodeAsync(queryParams, "ALPHA").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestRequirementRCCostRes>>(serviceResult)
                .Returns(new PaginationRes<TestRequirementRCCostRes> { Data = new List<TestRequirementRCCostRes>(), PaginationData = new Pagination() });

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

            _service.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
                .Returns(dto);
            _mapper.Map<TestRequirementRCCostRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<TestRequirementRCCostRes>(okResult.Value);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _service.GetByKeyAsync("NOTEXIST", "B999", "PC999")
                .Returns((TestRequirementRCCostDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetByKeyAsync("NOTEXIST", "B999", "PC999"));
        }

        #endregion

        #region Helper Methods

        private static TestRequirementRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        private static TestRequirementRCCostRes CreateTestRes() =>
            new()
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        #endregion
    }
}
