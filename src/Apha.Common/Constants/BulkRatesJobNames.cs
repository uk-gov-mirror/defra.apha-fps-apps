namespace Apha.Common.Constants
{
    /// <summary>
    /// Canonical Bulk Rates job names, matching fps.job_master.jobname and
    /// Apha.BatchJobs.Domain.Constants.BatchJobNames on the worker side.
    /// </summary>
    public static class BulkRatesJobNames
    {
        public const string Fec = "BulkTestRatesUpdate";
        public const string Staff = "BulkStaffRatesUpdate";
        public const string Animal = "BulkAnimalRatesUpdate";

        /// <summary>
        /// The single fps.tblyearmaster yearstatus a request of this job type may target.
        /// FEC Test Rates changes apply to the year currently being processed (Open); Staff and
        /// Animal rate changes are prepared ahead of the year they take effect in (Planned).
        /// </summary>
        public static string RequiredYearStatus(string jobName) => jobName switch
        {
            Fec => "Open",
            Staff or Animal => "Planned",
            _ => throw new ArgumentOutOfRangeException(nameof(jobName), jobName, "Unknown Bulk Rates job name.")
        };
    }
}
