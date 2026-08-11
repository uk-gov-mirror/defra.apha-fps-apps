using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.RadTrackInvoiceControllerTest
{
    public class RadTrackInvoiceControllerTests
    {
        private readonly IRadTrackInvoiceService _service;
        private readonly IMapper                 _mapper;
        private readonly RadTrackInvoiceController _controller;

        public RadTrackInvoiceControllerTests()
        {
            _service    = Substitute.For<IRadTrackInvoiceService>();
            _mapper     = Substitute.For<IMapper>();
            _controller = new RadTrackInvoiceController(_service, _mapper);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new RadTrackInvoiceController(_service, _mapper);
            Assert.NotNull(controller);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithMappedPaginatedResult()
        {
            // Arrange
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var dtos = new List<RadTrackInvoiceDto>
            {
                new() { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" },
                new() { InvoiceCounter = 2, Project = "PP001", InvoiceRef = "INV-002" }
            };
            var paginatedResult = new PaginatedResult<RadTrackInvoiceDto>(dtos, new PaginationDto { TotalRecords = 2 });
            var resList = new List<RadTrackInvoiceRes>
            {
                new() { InvoiceCounter = 1, Project = "PP001" },
                new() { InvoiceCounter = 2, Project = "PP001" }
            };
            var paginationRes = new PaginationRes<RadTrackInvoiceRes>(resList, new Pagination { TotalRecords = 2 });

            _service.GetAllAsync(parameters).Returns(paginatedResult);
            _mapper.Map<PaginationRes<RadTrackInvoiceRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetAll(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationRes, okResult.Value);
            await _service.Received(1).GetAllAsync(parameters);
            _mapper.Received(1).Map<PaginationRes<RadTrackInvoiceRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetAll_WithEmptyResult_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            var parameters      = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var emptyResult     = new PaginatedResult<RadTrackInvoiceDto>([], new PaginationDto { TotalRecords = 0 });
            var emptyPageRes    = new PaginationRes<RadTrackInvoiceRes>();

            _service.GetAllAsync(parameters).Returns(emptyResult);
            _mapper.Map<PaginationRes<RadTrackInvoiceRes>>(emptyResult).Returns(emptyPageRes);

            // Act
            var result = await _controller.GetAll(parameters);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyPageRes, okResult.Value);
            await _service.Received(1).GetAllAsync(parameters);
        }

        [Fact]
        public async Task GetAll_WithFilterParameters_PassesParametersToService()
        {
            // Arrange
            var filter     = new RadTrackInvoiceFilter { Project = "PP001", Year = 2024 };
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10, Filter = filter };
            var paginated  = new PaginatedResult<RadTrackInvoiceDto>([], new PaginationDto());
            var pageRes    = new PaginationRes<RadTrackInvoiceRes>();

            _service.GetAllAsync(parameters).Returns(paginated);
            _mapper.Map<PaginationRes<RadTrackInvoiceRes>>(paginated).Returns(pageRes);

            // Act
            await _controller.GetAll(parameters);

            // Assert
            await _service.Received(1).GetAllAsync(parameters);
        }

        [Fact]
        public async Task GetAll_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var parameters = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            _service.GetAllAsync(parameters).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll(parameters));
            await _service.Received(1).GetAllAsync(parameters);
            _mapper.DidNotReceive().Map<PaginationRes<RadTrackInvoiceRes>>(Arg.Any<PaginatedResult<RadTrackInvoiceDto>>());
        }

        #endregion

        #region GetTotals Tests

        [Fact]
        public async Task GetTotals_WithFilter_ReturnsOkWithTotalsDto()
        {
            // Arrange
            var filter  = new RadTrackInvoiceFilter { Project = "PP001" };
            var totals  = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 10000, TotalDueAmount = 8000, TotalActualAmount = 7500 };

            _service.GetTotalsAsync(filter).Returns(totals);

            // Act
            var result = await _controller.GetTotals(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(totals, okResult.Value);
            await _service.Received(1).GetTotalsAsync(filter);
        }

        [Fact]
        public async Task GetTotals_WithNullFilter_ReturnsOkWithTotalsDto()
        {
            // Arrange
            var totals = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 5000, TotalDueAmount = 4000, TotalActualAmount = 3000 };
            _service.GetTotalsAsync(null).Returns(totals);

            // Act
            var result = await _controller.GetTotals(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(totals, okResult.Value);
            await _service.Received(1).GetTotalsAsync(null);
        }

        [Fact]
        public async Task GetTotals_ReturnsTotalsDirectlyWithoutMapping()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Year = 2024 };
            var totals = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 1000 };
            _service.GetTotalsAsync(filter).Returns(totals);

            // Act
            await _controller.GetTotals(filter);

            // Assert
            _mapper.DidNotReceive().Map<RadTrackInvoiceTotalsDto>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetTotals_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };
            _service.GetTotalsAsync(filter).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetTotals(filter));
            await _service.Received(1).GetTotalsAsync(filter);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult_WithMappedInvoice()
        {
            // Arrange
            const int id  = 1;
            var dto = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };
            var res = new RadTrackInvoiceRes  { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };

            _service.GetByIdAsync(id).Returns(dto);
            _mapper.Map<RadTrackInvoiceRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _service.Received(1).GetByIdAsync(id);
            _mapper.Received(1).Map<RadTrackInvoiceRes>(dto);
        }

        [Fact]
        public async Task GetById_WhenNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            const int id = 99;
            _service.GetByIdAsync(id).Returns((RadTrackInvoiceDto?)null);

            // Act
            var result = await _controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            await _service.Received(1).GetByIdAsync(id);
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task GetById_WhenNotFound_DoesNotCallMapper()
        {
            // Arrange
            const int id = 99;
            _service.GetByIdAsync(id).Returns((RadTrackInvoiceDto?)null);

            // Act
            await _controller.GetById(id);

            // Assert
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task GetById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _service.GetByIdAsync(id).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(id));
            await _service.Received(1).GetByIdAsync(id);
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithMappedInvoice()
        {
            // Arrange
            var request    = new RadTrackInvoiceReq { Project = "PP001", InvoiceRef = "INV-001", PlannedAmount = 5000 };
            var dto        = new RadTrackInvoiceDto  { Project = "PP001", InvoiceRef = "INV-001", PlannedAmount = 5000 };
            var createdDto = new RadTrackInvoiceDto  { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" };
            var createdRes = new RadTrackInvoiceRes  { InvoiceCounter = 1, Project = "PP001", InvoiceRef = "INV-001" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Returns(createdDto);
            _mapper.Map<RadTrackInvoiceRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(1, createdResult.RouteValues["id"]);
            Assert.Equal(createdRes, createdResult.Value);
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            await _service.Received(1).CreateAsync(dto);
            _mapper.Received(1).Map<RadTrackInvoiceRes>(createdDto);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithCorrectStatusCode()
        {
            // Arrange
            var request    = new RadTrackInvoiceReq { Project = "PP001" };
            var dto        = new RadTrackInvoiceDto  { Project = "PP001" };
            var createdDto = new RadTrackInvoiceDto  { InvoiceCounter = 5, Project = "PP001" };
            var createdRes = new RadTrackInvoiceRes  { InvoiceCounter = 5, Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Returns(createdDto);
            _mapper.Map<RadTrackInvoiceRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            Assert.Equal(5, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new RadTrackInvoiceReq { Project = "PP001" };
            var dto     = new RadTrackInvoiceDto  { Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.CreateAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(request));
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            await _service.Received(1).CreateAsync(dto);
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task Create_WhenMapperThrowsOnRequestMapping_PropagatesException()
        {
            // Arrange
            var request = new RadTrackInvoiceReq { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(request).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.Create(request));
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            await _service.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoiceDto>());
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidRequest_ReturnsOkResult_WithMappedInvoice()
        {
            // Arrange
            const int id    = 1;
            var request     = new RadTrackInvoiceReq { Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var dto         = new RadTrackInvoiceDto  { Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var updatedDto  = new RadTrackInvoiceDto  { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-UPDATED" };
            var updatedRes  = new RadTrackInvoiceRes  { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-UPDATED" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Is<RadTrackInvoiceDto>(d => d.InvoiceCounter == id)).Returns(updatedDto);
            _mapper.Map<RadTrackInvoiceRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.Update(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            _mapper.Received(1).Map<RadTrackInvoiceRes>(updatedDto);
        }

        [Fact]
        public async Task Update_SetsInvoiceCounterOnDto_BeforeCallingService()
        {
            // Arrange
            const int id   = 7;
            var request    = new RadTrackInvoiceReq { Project = "PP001" };
            var dto        = new RadTrackInvoiceDto  { Project = "PP001" };
            var updatedDto = new RadTrackInvoiceDto  { InvoiceCounter = id, Project = "PP001" };
            var updatedRes = new RadTrackInvoiceRes  { InvoiceCounter = id };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<RadTrackInvoiceDto>()).Returns(updatedDto);
            _mapper.Map<RadTrackInvoiceRes>(updatedDto).Returns(updatedRes);

            // Act
            await _controller.Update(id, request);

            // Assert
            await _service.Received(1).UpdateAsync(
                Arg.Is<RadTrackInvoiceDto>(d => d.InvoiceCounter == id));
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            var request  = new RadTrackInvoiceReq { Project = "PP001" };
            var dto      = new RadTrackInvoiceDto  { Project = "PP001" };

            _mapper.Map<RadTrackInvoiceDto>(request).Returns(dto);
            _service.UpdateAsync(Arg.Any<RadTrackInvoiceDto>()).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(id, request));
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            await _service.Received(1).UpdateAsync(Arg.Any<RadTrackInvoiceDto>());
            _mapper.DidNotReceive().Map<RadTrackInvoiceRes>(Arg.Any<RadTrackInvoiceDto>());
        }

        [Fact]
        public async Task Update_WhenMapperThrowsOnRequestMapping_PropagatesException()
        {
            // Arrange
            const int id = 1;
            var request  = new RadTrackInvoiceReq { Project = "PP001" };
            _mapper.Map<RadTrackInvoiceDto>(request).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.Update(id, request));
            _mapper.Received(1).Map<RadTrackInvoiceDto>(request);
            await _service.DidNotReceive().UpdateAsync(Arg.Any<RadTrackInvoiceDto>());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WhenSuccessful_ReturnsOkWithSuccessTrue()
        {
            // Arrange
            const int id = 1;
            _service.DeleteAsync(id).Returns(true);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.True(Assert.IsType<bool>(okResult.Value));
            await _service.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task Delete_WhenNotFound_ReturnsOkWithSuccessFalse()
        {
            // Arrange
            const int id = 99;
            _service.DeleteAsync(id).Returns(false);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            Assert.False(Assert.IsType<bool>(okResult.Value));
            await _service.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _service.DeleteAsync(id).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(id));
            await _service.Received(1).DeleteAsync(id);
        }

        #endregion

        #region GetProjects Tests

        [Fact]
        public async Task GetProjects_ReturnsOkResult_WithProjectsList()
        {
            // Arrange
            var projects = new List<string> { "PP001", "PP002", "PP003" };
            _service.GetProjectsAsync().Returns(projects);

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(projects, okResult.Value);
            await _service.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjects_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetProjectsAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetProjects();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);
            await _service.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjects_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProjectsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetProjects());
            await _service.Received(1).GetProjectsAsync();
        }

        #endregion

        #region GetYears Tests

        [Fact]
        public async Task GetYears_ReturnsOkResult_WithYearsList()
        {
            // Arrange
            var years = new List<int> { 2022, 2023, 2024 };
            _service.GetYearsAsync().Returns(years);

            // Act
            var result = await _controller.GetYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(years, okResult.Value);
            await _service.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYears_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetYearsAsync().Returns(new List<int>());

            // Act
            var result = await _controller.GetYears();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<int>>(okResult.Value);
            Assert.Empty(value);
            await _service.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYears_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetYearsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetYears());
            await _service.Received(1).GetYearsAsync();
        }

        #endregion

        #region GetContracts Tests

        [Fact]
        public async Task GetContracts_ReturnsOkResult_WithContractsList()
        {
            // Arrange
            var contracts = new List<string> { "C001", "C002", "C003" };
            _service.GetContractsAsync().Returns(contracts);

            // Act
            var result = await _controller.GetContracts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(contracts, okResult.Value);
            await _service.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContracts_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetContractsAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetContracts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);
            await _service.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContracts_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetContractsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetContracts());
            await _service.Received(1).GetContractsAsync();
        }

        #endregion

        #region GetPrograms Tests

        [Fact]
        public async Task GetPrograms_ReturnsOkResult_WithProgramsList()
        {
            // Arrange
            var programs = new List<string> { "PROG1", "PROG2", "PROG3" };
            _service.GetProgramsAsync().Returns(programs);

            // Act
            var result = await _controller.GetPrograms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(programs, okResult.Value);
            await _service.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetPrograms_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            _service.GetProgramsAsync().Returns(new List<string>());

            // Act
            var result = await _controller.GetPrograms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<string>>(okResult.Value);
            Assert.Empty(value);
            await _service.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetPrograms_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.GetProgramsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPrograms());
            await _service.Received(1).GetProgramsAsync();
        }

        #endregion
    }
}
