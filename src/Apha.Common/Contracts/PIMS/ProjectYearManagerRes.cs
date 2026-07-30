namespace Apha.Common.Contracts.PIMS
{
    /// <summary>
    /// Response contract for project year manager details
    /// </summary>
    public class ProjectYearManagerRes
    {
        public int? ProjectYear { get; set; }

        public string? ParentProject { get; set; }

        public string? Manager { get; set; }

        public string? ManagerNumber { get; set; }
    }
}
