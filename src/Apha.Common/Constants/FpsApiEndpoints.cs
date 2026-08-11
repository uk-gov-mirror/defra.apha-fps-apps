namespace Apha.Common.Constants
{
    public static class FpsApiEndpoints
    {
        // Animal Master (tblAnimals_MAP maintenance)
        public const string GetAllAnimalMasters = "api/v1/animal";
        public const string GetPagedAnimalMasters = "api/v1/animal/paged";
        public const string GetAnimalMasterById = "api/v1/animal/{0}";
        public const string CreateAnimalMaster = "api/v1/animal";
        public const string UpdateAnimalMaster = "api/v1/animal";
        public const string DeleteAnimalMaster = "api/v1/animal/{0}";

        // Animal
        public const string GetAnimalCosts = "api/v1/animalrequest?jobCode={0}";
        public const string GetAnimalCostsByAnimalType = "api/v1/animalrequest/byanimaltype";
        public const string GetAnimalLookup = "api/v1/animalrequest/lookup";
        public const string GetAnimalRate = "api/v1/animalrequest/rate?animalType={0}&jobCode={1}";
        public const string CreateAnimalCost = "api/v1/animalrequest";
        public const string UpdateAnimalCost = "api/v1/animalrequest";
        public const string DeleteAnimalCost = "api/v1/animalrequest?indCounter={0}";
        public const string GetTotalAnimalCost = "api/v1/animalrequest/totalanimalcost?jobCode={0}";
        public const string GetAnimalCostViewById = "api/v1/animalrequest/view?indCounter={0}&jobCode={1}";
        public const string GetAnimalSnapshot = "api/v1/animalrequest/snapshot";

        // Employee
        public const string GetFilteredEmployees = "api/v1/employee/paginated?filterOption={0}";
        public const string GetEmployeeById = "api/v1/employee/{0}";
        public const string CreateEmployee = "api/v1/employee";
        public const string UpdateEmployee = "api/v1/employee";
        public const string DeleteEmployee = "api/v1/employee/{0}";
        public const string GetAllManagers = "api/v1/employee/managers";
        public const string GetAllPactManagers = "api/v1/employee/pactmanagers";
        public const string GetAllPerson = "api/v1/employee/persons";
        public const string GetWorkGroupStaffPaginated = "api/v1/employee/WorkGroupStaff/paginated";
        public const string GetpactStaffs = "api/v1/employee/pactstaff";
        public const string GetPactWorkGroupStaff = "api/v1/employee/PactWorkGroupStaff?workGroup={0}";

        // Lookup
        public const string GetAllStatuses = "api/v1/status";
        public const string GetAllDiseases = "api/v1/disease";
        public const string GetAllCustomers = "api/v1/customer";
        public const string GetAllContracts = "api/v1/contract";
        public const string GetAllPactContracts = "api/v1/contract/pact";
        public const string GetContractsByUser = "api/v1/contract/by-user";

        // Division
        public const string GetAllDivisions = "api/v1/division";
        public const string GetPagedDivisions = "api/v1/division/paged";
        public const string GetDivisionByName = "api/v1/division/{0}";
        public const string CreateDivision = "api/v1/division";
        public const string UpdateDivision = "api/v1/division/{0}";
        public const string DeleteDivision = "api/v1/division/{0}";

        // Grade Maintenance (frmMaintGrade → api/v1/Grade) — added Phase 14
        public const string GetPagedGrades = "api/v1/Grade/paged";
        public const string GetGradeById = "api/v1/Grade/{0}";
        public const string CreateGrade = "api/v1/Grade";
        public const string UpdateGrade = "api/v1/Grade/{0}";
        public const string DeleteGrade = "api/v1/Grade/{0}";

        // Total Business Overheads (frmMaintTotalBusinessOverheads)
        public const string GetTotalBusinessOverheads = "api/v1/totalbusinessoverheads";
        public const string UpdateTotalBusinessOverheads = "api/v1/totalbusinessoverheads";

        // Division Grade
        public const string GetPagedDivisionGrades = "api/v1/DivisionGrade/paged";
        public const string GetDivisionGradeById = "api/v1/DivisionGrade/{0}";
        public const string CreateDivisionGrade = "api/v1/DivisionGrade";
        public const string UpdateDivisionGrade = "api/v1/DivisionGrade/{0}";
        public const string DeleteDivisionGrade = "api/v1/DivisionGrade/{0}";
        public const string GetAllDivisionGrades = "api/v1/DivisionGrade/grades";
        public const string GetAllDivisionGradeCodes = "api/v1/DivisionGrade/divisiongrades";

        // Program
        public const string GetAllPrograms = "api/v1/program";
        public const string GetAllProgramsForAllUsers = "api/v1/program/all";
        public const string GetPagedPrograms = "api/v1/program/paged";
        public const string GetProgramTimeSnapshot = "api/v1/program/time-snapshot/paged";
        public const string GetProgramById = "api/v1/program/{0}";
        public const string CreateProgram = "api/v1/program";
        public const string UpdateProgram = "api/v1/program";
        public const string DeleteProgram = "api/v1/program/{0}";

        // Project
        public const string GetAllProjects = "api/v1/project";
        public const string GetPagedProjects = "api/v1/project/paged";
        public const string GetPagedProjectsByUser = "api/v1/project/paged/by-user";
        public const string GetAllProjectsForAllUsers = "api/v1/project/all";
        public const string GetAllProjectsPaged = "api/v1/project/paged/all";
        public const string GetPagedProjectSpecificQuery = "api/v1/project/specific-query/paged";
        public const string GetPagedProjectSnapshotData = "api/v1/project/project-snapshot/paged";
        public const string GetProjectExceptionalCostsPaged = "api/v1/project/exceptionalcosts/paged";
        public const string GetAllPactProjects = "api/v1/project/pactview/all";
        public const string GetPagedPactProjects = "api/v1/project/pactview";
        public const string GetPagedPactProjectsByProgram = "api/v1/project/pactview/by-program?programNo={0}";
        public const string GetProjectById = "api/v1/project/{0}";
        public const string CreateProject = "api/v1/project";
        public const string UpdateProject = "api/v1/project";
        public const string UpdatePactProject = "api/v1/project/external/pact";
        public const string UpdatePactPortfolio = "api/v1/project/external/portfolio";
        public const string UpdateFpsPortfolio = "api/v1/project/external/fps-portfolio";
        public const string DeleteProject = "api/v1/project/{0}";
        public const string GetProjectsByProgram = "api/v1/project/paged?programNo={0}";
        public const string GetProjectsByProjectGroup = "api/v1/project/paged/by-project-group?projectGroup={0}";
        public const string GetProjectsByProgramVla = "api/v1/project/paged-vla?programNo={0}";
        public const string GetProjectsByProjectGroupVla = "api/v1/project/paged-vla/by-project-group?projectGroup={0}";
        public const string GetWorkgroupStaffReplan = "api/v1/project/{0}/staff-replan";

        // Project Group
        public const string GetAllProjectGroups = "api/v1/projectgroup";
        public const string GetProjectGroupsByUser = "api/v1/projectgroup/by-user";

        // ProgrammeNewProject (merged into project route)
        public const string GetProgrammeNewProjectById = "api/v1/project/{0}";
        public const string CreateProgrammeNewProject = "api/v1/project";
        public const string UpdateProgrammeNewProject = "api/v1/project/{0}";
        public const string DeleteProgrammeNewProjectAndChildren = "api/v1/project/{0}/delete-with-children";
        public const string ChangeProjectCode = "api/v1/project/change-code";
        public const string CheckProjectExists = "api/v1/project/check-exists/{0}";
        public const string GetProgrammeNewProjectManagers = "api/v1/employee/managers";
        public const string GetProgrammeNewProjectCostCentres = "api/v1/costcentre";
        public const string GetProgrammeNewProjectProjectGroups = "api/v1/projectgroup";
        public const string GetProgrammeNewProjectAccountCodes = "api/v1/accountcode";
        public const string GetProgrammeNewProjectSubAccounts = "api/v1/subaccount";

        // Setting
        public const string GetHoursPerDay = "api/v1/setting/hoursperday";
        public const string GetAllSettings = "api/v1/setting";
        public const string GetYearEndSettings = "api/v1/setting/yearend";
        public const string CreateSetting = "api/v1/setting";
        public const string UpdateSetting = "api/v1/setting/{0}";
        public const string SaveSetting = "api/v1/setting/save";

        // Month Hour
        public const string GetPagedMonthHours = "api/v1/monthhour";
        public const string GetMonthHoursByYear = "api/v1/monthhour/year/{0}";
        public const string GetDistinctMonthHourYears = "api/v1/monthhour/years";
        public const string GetYearEndMonthHours = "api/v1/monthhour/yearend";
        public const string SaveMonthHour = "api/v1/monthhour/save";

        // Project Staff Plan
        public const string GetPagedProjectStaffPlan = "api/v1/projectstaffplan";

        // Project Staff Plan Details (fps.vwprojectstaffplandetails)
        public const string GetPagedProjectStaffPlanDetails = "api/v1/projectstaffplandetails";

        // Project Group Staff Plan (fps.vpvtprojectgroupmgrplan)
        public const string GetPagedProjectGroupStaffPlan = "api/v1/projectgroupstaffplan";

        // Staff Job
        public const string GetAllStaffJobs = "api/v1/staffjob?jobCode={0}";
        public const string GetStaffWorkgroupLookup = "api/v1/staffjob/workgrouplookup";
        public const string GetStaffChargeRate = "api/v1/staffjob/chargerate?staffId={0}&jobcode={1}";
        public const string GetTotalStaffCost = "api/v1/staffjob/totalstaffcost?jobCode={0}";
        public const string GetStaffJobById = "api/v1/staffjob/{0}/{1}";
        public const string CreateStaffJob = "api/v1/staffjob";
        public const string UpdateStaffJob = "api/v1/staffjob";
        public const string DeleteStaffJob = "api/v1/staffjob?staffId={0}&jobcode={1}";
        public const string GetStaffJobViewById = "api/v1/staffjob/view?staffId={0}&jobcode={1}";
        public const string GetStaffSummaryById = "api/v1/staffjob/staffsummary?staffId={0}";
        public const string GetZtTotalHoursByStaffId = "api/v1/staffjob/zttotalhours?staffId={0}";
        public const string GetZtStaffJobsByStaffIdPaged = "api/v1/staffjob/ztstaffjobs/paged";
        public const string GetZtStaffJobDetailsById = "api/v1/staffjob/ztstaffjob/{0}/{1}";
        public const string GetStaffResourceUtilisation = "api/v1/staffjob/resourceutilisation";
        public const string GetStaffJobsAllocationByJobCodeWgGradePaged = "api/v1/staffjob/staffjobsallocation/paged";

        // View Project Plan vs Actual Staff
        public const string GetTimeCostCalcsByProject = "api/v1/timecostcalcs?projectCode={0}";
        public const string GetTimeCostCalcsTotalsByProject = "api/v1/timecostcalcs/totals?projectCode={0}";
        public const string DeleteTimeCostCalcs = "api/v1/timecostcalcs";

        // Additional Cost
        public const string GetAdditionalCosts = "api/v1/additionalcost?jobCode={0}";
        public const string GetTotalItemCost = "api/v1/additionalcost/totalitemcost?jobCode={0}";
        public const string GetAccountCategories = "api/v1/additionalcost/accountcategories";
        public const string GetAdditionalCostById = "api/v1/additionalcost/{0}/{1}/{2}";
        public const string CreateAdditionalCost = "api/v1/additionalcost";
        public const string UpdateAdditionalCost = "api/v1/additionalcost";
        public const string DeleteAdditionalCost = "api/v1/additionalcost?jobCode={0}&account={1}&description={2}";

        // Account Category Maintenance
        public const string GetFilteredAccountCategories = "api/v1/accountcategory?filterType={0}";
        public const string GetAccountCategoryById = "api/v1/accountcategory/{0}";
        public const string CreateAccountCategory = "api/v1/accountcategory";
        public const string UpdateAccountCategory = "api/v1/accountcategory/{0}";
        public const string DeleteAccountCategory = "api/v1/accountcategory/{0}";

        // View Project Plan vs Actual Tests
        public const string GetMonthlyOutputByProject = "api/v1/MonthlyOutput?projectCode={0}";
        public const string GetMonthlyOutputTotalsByProject = "api/v1/MonthlyOutput/totals?projectCode={0}";
        public const string DeleteMonthlyOutput = "api/v1/MonthlyOutput";

        // Resource Set-Up – Profit Centres
        public const string GetProfitCentres = "api/v1/profitcentres";
        public const string GetPagedProfitCentres = "api/v1/profitcentres/paged";
        public const string GetProfitCentreById = "api/v1/profitcentres/{0}";
        public const string CreateProfitCentre = "api/v1/profitcentres";
        public const string UpdateProfitCentre = "api/v1/profitcentres/{0}";
        public const string DeleteProfitCentre = "api/v1/profitcentres/{0}";                    
        public const string GetAllProfitCentres = "api/v1/profitcentres/all";        
        public const string PatchProfitCentreSettings = "api/v1/profitcentres/settings";
        public const string GetPagedProfitCenterCostSummary = "api/v1/profitcentres/paged/costsummary";
        public const string GetPagedWgStaffPlan = "api/v1/profitcentres/wgstaffplan";

        // Resource Set-Up — PC Grades
        public const string GetPcGrades = "api/v1/pcgrades?profitCentre={0}";
        public const string GetPagedPcGrades = "api/v1/pcgrades/paged";
        public const string GetPcGradeById = "api/v1/pcgrades/{0}";
        public const string CreatePcGrade = "api/v1/pcgrades";
        public const string UpdatePcGrade = "api/v1/pcgrades/{0}";
        public const string DeletePcGrade = "api/v1/pcgrades/{0}";
        public const string GetPcGradeDivisionGrades = "api/v1/pcgrades/divisiongrades";
        public const string GetPcGradeGradeCodes = "api/v1/pcgrades/gradecodes";
        public const string GetPcGradeProfitCentres = "api/v1/pcgrades/profitcentres";
        public const string GetAllPcGrades = "api/v1/pcgrades/allpcgrades";

        // Resource Set-Up — WG Grades
        public const string GetWgGrades = "api/v1/wggrades?pcGrade={0}";

        // Resource Set-Up — WG Staff
        public const string GetWgStaff = "api/v1/wgstaff?wgGrade={0}";
        public const string GetWgStaffForStaff = "api/v1/wgstaff/staff?wgGrade={0}";
        public const string GetActiveWgStaffForStaff = "api/v1/wgstaff/activestaff?wgGrade={0}";
        public const string GetWgEmployeeById = "api/v1/wgstaff/{0}";
        public const string UpdateWgEmployee = "api/v1/wgstaff";
        public const string CreateWgEmployeeForStaff = "api/v1/wgstaff/staff";
        public const string UpdateWgEmployeeForStaff = "api/v1/wgstaff/staff";
        public const string DeleteWgEmployee = "api/v1/wgstaff/{0}";
        public const string DeleteWgGrade = "api/v1/wggrades/{0}";

        // Project profitability (merged into project route)
        public const string GetProjectProfitability = "api/v1/project/profitability/{0}";
        public const string GetProjectGroupProfitability = "api/v1/project/profitability/by-project-group/{0}";
        public const string GetProjectProfitabilityVla = "api/v1/project/profitability-vla";

        // WorkgroupGrade Maintainence
        public const string GetPagedWorkgroupGrades = "api/v1/wggrades/paged";
        public const string GetWorkgroupGradeByCode = "api/v1/wggrades/{0}";
        public const string CreateWorkgroupGrade = "api/v1/wggrades";
        public const string UpdateWorkgroupGrade = "api/v1/wggrades/{0}";
        public const string DeleteWorkgroupGrade = "api/v1/wggrades/maintain/{0}";
        public const string GetAllGradeCodes = "api/v1/wggrades/gradecodes";
        public const string GetWgGradesByWorkGroup = "api/v1/wggrades/byworkgroup?workGroup={0}";

        // Generic Bid — Budget Bids
        public const string GetBids = "api/v1/budgetbids?workgroup={0}";
        public const string GetBidsPagedView = "api/v1/budgetbids/paged";
        public const string GetGenericBidsPaged = "api/v1/budgetbids/generic/paged";
        public const string GetBidByWorkgroupAccount = "api/v1/budgetbids/{0}/{1}";
        public const string CreateBudgetBid = "api/v1/budgetbids";
        public const string UpdateBudgetBid = "api/v1/budgetbids";
        public const string DeleteBudgetBid = "api/v1/budgetbids?WorkGroupName={0}&account={1}";
        public const string GetBudgetBidsAccounts = "api/v1/budgetbids/accounts";

        // Misc Reports — Tests Required By Work Group (Test Manager WG Pivot)
        public const string GetTestsRequiredByWg = "api/v1/testsrequiredbywg?profitCentre={0}";
        public const string GetTestsRequiredByRc = "api/v1/testsrequiredbyrc?profitCentre={0}";

        // Income/Contribution from Time Sales (frmTimeSellerPC)
        public const string GetContributionSummaryRows   = "api/v1/timeseller/{0}/rows";
        public const string GetContributionSummaryTotals = "api/v1/timeseller/{0}/totals";
        // Project Audit Trail (frmProjectChangesLog → api/v1/projectaudittrail)
        public const string GetProjectLogs = "api/v1/projectaudittrail/projectlogs";
        public const string GetStaffJobLogs = "api/v1/projectaudittrail/staffjoblogs";
        public const string GetTestRequirementLogs = "api/v1/projectaudittrail/testrequirementlogs";
        public const string GetAnimalRequestLogs = "api/v1/projectaudittrail/animalrequestlogs";
        public const string GetAdditionalCostLogs = "api/v1/projectaudittrail/additionalcostlogs";

        // Generic Bid — Purchases
        public const string GetGenericPurchases = "api/v1/purchases?WorkGroupName={0}&account={1}";
        public const string GetGenericPurchasesPaged = "api/v1/purchases/paged";
        public const string GetPurchaseByKeys = "api/v1/purchases/{0}/{1}/{2}";
        public const string CreateGenericPurchase = "api/v1/purchases";
        public const string UpdateGenericPurchase = "api/v1/purchases";
        public const string DeleteGenericPurchase = "api/v1/purchases?WorkGroupName={0}&account={1}&itemDescription={2}";

        // User Permission
        public const string GetAllUsers = "api/v1/user/users";
        public const string GetPagedUsers = "api/v1/user/users/paged";
        public const string GetPagedNonSuperUsers = "api/v1/user/users/paged/nonsuperusers";
        public const string GetUserById = "api/v1/user/users/{0}";
        public const string CreateUser = "api/v1/user/users";
        public const string UpdateUser = "api/v1/user/users";
        public const string DeleteUser = "api/v1/user/users/{0}";
        public const string GetUserPermissions = "api/v1/user/{0}/permissions";
        public const string SaveUserPermissions = "api/v1/user/{0}/permissions";
        public const string GetPermissionOptions = "api/v1/user/options";

        // TRANSFORMENGINE: WorkgroupMaintenance (frmMaintWorkGroup2 — fps.workgroup) — added Phase 6
        // CRUD endpoints
        public const string GetPagedWorkgroups = "api/v1/workgroup/paged";
        public const string GetWorkgroupByName = "api/v1/workgroup/{0}";
        public const string CreateWorkgroup = "api/v1/workgroup";
        public const string UpdateWorkgroup = "api/v1/workgroup/{0}";
        public const string DeleteWorkgroup = "api/v1/workgroup/{0}";
        // Lookup endpoints (separate from CRUD resource family)
        public const string GetWorkgroupProfitCentres = "api/v1/workgroup/profitcentres";
        public const string GetWorkgroupOwners = "api/v1/workgroup/owners";
        public const string GetWorkgroupCostCentres = "api/v1/workgroup/costcentres?profitCentre={0}";
        // Year End Batch Job
        public const string GetYearEndBatchJobHistory = "api/v1/yearend/batchjob/history";
        public const string GetCanInitiateDataSetupRequest = "api/v1/yearend/dataSetup/caninitiate";
        public const string GetCanApproveDataSetupRequest = "api/v1/yearend/dataSetup/canapproveorreject";
        public const string EnqueueYearEndDataSetupInitiationJob = "api/v1/yearend/dataSetup/initiation";
        public const string EnqueueYearEndDataSetupApprovalJob    = "api/v1/yearend/dataSetup/approval";
        public const string EnqueueYearEndDataSetupRejectJob = "api/v1/yearend/dataSetup/reject";

        // Cost Centre Maintenance (frmMaintCostCentres → api/v1/costcentre) — added Phase 7
        public const string GetAllCostCentres = "api/v1/costcentre";
        public const string GetPagedCostCentres = "api/v1/costcentre/paged";
        public const string GetCostCentreById = "api/v1/costcentre/{0}";
        public const string CreateCostCentre = "api/v1/costcentre";
        public const string UpdateCostCentre = "api/v1/costcentre/{0}";
        public const string DeleteCostCentre = "api/v1/costcentre/{0}";

        // ResourceAllocation — Stage 2 Check Resource Allocation
        public const string GetPagedResourceStaffAllocations = "api/v1/ResourceAllocation/staffallocations/paged";
        public const string GetPagedResourceStaffJobDetails = "api/v1/ResourceAllocation/staffjobdetails/paged";

        // ResourceMgmtReplan — Resource Re-allocation Screen (frmRM_RePlan)
        public const string GetResourceMgmtReplanGrid = "api/v1/resourcemgmtreplan/grid";
        public const string GetResourceMgmtReplanStaffJobs = "api/v1/resourcemgmtreplan/staffjobs";
        public const string GetResourceMgmtReplanStaged = "api/v1/resourcemgmtreplan/staged";
        public const string CommitResourceMgmtReplan = "api/v1/resourcemgmtreplan/commit";
    }
}
