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

namespace Apha.PACT.Api.UnitTests.Controller.TestRequirementControllerTest
{
    public class TestRequirementControllerTests
    {
        private readonly ITestRequirementService _service;
        private readonly IMapper _mapper;
        private readonly TestRequirementController _sut;

        public TestRequirementControllerTests()
        {
            _service = Substitute.For<ITestRequirementService>();
            _mapper  = Substitute.For<IMapper>();
            _sut     = new TestRequirementController(_service, _mapper);
        }

        #region GetPagedTestReqmt

        [Fact]
        public async Task GetPagedTestReqmt_ValidQuery_ReturnsOk()
        {
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result  = new PaginatedResult<TestRequirementtDto>([], new PaginationDto());
            var mapped  = new PaginationRes<TestRequirementtRes>();

            _service.GetPagedTestReqmtAsync(query, "PT0001").Returns(result);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(result).Returns(mapped);

            var action = await _sut.GetPagedTestReqmt(query, "PT0001");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedTestReqmt_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedTestReqmtAsync(query, "PT0001").ThrowsAsync(new Exception("error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedTestReqmt(query, "PT0001"));
        }

        [Fact]
        public async Task GetPagedTestReqmt_CallsServiceExactlyOnce()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result = new PaginatedResult<TestRequirementtDto>([], new PaginationDto());
            _service.GetPagedTestReqmtAsync(query, "PT0001").Returns(result);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(result).Returns(new PaginationRes<TestRequirementtRes>());

            await _sut.GetPagedTestReqmt(query, "PT0001");

            await _service.Received(1).GetPagedTestReqmtAsync(query, "PT0001");
        }

        #endregion

        #region GetPagedBySupplierTestCode

        [Fact]
        public async Task GetPagedBySupplierTestCode_ValidQuery_ReturnsOk()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result = new PaginatedResult<TestSupplierViewDto>([], new PaginationDto());
            var mapped = new PaginationRes<TestSupplierViewRes>();

            _service.GetPagedBySupplierTestCodeAsync(query, "PT0001", false).Returns(result);
            _mapper.Map<PaginationRes<TestSupplierViewRes>>(result).Returns(mapped);

            var action = await _sut.GetPagedBySupplierTestCode(query, "PT0001");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCode_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedBySupplierTestCodeAsync(query, "PT0001", false)
                    .ThrowsAsync(new Exception("error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetPagedBySupplierTestCode(query, "PT0001"));
        }

        #endregion

        #region GetPagedByProject

        [Fact]
        public async Task GetPagedByProject_ValidQuery_ReturnsOk()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result = new PaginatedResult<TestRequirementtDto>([], new PaginationDto());
            var mapped = new PaginationRes<TestRequirementtRes>();

            _service.GetPagedTestReqmtByProjectAsync(query, "PROJ01").Returns(result);
            _mapper.Map<PaginationRes<TestRequirementtRes>>(result).Returns(mapped);

            var action = await _sut.GetPagedByProject(query, "PROJ01");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPagedByProject_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPagedTestReqmtByProjectAsync(query, "PROJ01")
                    .ThrowsAsync(new Exception("error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedByProject(query, "PROJ01"));
        }

        #endregion

        #region GetAllTestReqmtForExport

        [Fact]
        public async Task GetAllTestReqmtForExport_ValidTestCode_ReturnsOk()
        {
            var items  = new List<TestRequirementtDto>();
            var mapped = new List<TestRequirementtRes>();

            _service.GetAllTestReqmtForExportAsync("PT0001", null).Returns(items);
            _mapper.Map<IEnumerable<TestRequirementtRes>>(items).Returns(mapped);

            var action = await _sut.GetAllTestReqmtForExport("PT0001");

            action.Should().BeOfType<OkObjectResult>();
        }

        #endregion

        #region GetTestReqmtById

        [Fact]
        public async Task GetTestReqmtById_ExistingRecord_ReturnsOk()
        {
            var dto    = new TestRequirementtDto { TestCode = "PT0001", Buyer = "SV3300" };
            var mapped = new TestRequirementtRes();

            _service.GetTestReqmtByIdAsync("PT0001", "SV3300").Returns(dto);
            _mapper.Map<TestRequirementtRes>(dto).Returns(mapped);

            var action = await _sut.GetTestReqmtById("PT0001", "SV3300");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestReqmtById_NotFound_ThrowsKeyNotFoundException()
        {
            _service.GetTestReqmtByIdAsync("PT9999", "SV0000").Returns((TestRequirementtDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _sut.GetTestReqmtById("PT9999", "SV0000"));
        }

        #endregion

        #region CreateTestReqmt

        [Fact]
        public async Task CreateTestReqmt_ValidRequest_ReturnsOk()
        {
            var request = new TestRequirementReq();
            var dto     = new TestRequirementtDto();
            var created = new TestRequirementtDto();
            var mapped  = new TestRequirementtRes();

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.AddTestReqmtAsync(dto).Returns(created);
            _mapper.Map<TestRequirementtRes>(created).Returns(mapped);

            var action = await _sut.CreateTestReqmt(request);

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task CreateTestReqmt_ServiceThrows_PropagatesException()
        {
            var request = new TestRequirementReq();
            var dto     = new TestRequirementtDto();
            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.AddTestReqmtAsync(dto).ThrowsAsync(new InvalidOperationException("Duplicate"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTestReqmt(request));
        }

        #endregion

        #region UpdateTestReqmt

        [Fact]
        public async Task UpdateTestReqmt_ValidRequest_ReturnsOk()
        {
            var request = new TestRequirementReq();
            var dto     = new TestRequirementtDto();
            var updated = new TestRequirementtDto();
            var mapped  = new TestRequirementtRes();

            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.UpdateTestReqmtAsync(dto).Returns(updated);
            _mapper.Map<TestRequirementtRes>(updated).Returns(mapped);

            var action = await _sut.UpdateTestReqmt(request);

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task UpdateTestReqmt_ServiceThrows_PropagatesException()
        {
            var request = new TestRequirementReq();
            var dto     = new TestRequirementtDto();
            _mapper.Map<TestRequirementtDto>(request).Returns(dto);
            _service.UpdateTestReqmtAsync(dto).ThrowsAsync(new InvalidOperationException("Cannot update"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTestReqmt(request));
        }

        #endregion

        #region DeleteTestReqmt

        [Fact]
        public async Task DeleteTestReqmt_ExistingRecord_ReturnsOkTrue()
        {
            _service.DeleteTestReqmtAsync("PT0001", "SV3300").Returns(true);

            var action = await _sut.DeleteTestReqmt("PT0001", "SV3300");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(true);
        }

        [Fact]
        public async Task DeleteTestReqmt_NotFound_ReturnsOkFalse()
        {
            _service.DeleteTestReqmtAsync("PT9999", "SV0000").Returns(false);

            var action = await _sut.DeleteTestReqmt("PT9999", "SV0000");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(false);
        }

        #endregion

        #region GetTestReqmtPricing

        [Fact]
        public async Task GetTestReqmtPricing_Found_ReturnsOk()
        {
            var dto    = new TestRequirementtDto();
            var mapped = new TestRequirementtRes();

            _service.GetTestReqmtPricingAsync("PT0001", null).Returns(dto);
            _mapper.Map<TestRequirementtRes>(dto).Returns(mapped);

            var action = await _sut.GetTestReqmtPricing("PT0001");

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetTestReqmtPricing_NotFound_ReturnsNotFound()
        {
            _service.GetTestReqmtPricingAsync("PT9999", null).Returns((TestRequirementtDto?)null);

            var action = await _sut.GetTestReqmtPricing("PT9999");

            action.Should().BeOfType<NotFoundObjectResult>();
        }

        #endregion

        #region GetPlannedTestsByWorkgroup

        [Fact]
        public async Task GetPlannedTestsByWorkgroup_ValidQuery_ReturnsOk()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result = new PaginatedResult<TestReqBreakdownDto>([], new PaginationDto());
            var mapped = new PaginationRes<TestReqBreakdownRes>();

            _service.GetPlannedTestsByWorkgroupAsync(query).Returns(result);
            _mapper.Map<PaginationRes<TestReqBreakdownRes>>(result).Returns(mapped);

            var action = await _sut.GetPlannedTestsByWorkgroup(query);

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mapped);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroup_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetPlannedTestsByWorkgroupAsync(query).ThrowsAsync(new Exception("error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPlannedTestsByWorkgroup(query));
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroup_CallsServiceExactlyOnce()
        {
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var result = new PaginatedResult<TestReqBreakdownDto>([], new PaginationDto());
            _service.GetPlannedTestsByWorkgroupAsync(query).Returns(result);
            _mapper.Map<PaginationRes<TestReqBreakdownRes>>(result).Returns(new PaginationRes<TestReqBreakdownRes>());

            await _sut.GetPlannedTestsByWorkgroup(query);

            await _service.Received(1).GetPlannedTestsByWorkgroupAsync(query);
        }

        #endregion

        #region GetActualsTestsWithPlannedDataByWorkgroupAsync

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_ValidQuery_ReturnsOk()
        {
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            var mappedRes       = new PaginationRes<TestActualBreakdownRes>();

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult).Returns(mappedRes);

            var action = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            action.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().Be(mappedRes);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithItems_ReturnsOkWithData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300" },
                new() { TestCode = "PT0049", Buyer = "SB4600" }
            };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>(dtos, new PaginationDto());
            var mappedRes       = new PaginationRes<TestActualBreakdownRes>
            {
                Data = [new() { TestCode = "PT0047" }, new() { TestCode = "PT0049" }]
            };

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult).Returns(mappedRes);

            var ok    = (await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query))
                            .Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeOfType<PaginationRes<TestActualBreakdownRes>>()
              .Which.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            var mappedRes       = new PaginationRes<TestActualBreakdownRes> { Data = [] };

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult).Returns(mappedRes);

            var ok = (await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query))
                         .Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeOfType<PaginationRes<TestActualBreakdownRes>>()
              .Which.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query)
                    .ThrowsAsync(new Exception("service error"));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query));
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_CallsServiceExactlyOnce()
        {
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult)
                   .Returns(new PaginationRes<TestActualBreakdownRes>());

            await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            await _service.Received(1).GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_CallsMapperExactlyOnce()
        {
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            var mappedRes       = new PaginationRes<TestActualBreakdownRes>();

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult).Returns(mappedRes);

            await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            _mapper.Received(1).Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithSortingQuery_ReturnsOk()
        {
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };
            var paginatedResult = new PaginatedResult<TestActualBreakdownDto>([], new PaginationDto());
            var mappedRes       = new PaginationRes<TestActualBreakdownRes>();

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(paginatedResult);
            _mapper.Map<PaginationRes<TestActualBreakdownRes>>(paginatedResult).Returns(mappedRes);

            var action = await _sut.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            action.Should().BeOfType<OkObjectResult>();
        }

        #endregion
    }
}
