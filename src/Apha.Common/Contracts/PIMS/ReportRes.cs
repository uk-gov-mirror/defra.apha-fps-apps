namespace Apha.Common.Contracts.PIMS
{
    public class ReportRes
    {
        public int Id { get; set; }
        public string ReportName { get; set; } = null!;
        public string? ReportDescription { get; set; }
        public string? Filter { get; set; }
        public string? MailComment { get; set; }
        public string? MailTitle { get; set; }
        public bool Emailable { get; set; }
        public int? SortOrder { get; set; }
        public bool AllowPickProgramme { get; set; }
        public bool AllowPickProject { get; set; }
        public bool AllowPickManager { get; set; }
        public bool AllowPickContract { get; set; }
        public bool AllowPickCustomer { get; set; }
        public bool AllowPickMonth { get; set; }
        public bool AllowPickFYear { get; set; }
        public string? ReportHelp { get; set; }
        public string Type { get; set; } = null!;
    }
}
