namespace Apha.PIMS.Core.Entities
{
    /// <summary>
    /// Represents project year manager details combining project lookup and manager information
    /// </summary>
    public class ProjectYearManager
    {
        public int? ProjectYear { get; set; }

        public string? ParentProject { get; set; }

        public string? Manager { get; set; }

        public string? ManagerNumber { get; set; }
    }
}
