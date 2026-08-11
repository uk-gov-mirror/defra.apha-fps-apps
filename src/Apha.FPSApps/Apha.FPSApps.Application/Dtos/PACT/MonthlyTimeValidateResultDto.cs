namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class MonthlyTimeValidateResultDto
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
