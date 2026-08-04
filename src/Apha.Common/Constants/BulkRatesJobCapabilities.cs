namespace Apha.Common.Constants
{
    /// <summary>
    /// A capability check, not a hardcoded or growing job-name
    /// chain (<c>if (Fec || Staff || Animal)</c>) — callers ask what a job type requires rather
    /// than enumerating job types themselves, so the upload path stays extensible to future job
    /// types without another hardcoded branch.
    /// </summary>
    public static class BulkRatesJobCapabilities
    {
        /// <summary>
        /// Job names currently enrolled in the stale-download-version rejection at
        /// upload. Staff/Animal joined FEC here once the request-scoped download route
        /// (DownloadStaffTestDataAsync/DownloadAnimalTestDataAsync) shipped, embedding the
        /// BulkRatesDownloadMetadataKeys metadata this check reads. The old, unversioned
        /// year-based export routes (ExportStaffTestDataAsync/ExportAnimalTestDataAsync) remain
        /// live for the queue page's "current year" download but embed no
        /// metadata at all — a workbook downloaded from there and then uploaded is now rejected
        /// as stale, the same treatment FEC's own old year-based route already receives today.
        /// </summary>
        private static readonly HashSet<string> DownloadVersionRequiredJobs = new(StringComparer.OrdinalIgnoreCase)
        {
            BulkRatesJobNames.Fec,
            BulkRatesJobNames.Staff,
            BulkRatesJobNames.Animal,
        };

        public static bool RequiresDownloadVersion(string jobName) => DownloadVersionRequiredJobs.Contains(jobName);
    }
}
