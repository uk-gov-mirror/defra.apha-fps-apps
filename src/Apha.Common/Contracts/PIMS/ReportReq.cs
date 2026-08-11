namespace Apha.Common.Contracts.PIMS
{
    public class ReportReq
    {
        public string ReportName { get; set; } = null!;
        public string? ReportDescription { get; set; }
        public string? Filter { get; set; }
        public string? MailComment { get; set; }
        public string? MailTitle { get; set; }

        public bool Emailable { get; set; }
        public int? SortOrder { get; set; }
        public bool AllowPickProgramme { get; set; } = false;
        public bool AllowPickProject { get; set; } = false;
        public bool AllowPickManager { get; set; } = false;
        public bool AllowPickContract { get; set; } = false;
        public bool AllowPickCustomer { get; set; } = false;
        public bool AllowPickMonth { get; set; } = false;
        public bool AllowPickFYear { get; set; } = false;
        public string? ReportHelp { get; set; }

        public string Type { get; set; } = null!;
    }
}
