namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Read-only projection representing a joined row of project general data,
    /// additional costs and account category information for the
    /// Project Specific Query grid.
    /// </summary>
    public class ProjectSpecificQueryItem
    {
        public string? Program { get; set; }

        public string? ParentProject { get; set; }

        public string? ProjectTitle { get; set; }

        public string? ShortTitle { get; set; }

        public string? ProjectStatus { get; set; }

        public string? Account { get; set; }

        public string? Description { get; set; }

        public string? AccountDescription { get; set; }

        public string? ConstituentAccountCodes { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public decimal ItemCost { get; set; }

        public string? Manager { get; set; }
    }
}
