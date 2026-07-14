namespace Apha.Common.Contracts.Costbook
{
    public class MaintenanceSettingsRes
    {
        public decimal InflationAnimals { get; set; }

        public decimal InflationExceptionalCosts { get; set; }

        public decimal InflationStaff { get; set; }

        public decimal InflationTests { get; set; }

        public int CurrentFinancialYear { get; set; }

        public decimal WorkingHoursInDay { get; set; }

        public decimal WorkingDaysInYear { get; set; }

        public decimal ProfitAnimals { get; set; }

        public decimal ProfitExceptionalCosts { get; set; }

        public decimal ProfitStaff { get; set; }

        public decimal ProfitTests { get; set; }
    }
}
