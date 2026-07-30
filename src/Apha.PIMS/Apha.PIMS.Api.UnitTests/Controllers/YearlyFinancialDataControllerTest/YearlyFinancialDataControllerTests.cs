using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.YearlyFinancialDataControllerTest
{
    public class YearlyFinancialDataControllerTests
    {
        private readonly IYearlyFinancialDataService _service;
        private readonly IMapper                     _mapper;
        private readonly YearlyFinancialDataController _controller;

        public YearlyFinancialDataControllerTests()
        {
            _service    = Substitute.For<IYearlyFinancialDataService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new YearlyFinancialDataController(_service, _mapper);
        }

        // ── shared helpers ────────────────────────────────────────────────

        private static YearlyFinancialDataDto SampleDto(short year = 2024, string project = "PP001")
            => new() { Year = year, Project = project, BfBudget = 10000m };

        private static YearlyFinancialDataRes SampleRes(short year = 2024, string project = "PP001")
            => new() { Year = year, Project = project, BfBudget = 10000m };

        private static PactProjectYearCostsDto SamplePactDto(string project = "PP001", short year = 2024)
            => new() { Project = project, Year = year };

        private static PactProjectYearCostsRes SamplePactRes(string project = "PP001", short year = 2024)
            => new() { Project = project, Year = year };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new YearlyFinancialDataController(_service, _mapper);
            Assert.NotNull(controller);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithValidProject_ReturnsOkWithMappedPaginatedResult()
        {
            // Arrange
            const string project    = "PP001";
            var paginationReq       = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams         = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = project };
            var dtos                = new List<YearlyFinancialDataDto> { SampleDto() };
            var paginatedResult     = new PaginatedResult<YearlyFinancialDataDto>(dtos, new PaginationDto { TotalRecords = 1 });
            var resList             = new List<YearlyFinancialDataRes> { SampleRes() };
            var paginationRes       = new PaginationRes<YearlyFinancialDataRes>(resList, new Pagination { TotalRecords = 1 });

            _mapper.Map<QueryParameters<string>>(paginationReq).Returns(queryParams);
            _service.GetAllAsync(Arg.Is<QueryParameters<string>>(p => p.Filter == project)).Returns(paginatedResult);
            _mapper.Map<PaginationRes<YearlyFinancialDataRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAll(project, paginationReq);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);
            await _service.Received(1).GetAllAsync(Arg.Is<QueryParameters<string>>(p => p.Filter == project));
            _mapper.Received(1).Map<PaginationRes<YearlyFinancialDataRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAll_WithEmptyResult_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            const string project  = "PP999";
            var req               = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams       = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = project };
            var emptyResult       = new PaginatedResult<YearlyFinancialDataDto>([], new PaginationDto { TotalRecords = 0 });
            var emptyPageRes      = new PaginationRes<YearlyFinancialDataRes>();

            _mapper.Map<QueryParameters<string>>(req).Returns(queryParams);
            _service.GetAllAsync(Arg.Any<QueryParameters<string>>()).Returns(emptyResult);
            _mapper.Map<PaginationRes<YearlyFinancialDataRes>>(emptyResult).Returns(emptyPageRes);

            // Act
            var result = await _controller.GetAll(project, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyPageRes, okResult.Value);
            await _service.Received(1).GetAllAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetAll_SetsFilterToProjectBeforeCallingService()
        {
            // Arrange
            const string project = "PP002";
            var req              = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var baseParams       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginated        = new PaginatedResult<YearlyFinancialDataDto>([], new PaginationDto());
            var pageRes          = new PaginationRes<YearlyFinancialDataRes>();

            _mapper.Map<QueryParameters<string>>(req).Returns(baseParams);
            _service.GetAllAsync(Arg.Any<QueryParameters<string>>()).Returns(paginated);
            _mapper.Map<PaginationRes<YearlyFinancialDataRes>>(paginated).Returns(pageRes);

            // Act
            await _controller.GetAll(project, req);

            // Assert — the controller must override Filter with the project route param
            await _service.Received(1).GetAllAsync(Arg.Is<QueryParameters<string>>(p => p.Filter == project));
        }

        [Fact]
        public async Task GetAll_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var req        = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(req).Returns(queryParams);
            _service.GetAllAsync(Arg.Any<QueryParameters<string>>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll("PP001", req));
            _mapper.DidNotReceive().Map<PaginationRes<YearlyFinancialDataRes>>(
                Arg.Any<PaginatedResult<YearlyFinancialDataDto>>());
        }

        #endregion

        #region GetByKey Tests

        [Fact]
        public async Task GetByKey_WithValidCompositeKey_ReturnsOkWithMappedDto()
        {
            // Arrange
            const int year    = 2024;
            const string project = "PP001";
            var dto  = SampleDto();
            var res  = SampleRes();

            _service.GetByKeyAsync((short)year, project).Returns(dto);
            _mapper.Map<YearlyFinancialDataRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByKey(year, project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _service.Received(1).GetByKeyAsync((short)year, project);
            _mapper.Received(1).Map<YearlyFinancialDataRes>(dto);
        }

        [Fact]
        public async Task GetByKey_WhenNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            _service.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Returns((YearlyFinancialDataDto?)null);

            // Act
            var result = await _controller.GetByKey(9999, "UNKNOWN");

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mapper.DidNotReceive().Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public async Task GetByKey_WhenNotFound_DoesNotCallMapper()
        {
            // Arrange
            _service.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Returns((YearlyFinancialDataDto?)null);

            // Act
            await _controller.GetByKey(9999, "UNKNOWN");

            // Assert
            _mapper.DidNotReceive().Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public async Task GetByKey_CastsIntYearToShort_BeforeCallingService()
        {
            // Arrange
            const int year = 2023;
            _service.GetByKeyAsync((short)year, "PP001").Returns(SampleDto((short)year));
            _mapper.Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>()).Returns(SampleRes((short)year));

            // Act
            await _controller.GetByKey(year, "PP001");

            // Assert
            await _service.Received(1).GetByKeyAsync((short)year, "PP001");
        }

        [Fact]
        public async Task GetByKey_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetByKey(2024, "PP001"));
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidRequest_ReturnsCreatedAtActionWithMappedDto()
        {
            // Arrange
            var request    = new YearlyFinancialDataReq { Year = 2024, Project = "PP001" };
            var dto        = SampleDto();
            var createdDto = SampleDto();
            createdDto.Year    = 2024;
            createdDto.Project = "PP001";
            var createdRes = SampleRes();

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.CreateAsync(dto).Returns(createdDto);
            _mapper.Map<YearlyFinancialDataRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetByKey), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(createdDto.Year,    createdResult.RouteValues["year"]);
            Assert.Equal(createdDto.Project, createdResult.RouteValues["project"]);
            Assert.Equal(createdRes, createdResult.Value);
            _mapper.Received(1).Map<YearlyFinancialDataDto>(request);
            await _service.Received(1).CreateAsync(dto);
            _mapper.Received(1).Map<YearlyFinancialDataRes>(createdDto);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithStatusCode201()
        {
            // Arrange
            var request    = new YearlyFinancialDataReq { Year = 2024, Project = "PP001" };
            var dto        = SampleDto();
            var createdDto = SampleDto();
            var createdRes = SampleRes();

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.CreateAsync(dto).Returns(createdDto);
            _mapper.Map<YearlyFinancialDataRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearlyFinancialDataReq { Year = 2024, Project = "PP001" };
            var dto     = SampleDto();

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.CreateAsync(dto).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(request));
            _mapper.Received(1).Map<YearlyFinancialDataDto>(request);
            await _service.Received(1).CreateAsync(dto);
            _mapper.DidNotReceive().Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>());
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidCompositeKey_ReturnsOkWithMappedDto()
        {
            // Arrange
            const int year    = 2024;
            const string project = "PP001";
            var request   = new YearlyFinancialDataReq { Year = (short)year, Project = project };
            var dto       = SampleDto();
            var updatedDto = SampleDto();
            var updatedRes = SampleRes();

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Is<YearlyFinancialDataDto>(d => d.Year == year && d.Project == project))
                    .Returns(updatedDto);
            _mapper.Map<YearlyFinancialDataRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.Update(year, project, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);
            _mapper.Received(1).Map<YearlyFinancialDataDto>(request);
            _mapper.Received(1).Map<YearlyFinancialDataRes>(updatedDto);
        }

        [Fact]
        public async Task Update_OverridesYearAndProjectFromRoute_BeforeCallingService()
        {
            // Arrange
            const int year    = 2025;
            const string project = "PP002";
            var request    = new YearlyFinancialDataReq { Year = 9999, Project = "BODY" };
            var dto        = new YearlyFinancialDataDto { Year = 9999, Project = "BODY" };
            var updatedDto = SampleDto((short)year, project);
            var updatedRes = SampleRes((short)year, project);

            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<YearlyFinancialDataDto>()).Returns(updatedDto);
            _mapper.Map<YearlyFinancialDataRes>(updatedDto).Returns(updatedRes);

            // Act
            await _controller.Update(year, project, request);

            // Assert — controller must override Year and Project from the route
            await _service.Received(1).UpdateAsync(
                Arg.Is<YearlyFinancialDataDto>(d => d.Year == (short)year && d.Project == project));
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearlyFinancialDataReq { Year = 2024, Project = "PP001" };
            var dto     = SampleDto();
            _mapper.Map<YearlyFinancialDataDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<YearlyFinancialDataDto>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(2024, "PP001", request));
            _mapper.DidNotReceive().Map<YearlyFinancialDataRes>(Arg.Any<YearlyFinancialDataDto>());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WhenSuccessful_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync((short)2024, "PP001").Returns(true);

            // Act
            var result = await _controller.Delete(2024, "PP001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = okResult.Value!;
            Assert.NotNull(value);
            var successProp = value.GetType().GetProperty("success");
            Assert.NotNull(successProp);
            Assert.True((bool)successProp.GetValue(value)!);
            await _service.Received(1).DeleteAsync((short)2024, "PP001");
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsOkWithSuccessFalse()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<short>(), Arg.Any<string>()).Returns(false);

            // Act
            var result = await _controller.Delete(9999, "UNKNOWN");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = okResult.Value!;
            var successProp = value.GetType().GetProperty("success");
            Assert.NotNull(successProp);
            Assert.False((bool)successProp.GetValue(value)!);
            await _service.Received(1).DeleteAsync(Arg.Any<short>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<short>(), Arg.Any<string>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(2024, "PP001"));
        }

        #endregion

        #region GetPactCosts Tests

        [Fact]
        public async Task GetPactCosts_WithValidProjectAndYear_ReturnsOkWithMappedList()
        {
            // Arrange
            const string project = "PP001";
            const int year       = 2024;
            var dtoList          = new List<PactProjectYearCostsDto> { SamplePactDto() }.AsReadOnly();
            var resList          = new List<PactProjectYearCostsRes> { SamplePactRes() }.AsReadOnly();

            _service.GetPactCostsAsync(project, (short)year).Returns(dtoList);
            _mapper.Map<IReadOnlyList<PactProjectYearCostsRes>>(Arg.Any<IReadOnlyList<PactProjectYearCostsDto>>()).Returns(resList);

            // Act
            var result = await _controller.GetPactCosts(project, year);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resList, okResult.Value);
            await _service.Received(1).GetPactCostsAsync(project, (short)year);
            _mapper.Received(1).Map<IReadOnlyList<PactProjectYearCostsRes>>(Arg.Any<IReadOnlyList<PactProjectYearCostsDto>>());
        }

        [Fact]
        public async Task GetPactCosts_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<PactProjectYearCostsDto>().AsReadOnly();
            var emptyRes  = new List<PactProjectYearCostsRes>().AsReadOnly();

            _service.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>()).Returns(emptyList);
            _mapper.Map<IReadOnlyList<PactProjectYearCostsRes>>(Arg.Any<IReadOnlyList<PactProjectYearCostsDto>>()).Returns(emptyRes);

            // Act
            var result = await _controller.GetPactCosts("PP001", 2024);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetPactCosts_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>())
                    .Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPactCosts("PP001", 2024));
        }

        #endregion

        #region GetSettingValueById Tests

        [Fact]
        public async Task GetSettingValueById_WhenSettingExists_ReturnsOkWithValue()
        {
            // Arrange
            _service.GetSettingValueByIdAsync("HoursInDay").Returns("7.2");

            // Act
            var result = await _controller.GetSettingValueById("HoursInDay");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("7.2", okResult.Value);
            await _service.Received(1).GetSettingValueByIdAsync("HoursInDay");
        }

        [Fact]
        public async Task GetSettingValueById_WhenSettingMissing_ReturnsOkWithEmptyString()
        {
            // Arrange
            _service.GetSettingValueByIdAsync("UnknownSetting").Returns((string?)null);

            // Act
            var result = await _controller.GetSettingValueById("UnknownSetting");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(string.Empty, okResult.Value);
            await _service.Received(1).GetSettingValueByIdAsync("UnknownSetting");
        }

        [Fact]
        public async Task GetSettingValueById_WhenIdIsNull_CallsServiceWithEmptyString()
        {
            // Arrange
            _service.GetSettingValueByIdAsync(string.Empty).Returns(string.Empty);

            // Act
            var result = await _controller.GetSettingValueById(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(string.Empty, okResult.Value);
            await _service.Received(1).GetSettingValueByIdAsync(string.Empty);
        }

        [Fact]
        public async Task GetSettingValueById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetSettingValueByIdAsync(Arg.Any<string>()).Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetSettingValueById("HoursInDay"));
        }

        #endregion
    }
}
