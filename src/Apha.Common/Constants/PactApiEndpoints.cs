namespace Apha.Common.Constants
{
    public static class PactApiEndpoints
    {
        // Job Code
        public const string GetJobCodesByProject = "api/v1/jobcode/project/{0}";
        public const string GetPagedJobCodes = "api/v1/jobcode/paged";
        public const string GetPagedJobCodesByProject = "api/v1/jobcode/paged?parentProject={0}";
        public const string GetJobCodeById = "api/v1/jobcode/{0}";
        public const string GetJobCodeTypes = "api/v1/jobcode/types";
        public const string CreateJobCode = "api/v1/jobcode";
        public const string UpdateJobCode = "api/v1/jobcode";
        public const string DeleteJobCode = "api/v1/jobcode/{0}";

        // Time Code Valid
        public const string GetTimeCodesByJobCode = "api/v1/timecodevalid/jobcode/{0}/project/{1}";
        public const string GetTimeCodeValidById = "api/v1/timecodevalid/{0}/{1}/{2}";
        public const string GetPagedTimeCodes = "api/v1/timecodevalid/paged";
        public const string GetPagedTimeCodesByJobCode = "api/v1/timecodevalid/paged?jobCode={0}";
        public const string GetPagedTimeCodesByProject = "api/v1/timecodevalid/paged?parentProject={0}";
        public const string GetPagedTimeCodesByJobCodeAndProject = "api/v1/timecodevalid/paged?jobCode={0}&parentProject={1}";
        public const string GetPagedTimeCodesByProjectAndTestCode = "api/v1/timecodevalid/paged/project/{0}/testcode/{1}";
        public const string CreateTimeCodeValid = "api/v1/timecodevalid";
        public const string UpdateTimeCodeValid = "api/v1/timecodevalid";
        public const string DeleteTimeCodeValid = "api/v1/timecodevalid/{0}/{1}/{2}";
        public const string DeleteTimeCodesByJobCode = "api/v1/timecodevalid/jobcode/{0}/project/{1}";
        public const string CopyWorkGroup = "api/v1/timecodevalid/copy?sourceJobCode={0}&targetJobCode={1}&parentProject={2}";
        public const string DeleteBulkTimeCodes = "api/v1/timecodevalid/deletebulk";
        public const string CopySelectedWorkGroups = "api/v1/timecodevalid/copybulkworkgroups";

        // Work Group
        public const string GetAllWorkGroups = "api/v1/workgroup";
        public const string GetPagedWorkGroupTimeCodes = "api/v1/workgroup/paged/timecodes";
        public const string GetPagedWorkGroupValidTimeCodes = "api/v1/workgroup/paged/validtimecodes";

        // Month
        public const string GetAllMonths = "api/v1/months";

        // Calender Month
        public const string GetCalenderMonths = "api/v1/calendermonth";

        // Test List
        public const string GetPagedTestOrProducts = "api/v1/testorproduct/paged";
        public const string GetTestOrProductById = "api/v1/testorproduct/{0}";
        public const string CreateTestOrProduct = "api/v1/testorproduct";
        public const string UpdateTestOrProduct = "api/v1/testorproduct/{0}";
        public const string DeleteTestOrProduct = "api/v1/testorproduct/{0}";
        public const string GetTestListOwners = "api/v1/testorproduct/owners";
        public const string GetAllTestorProducts = "api/v1/testorproduct";

        // Project Invoice
        public const string GetPagedProjectInvoices = "api/v1/projectinvoice?parentProject={0}";
        public const string GetPagedProjectInvoiceManual = "api/v1/projectinvoice";
        public const string GetProjectInvoiceTotalAmount = "api/v1/projectinvoice/total";
        public const string GetProjectInvoiceById = "api/v1/projectinvoice/{0}";
        public const string CreateProjectInvoice = "api/v1/projectinvoice";
        public const string UpdateProjectInvoice = "api/v1/projectinvoice/{0}";
        public const string DeleteProjectInvoice = "api/v1/projectinvoice/{0}";
        public const string GetMonthlyInvoicesSummary = "api/v1/projectinvoice/monthly-summary";
        
        // Project SubContract
        public const string GetPagedProjectSubContracts = "api/v1/projectsubcontract";
        public const string GetProjectSubContractTotalAmount = "api/v1/projectsubcontract/total";
        public const string GetProjectSubContractById = "api/v1/projectsubcontract/{0}";
        public const string CreateProjectSubContract = "api/v1/projectsubcontract";
        public const string UpdateProjectSubContract = "api/v1/projectsubcontract/{0}";
        public const string DeleteProjectSubContract = "api/v1/projectsubcontract/{0}";
        public const string GetFpsProjectSubContracts = "api/v1/projectsubcontract/animals";
        public const string GetFpsProjectSubContractTotalAmount = "api/v1/projectsubcontract/animals/total";
        public const string GetMonthlySubContractsSummary = "api/v1/projectsubcontract/monthly-summary";

        // WorkGroup Test Capability
        public const string GetPagedTestCapabilityByWorkGroup = "api/v1/testcapability/paged/workgroup";
        public const string GetPagedTestCapabilityByTestCode = "api/v1/testcapability/paged/testcode";
        public const string GetPagedTestCapabilityByPortfolio = "api/v1/testcapability/paged/portfolio";
        public const string GetTestCapabilityById = "api/v1/testcapability/testcapability/{0}/{1}";
        public const string CreateTestCapability = "api/v1/testcapability/testcapability";
        public const string UpdateTestCapability = "api/v1/testcapability/testcapability";
        public const string DeleteTestCapability = "api/v1/testcapability/testcapability/{0}/{1}";

        // Test Requirement
        public const string GetPagedTestReqmt = "api/v1/testrequirement/paged/{0}";
        public const string GetPagedTestReqmtbyProject = "api/v1/testrequirement/pagedbyproject/{0}";
        public const string GetAllTestReqmtForExport = "api/v1/testrequirement/all/{0}";
        public const string GetTestReqmtById = "api/v1/testrequirement/{0}/{1}";
        public const string CreateTestReqmt = "api/v1/testrequirement";
        public const string UpdateTestReqmt = "api/v1/testrequirement";
        public const string DeleteTestReqmt = "api/v1/testrequirement/{0}/{1}";

        // Lookups
        //public const string GetAllTestorProducts = "api/v1/testcapability/testorproducts";
        public const string GetTestReqmtPricing = "api/v1/testrequirement/pricing";

        // Project Month (Cost Profile Grid)
        public const string GetProjectMonthsByProject = "api/v1/projectmonth/project/{0}";
        public const string GetProjectMonthById = "api/v1/projectmonth/project/{0}/month/{1}";
        public const string CreateProjectMonth = "api/v1/projectmonth";
        public const string UpdateProjectMonth = "api/v1/projectmonth";
        public const string DeleteProjectMonth = "api/v1/projectmonth/project/{0}/month/{1}";

        // Project Profile
        public const string GetProjectProfile = "api/v1/projectprofile/{0}/data";
        public const string GetProjectProfileCumulative = "api/v1/projectprofile/{0}/data/cumulative";

        // Monthly Output Log
        public const string SearchMonthlyOutputLog = "api/v1/monthlyoutput/log/search";

        // Monthly Time
        public const string GetMonthlyTimeByTimeCodeAndProject = "api/v1/monthlytime/timecode/{0}/workgroup/{1}/project/{2}";
        public const string GetPagedMonthlyTime = "api/v1/monthlytime/paged";
        public const string GetMonthlyTimeById = "api/v1/monthlytime/{0}/{1}/{2}/{3}";
        public const string CreateMonthlyTime = "api/v1/monthlytime";
        public const string UpdateMonthlyTime = "api/v1/monthlytime";
        public const string DeleteMonthlyTime = "api/v1/monthlytime/{0}/{1}/{2}/{3}";
    }
}
