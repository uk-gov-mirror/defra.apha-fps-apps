namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyOutputMakeLiveResultDto
    {
        public int ProcessedCount { get; set; }
        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
