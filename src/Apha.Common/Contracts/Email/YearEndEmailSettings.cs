namespace Apha.Common.Contracts.Email
{
    public class YearEndEmailSettings
    {
        public const string SectionName = "YearEndEmailSettings";

        public string DataSetupInitiatedEmailRecipient { get; set; } = string.Empty;
        public string DataSetupInitiatedEmailSubject { get; set; } = string.Empty;
        public string DataSetupInitiatedEmailBody { get; set; } = string.Empty;
        public string DataSetupApprovalEmailRecipient { get; set; } = string.Empty;
        public string DataSetupApprovalEmailSubject { get; set; } = string.Empty;
        public string DataSetupApprovalEmailBody     { get; set; } = string.Empty;
        public string DataSetupRejectionEmailRecipient { get; set; } = string.Empty;
        public string DataSetupRejectionEmailSubject { get; set; } = string.Empty;
        public string DataSetupRejectionEmailBody { get; set; } = string.Empty;
    }
}
