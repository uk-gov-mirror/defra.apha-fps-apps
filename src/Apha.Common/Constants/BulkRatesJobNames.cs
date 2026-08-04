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
    }
}
