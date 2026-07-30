namespace Apha.PIMS.Core.Entities
{
    public class YearlyFinancialData
    {
        public short Year { get; set; }
        public string Project { get; set; } = null!;
        public decimal? BfBudget { get; set; }
        public decimal? PyBudget { get; set; }
        public decimal? Seedcorn { get; set; }
        public decimal? PayCosts { get; set; }
        public decimal? NonPayOhCosts { get; set; }
        public decimal? TestCosts { get; set; }
        public decimal? AnimalCosts { get; set; }
        public decimal? NonAnimalCosts { get; set; }
        public decimal? Adjustment { get; set; }
        public decimal? ActualExpenditure { get; set; }
        public decimal? VlaBudget { get; set; }
        public double? ManHours { get; set; }
        public double? ManDays { get; set; }
        public double? ManYears { get; set; }
        public double? ActualManYears { get; set; }
        public short ManHoursChanged { get; set; }
        public short PayCostsChanged { get; set; }
        public short NonPayOhCostsChanged { get; set; }
        public short TestCostsChanged { get; set; }
        public short AnimalCostsChanged { get; set; }
        public short NonAnimalCostsChanged { get; set; }
        public string? AdjustmentComment { get; set; }
        public short Locked { get; set; }
        public DateTime? DateCosted { get; set; }
        public string? CostedBy { get; set; }
    }
}
