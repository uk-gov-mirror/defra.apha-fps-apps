namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Keys used in the workbook's protected metadata sheet (Apha.Common's
    /// IExcelExportService.ExportToExcelMultiSheet VeryHidden-sheet overload). Written by
    /// BulkRatesRequestService.DownloadFecTestDataAsync and read back by upload
    /// validation — a single shared place so the two sides can never disagree on
    /// the key names.
    /// </summary>
    internal static class BulkRatesDownloadMetadataKeys
    {
        public const string JobQueueId = "BulkRatesJobQueueId";
        public const string DownloadVersion = "BulkRatesDownloadVersion";
    }
}
