namespace Apha.Common.Contracts.FPS
{
    // Binds the three HTML filter inputs from projectaudit_trail.html:
    //   #filter-project (select), #filter-from (date), #filter-to (date)
    public class ProjectAuditTrailReq
    {
        public string? ParentProject { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }
    }
}
