namespace Apha.Common.Constants
{
    public static class CostBookApiEndpoints
    {
        // Projects
        public const string GetFilteredProjects = "api/v1/projects/paginated";
        public const string GetProjectById = "api/v1/projects/{0}";
        public const string AddProject = "api/v1/projects";
        public const string UpdateProject = "api/v1/projects/{0}";
        public const string DeleteProject = "api/v1/projects/{0}/delete";
        public const string CopyProject = "api/v1/projects/{0}/copy";
        public const string RecostProject = "api/v1/projects/{0}/recost";
        public const string GetNextProjectNumber = "api/v1/projects/number";
        public const string GetAllCustomers = "api/v1/projects/customers";
        public const string GetAllDiseases = "api/v1/projects/diseases";
        public const string GetAllPrograms = "api/v1/projects/programs";
        public const string GetAllStaff = "api/v1/projects/staff";
        public const string GetAllContracts = "api/v1/projects/contracts";

        // Yearly Details – Project Header & Years
        public const string GetProjectHeader = "api/v1/yearlydetails/{0}/header";
        public const string GetProjectYears = "api/v1/yearlydetails/{0}/years";
        public const string AddProjectYear = "api/v1/yearlydetails/{0}/years";
        public const string UpdateProjectYear = "api/v1/yearlydetails/{0}/years/{1}";
        public const string DeleteProjectYear = "api/v1/yearlydetails/{0}/years/{1}";

        // Yearly Details – Staff
        public const string GetStaffRequirements = "api/v1/yearlydetails/{0}/years/{1}/staff";
        public const string AddStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff";
        public const string UpdateStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff/{2}";
        public const string DeleteStaffRequirement = "api/v1/yearlydetails/{0}/years/{1}/staff/{2}";

        // Yearly Details – Tests
        public const string GetTestRequirements = "api/v1/yearlydetails/{0}/years/{1}/tests";
        public const string AddTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests";
        public const string UpdateTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests/{2}";
        public const string DeleteTestRequirement = "api/v1/yearlydetails/{0}/years/{1}/tests/{2}";

        // Yearly Details – Animals
        public const string GetAnimalRequirements = "api/v1/yearlydetails/{0}/years/{1}/animals";
        public const string AddAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals";
        public const string UpdateAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals/{2}";
        public const string DeleteAnimalRequirement = "api/v1/yearlydetails/{0}/years/{1}/animals/{2}";

        // Yearly Details – Additional Costs
        public const string GetAdditionalCosts = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts";
        public const string AddAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts";
        public const string UpdateAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}";
        public const string DeleteAdditionalCost = "api/v1/yearlydetails/{0}/years/{1}/additionalcosts/{2}";

        // Yearly Details – Lookups
        public const string GetPayRates = "api/v1/yearlydetails/lookups/payrates";
        public const string GetAnimalRates = "api/v1/yearlydetails/lookups/animalrates";
        public const string GetAccountCategories = "api/v1/yearlydetails/lookups/accountcategories";
        public const string GetTestCodeLookups = "api/v1/yearlydetails/lookups/testcodes";
        public const string GetAllAnimals = "api/v1/yearlydetails/lookups/animals";

        // Project Summary
        public const string GetProfitIncludedTotal = "api/v1/projectsummary/{0}/years/{1}/profittotal";
        public const string GetStaffYearsPivot = "api/v1/projectsummary/{0}/staff-years";
        public const string GetStaffEffortPivot = "api/v1/projectsummary/{0}/staff-effort";
        public const string GetProjectCostsPivot = "api/v1/projectsummary/{0}/project-costs";
        public const string ExportProjectSummaryToExcel = "api/v1/projectsummary/{0}/export-excel";
        public const string GetAllCostTotal = "api/v1/projectsummary/{0}/years/{1}/costsummary";

        // Settings
        public const string GetSettingValueById = "api/v1/settings/getvaluebyid";
        public const string GetAdditionalCostinflamation = "api/v1/yearlydetails/additionalcostinflamation";
        // Maintenance – Settings (Tabs 1 + 4)
        public const string GetMaintenanceSettings = "api/v1/maintenance/settings";
        public const string UpdateMaintenanceSettings = "api/v1/maintenance/settings";

        // Maintenance – Account Categories (Tab 2)
        public const string GetMaintenanceAccountCategories = "api/v1/maintenance/account-categories";
        public const string GetPaginatedMaintenanceAccountCategories = "api/v1/maintenance/account-categories/paginated";
        public const string UpdateMaintenanceAccountCategory = "api/v1/maintenance/account-categories/{0}";

        // Maintenance – CapsStaff (Tab 5)
        public const string GetAllCapsStaff = "api/v1/capsstaff";
        public const string GetPaginatedCapsStaff = "api/v1/capsstaff/paginated";
        public const string GetCapsStaffByMNumber = "api/v1/capsstaff/{0}";
        public const string AddCapsStaff = "api/v1/capsstaff";
        public const string UpdateCapsStaff = "api/v1/capsstaff/{0}";
        public const string DeleteCapsStaff = "api/v1/capsstaff/{0}";

        // Maintenance – Account Groups / CSG7 (Tab 3)
        public const string GetAllAccountGroups = "api/v1/accountgroup";
        public const string GetPaginatedAccountGroups = "api/v1/accountgroup/paginated";
        public const string GetAccountGroupByCsg7 = "api/v1/accountgroup/{0}";
        public const string AddAccountGroup = "api/v1/accountgroup";
        public const string UpdateAccountGroup = "api/v1/accountgroup/{0}";
        public const string DeleteAccountGroup = "api/v1/accountgroup/{0}";

    }
}