using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Constants;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.Common.Utilities.StateManagement;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class ProjectPlanViewerController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly IStaffJobService _staffJobService;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly IAdditionalCostService _additionalCostService;
        private readonly ITimeCostCalcsService _timeCostCalcsService;
        private readonly IMonthlyOutputService _monthlyOutputService;
        private readonly IProjectSubContractService _projectSubContractService;
        private readonly IAppStateService _appStateService;

        public ProjectPlanViewerController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService,
            IStaffJobService staffJobService,
            IAnimalPlanService animalPlanService,
            ITestRequirementService testRequirementService,
            IAdditionalCostService additionalCostService,
            ITimeCostCalcsService timeCostCalcsService,
            IMonthlyOutputService monthlyOutputService,
            IProjectSubContractService projectSubContractService,
            IAppStateService appStateService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
            _staffJobService = staffJobService;
            _animalPlanService = animalPlanService;
            _testRequirementService = testRequirementService;
            _additionalCostService = additionalCostService;
            _timeCostCalcsService = timeCostCalcsService;
            _monthlyOutputService = monthlyOutputService;
            _projectSubContractService = projectSubContractService;
            _appStateService = appStateService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? projectCode = null, string? program = null, string? projectGroup = null)
        {
            var programList = await GetProgramListAsync();
            var projectGroupList = await GetProjectGroupListAsync();
            var projectList = await GetProjectListAsync();

            if (string.IsNullOrWhiteSpace(projectCode))
                projectCode = await _appStateService.GetSessionAsync<string>(SessionKeys.SelectedProjectCode);

            var selectedProjectCode = !string.IsNullOrWhiteSpace(projectCode)
                && projectList.Any(p => p.Value == projectCode)
                ? projectCode
                : string.Empty;

            var model = new ProjectPlanViewerViewModel
            {
                ProgramList = programList,
                ProjectGroupList = projectGroupList,
                ProjectList = projectList,
                SelectedProgram = program ?? string.Empty,
                SelectedProjectGroup = projectGroup ?? string.Empty,
                SelectedProjectCode = selectedProjectCode,
                ProjectDetailsGrid = GetReadOnlyProjectDetailsGrid(),
                ProjectDetails = new ProjectDetailsPartialViewModel
                {
                    SelectedProjectCode = selectedProjectCode,
                    PlanSummaryStaffGrid = GetReadOnlyStaffGrid("planSummaryStaffGrid", "Staff Plans"),
                    PlanSummaryTestGrid = GetReadOnlyTestPlanGrid("planSummaryTestGrid", "Test Plans"),
                    PlanSummaryAnimalGrid = GetReadOnlyAnimalGrid("planSummaryAnimalGrid", "Animal Plans"),
                    PlanSummaryAdditionalGrid = GetReadOnlyAdditionalGrid("planSummaryAdditionalGrid", "Additional Cost Plans"),
                    StaffPlanGrid = GetReadOnlyStaffGrid("staffPlanGrid", "Staff Plans"),
                    StaffActualGrid = GetReadOnlyCompareStaff2Grid(),
                    TestPlanGrid = GetReadOnlyTestPlanGrid("testPlanGrid", "Test Plans"),
                    TestActualGrid = GetReadOnlyTestActualGrid(),
                    AnimalPlanGrid = GetReadOnlyAnimalGrid("animalPlanGrid", "Animal Plans"),
                    AnimalActualGrid = GetReadOnlyActualCostGrid("actualAnimalCostGrid", "Actual Animal Costs (PACT)"),
                    AdditionalPlanGrid = GetReadOnlyAdditionalGrid("additionalCostPlanGrid", "Additional Cost Plans"),
                    AdditionalActualGrid = GetReadOnlyActualCostGrid("actualAdditionalCostGrid", "Actual Additional Costs (PACT)")
                }
            };

            if (!string.IsNullOrWhiteSpace(selectedProjectCode))
            {
                await PopulateProjectDetailsAsync(model, selectedProjectCode);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectDetails(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var result = await _projectService.GetProjectByIdAsync(projectCode);
            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Project not found."
                });
            }

            var p = result.Data;
            return Json(new
            {
                success = true,
                projectCode = p.ParentProject,
                projectTitle = p.ProjectTitle,
                shortTitle = p.ShortTitle,
                customer = p.Customer,
                program = p.Program,
                manager = p.Manager,
                disease = p.Disease,
                custIncome = p.CustIncome,
                transferIncome = p.TransferIncome,
                targetProfit = p.Profit,
                projectStatus = p.ProjectStatus,
                costBookNo = p.CostBookNo,
                contract = p.Contract,
                isDefraProject = p.IsDefraProject,
                costCentre = p.CostCentre,
                owningRc = p.OwningRc,
                projectGroup = p.ProjectGroup,
                incomeAccountCode = p.IncomeAccountCode,
                subAccountCode = p.SubAccountCode,
                budgetCvl = p.BudgetCvl,
                pvsIncome = p.PvsIncome,
                planCaseWorkDebit = p.PlanCaseWorkDebit,
                carryOver = p.CarryOver,
                carryOverSeed = p.CarryOverSeed,
                comments = p.Comments
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlanSummaryTotals(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var staffCostTask = _staffJobService.GetTotalStaffCostAsync(projectCode);
            var animalCostTask = _animalPlanService.GetTotalAnimalCostAsync(projectCode);
            var additionalCostTask = _additionalCostService.GetTotalItemCostAsync(projectCode);

            await Task.WhenAll(staffCostTask, animalCostTask, additionalCostTask);

            var staffCost = staffCostTask.Result.Data;
            var animalCost = animalCostTask.Result.Data;
            var additionalCost = additionalCostTask.Result.Data;

            // Test cost is computed from test requirement data
            var testQuery = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var testResult = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(testQuery, projectCode);
            decimal testCost = 0m;
            if (testResult.Success && testResult.Data != null)
            {
                testCost = testResult.Data.Sum(t => (t.UnitPrice ?? 0) * (decimal)(t.NoRequired ?? 0));
            }

            return Json(new
            {
                success = true,
                totalStaffCost = staffCost,
                totalTestCost = testCost,
                totalAnimalCost = animalCost,
                totalAdditionalCost = additionalCost
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStaffPlanVsActualTotals(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var plannedResult = await _staffJobService.GetTotalStaffCostAsync(projectCode);
            var actualResult = await _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode);

            var plannedCost = plannedResult.Data;
            var totalHours = actualResult.Data?.TotalHours ?? 0;
            var totalCost = actualResult.Data?.TotalCost ?? 0;
            var percentOfPlan = plannedCost > 0 ? (totalCost / (double)plannedCost) * 100 : 0;

            return Json(new
            {
                success = true,
                totalPlannedCost = plannedCost,
                totalActualHrs = totalHours,
                totalActualCost = totalCost,
                percentOfPlan
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTestPlanVsActualTotals(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var testQuery = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var testResult = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(testQuery, projectCode);
            decimal plannedCost = 0m;
            var priceLookup = new Dictionary<(string TestCode, string Buyer), decimal>();

            if (testResult.Success && testResult.Data != null)
            {
                plannedCost = testResult.Data.Sum(t => (t.UnitPrice ?? 0) * (decimal)(t.NoRequired ?? 0));
                foreach (var t in testResult.Data)
                {
                    var key = (t.TestCode ?? string.Empty, t.Buyer ?? string.Empty);
                    if (!priceLookup.ContainsKey(key))
                        priceLookup[key] = t.UnitPrice ?? 0;
                }
            }

            var actualResult = await _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, priceLookup);
            double totalActualCost = actualResult.Success ? actualResult.Data : 0;
            double percentOfPlan = plannedCost > 0 ? (totalActualCost / (double)plannedCost) * 100 : 0;

            return Json(new
            {
                success = true,
                totalPlannedCost = plannedCost,
                totalActualCost,
                percentOfPlan
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAnimalPlanVsActualTotals(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var plannedResult = await _animalPlanService.GetTotalAnimalCostAsync(projectCode);
            var actualResult = await _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true);

            var plannedCost = plannedResult.Data;
            var totalActualCost = actualResult.Data;
            double percentOfPlan = plannedCost > 0 ? ((double)totalActualCost / (double)plannedCost) * 100 : 0;

            return Json(new
            {
                success = true,
                totalPlannedCost = plannedCost,
                totalActualCost,
                percentOfPlan
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAdditionalPlanVsActualTotals(string projectCode)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Json(new { success = false, message = "Project code is required." });

            var plannedResult = await _additionalCostService.GetTotalItemCostAsync(projectCode);
            var actualResult = await _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false);

            var plannedCost = plannedResult.Data;
            var totalActualCost = actualResult.Data;
            double percentOfPlan = plannedCost > 0 ? ((double)totalActualCost / (double)plannedCost) * 100 : 0;

            return Json(new
            {
                success = true,
                totalPlannedCost = plannedCost,
                totalActualCost,
                percentOfPlan
            });
        }

        #region Private Helpers

        private static DataGridConfig<ProjectDetailsGridItem> GetReadOnlyProjectDetailsGrid()
        {
            return new DataGridConfig<ProjectDetailsGridItem>
            {
                GridId = "isProjectDetailsGrid",
                Title = "Project Details",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "onProjectDetailsRowSelect",
                KeyProperty = "ParentProject",
                ExtraFilterMethod = "getisProjectDetailsGridExtraFilters",
                BindGridUrl = "/FPS/ProjectPlanViewer/LoadProjectDetailsGrid",
                Data = new List<ProjectDetailsGridItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ProjectDetailsGridItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private async Task PopulateProjectDetailsAsync(ProjectPlanViewerViewModel model, string projectCode)
        {
            var result = await _projectService.GetProjectByIdAsync(projectCode);
            if (!result.Success || result.Data == null) return;

            var p = result.Data;
            var details = model.ProjectDetails;
            details.SelectedProjectCode = projectCode;
            details.Program = p.Program ?? string.Empty;

            details.ProjectDetails.ProjectCode = p.ParentProject;
            details.ProjectDetails.Description = p.ProjectTitle;
            details.ProjectDetails.ShortTitle = p.ShortTitle;
            details.ProjectDetails.Customer = p.Customer;
            details.ProjectDetails.Manager = p.Manager;
            details.ProjectDetails.Disease = p.Disease;
            details.ProjectDetails.CustIncome = p.CustIncome;
            details.ProjectDetails.TransferIncome = p.TransferIncome;
            details.ProjectDetails.TargetProfit = p.Profit;
            details.ProjectDetails.ProjectStatus = p.ProjectStatus;
            details.ProjectDetails.CostBookNo = p.CostBookNo;
            details.ProjectDetails.Contract = p.Contract;
            details.ProjectDetails.IsDefraProject = p.IsDefraProject;
            details.ProjectDetails.CostCentre = p.CostCentre;
            details.ProjectDetails.OwningRc = p.OwningRc;
            details.ProjectDetails.ProjectGroup = p.ProjectGroup;
            details.ProjectDetails.IncomeAccountCode = p.IncomeAccountCode;
            details.ProjectDetails.SubAccountCode = p.SubAccountCode;
            details.ProjectDetails.BudgetCvl = p.BudgetCvl;
            details.ProjectDetails.PvsIncome = p.PvsIncome;
            details.ProjectDetails.PlanCaseWorkDebit = p.PlanCaseWorkDebit;
            details.ProjectDetails.CarryOver = p.CarryOver;
            details.ProjectDetails.CarryOverSeed = p.CarryOverSeed;
            details.ProjectDetails.Comments = p.Comments;

            // Fetch plan summary totals
            var staffCostResult = await _staffJobService.GetTotalStaffCostAsync(projectCode);
            details.TotalStaffPlanCost = staffCostResult.Data;

            var animalCostResult = await _animalPlanService.GetTotalAnimalCostAsync(projectCode);
            details.TotalAnimalPlanCost = animalCostResult.Data;

            var additionalCostResult = await _additionalCostService.GetTotalItemCostAsync(projectCode);
            details.TotalAdditionalPlanCost = additionalCostResult.Data;

            // Test plan total
            var testQuery = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var testResult = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(testQuery, projectCode);
            if (testResult.Success && testResult.Data != null)
            {
                details.TotalTestPlanCost = testResult.Data.Sum(t => (t.UnitPrice ?? 0) * (decimal)(t.NoRequired ?? 0));
            }

            // Staff plan vs actuals
            details.StaffTotalPlannedCost = staffCostResult.Data;
            var staffActualResult = await _timeCostCalcsService.GetTotalActualByProjectAsync(projectCode);
            if (staffActualResult.Success && staffActualResult.Data != null)
            {
                details.StaffTotalActualHrs = staffActualResult.Data.TotalHours;
                details.StaffTotalActualCost = staffActualResult.Data.TotalCost;
                details.StaffPercentOfPlan = staffCostResult.Data > 0
                    ? (staffActualResult.Data.TotalCost / (double)staffCostResult.Data) * 100
                    : 0;
            }

            // Test plan vs actuals
            details.TestTotalPlannedCost = details.TotalTestPlanCost;
            var priceLookup = new Dictionary<(string TestCode, string Buyer), decimal>();
            if (testResult.Success && testResult.Data != null)
            {
                foreach (var t in testResult.Data)
                {
                    var key = (t.TestCode ?? string.Empty, t.Buyer ?? string.Empty);
                    if (!priceLookup.ContainsKey(key))
                        priceLookup[key] = t.UnitPrice ?? 0;
                }
            }
            var testActualResult = await _monthlyOutputService.GetTotalActualByProjectAsync(projectCode, priceLookup);
            if (testActualResult.Success)
            {
                details.TestTotalActualCost = testActualResult.Data;
                details.TestPercentOfPlan = details.TestTotalPlannedCost > 0
                    ? (testActualResult.Data / (double)details.TestTotalPlannedCost) * 100
                    : 0;
            }

            // Animal plan vs actuals
            details.AnimalTotalPlannedCost = animalCostResult.Data;
            var animalActualResult = await _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: true);
            details.AnimalTotalActualCost = animalActualResult.Data;
            details.AnimalPercentOfPlan = animalCostResult.Data > 0
                ? ((double)animalActualResult.Data / (double)animalCostResult.Data) * 100
                : 0;

            // Additional plan vs actuals
            details.AdditionalTotalPlannedCost = additionalCostResult.Data;
            var additionalActualResult = await _projectSubContractService.GetFpsProjectSubContractTotalAmountAsync(projectCode, filterByAnimalAcctCodes: false);
            details.AdditionalTotalActualCost = additionalActualResult.Data;
            details.AdditionalPercentOfPlan = additionalCostResult.Data > 0
                ? ((double)additionalActualResult.Data / (double)additionalCostResult.Data) * 100
                : 0;
        }

        private async Task<List<SelectListItem>> GetProgramListAsync()
        {
            var result = await _programService.GetAllProgramsForAllUsersAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem { Value = p.ProgramNo, Text = p.ProgramNo })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetProjectGroupListAsync()
        {
            var result = await _projectService.GetAllProjectGroupsAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(g => new SelectListItem { Value = g.ProjectGroupName, Text = g.ProjectGroupName })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private async Task<List<SelectListItem>> GetProjectListAsync()
        {
            var result = await _projectService.GetAllProjectsForAllUsersAsync();

            if (result != null && result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem { Value = p.ParentProject, Text = p.ParentProject })
                    .ToList();
            }

            return new List<SelectListItem>();
        }

        private static DataGridConfig<StaffJobItemViewModel> GetReadOnlyStaffGrid(string gridId, string title)
        {
            var columns = new List<DataGridColumn>
            {
                new DataGridColumn { PropertyName = "WorkGroupGrade", DisplayName = "WG Grad", Width = 100, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "Name", DisplayName = "Name", Width = 169, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "PlannedHours", DisplayName = "Hours", Width = 100, ColumnType = GridColumnType.Number, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "ChargeRate", DisplayName = "Charge Rate", Width = 100, ColumnType = GridColumnType.GbpValue, IsVisible = true },
                new DataGridColumn { PropertyName = "StaffCost", DisplayName = "Cost", Width = 110, ColumnType = GridColumnType.GbpValue, IsVisible = true }
            };

            return new DataGridConfig<StaffJobItemViewModel>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "StaffID",
                ExtraFilterMethod = $"get{gridId}ExtraFilters",
                BindGridUrl = $"/FPS/ProjectPlanViewer/LoadStaffPlanGrid?gridId={gridId}",
                Data = new List<StaffJobItemViewModel>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<TestPlanActualItem> GetReadOnlyTestPlanGrid(string gridId, string title)
        {
            var columns = new List<DataGridColumn>
            {
                new DataGridColumn { PropertyName = "TestCode", DisplayName = "TestCode", Width = 120, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "UnitPrice", DisplayName = "UnitPrice", Width = 120, ColumnType = GridColumnType.GbpValue, IsVisible = true },
                new DataGridColumn { PropertyName = "NoRequired", DisplayName = "NoRequired", Width = 110, ColumnType = GridColumnType.DecimalNumber, IsVisible = true },
                new DataGridColumn { PropertyName = "TestCost", DisplayName = "Cost", Width = 110, ColumnType = GridColumnType.GbpValue, IsVisible = true }
            };

            return new DataGridConfig<TestPlanActualItem>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "TestCode",
                ExtraFilterMethod = $"get{gridId}ExtraFilters",
                BindGridUrl = $"/FPS/ProjectPlanViewer/LoadTestPlanGrid?gridId={gridId}",
                Data = new List<TestPlanActualItem>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<AnimalPlanItem> GetReadOnlyAnimalGrid(string gridId, string title)
        {
            var columns = new List<DataGridColumn>
            {
                new DataGridColumn { PropertyName = "AnimalType", DisplayName = "Animal Type", Width = 120, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "NumberOfDays", DisplayName = "No Days", Width = 80, ColumnType = GridColumnType.DecimalNumber, IsVisible = true },
                new DataGridColumn { PropertyName = "NumberOfAnimals", DisplayName = "No Animals", Width = 100, ColumnType = GridColumnType.DecimalNumber, IsVisible = true },
                new DataGridColumn { PropertyName = "DailyRate", DisplayName = "DailyRate", Width = 120, ColumnType = GridColumnType.GbpValue, IsVisible = true },
                new DataGridColumn { PropertyName = "AnimalCost", DisplayName = "Cost", Width = 110, ColumnType = GridColumnType.GbpValue, IsVisible = true }
            };

            return new DataGridConfig<AnimalPlanItem>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "IndCounter",
                ExtraFilterMethod = $"get{gridId}ExtraFilters",
                BindGridUrl = $"/FPS/ProjectPlanViewer/LoadAnimalPlanGrid?gridId={gridId}",
                Data = new List<AnimalPlanItem>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<AdditionalCostItemViewModel> GetReadOnlyAdditionalGrid(string gridId, string title)
        {
            var columns = new List<DataGridColumn>
            {
                new DataGridColumn { PropertyName = "Account", DisplayName = "Account", Width = 130, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "Description", DisplayName = "Description", Width = 160, ColumnType = GridColumnType.Text, IsVisible = true, IsFilterable = true },
                new DataGridColumn { PropertyName = "ItemCost", DisplayName = "Cost", Width = 110, ColumnType = GridColumnType.GbpValue, IsVisible = true }
            };

            return new DataGridConfig<AdditionalCostItemViewModel>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "Description",
                ExtraFilterMethod = $"get{gridId}ExtraFilters",
                BindGridUrl = $"/FPS/ProjectPlanViewer/LoadAdditionalCostGrid?gridId={gridId}",
                Data = new List<AdditionalCostItemViewModel>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<CompareStaff2Item> GetReadOnlyCompareStaff2Grid()
        {
            var columns = GridDataProvider.GetColumnsDefination<CompareStaff2Item>(null);
            foreach (var col in columns)
            {
                if (col.PropertyName == "WorkGroup") col.DisplayName = "Work Group";
                else if (col.PropertyName == "GradeCode") col.DisplayName = "Grade";
                else if (col.PropertyName == "JobCode") col.DisplayName = "Job Code";
            }

            return new DataGridConfig<CompareStaff2Item>
            {
                GridId = "staffActualGrid",
                Title = "Actual Time (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "RowKey",
                ExtraFilterMethod = "getStaffActualGridExtraFilters",
                BindGridUrl = "/FPS/ProjectPlanViewer/LoadStaffActualGrid",
                Data = new List<CompareStaff2Item>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<ActualTestOutputItem> GetReadOnlyTestActualGrid()
        {
            return new DataGridConfig<ActualTestOutputItem>
            {
                GridId = "testActualGrid",
                Title = "Actual Tests (PACT)",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "RowKey",
                ExtraFilterMethod = "getTestActualGridExtraFilters",
                BindGridUrl = "/FPS/ProjectPlanViewer/LoadTestActualGrid",
                Data = new List<ActualTestOutputItem>(),
                Columns = GridDataProvider.GetColumnsDefination<ActualTestOutputItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private static DataGridConfig<ActualProjectCostItem> GetReadOnlyActualCostGrid(string gridId, string title)
        {
            var columns = GridDataProvider.GetColumnsDefination<ActualProjectCostItem>(null);
            foreach (var col in columns)
            {
                if (col.PropertyName == "AcctCode") col.DisplayName = "Acct Code";
            }

            return new DataGridConfig<ActualProjectCostItem>
            {
                GridId = gridId,
                Title = title,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                KeyProperty = "SubContCounter",
                ExtraFilterMethod = $"get{gridId}ExtraFilters",
                BindGridUrl = $"/FPS/ProjectPlanViewer/LoadActualCostGrid?gridId={gridId}",
                Data = new List<ActualProjectCostItem>(),
                Columns = columns,
                Pagination = new PaginationModel()
            };
        }

        #endregion

        #region Grid Data Load Actions

        [HttpPost]
        public async Task<IActionResult> LoadProjectDetailsGrid(PaginationFilter<string> request, string? program = null,
            string? projectGroup = null, string? parentProject = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var query = _mapper.Map<QueryParameters<string>>(request);
            ApiResponseDto<List<ProjectDto>>? result = null;

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                // Single project selected - fetch that specific project
                var singleResult = await _projectService.GetProjectByIdAsync(parentProject);
                if (singleResult.Success && singleResult.Data != null
                    && MatchesProjectDetailsFilter(singleResult.Data, filterDict))
                {
                    result = new ApiResponseDto<List<ProjectDto>>
                    {
                        Success = true,
                        Data = new List<ProjectDto> { singleResult.Data },
                        Pagination = new Application.Dtos.PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = query.PageSize }
                    };
                }
                else
                {
                    result = new ApiResponseDto<List<ProjectDto>>
                    {
                        Success = true,
                        Data = new List<ProjectDto>(),
                        Pagination = new Application.Dtos.PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = query.PageSize }
                    };
                }
            }
            else if (!string.IsNullOrWhiteSpace(program))
            {
                result = await _projectService.GetProjectsByProgramAsync(query, program);
            }
            else if (!string.IsNullOrWhiteSpace(projectGroup))
            {
                result = await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);
            }
            else
            {
                result = await _projectService.GetPagedProjectsAsync(query);
            }

            var items = new List<ProjectDetailsGridItem>();
            if (result != null && result.Success && result.Data != null)
            {
                items = result.Data.Select(p => new ProjectDetailsGridItem
                {
                    ParentProject = p.ParentProject,
                    ProjectTitle = p.ProjectTitle,
                    Program = p.Program,
                    Manager = p.Manager,
                    Customer = p.Customer,
                    Contract = p.Contract,
                    Status = p.ProjectStatus,
                    TransferIncome = p.TransferIncome,
                    CustIncome = p.CustIncome,
                    Budget = p.BudgetCvl
                }).ToList();
            }

            var paginationModel = result?.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(result.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyProjectDetailsGrid();
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;

            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        private static bool MatchesProjectDetailsFilter(ProjectDto project, Dictionary<string, string>? filterDict)
        {
            if (filterDict == null || filterDict.Count == 0)
                return true;

            foreach (var filter in filterDict)
            {
                if (string.IsNullOrWhiteSpace(filter.Value))
                    continue;

                var value = filter.Key switch
                {
                    nameof(ProjectDetailsGridItem.ParentProject) => project.ParentProject,
                    nameof(ProjectDetailsGridItem.ProjectTitle) => project.ProjectTitle,
                    nameof(ProjectDetailsGridItem.Manager) => project.Manager,
                    _ => null
                };

                if (value == null ||
                    value.IndexOf(filter.Value, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }


        [HttpPost]
        public async Task<IActionResult> LoadStaffPlanGrid(PaginationFilter<string> request, string? parentProject = null, string? gridId = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<StaffJobItemViewModel>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _staffJobService.GetAllStaffJobsAsync(query, parentProject);

                items = pagedData.Data != null
                    ? _mapper.Map<List<StaffJobItemViewModel>>(pagedData.Data.ToList())
                    : new List<StaffJobItemViewModel>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyStaffGrid(gridId ?? "planSummaryStaffGrid", "Staff Plans");
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestPlanGrid(PaginationFilter<string> request, string? parentProject = null, string? gridId = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<TestPlanActualItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, parentProject);

                items = response.Success && response.Data != null
                    ? _mapper.Map<List<TestPlanActualItem>>(response.Data)
                    : new List<TestPlanActualItem>();

                paginationModel = response.Pagination is null
                    ? new PaginationModel()
                    : _mapper.Map<PaginationModel>(response.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyTestPlanGrid(gridId ?? "planSummaryTestGrid", "Test Plans");
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalPlanGrid(PaginationFilter<string> request, string? parentProject = null, string? gridId = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<AnimalPlanItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _animalPlanService.GetAllAnimalCostAsync(query, parentProject);

                items = pagedData.Data != null
                    ? _mapper.Map<List<AnimalPlanItem>>(pagedData.Data.ToList())
                    : new List<AnimalPlanItem>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyAnimalGrid(gridId ?? "planSummaryAnimalGrid", "Animal Plans");
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAdditionalCostGrid(PaginationFilter<string> request, string? parentProject = null, string? gridId = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<AdditionalCostItemViewModel>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _additionalCostService.GetAdditionalCostsAsync(query, parentProject);

                items = pagedData.Data != null
                    ? _mapper.Map<List<AdditionalCostItemViewModel>>(pagedData.Data)
                    : new List<AdditionalCostItemViewModel>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyAdditionalGrid(gridId ?? "planSummaryAdditionalGrid", "Additional Cost Plans");
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffActualGrid(PaginationFilter<string> request, string? parentProject = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<CompareStaff2Item>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _timeCostCalcsService.GetTimeCostCalcsByProjectAsync(query, parentProject);

                items = pagedData.Data != null
                    ? _mapper.Map<List<CompareStaff2Item>>(pagedData.Data)
                    : new List<CompareStaff2Item>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyCompareStaff2Grid();
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestActualGrid(PaginationFilter<string> request, string? parentProject = null)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<ActualTestOutputItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);

                // Build price lookup from test requirements
                var testQuery = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
                var testResult = await _testRequirementService.GetPagedTestReqmtbyProjectAsync(testQuery, parentProject);
                var priceLookup = new Dictionary<(string TestCode, string Buyer), decimal>();
                if (testResult.Success && testResult.Data != null)
                {
                    foreach (var t in testResult.Data)
                    {
                        var key = (t.TestCode ?? string.Empty, t.Buyer ?? string.Empty);
                        if (!priceLookup.ContainsKey(key))
                            priceLookup[key] = t.UnitPrice ?? 0;
                    }
                }

                var pagedData = await _monthlyOutputService.GetMonthlyOutputByProjectAsync(query, parentProject, priceLookup);

                items = pagedData.Data != null
                    ? _mapper.Map<List<ActualTestOutputItem>>(pagedData.Data)
                    : new List<ActualTestOutputItem>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyTestActualGrid();
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadActualCostGrid(PaginationFilter<string> request, string? parentProject = null, string? gridId = null, bool animalOnly = false)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid request data" });

            var items = new List<ActualProjectCostItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _projectSubContractService.GetFpsProjectSubContractsAsync(query, parentProject, filterByAnimalAcctCodes: animalOnly);

                items = pagedData.Data != null
                    ? _mapper.Map<List<ActualProjectCostItem>>(pagedData.Data)
                    : new List<ActualProjectCostItem>();

                paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridConfig = GetReadOnlyActualCostGrid(gridId ?? "actualAnimalCostGrid", animalOnly ? "Actual Animal Costs (PACT)" : "Actual Additional Costs (PACT)");
            gridConfig.Data = items;
            gridConfig.Pagination = paginationModel;
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;
            gridConfig.CurrentFilters = filterDict;

            return PartialView("_DataGrid", gridConfig);
        }

        #endregion
    }
}
