namespace Apha.FPSApps.Application.Dtos.CostBook;

public class PayRateDto
{
    public string WgGrade { get; set; } = null!;
    public decimal? ChargeRate { get; set; }
    //public decimal? PayRate { get; set; }
    //public decimal? Npr { get; set; }
    //public decimal? Ohr { get; set; }
    public decimal? ChargeRateWithInflamation { get; set; }
}

public class AnimalRateDto
{
    public string AnimalType { get; set; } = null!;
    public decimal? DailyRate { get; set; }
    public decimal? DailyRateWithInflamation { get; set; }
}

public class AccountCategoryDto
{
    public string AccShortName { get; set; } = null!;
    public bool UseInflation { get; set; }
}

public class TestCodeLookupDto
{
    public string ItemCode { get; set; } = null!;
    public string? ItemDescription { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? UnitPriceWithInflamation { get; set; }
}

public class AnimalLookupDto
{
    public string AnimalType { get; set; } = null!;  
    public decimal? DailyRate { get; set; }
    public decimal? DailyRateWithInflamation { get; set; }
}
