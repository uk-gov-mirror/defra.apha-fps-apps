using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BBQueryControllerTest
{
    public class BBQueryControllerTests
    {
        private readonly IWorkGroupService _workGroupService;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly BBQueryController _controller;

        public BBQueryControllerTests()
        {
            _workGroupService = Substitute.For<IWorkGroupService>();
            _budgetBidsService = Substitute.For<IBudgetBidsService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _controller = new BBQueryController(_workGroupService, _budgetBidsService, _profitCentreService)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
            };
        }

        private static ApiResponseDto<T> Fail<T>() =>
            ApiResponseDto<T>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());

        [Fact]
        public async Task Index_ReturnsView_WithEmptyGrid_AndFiltersBlankProfitCentres()
        {
            _controller.HttpContext.Items["SelectedFPSYear"] = "2025";
            var profitCentres = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "One" },
                new() { ProfitCentreId = "  ", ProfitCentreName = "Blank" }
            };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BBQueryViewModel>(view.Model);
            Assert.Equal(2025, model.FpsYear);
            Assert.Null(model.SelectedProfitCentre);
            Assert.Single(model.ProfitCentreOptions);
            Assert.Equal("PC1", model.ProfitCentreOptions[0].Value);
            // Empty grid: only the two fixed columns and no rows.
            Assert.Equal(2, model.Grid.Columns.Count);
            Assert.Empty(model.Grid.Data);
            Assert.Equal("bbQueryGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_UsesCurrentYear_WhenSelectedFpsYearItemMissing()
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_UsesCurrentYear_WhenSelectedFpsYearItemNotParsable()
        {
            _controller.HttpContext.Items["SelectedFPSYear"] = "not-a-year";
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_ProfitCentreOptions_Empty_WhenServiceFails()
        {
            _profitCentreService.GetProfitCentresAsync().Returns(Fail<List<ProfitCentreDto>>());

            var result = await _controller.Index();

            var model = Assert.IsType<BBQueryViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task LoadGrid_NullProfitCentre_ReturnsEmptyGridPartial()
        {
            var result = await _controller.LoadGrid(null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
            Assert.Equal(2, grid.Columns.Count);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WithProfitCentre_BuildsCrosstab_WithDynamicColumnsAndSummaries()
        {
            var workgroups = new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG2", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(workgroups));

            _budgetBidsService.GetBidViewAsync("WG1").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG1", GenBid = 10m },
                    new() { Account = "A2", WorkGroupName = "WG1", GenBid = 5m }
                }));
            _budgetBidsService.GetBidViewAsync("WG2").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG2", GenBid = 20m }
                }));

            _budgetBidsService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto>
                {
                    new() { AccShortName = "A2" },
                    new() { AccShortName = "A1" }
                }));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);

            // Columns: AccShortName, RowSummary, then workgroups ordered ascending (WG1, WG2).
            Assert.Collection(grid.Columns.Select(c => c.PropertyName),
                p => Assert.Equal("AccShortName", p),
                p => Assert.Equal("RowSummary", p),
                p => Assert.Equal("WG1", p),
                p => Assert.Equal("WG2", p));

            // Rows ordered by account (A1, A2).
            Assert.Equal(2, grid.Data.Count);
            var a1 = grid.Data[0];
            Assert.Equal("A1", a1["AccShortName"]);
            Assert.Equal("10", a1["WG1"]);
            Assert.Equal("20", a1["WG2"]);
            Assert.Equal("30", a1["RowSummary"]);

            var a2 = grid.Data[1];
            Assert.Equal("A2", a2["AccShortName"]);
            Assert.Equal("5", a2["WG1"]);
            Assert.Equal("0", a2["WG2"]);
            Assert.Equal("5", a2["RowSummary"]);
        }

        [Fact]
        public async Task LoadGrid_FallsBackToBidAccounts_WhenNoCategoriesReturned()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG1").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG1", GenBid = 10m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Single(grid.Data);
            Assert.Equal("A1", grid.Data[0]["AccShortName"]);
            Assert.Equal("10", grid.Data[0]["RowSummary"]);
        }

        [Fact]
        public async Task LoadGrid_WorkGroupServiceFails_ProducesNoRows()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(Fail<List<WorkGroupViewDto>>());
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Columns.Count);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_SkipsWorkgroup_WhenBidServiceFails()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                    new() { WorkGroupName = "WG2", ProfitCentre = "PC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG1").Returns(Fail<List<BidViewDto>>());
            _budgetBidsService.GetBidViewAsync("WG2").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG2", GenBid = 15m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto> { new() { AccShortName = "A1" } }));

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            var a1 = Assert.Single(grid.Data);
            Assert.Equal("0", a1["WG1"]);
            Assert.Equal("15", a1["WG2"]);
            Assert.Equal("15", a1["RowSummary"]);
        }

        private void ArrangeSortableGrid()
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" },
                    new() { WorkGroupName = "WG2", ProfitCentre = "PC1" }
                }));
            _budgetBidsService.GetBidViewAsync("WG1").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG1", GenBid = 10m },
                    new() { Account = "A2", WorkGroupName = "WG1", GenBid = 5m }
                }));
            _budgetBidsService.GetBidViewAsync("WG2").Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(
                new List<BidViewDto>
                {
                    new() { Account = "A1", WorkGroupName = "WG2", GenBid = 20m },
                    new() { Account = "A2", WorkGroupName = "WG2", GenBid = 40m }
                }));
            _budgetBidsService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(
                new List<AccountCategoryDto>
                {
                    new() { AccShortName = "A1" },
                    new() { AccShortName = "A2" }
                }));
        }

        [Fact]
        public async Task LoadGrid_NoSort_DefaultsToAccountAscending_AndClearsSortState()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Null(grid.Pagination.SortColumn);
            Assert.False(grid.Pagination.SortDirection);
            Assert.Collection(grid.Data,
                r => Assert.Equal("A1", r["AccShortName"]),
                r => Assert.Equal("A2", r["AccShortName"]));
        }

        [Fact]
        public async Task LoadGrid_SortByAccShortName_Descending_ReordersRows_AndReflectsSortState()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", "AccShortName", true);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal("AccShortName", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
            Assert.Collection(grid.Data,
                r => Assert.Equal("A2", r["AccShortName"]),
                r => Assert.Equal("A1", r["AccShortName"]));
        }

        [Fact]
        public async Task LoadGrid_SortByRowSummary_Ascending_OrdersNumerically()
        {
            ArrangeSortableGrid();

            // A1 summary = 30, A2 summary = 45.
            var result = await _controller.LoadGrid("PC1", "RowSummary", false);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Collection(grid.Data,
                r => Assert.Equal("30", r["RowSummary"]),
                r => Assert.Equal("45", r["RowSummary"]));
        }

        [Fact]
        public async Task LoadGrid_SortByRowSummary_Descending_OrdersNumerically()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", "RowSummary", true);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Collection(grid.Data,
                r => Assert.Equal("45", r["RowSummary"]),
                r => Assert.Equal("30", r["RowSummary"]));
        }

        [Fact]
        public async Task LoadGrid_SortByDynamicWorkgroupColumn_OrdersByValuesDictionary()
        {
            ArrangeSortableGrid();

            // WG2 values: A1 = 20, A2 = 40. Ascending -> A1 then A2; descending -> A2 then A1.
            var ascResult = await _controller.LoadGrid("PC1", "WG2", false);
            var ascGrid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(ascResult).Model);
            Assert.Collection(ascGrid.Data,
                r => Assert.Equal("A1", r["AccShortName"]),
                r => Assert.Equal("A2", r["AccShortName"]));

            var descResult = await _controller.LoadGrid("PC1", "WG2", true);
            var descGrid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(descResult).Model);
            Assert.Collection(descGrid.Data,
                r => Assert.Equal("A2", r["AccShortName"]),
                r => Assert.Equal("A1", r["AccShortName"]));
        }

        [Fact]
        public async Task LoadGrid_SortByUnknownColumn_KeepsRowsStable()
        {
            ArrangeSortableGrid();

            // Unknown column resolves to null keys for all rows -> original order preserved.
            var result = await _controller.LoadGrid("PC1", "DoesNotExist", false);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Collection(grid.Data,
                r => Assert.Equal("A1", r["AccShortName"]),
                r => Assert.Equal("A2", r["AccShortName"]));
        }

        [Fact]
        public async Task LoadGrid_SortWithNoProfitCentre_ReturnsEmptyData_WithSortState()
        {
            var result = await _controller.LoadGrid(null, "RowSummary", true);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Empty(grid.Data);
            Assert.Equal("RowSummary", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_FilterByAccShortName_ReturnsMatchingRows_AndKeepsFilterState()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "{\"AccShortName\":\"A1\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal("A1", row["AccShortName"]);
            Assert.NotNull(grid.CurrentFilters);
            Assert.Equal("A1", grid.CurrentFilters!["AccShortName"]);
        }

        [Fact]
        public async Task LoadGrid_FilterByAccShortName_IsCaseInsensitiveContains()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "{\"AccShortName\":\"a\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_FilterByRowSummary_MatchesNumericText()
        {
            ArrangeSortableGrid();

            // A1 summary = 30, A2 summary = 45.
            var result = await _controller.LoadGrid("PC1", filter: "{\"RowSummary\":\"45\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal("A2", row["AccShortName"]);
        }

        [Fact]
        public async Task LoadGrid_FilterByDynamicWorkgroupColumn_MatchesValuesDictionary()
        {
            ArrangeSortableGrid();

            // WG2 values: A1 = 20, A2 = 40.
            var result = await _controller.LoadGrid("PC1", filter: "{\"WG2\":\"40\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal("A2", row["AccShortName"]);
        }

        [Fact]
        public async Task LoadGrid_MultipleFilters_AppliedWithAndSemantics()
        {
            ArrangeSortableGrid();

            // A1 has AccShortName "A1" and RowSummary 30 -> matches both.
            var result = await _controller.LoadGrid("PC1", filter: "{\"AccShortName\":\"A\",\"RowSummary\":\"30\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            var row = Assert.Single(grid.Data);
            Assert.Equal("A1", row["AccShortName"]);
        }

        [Fact]
        public async Task LoadGrid_FilterWithNoMatch_ReturnsNoRows()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "{\"AccShortName\":\"ZZZ\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_EmptyFilterObject_ReturnsAllRows_WithNoFilterState()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "{}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.Null(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_FilterWithBlankValue_IsIgnored()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "{\"AccShortName\":\"   \"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.Null(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_InvalidFilterJson_IsIgnored_ReturnsAllRows()
        {
            ArrangeSortableGrid();

            var result = await _controller.LoadGrid("PC1", filter: "not-json");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.Null(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_FilterAndSort_AppliedTogether()
        {
            ArrangeSortableGrid();

            // Filter to rows containing "A", then sort by RowSummary descending (A2=45, A1=30).
            var result = await _controller.LoadGrid("PC1", "RowSummary", true, "{\"AccShortName\":\"A\"}");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Collection(grid.Data,
                r => Assert.Equal("A2", r["AccShortName"]),
                r => Assert.Equal("A1", r["AccShortName"]));
        }

        private void ArrangeManyAccountsGrid(int accountCount)
        {
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync("PC1")
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
                {
                    new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
                }));

            var bids = new List<BidViewDto>();
            var categories = new List<AccountCategoryDto>();
            for (int i = 1; i <= accountCount; i++)
            {
                var account = $"A{i:D3}";
                bids.Add(new BidViewDto { Account = account, WorkGroupName = "WG1", GenBid = i });
                categories.Add(new AccountCategoryDto { AccShortName = account });
            }

            _budgetBidsService.GetBidViewAsync("WG1")
                .Returns(ApiResponseDto<List<BidViewDto>>.SuccessResponse(bids));
            _budgetBidsService.GetAccountCategoriesAsync()
                .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories));
        }

        [Fact]
        public async Task LoadGrid_EnablesPagination_AndReportsTotalRecords()
        {
            ArrangeManyAccountsGrid(25);

            var result = await _controller.LoadGrid("PC1");

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.True(grid.ShowPagination);
            Assert.Equal(25, grid.Pagination.TotalRecords);
            // Default page size is 20.
            Assert.Equal(20, grid.Pagination.PageSize);
            Assert.Equal(1, grid.Pagination.PageNumber);
            Assert.Equal(20, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_ReturnsRequestedPage_WithRemainingRows()
        {
            ArrangeManyAccountsGrid(25);

            var result = await _controller.LoadGrid("PC1", page: 2, pageSize: 20);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Data.Count);
            Assert.Equal("A021", grid.Data.First()["AccShortName"]);
            Assert.Equal("A025", grid.Data.Last()["AccShortName"]);
        }

        [Fact]
        public async Task LoadGrid_RespectsCustomPageSize()
        {
            ArrangeManyAccountsGrid(25);

            var result = await _controller.LoadGrid("PC1", page: 1, pageSize: 10);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(10, grid.Pagination.PageSize);
            Assert.Equal(10, grid.Data.Count);
            Assert.Equal(25, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_InvalidPageAndPageSize_FallBackToDefaults()
        {
            ArrangeManyAccountsGrid(25);

            var result = await _controller.LoadGrid("PC1", page: 0, pageSize: 0);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(1, grid.Pagination.PageNumber);
            Assert.Equal(20, grid.Pagination.PageSize);
            Assert.Equal(20, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_PaginationAppliedAfterFilterAndSort()
        {
            ArrangeManyAccountsGrid(25);

            // Sort by RowSummary descending -> A025 (25) first; page 1 size 5 -> A025..A021.
            var result = await _controller.LoadGrid("PC1", "RowSummary", true, null, 1, 5);

            var grid = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(Assert.IsType<PartialViewResult>(result).Model);
            Assert.Equal(25, grid.Pagination.TotalRecords);
            Assert.Equal(5, grid.Data.Count);
            Assert.Equal("A025", grid.Data.First()["AccShortName"]);
            Assert.Equal("A021", grid.Data.Last()["AccShortName"]);
        }
    }
}



