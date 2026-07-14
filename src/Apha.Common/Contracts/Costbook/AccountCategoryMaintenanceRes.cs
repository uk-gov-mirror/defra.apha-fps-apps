namespace Apha.Common.Contracts.Costbook
{
    public class AccountCategoryMaintenanceRes
    {
        public string AccShortName { get; set; } = string.Empty;

        public string? AccountDescription { get; set; }

        public string? Csg7Group { get; set; }

        public int FpsYear { get; set; }
    }
}
