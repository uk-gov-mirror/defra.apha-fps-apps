using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.Costbook
{    
    public class MaintenanceSettingsReq
    {
        [Range(0, 999.99, ErrorMessage = "Animals Inflation must be between 0 and 999.99.")]
        public decimal InflationAnimals { get; set; }

        [Range(0, 999.99, ErrorMessage = "Exceptional Costs Inflation must be between 0 and 999.99.")]
        public decimal InflationExceptionalCosts { get; set; }

        [Range(0, 999.99, ErrorMessage = "Staff Inflation must be between 0 and 999.99.")]
        public decimal InflationStaff { get; set; }

        [Range(0, 999.99, ErrorMessage = "Tests Inflation must be between 0 and 999.99.")]
        public decimal InflationTests { get; set; }

        public int CurrentFinancialYear { get; set; }

        public decimal WorkingHoursInDay { get; set; }

        public decimal WorkingDaysInYear { get; set; }

        [Range(0, 999.99, ErrorMessage = "Animals Profit must be between 0 and 999.99.")]
        public decimal ProfitAnimals { get; set; }

        [Range(0, 999.99, ErrorMessage = "Exceptional Costs Profit must be between 0 and 999.99.")]
        public decimal ProfitExceptionalCosts { get; set; }

        [Range(0, 999.99, ErrorMessage = "Staff Profit must be between 0 and 999.99.")]
        public decimal ProfitStaff { get; set; }

        [Range(0, 999.99, ErrorMessage = "Tests Profit must be between 0 and 999.99.")]
        public decimal ProfitTests { get; set; }
    }
}
