namespace Apha.PACT.Application.Dtos
{
    public class MonthlyOutputValidateResultDto
    {
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
