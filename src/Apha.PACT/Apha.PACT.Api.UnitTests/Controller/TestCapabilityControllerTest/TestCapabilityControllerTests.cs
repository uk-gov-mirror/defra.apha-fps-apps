using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.TestCapabilityControllerTest
{
    public class TestCapabilityControllerTests
    {
        private readonly ITestCapabilityService _service;
        private readonly IMapper _mapper;
        private readonly TestCapabilityController _controller;

        public TestCapabilityControllerTests()
        {
            _service = Substitute.For<ITestCapabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new TestCapabilityController(_service, _mapper);
        }

        #region GetPagedByWorkGroup

        [Fact]
        public async Task GetPagedByWorkGroup_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByWorkGroupAsync(query, "WG1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByWorkGroup(query, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByWorkGroup_NullWorkGroup_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByWorkGroupAsync(query, null).Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByWorkGroup(query, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedByWorkGroup_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedByWorkGroupAsync(query, null).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedByWorkGroup(query, null));
        }

        #endregion

        #region GetPagedByTestCode

        [Fact]
        public async Task GetPagedByTestCode_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedByTestCodeAsync(query, "TC1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedByTestCode(query, "TC1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByTestCode_NullTestCode_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedByTestCodeAsync(query, null).Returns(new PaginatedResult<TestCapabilityDto>());
            _mapper.Map<PaginationRes<TestCapabilityRes>>(Arg.Any<PaginatedResult<TestCapabilityDto>>())
                .Returns(new PaginationRes<TestCapabilityRes>());

            var result = await _controller.GetPagedByTestCode(query, null);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region GetTestCapabilityById

        [Fact]
        public async Task GetTestCapabilityById_RecordFound_ReturnsOk()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _service.GetTestCapabilityByIdAsync("TC1", "WG1").Returns(dto);
            _mapper.Map<TestCapabilityRes>(dto).Returns(mapped);

            var result = await _controller.GetTestCapabilityById("TC1", "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestCapabilityById_RecordNotFound_ThrowsKeyNotFoundException()
        {
            _service.GetTestCapabilityByIdAsync("MISSING", "WG1").Returns((TestCapabilityDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetTestCapabilityById("MISSING", "WG1"));
        }

        #endregion

        #region CreateTestCapability

        [Fact]
        public async Task CreateTestCapability_ValidRequest_ReturnsOk()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var created = new TestCapabilityDto { TestCode = "TC1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.AddTestCapabilityAsync(dto).Returns(created);
            _mapper.Map<TestCapabilityRes>(created).Returns(mapped);

            var result = await _controller.CreateTestCapability(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task CreateTestCapability_DuplicateRecord_ThrowsInvalidOperationException()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto();

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.AddTestCapabilityAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Duplicate record exists."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.CreateTestCapability(request));
        }

        #endregion

        #region UpdateTestCapability

        [Fact]
        public async Task UpdateTestCapability_ValidRequest_ReturnsOk()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var updated = new TestCapabilityDto { TestCode = "TC1" };
            var mapped = new TestCapabilityRes { TestCode = "TC1" };

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto).Returns(updated);
            _mapper.Map<TestCapabilityRes>(updated).Returns(mapped);

            var result = await _controller.UpdateTestCapability(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task UpdateTestCapability_HasDependentReqmts_ThrowsInvalidOperationException()
        {
            var request = new TestCapabilityReq { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto();

            _mapper.Map<TestCapabilityDto>(request).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Cannot update, test requirements are dependant on this."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.UpdateTestCapability(request));
        }

        #endregion

        #region DeleteTestCapability

        [Fact]
        public async Task DeleteTestCapability_RecordDeleted_ReturnsOkWithTrue()
        {
            _service.DeleteTestCapabilityAsync("TC1", "WG1").Returns(true);

            var result = await _controller.DeleteTestCapability("TC1", "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(true);
        }

        [Fact]
        public async Task DeleteTestCapability_HasReqmtsDependency_ThrowsInvalidOperationException()
        {
            _service.DeleteTestCapabilityAsync("TC1", "WG1")
                .ThrowsAsync(new InvalidOperationException("Cannot delete, test requirements are dependant on this."));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.DeleteTestCapability("TC1", "WG1"));
        }

        #endregion

        #region GetPagedTestCapabilityByPortfolio

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolio_HappyPath_ReturnsOkWithPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<TestCapabilityDto>();
            var mapped = new PaginationRes<TestCapabilityRes>();

            _service.GetPagedTestCapabilityByPortfolioAsync(query, "PF1").Returns(serviceResult);
            _mapper.Map<PaginationRes<TestCapabilityRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedTestCapabilityByPortfolio(query, "PF1");

            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolio_NullPortfolio_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedTestCapabilityByPortfolioAsync(query, null).Returns(new PaginatedResult<TestCapabilityDto>());
            _mapper.Map<PaginationRes<TestCapabilityRes>>(Arg.Any<PaginatedResult<TestCapabilityDto>>())
                .Returns(new PaginationRes<TestCapabilityRes>());

            var result = await _controller.GetPagedTestCapabilityByPortfolio(query, null);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetPagedTestCapabilityByPortfolio_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedTestCapabilityByPortfolioAsync(query, null).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedTestCapabilityByPortfolio(query, null));
        }

        #endregion

        #region GetPagedWgTestCapabilitiesWithDescriptionAsync

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_HappyPath_ReturnsOkWithMappedPaginationResAndProperties()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "TestCode", Descending = true, Filter = "{\"TestCode\":\"TC\"}" };

            var serviceResult = new PaginatedResult<WgTestCapabilitiesWithDescriptionDto>
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

            var mapped = new PaginationRes<WgTestCapabilitiesWithDescriptionRes>
            {
                Data = new List<WgTestCapabilitiesWithDescriptionRes>
                {
                    new() { WorkGroup = "WG1", TestCode = "TC001", ItemDescription = "Item 1" },
                    new() { WorkGroup = "WG1", TestCode = "TC002", ItemDescription = "Item 2" }
                },
                PaginationData = new Pagination
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 12
                }
            };

            _service.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1").Returns(serviceResult);
            _mapper.Map<PaginationRes<WgTestCapabilitiesWithDescriptionRes>>(serviceResult).Returns(mapped);

            var result = await _controller.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<WgTestCapabilitiesWithDescriptionRes>>(ok.Value);

            value.Data.Should().HaveCount(2);
            value.Data!.First().WorkGroup.Should().Be("WG1");
            value.Data.First().TestCode.Should().Be("TC001");
            value.Data.First().ItemDescription.Should().Be("Item 1");
            value.PaginationData.PageNumber.Should().Be(2);
            value.PaginationData.PageSize.Should().Be(5);
            value.PaginationData.TotalPages.Should().Be(3);
            value.PaginationData.TotalRecords.Should().Be(12);

            await _service.Received(1).GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1");
            _mapper.Received(1).Map<PaginationRes<WgTestCapabilitiesWithDescriptionRes>>(serviceResult);
        }

        [Fact]
        public async Task GetPagedWgTestCapabilitiesWithDescriptionAsync_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            _service.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1").ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, "WG1"));
        }

        #endregion

        #region GetPagedTestPlanCrossTab

        [Fact]
        public async Task GetPagedTestPlanCrossTab_HappyPath_ReturnsOkWithResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var serviceResult = new Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto
            {
                Columns    = ["testcode", "shortdescription", "PROG01"],
                Rows       = [new Dictionary<string, string?> { ["testcode"] = "PT001", ["PROG01"] = "200" }],
                TotalCount = 1,
                Page       = 1,
                PageSize   = 20
            };

            _service.GetPagedTestPlanCrossTabAsync(query).Returns(serviceResult);

            // Act
            var result = await _controller.GetPagedTestPlanCrossTab(query);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            ok.Value.Should().Be(serviceResult);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTab_EmptyResult_ReturnsOkWithEmptyCollections()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var serviceResult = new Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto
            {
                Columns    = [],
                Rows       = [],
                TotalCount = 0,
                Page       = 1,
                PageSize   = 20
            };

            _service.GetPagedTestPlanCrossTabAsync(query).Returns(serviceResult);

            // Act
            var result = await _controller.GetPagedTestPlanCrossTab(query);

            // Assert
            var ok  = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto>(ok.Value);
            dto.Rows.Should().BeEmpty();
            dto.Columns.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTab_WithMultipleRows_ReturnsOkWithAllRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var serviceResult = new Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto
            {
                Columns    = ["testcode", "shortdescription", "PROG01", "PROG02"],
                Rows       =
                [
                    new() { ["testcode"] = "PT001", ["PROG01"] = "200", ["PROG02"] = "100" },
                    new() { ["testcode"] = "PT002", ["PROG01"] = "50",  ["PROG02"] = null  }
                ],
                TotalCount = 2,
                Page       = 1,
                PageSize   = 20
            };

            _service.GetPagedTestPlanCrossTabAsync(query).Returns(serviceResult);

            // Act
            var result = await _controller.GetPagedTestPlanCrossTab(query);

            // Assert
            var ok  = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto>(ok.Value);
            dto.TotalCount.Should().Be(2);
            dto.Rows.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTab_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _service.GetPagedTestPlanCrossTabAsync(query).ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPagedTestPlanCrossTab(query));
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTab_ReturnsCorrectPaginationValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 10 };
            var serviceResult = new Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto
            {
                Columns    = ["testcode"],
                Rows       = [],
                TotalCount = 250,
                Page       = 2,
                PageSize   = 10
            };

            _service.GetPagedTestPlanCrossTabAsync(query).Returns(serviceResult);

            // Act
            var result = await _controller.GetPagedTestPlanCrossTab(query);

            // Assert
            var ok  = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<Apha.PACT.Application.Dtos.TestPlanCostBreakdownDto>(ok.Value);
            dto.TotalCount.Should().Be(250);
            dto.Page.Should().Be(2);
            dto.PageSize.Should().Be(10);
        }

        #endregion

    }
}
