namespace Apha.Costbook.Application.Dtos
{
    public class AccountCategoryMaintenanceDto
    {
        public string AccShortName { get; set; } = null!;

        public string? AccountDescription { get; set; }

        public string? Csg7Group { get; set; }

        public int FpsYear { get; set; }
    }
}
