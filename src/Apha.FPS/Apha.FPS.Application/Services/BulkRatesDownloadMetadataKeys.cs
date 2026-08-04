namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Keys used in the workbook's protected metadata sheet (Apha.Common's
    /// IExcelExportService.ExportToExcelMultiSheet VeryHidden-sheet overload). Written by
    /// BulkRatesRequestService.DownloadFecTestDataAsync and
    /// DownloadStaffTestDataAsync/DownloadAnimalTestDataAsync — read back by upload
    /// validation (for FEC today; Staff/Animal upload-side enforcement is separate) — a
    /// single shared place so all sides can never disagree on the key names. JobQueueId alone is
    /// sufficient to disambiguate job type at upload time (a JobQueueId is already 1:1 with one
    /// request, which has a fixed JobName) — no separate JobType key is needed.
    /// </summary>
    internal static class BulkRatesDownloadMetadataKeys
    {
        public const string JobQueueId = "BulkRatesJobQueueId";
        public const string DownloadVersion = "BulkRatesDownloadVersion";
    }
}
