namespace Apha.Common.Contracts.PACT
{
    public class MonthlyTimeMakeLiveRes
    {
        public int ProcessedCount { get; set; }
        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
