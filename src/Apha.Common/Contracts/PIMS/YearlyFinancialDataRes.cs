using System;

namespace Apha.Common.Contracts.PIMS
{
    public class YearlyFinancialDataRes
    {
        public short Year { get; set; }
        public string? Project { get; set; }
        public decimal? BfBudget { get; set; }              // Display label: "PP/Acc"
        public decimal? PyBudget { get; set; }              // Display label: "Customer Income"
        public decimal? VlaBudget { get; set; }             // DB column: vla_budget
        public decimal? Seedcorn { get; set; }
        public decimal? PayCosts { get; set; }
        public decimal? NonPayOhCosts { get; set; }
        public decimal? TestCosts { get; set; }
        public decimal? AnimalCosts { get; set; }
        public decimal? NonAnimalCosts { get; set; }        // Display label: "Project-Specific Costs"
        public decimal? Adjustment { get; set; }
        public decimal? ActualExpenditure { get; set; }
        public double? ManHours { get; set; }
        public double? ManDays { get; set; }
        public double? ManYears { get; set; }
        public double? ActualManYears { get; set; }
        public string? AdjustmentComment { get; set; }     // varchar(250)
        public short Locked { get; set; }                  // Display label: "Fixed"; smallint DEFAULT 0
        public DateTime? DateCosted { get; set; }           // Display label: "Date Fixed"
        public string? CostedBy { get; set; }               // Display label: "Fixed By"; varchar(20)
        public short ManHoursChanged { get; set; }
        public short PayCostsChanged { get; set; }
        public short NonPayOhCostsChanged { get; set; }
        public short TestCostsChanged { get; set; }
        public short AnimalCostsChanged { get; set; }
        public short NonAnimalCostsChanged { get; set; }
        public decimal? TotalCosts { get; set; }
    }
}
