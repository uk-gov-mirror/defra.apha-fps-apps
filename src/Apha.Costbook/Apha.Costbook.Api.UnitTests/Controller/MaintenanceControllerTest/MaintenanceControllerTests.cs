using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Api.Controllers;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Api.UnitTests.Controllers.MaintenanceControllerTest
{
    public class MaintenanceControllerTests
    {
        private readonly IMaintenanceSettingsService _settingsService;
        private readonly IAccountCategoryMaintenanceService _accountCategoryService;
        private readonly IMapper _mapper;
        private readonly MaintenanceController _controller;

        public MaintenanceControllerTests()
        {
            _settingsService = Substitute.For<IMaintenanceSettingsService>();
            _accountCategoryService = Substitute.For<IAccountCategoryMaintenanceService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new MaintenanceController(_settingsService, _accountCategoryService, _mapper);
        }

        // ── GetSettings ───────────────────────────────────────────────────────

        #region GetSettings Tests

        [Fact]
        public async Task GetSettings_ServiceReturnsDto_ReturnsOkWithMappedRes()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 2.5m, ProfitAnimals = 15m, CurrentFinancialYear = 2024 };
            var res = new MaintenanceSettingsRes();
            _settingsService.GetSettingsAsync().Returns(dto);
            _mapper.Map<MaintenanceSettingsRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetSettings();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _settingsService.Received(1).GetSettingsAsync();
            _mapper.Received(1).Map<MaintenanceSettingsRes>(dto);
        }

        [Fact]
        public async Task GetSettings_ServiceThrows_PropagatesException()
        {
            // Arrange
            _settingsService.GetSettingsAsync().Throws(new InvalidOperationException("Required setting not found."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetSettings());
        }

        #endregion

        // ── UpdateSettings ────────────────────────────────────────────────────

        #region UpdateSettings Tests

        [Fact]
        public async Task UpdateSettings_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var req = new MaintenanceSettingsReq();
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            var updatedDto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            var res = new MaintenanceSettingsRes();
            _mapper.Map<MaintenanceSettingsDto>(req).Returns(dto);
            _settingsService.UpdateSettingsAsync(dto).Returns(Task.CompletedTask);
            _settingsService.GetSettingsAsync().Returns(updatedDto);
            _mapper.Map<MaintenanceSettingsRes>(updatedDto).Returns(res);

            // Act
            var result = await _controller.UpdateSettings(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _settingsService.Received(1).UpdateSettingsAsync(dto);
            await _settingsService.Received(1).GetSettingsAsync();
        }

        [Fact]
        public async Task UpdateSettings_ServiceUpdateThrows_PropagatesException()
        {
            // Arrange
            var req = new MaintenanceSettingsReq();
            var dto = new MaintenanceSettingsDto();
            _mapper.Map<MaintenanceSettingsDto>(req).Returns(dto);
            _settingsService.UpdateSettingsAsync(dto).Throws(new InvalidOperationException("Update failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateSettings(req));
        }

        #endregion

        // ── GetAccountCategories ──────────────────────────────────────────────

        #region GetAccountCategories Tests

        [Fact]
        public async Task GetAccountCategories_ServiceReturnsList_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<AccountCategoryMaintenanceDto>
            {
                new AccountCategoryMaintenanceDto { AccShortName = "ACC01", Csg7Group = "CSG001" },
                new AccountCategoryMaintenanceDto { AccShortName = "ACC02", Csg7Group = "CSG002" }
            };
            var resList = new List<AccountCategoryMaintenanceRes>();
            _accountCategoryService.GetAllForMaintenanceAsync().Returns(dtos);
            _mapper.Map<List<AccountCategoryMaintenanceRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAccountCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
            await _accountCategoryService.Received(1).GetAllForMaintenanceAsync();
        }

        [Fact]
        public async Task GetAccountCategories_ServiceReturnsEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<AccountCategoryMaintenanceDto>();
            var resList = new List<AccountCategoryMaintenanceRes>();
            _accountCategoryService.GetAllForMaintenanceAsync().Returns(dtos);
            _mapper.Map<List<AccountCategoryMaintenanceRes>>(dtos).Returns(resList);

            // Act
            var result = await _controller.GetAccountCategories();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(resList, okResult.Value);
        }

        #endregion

        // ── GetAccountCategoriesPaginated ─────────────────────────────────────

        #region GetAccountCategoriesPaginated Tests

        [Fact]
        public async Task GetAccountCategoriesPaginated_ValidQuery_ReturnsOkWithPaginatedResult()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PaginatedResult<AccountCategoryMaintenanceDto>(
                new List<AccountCategoryMaintenanceDto>
                {
                    new AccountCategoryMaintenanceDto { AccShortName = "ACC01", Csg7Group = "CSG001" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });
            var pagedRes = new PaginationRes<AccountCategoryMaintenanceRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(queryParams);
            _accountCategoryService.GetPaginatedAsync(queryParams).Returns(pagedData);
            _mapper.Map<PaginationRes<AccountCategoryMaintenanceRes>>(pagedData).Returns(pagedRes);

            // Act
            var result = await _controller.GetAccountCategoriesPaginated(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(pagedRes, okResult.Value);
            await _accountCategoryService.Received(1).GetPaginatedAsync(queryParams);
        }

        #endregion

        // ── UpdateAccountCategory ─────────────────────────────────────────────

        #region UpdateAccountCategory Tests

        [Fact]
        public async Task UpdateAccountCategory_ValidRequest_ReturnsOkWithUpdatedRes()
        {
            // Arrange
            var accShortName = "ACC01";
            var req = new AccountCategoryMaintenanceReq { Csg7Group = "CSG001" };
            var updatedDto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var res = new AccountCategoryMaintenanceRes();
            _accountCategoryService.UpdateCsg7GroupAsync(accShortName, req.Csg7Group).Returns(updatedDto);
            _mapper.Map<AccountCategoryMaintenanceRes>(updatedDto).Returns(res);

            // Act
            var result = await _controller.UpdateAccountCategory(accShortName, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
            await _accountCategoryService.Received(1).UpdateCsg7GroupAsync(accShortName, req.Csg7Group);
        }

        [Fact]
        public async Task UpdateAccountCategory_NullCsg7Group_ReturnsOkWithClearedRes()
        {
            // Arrange
            var accShortName = "ACC01";
            var req = new AccountCategoryMaintenanceReq { Csg7Group = null };
            var updatedDto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = null };
            var res = new AccountCategoryMaintenanceRes();
            _accountCategoryService.UpdateCsg7GroupAsync(accShortName, null).Returns(updatedDto);
            _mapper.Map<AccountCategoryMaintenanceRes>(updatedDto).Returns(res);

            // Act
            var result = await _controller.UpdateAccountCategory(accShortName, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(res, okResult.Value);
        }

        [Fact]
        public async Task UpdateAccountCategory_ServiceThrowsKeyNotFound_PropagatesException()
        {
            // Arrange
            var accShortName = "NOTEXIST";
            var req = new AccountCategoryMaintenanceReq { Csg7Group = "CSG001" };
            _accountCategoryService.UpdateCsg7GroupAsync(accShortName, req.Csg7Group)
                .Throws(new KeyNotFoundException($"Account category '{accShortName}' not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateAccountCategory(accShortName, req));
        }

        #endregion
    }
}
