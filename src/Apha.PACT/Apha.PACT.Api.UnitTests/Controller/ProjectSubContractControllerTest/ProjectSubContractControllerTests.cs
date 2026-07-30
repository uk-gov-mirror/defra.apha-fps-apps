using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.ProjectSubContractControllerTest
{
    public class ProjectSubContractControllerTests
    {
        private readonly IProjectSubContractService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ICurrentUserContext _currentUserContextMock;
        private readonly ProjectSubContractController _controller;

        public ProjectSubContractControllerTests()
        {
            _serviceMock = Substitute.For<IProjectSubContractService>();
            _mapperMock = Substitute.For<IMapper>();
            _currentUserContextMock = Substitute.For<ICurrentUserContext>();
            _controller = new ProjectSubContractController(_serviceMock, _mapperMock, _currentUserContextMock);
        }

        #region GetPaged

        [Fact]
        public async Task GetPaged_ValidQueryWithProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectSubContractDto> { new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectSubContractRes>
            {
                Data = new List<ProjectSubContractRes> { new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetPagedProjectSubContractsAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetPaged_NullProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(Enumerable.Empty<ProjectSubContractDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectSubContractRes>();

            _serviceMock.GetPagedProjectSubContractsAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetPaged(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetTotal

        [Fact]
        public async Task GetTotal_ValidProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync("PRJ1").Returns(2500.00m);

            var result = await _controller.GetTotal("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(2500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetTotal_NullProject_ReturnsOk()
        {
            _serviceMock.GetTotalAmountAsync(null).Returns(0m);

            var result = await _controller.GetTotal(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_ExistingId_ReturnsOk()
        {
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" };

            _serviceMock.GetByIdAsync(1).Returns(dto);
            _mapperMock.Map<ProjectSubContractRes>(dto).Returns(mapped);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetById_NullResult_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetByIdAsync(99).Returns((ProjectSubContractDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetById(99));
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreatedAtAction()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1", Amount = 500m };
            var dto = new ProjectSubContractDto { Project = "PRJ1", Amount = 500m };
            var createdDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 500m };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1", Amount = 500m };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(createdDto);
            _mapperMock.Map<ProjectSubContractRes>(createdDto).Returns(mapped);

            var result = await _controller.Create(req);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(mapped, createdResult.Value);
            Assert.Equal(1, createdResult.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_ServiceThrows_PropagatesException()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1" };
            var dto = new ProjectSubContractDto { Project = "PRJ1" };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Create(req));
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ValidRequest_ReturnsOk()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1", Amount = 750m };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 750m };
            var updatedDto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", Amount = 750m };
            var mapped = new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1", Amount = 750m };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).Returns(updatedDto);
            _mapperMock.Map<ProjectSubContractRes>(updatedDto).Returns(mapped);

            var result = await _controller.Update(1, req);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task Update_ServiceThrows_PropagatesException()
        {
            var req = new ProjectSubContractReq { Project = "PRJ1" };
            var dto = new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1" };

            _mapperMock.Map<ProjectSubContractDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(dto).ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(1, req));
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            _serviceMock.DeleteAsync(1).Returns(true);

            var result = await _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task Delete_RecordNotFound_ReturnsOkWithFalse()
        {
            _serviceMock.DeleteAsync(99).Returns(false);

            var result = await _controller.Delete(99);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        #endregion

        #region GetFpsProjectSubContracts

        [Fact]
        public async Task GetFpsProjectSubContracts_ValidQueryWithProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectSubContractDto> { new ProjectSubContractDto { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals" } };
            var paginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(dtos, paginationData);
            var expectedResponse = new PaginationRes<ProjectSubContractRes>
            {
                Data = new List<ProjectSubContractRes> { new ProjectSubContractRes { SubContCounter = 1, Project = "PRJ1" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetFpsProjectSubContractsAsync(query, "PRJ1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetFpsProjectSubContracts(query, "PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetFpsProjectSubContracts_NullProject_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectSubContractDto>(Enumerable.Empty<ProjectSubContractDto>(), new PaginationDto());
            var expectedResponse = new PaginationRes<ProjectSubContractRes>();

            _serviceMock.GetFpsProjectSubContractsAsync(query, null).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectSubContractRes>>(serviceResult).Returns(expectedResponse);

            var result = await _controller.GetFpsProjectSubContracts(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        #endregion

        #region GetFpsProjectTotal

        [Fact]
        public async Task GetFpsProjectTotal_ValidProject_ReturnsOk()
        {
            _serviceMock.GetFpsProjectSubContractTotalAmountAsync("PRJ1").Returns(1500.00m);

            var result = await _controller.GetFpsProjectTotal("PRJ1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(1500.00m, okResult.Value);
        }

        [Fact]
        public async Task GetFpsProjectTotal_NullProject_ReturnsOk()
        {
            _serviceMock.GetFpsProjectSubContractTotalAmountAsync(null).Returns(0m);

            var result = await _controller.GetFpsProjectTotal(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(0m, okResult.Value);
        }

        #endregion

        #region GetMonthlySubContractsSummary

        [Fact]
        public async Task GetMonthlySubContractsSummary_ValidQuery_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new MonthlySubContractsPivotDto
            {
                Months = [1, 2, 3],
                Rows =
                [
                    new MonthlySubContractsSummaryDto
                    {
                        Program = "ADMIN",
                        ParentProject = "AH",
                        MonthlyAmounts = new Dictionary<int, decimal> { { 1, 100m }, { 2, 200m } }
                    }
                ],
                Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };
            var expectedRes = new MonthlySubContractsPivotRes
            {
                Months = [1, 2, 3],
                Rows = [new MonthlySubContractsSummaryItemRes { Program = "ADMIN", ParentProject = "AH" }],
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetMonthlySubContractsSummaryAsync(query).Returns(dto);
            _mapperMock.Map<MonthlySubContractsPivotRes>(dto).Returns(expectedRes);

            var result = await _controller.GetMonthlySubContractsSummary(query);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedRes, okResult.Value);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummary_EmptyData_ReturnsOkWithEmptyPivot()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new MonthlySubContractsPivotDto();
            var expectedRes = new MonthlySubContractsPivotRes();

            _serviceMock.GetMonthlySubContractsSummaryAsync(query).Returns(dto);
            _mapperMock.Map<MonthlySubContractsPivotRes>(dto).Returns(expectedRes);

            var result = await _controller.GetMonthlySubContractsSummary(query);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedRes, okResult.Value);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummary_CallsServiceOnce()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new MonthlySubContractsPivotDto();

            _serviceMock.GetMonthlySubContractsSummaryAsync(query).Returns(dto);
            _mapperMock.Map<MonthlySubContractsPivotRes>(dto).Returns(new MonthlySubContractsPivotRes());

            await _controller.GetMonthlySubContractsSummary(query);

            await _serviceMock.Received(1).GetMonthlySubContractsSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummary_MapsServiceResultToRes()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dto = new MonthlySubContractsPivotDto();

            _serviceMock.GetMonthlySubContractsSummaryAsync(query).Returns(dto);
            _mapperMock.Map<MonthlySubContractsPivotRes>(dto).Returns(new MonthlySubContractsPivotRes());

            await _controller.GetMonthlySubContractsSummary(query);

            _mapperMock.Received(1).Map<MonthlySubContractsPivotRes>(dto);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummary_ServiceThrows_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetMonthlySubContractsSummaryAsync(query)
                .ThrowsAsync(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetMonthlySubContractsSummary(query));
        }

        #endregion

        #region FailedSubContractRms

        [Fact]
        public async Task GetFailedSubContractRms_ValidQuery_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _currentUserContextMock.UserId.Returns("user1");

            var dtos = new List<SubContractRmsImportRowDto>
            {
                new() { Id = 1, Project = "PRJ1" }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<SubContractRmsImportRowDto>(dtos, pagination);
            var mapped = new PaginationRes<SubContractRmsImportRowRes>
            {
                Data = [new SubContractRmsImportRowRes { Id = 1, Project = "PRJ1" }],
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetFailedSubContractRmsAsync(query, "user1").Returns(serviceResult);
            _mapperMock.Map<PaginationRes<SubContractRmsImportRowRes>>(serviceResult).Returns(mapped);

            // Act
            var result = await _controller.GetFailedSubContractRms(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetFailedSubContractRmsById_RecordExists_ReturnsOk()
        {
            // Arrange
            const int id = 5;
            _currentUserContextMock.UserId.Returns("user2");
            var dto = new SubContractRmsImportRowDto { Id = id, Project = "PRJ5" };
            var res = new SubContractRmsImportRowRes { Id = id, Project = "PRJ5" };

            _serviceMock.GetFailedSubContractRmsByIdAsync(id, "user2").Returns(dto);
            _mapperMock.Map<SubContractRmsImportRowRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetFailedSubContractRmsById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task GetFailedSubContractRmsById_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _currentUserContextMock.UserId.Returns("user3");
            _serviceMock.GetFailedSubContractRmsByIdAsync(99, "user3").Returns((SubContractRmsImportRowDto?)null);

            // Act / Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetFailedSubContractRmsById(99));
        }

        [Fact]
        public async Task SaveFailedSubContractRms_ValidRequest_ReturnsOk()
        {
            // Arrange
            const int id = 3;
            _currentUserContextMock.UserId.Returns("user4");
            var request = new SubContractRmsImportRowReq { Project = "PRJ3" };
            var dto = new SubContractRmsImportRowDto { Project = "PRJ3" };

            _mapperMock.Map<SubContractRmsImportRowDto>(request).Returns(dto);
            _serviceMock.SaveFailedSubContractRmsAsync(id, dto, "user4").Returns(true);

            // Act
            var result = await _controller.SaveFailedSubContractRms(id, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsById_ServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            const int id = 11;
            _currentUserContextMock.UserId.Returns("user5");
            _serviceMock.DeleteFailedSubContractRmsByIdAsync(id, "user5").Returns(false);

            // Act
            var result = await _controller.DeleteFailedSubContractRmsById(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByUser_DeletedCountGreaterThanZero_ReturnsOkWithTrue()
        {
            // Arrange
            _currentUserContextMock.UserId.Returns("user6");
            _serviceMock.DeleteFailedSubContractRmsByUserAsync("user6").Returns(2);

            // Act
            var result = await _controller.DeleteFailedSubContractRmsByUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByUser_DeletedCountIsZero_ReturnsOkWithFalse()
        {
            // Arrange
            _currentUserContextMock.UserId.Returns("user7");
            _serviceMock.DeleteFailedSubContractRmsByUserAsync("user7").Returns(0);

            // Act
            var result = await _controller.DeleteFailedSubContractRmsByUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
        }

        [Fact]
        public async Task ImportSubContractRms_ValidRequest_ReturnsOk()
        {
            // Arrange
            _currentUserContextMock.UserId.Returns("user8");
            var request = new SubContractRmsImportReq
            {
                FileName = "rms.xlsx",
                Rows = [new SubContractRmsImportRowReq { Project = "PRJ9" }]
            };

            var dto = new SubContractRmsImportDto
            {
                FileName = "rms.xlsx",
                Rows = [new SubContractRmsImportRowDto { Project = "PRJ9" }]
            };

            var resultDto = new SubContractRmsImportResultDto { PassedCount = 1, FailedCount = 0, Message = "Imported" };
            var mappedResponse = new SubContractRmsImportRes { PassedCount = 1, FailedCount = 0, Message = "Imported" };

            _mapperMock.Map<SubContractRmsImportDto>(request).Returns(dto);
            _serviceMock.ImportSubContractRmsAsync(dto, "user8").Returns(resultDto);
            _mapperMock.Map<SubContractRmsImportRes>(resultDto).Returns(mappedResponse);

            // Act
            var result = await _controller.ImportSubContractRms(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResponse, okResult.Value);
        }

        #endregion
    }
}
