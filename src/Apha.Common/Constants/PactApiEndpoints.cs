namespace Apha.Common.Constants
{
    public static class PactApiEndpoints
    {
        // Job Code
        public const string GetAllJobCodes = "api/v1/jobcode";
        public const string GetZtJobCodes = "api/v1/jobcode/zt";
        public const string GetJobCodesByProject = "api/v1/jobcode/project?parentProject={0}";
        public const string GetPagedJobCodes = "api/v1/jobcode/paged";
        public const string GetPagedJobCodesByProject = "api/v1/jobcode/paged?parentProject={0}";
        public const string GetJobCodeById = "api/v1/jobcode/jobCodeId?jobCodeId={0}";
        public const string GetJobCodeTypes = "api/v1/jobcode/types";
        public const string CreateJobCode = "api/v1/jobcode";
        public const string UpdateJobCode = "api/v1/jobcode";
        public const string DeleteJobCode = "api/v1/jobcode/jobCodeId?jobCodeId={0}";

        // Time Code Valid
        public const string GetTimeCodesByJobCode = "api/v1/timecodevalid/jobcode?jobCode={0}&parentProject={1}";
        public const string GetTimeCodeValidById = "api/v1/timecodevalid/wgtimecodeprojectcode/?workGroup={0}&timeCode={1}&parentProject={2}";
        public const string GetPagedTimeCodes = "api/v1/timecodevalid/paged";
        public const string GetPagedTimeCodesByJobCode = "api/v1/timecodevalid/paged?jobCode={0}";
        public const string GetPagedTimeCodesByProject = "api/v1/timecodevalid/paged?parentProject={0}";
        public const string GetPagedTimeCodesByJobCodeAndProject = "api/v1/timecodevalid/paged?jobCode={0}&parentProject={1}";
        public const string GetPagedTimeCodesByProjectAndTestCode = "api/v1/timecodevalid/paged/byprojectandtest?parentProject={0}&testCode={1}";
        public const string CreateTimeCodeValid = "api/v1/timecodevalid";
        public const string UpdateTimeCodeValid = "api/v1/timecodevalid";
        public const string DeleteTimeCodeValid = "api/v1/timecodevalid/delete?workGroup={0}&timeCode={1}&parentProject={2}";
        public const string DeleteTimeCodesByJobCode = "api/v1/timecodevalid/deletebyjobcode?jobCode={0}&parentProject={1}";
        public const string CopyWorkGroup = "api/v1/timecodevalid/copy?sourceJobCode={0}&targetJobCode={1}&parentProject={2}";
        public const string DeleteBulkTimeCodes = "api/v1/timecodevalid/deletebulk";
        public const string CopySelectedWorkGroups = "api/v1/timecodevalid/copybulkworkgroups";

        // Work Group
        public const string GetAllWorkGroups = "api/v1/workgroup";
        public const string GetAllWorkGroupNames = "api/v1/workgroup/names";
        public const string GetPagedWorkGroupTimeCodes = "api/v1/workgroup/paged/timecodes";
        public const string GetPagedWorkGroupValidTimeCodes = "api/v1/workgroup/paged/validtimecodes";
        public const string GetWgSummarisedStaffTimeUsage = "api/v1/workgroup/staff/paged/summarisedtimeusage";
        public const string GetPagedSummarisedWorkgroupTime = "api/v1/workgroup/paged/summarisedtimeusage";        
        public const string GetPagedWorkGroupsByProfitCentre = "api/v1/workgroup/profitcentre";
        public const string GetWorkGroupsByProfitCentreForBudget = "api/v1/workgroup/budget/by-profitcentre";
        public const string GetWorkGroupsByProfitCentreForBudgetPaged = "api/v1/workgroup/budget/by-profitcentre/paged";
        public const string SetSendEmailForProfitCentreWorkGroups = "api/v1/workgroup/setsendemail/profitcentre";
        public const string SetSendEmailForAllWorkGroups = "api/v1/workgroup/setsendemail/all";
        public const string UpdateWorkGroupEmail = "api/v1/workgroup/email?workGroupName={0}";        

        // Work Group Maintenance (CRUD + lookups)
        public const string GetPagedWorkGroupMaintenance = "api/v1/workgroup/paged";
        public const string GetWorkGroupMaintenanceByName = "api/v1/workgroup/maintenance/{0}";
        public const string CreateWorkGroupMaintenance = "api/v1/workgroup/maintenance";
        public const string UpdateWorkGroupMaintenance = "api/v1/workgroup/maintenance/{0}";
        public const string DeleteWorkGroupMaintenance = "api/v1/workgroup/maintenance/{0}";
        public const string GetWorkGroupProfitCentres = "api/v1/workgroup/profitcentres";
        public const string GetWorkGroupOwners = "api/v1/workgroup/owners";
        public const string GetWorkGroupCostCentres = "api/v1/workgroup/costcentres";

        // Month
        public const string GetAllMonths = "api/v1/months";

        // Calender Month
        public const string GetCalenderMonths = "api/v1/calendermonth";

        // Test List
        public const string GetPagedTestOrProducts = "api/v1/testorproduct/paged";
        public const string GetTestOrProductById = "api/v1/testorproduct/itemCode?itemCode={0}";
        public const string CreateTestOrProduct = "api/v1/testorproduct";
        public const string UpdateTestOrProduct = "api/v1/testorproduct/itemCode?itemCode={0}";
        public const string DeleteTestOrProduct = "api/v1/testorproduct/itemCode?itemCode={0}";
        public const string GetTestListOwners = "api/v1/testorproduct/owners";
        public const string GetAllTestorProducts = "api/v1/testorproduct";
        public const string GetTestPriceCheckPaged  = "api/v1/testorproduct/testpricecheck";
        public const string GetTestPriceCheckByKey    = "api/v1/testorproduct/testpricechecktestCodejobCode?testCode={0}&jobCode={1}";
        public const string UpdateTestPriceCheckByKey  = "api/v1/testorproduct/testpricecheck?testCode={0}&jobCode={1}";

        // Recreate Summaries Log
        public const string GetRecreateSummaryLog = "api/v1/recreatereleasesummary/recreatesummary/log";

        // Batch Job
        public const string GetRecreateSummaryBatchJobHistory = "api/v1/recreatesummary/batchjob/history";
        public const string CanRunRecreateSummaryBatchJob = "api/v1/recreatesummary/batchjob/canrun";
        public const string TriggerRecreateSummariesBatchJob = "api/v1/recreatesummary/trigger";

        // Release Summaries
        public const string GetReleaseSummaries = "api/v1/recreatereleasesummary/releasesummary";
        public const string GetReleasePeriods = "api/v1/recreatereleasesummary/releaseperiods";
        public const string SetFinalSummaryRun = "api/v1/recreatereleasesummary/releasesummary/finalrun";

        // Project Invoice
        public const string GetPagedProjectInvoices = "api/v1/projectinvoice?parentProject={0}";
        public const string GetPagedProjectInvoiceManual = "api/v1/projectinvoice";
        public const string GetProjectInvoiceTotalAmount = "api/v1/projectinvoice/total";
        public const string GetProjectInvoiceById = "api/v1/projectinvoice/invoice/id?id={0}";
        public const string CreateProjectInvoice = "api/v1/projectinvoice";
        public const string UpdateProjectInvoice = "api/v1/projectinvoice/invoice/id?id={0}";
        public const string DeleteProjectInvoice = "api/v1/projectinvoice/invoice/id?id={0}";
        public const string GetMonthlyInvoicesSummary = "api/v1/projectinvoice/monthly-summary";
        
        // Project SubContract
        public const string GetPagedProjectSubContracts = "api/v1/projectsubcontract";
        public const string GetProjectSubContractTotalAmount = "api/v1/projectsubcontract/total";
        public const string GetProjectSubContractById = "api/v1/projectsubcontract/subcontract/id?id={0}";
        public const string CreateProjectSubContract = "api/v1/projectsubcontract";
        public const string UpdateProjectSubContract = "api/v1/projectsubcontract/subcontract/id?id={0}";
        public const string DeleteProjectSubContract = "api/v1/projectsubcontract/subcontract/id?id={0}";
        public const string GetFpsProjectSubContracts = "api/v1/projectsubcontract/animals";
        public const string GetFpsProjectSubContractTotalAmount = "api/v1/projectsubcontract/animals/total";
        public const string GetMonthlySubContractsSummary = "api/v1/projectsubcontract/monthly-summary";
        public const string GetFailedProjectSubContractRms = "api/v1/projectsubcontract/rms/failed";
        public const string GetFailedProjectSubContractRmsById = "api/v1/projectsubcontract/rms/failed/{id}";
        public const string SaveFailedProjectSubContractRms = "api/v1/projectsubcontract/rms/failed/{id}";
        public const string DeleteFailedProjectSubContractRmsById = "api/v1/projectsubcontract/rms/failed/{id}";
        public const string DeleteFailedProjectSubContractRmsByUser = "api/v1/projectsubcontract/rms/failed/user";
        public const string ImportProjectSubContractRms = "api/v1/projectsubcontract/rms/import";

        // WorkGroup Test Capability
        public const string GetPagedTestCapabilityByWorkGroup = "api/v1/testcapability/paged/workgroup";
        public const string GetPagedTestCapabilityByTestCode = "api/v1/testcapability/paged/testcode";
        public const string GetPagedTestCapabilityByPortfolio = "api/v1/testcapability/paged/portfolio";
        public const string GetTestCapabilityById = "api/v1/testcapability/testcapability?testCode={0}&workGroup={1}";
        public const string CreateTestCapability = "api/v1/testcapability/testcapability";

        // Plan CrossTab
        public const string RebuildTestPlanCrossTab = "api/v1/testcapability/plantestcrosstab/rebuild";
        public const string GetPagedTestPlanCrossTab = "api/v1/testcapability/paged/plantestcrosstab";
        public const string UpdateTestCapability = "api/v1/testcapability/testcapability";
        public const string DeleteTestCapability = "api/v1/testcapability/testcapability?testCode={0}&workGroup={1}";
        public const string GetPagedWgTestCapabilitiesWithDescription = "api/v1/testcapability/paged/wg-test-capabilities";

        // Test Requirement Breakdown
        public const string GetPagedTestReqBreakdown = "api/v1/testrequirement/testreqbreakdown";

        // Test Actual Breakdown
        public const string GetPagedTestActualBreakdown = "api/v1/testrequirement/getactualstestswithplanneddatabyworkgroup";

        // Test Requirement
        public const string GetPagedTestReqmt = "api/v1/testrequirement/testcode/paged?testCode={0}";
        public const string GetPagedBySupplierTestCode = "api/v1/testrequirement/supplier/paged?testCode={0}";
        public const string GetPagedTestReqmtbyProject = "api/v1/testrequirement/pagedbyproject?parentProject={0}";
        public const string GetAllTestReqmtForExport = "api/v1/testrequirement/export?testCode={0}";
        public const string GetTestReqmtById = "api/v1/testrequirement/testcodebuyer?testCode={0}&buyer={1}";
        public const string CreateTestReqmt = "api/v1/testrequirement";
        public const string UpdateTestReqmt = "api/v1/testrequirement";
        public const string DeleteTestReqmt = "api/v1/testrequirement/testcodebuyer?testCode={0}&buyer={1}";

        // Lookups
        //public const string GetAllTestorProducts = "api/v1/testcapability/testorproducts";
        public const string GetTestReqmtPricing = "api/v1/testrequirement/pricing";

        // Project Month (Cost Profile Grid)
        public const string GetProjectMonthsByProject = "api/v1/projectmonth/project?project={0}";
        public const string GetProjectMonthById = "api/v1/projectmonth/project/month?project={0}&monthNo={1}";
        public const string CreateProjectMonth = "api/v1/projectmonth";
        public const string UpdateProjectMonth = "api/v1/projectmonth";
        public const string DeleteProjectMonth = "api/v1/projectmonth/project/month?project={0}&monthNo={1}";

        // Project Profile
        public const string GetProjectProfile = "api/v1/projectprofile/project/data?project={0}";
        public const string GetProjectProfileCumulative = "api/v1/projectprofile/project/data/cumulative?project={0}";

        // Monthly Output Log
        public const string SearchMonthlyOutputLog = "api/v1/monthlyoutput/log/search";

        // Monthly Time Log (MT_LOG)
        public const string SearchMonthlyTimeLog = "api/v1/monthlytime/log/search";

        // Work Group Report Email
        public const string SendEmails = "api/v1/workgroupreport/send";

        // Bosworth Interface
        public const string GetTimePurchaseProject = "api/v1/bosworth-interface/time-purchase-project";
        public const string GetTimeSaleProfitCentre = "api/v1/bosworth-interface/time-sale-profit-centre";
        public const string GetTestSaleSellingWorkgroup = "api/v1/bosworth-interface/test-sale-selling-workgroup";
        public const string GetTestSaleBuyingProject = "api/v1/bosworth-interface/test-sale-buying-project";
    }
}
