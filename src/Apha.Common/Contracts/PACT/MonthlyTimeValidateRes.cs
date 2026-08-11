namespace Apha.Common.Contracts.PACT
{
    public class MonthlyTimeValidateRes
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
