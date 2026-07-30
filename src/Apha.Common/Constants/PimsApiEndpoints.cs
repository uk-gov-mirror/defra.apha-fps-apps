namespace Apha.Common.Constants
{
    public static class PimsApiEndpoints
    {
        // Project List
        public const string GetAllProjects = "api/v1/projectlist";
        public const string GetAllProjectsList = "api/v1/projectlist/AllProjectsList";
        public const string GetAllProjectsMilestone = "api/v1/projectlist/AllProjectsMilestone";
        public const string GetFpsProjectById = "api/v1/projectlist/{0}/fps";
        public const string GetProposedProjectById = "api/v1/projectlist/{0}/proposed";
        public const string GetYearlyDetailsByProject = "api/v1/projectlist/{0}/yearly";

        // Proposed Project
        public const string CreateProject = "api/v1/proposedproject";
        public const string GetProjectStatuses = "api/v1/proposedproject/statuses";
        public const string GetProjectPrograms = "api/v1/proposedproject/programs";
        public const string GetProjectCustomers = "api/v1/proposedproject/customers";

        // Project Details
        public const string GetAllRisks = "api/v1/projectdetails/risks";
        public const string GetAllYears = "api/v1/projectdetails/years";
        public const string GetPimsDetail = "api/v1/projectdetails/{0}/pims";
        public const string SavePimsDetail = "api/v1/projectdetails/{0}/pims";
        public const string GetProposedProject = "api/v1/projectdetails/{0}/proposed";
        public const string UpdateProposedProject = "api/v1/projectdetails/{0}/proposed";
        public const string GetFpsProjectByProjectDetails = "api/v1/projectdetails/{0}/fps";

        // Project Comment
        public const string GetCommentsByProject = "api/v1/projectcomment";
        public const string GetCommentById = "api/v1/projectcomment/{0}";
        public const string CreateComment = "api/v1/projectcomment";
        public const string UpdateComment = "api/v1/projectcomment/{0}";
        public const string DeleteComment = "api/v1/projectcomment/{0}";
        public const string GetCommentTopics = "api/v1/projectcomment/commenttopics";

        // Project Year Costs
        public const string GetAdditionalActuals = "api/v1/projectyearcosts/{0}/{1}/additionalactuals";
        public const string GetAdditionalPlans = "api/v1/projectyearcosts/{0}/{1}/additionalplans";
        public const string GetAnimalActuals = "api/v1/projectyearcosts/{0}/{1}/animalactuals";
        public const string GetAnimalPlans = "api/v1/projectyearcosts/{0}/{1}/animalplans";
        public const string GetTestActuals = "api/v1/projectyearcosts/{0}/{1}/testactuals";
        public const string GetTestPlans = "api/v1/projectyearcosts/{0}/{1}/testplans";
        public const string GetStaffPlans = "api/v1/projectyearcosts/{0}/{1}/staffplans";
        public const string GetStaffActuals = "api/v1/projectyearcosts/{0}/{1}/staffactuals";
        public const string GetProjectYearDetails = "api/v1/projectyearcosts/{0}/{1}/projectyeardetails";
        public const string GetPactPay = "api/v1/projectyearcosts/{0}/{1}/pactpay";
        public const string GetMonthlyPactData = "api/v1/projectyearcosts/{0}/{1}/monthlypactdata";
        public const string GetFpsYearTotals = "api/v1/projectyearcosts/{0}/{1}/fpsyeartotals";
        public const string ExportProjectYearCostsToExcel = "api/v1/projectyearcosts/{0}/{1}/export-excel";

        // Milestones
        public const string GetAllMilestones = "api/v1/milestone";
        public const string GetMilestone = "api/v1/milestone/{0}/milestones/{1}";
        public const string SaveMilestone = "api/v1/milestone/{0}/milestones";
        public const string UpdateMilestone = "api/v1/milestone/{0}/milestones/{1}";
        public const string DeleteMilestone = "api/v1/milestone/{0}/milestones/{1}";
        public const string UpdateFormRequired = "api/v1/milestone/{0}/formrequired";
        public const string GetAllProjectsForMilestone = "api/v1/projectlist/AllProjectsMilestone";
        public const string GetProjectsDetailsForMilestone = "api/v1/projectlist/ProjectDetailsMilestone/{0}";        

        // Milestone Types
        public const string GetMilestoneTypes = "api/v1/milestone/milestonetypes";

        // Milestone Form Dates
        public const string GetAllMilestoneFormDates = "api/v1/milestone/{0}/formdates";
        public const string GetMilestoneFormDates = "api/v1/milestone/{0}/formdates/{1}";
        public const string SaveMilestoneFormDates = "api/v1/milestone/{0}/formdates";
        public const string DeleteMilestoneFormDates = "api/v1/milestone/{0}/formdates/{1}";

        // Log Milestone
        public const string GetLogMilestones = "api/v1/milestone/log";

        // RadTrack Invoice
        public const string GetAllRadTrackInvoices = "api/v1/radtrackinvoice";
        public const string GetRadTrackInvoiceTotals = "api/v1/radtrackinvoice/totals";
        public const string GetRadTrackInvoiceById = "api/v1/radtrackinvoice/{0}";
        public const string CreateRadTrackInvoice = "api/v1/radtrackinvoice";
        public const string UpdateRadTrackInvoice = "api/v1/radtrackinvoice/{0}";
        public const string DeleteRadTrackInvoice = "api/v1/radtrackinvoice/{0}";
        public const string GetRadTrackInvoiceProjects = "api/v1/radtrackinvoice/lookups/projects";
        public const string GetRadTrackInvoiceYears = "api/v1/radtrackinvoice/lookups/years";
        public const string GetRadTrackInvoiceContracts = "api/v1/radtrackinvoice/lookups/contracts";
        public const string GetRadTrackInvoicePrograms = "api/v1/radtrackinvoice/lookups/programs";

        // Yearly Financial Details
        public const string GetAllYearlyFinancialData = "api/v1/yearlyfinancialdata/{0}";
        public const string GetYearlyFinancialDataByKey = "api/v1/yearlyfinancialdata/{0}/{1}";
        public const string CreateYearlyFinancialData = "api/v1/yearlyfinancialdata";
        public const string UpdateYearlyFinancialData = "api/v1/yearlyfinancialdata/{0}/{1}";
        public const string DeleteYearlyFinancialData = "api/v1/yearlyfinancialdata/{0}/{1}";
        public const string GetYearlyFinancialDataPactCosts = "api/v1/yearlyfinancialdata/{0}/{1}/pactcosts";
        public const string GetSettingValueById = "api/v1/yearlyfinancialdata/settings/{0}";

        // Staging / Import
        public const string GetStagingMilestones = "api/v1/milestone/staging";
        public const string GetAllStagingMilestones = "api/v1/milestone/allstaging";
        public const string AddStagingMilestone = "api/v1/milestone/staging/{0}";
        public const string UpdateStagingMilestone = "api/v1/milestone/staging/{0}";
        public const string DeleteStagingMilestone = "api/v1/milestone/staging/{0}";
        public const string ClearStagingMilestones = "api/v1/milestone/{0}/staging";
        public const string ValidateStagingMilestones = "api/v1/milestone/{0}/staging/validate";
        public const string ImportStagingMilestones = "api/v1/milestone/{0}/staging/import";
        public const string ImportOverwriteStagingMilestones = "api/v1/milestone/{0}/staging/import-overwrite";
        public const string GetNextStagingMilestoneNumber = "api/v1/milestone/{0}/staging/nextnumber";

        //PMD
        public const string GetProjectYearManagers = "api/v1/pmd/projectyearmanagers/{0}";
        public const string GetPMDMilestones = "api/v1/pmd/milestones";
    }
}