namespace Apha.PIMS.Application.Dtos
{
    public class YearlyFinancialDataDto
    {
        public short Year { get; set; }

        public string? Project { get; set; }

        public decimal? BfBudget { get; set; }

        public decimal? PyBudget { get; set; }

        public decimal? VlaBudget { get; set; }

        public decimal? Seedcorn { get; set; }

        public decimal? PayCosts { get; set; }

        public decimal? NonPayOhCosts { get; set; }

        public decimal? TestCosts { get; set; }

        public decimal? AnimalCosts { get; set; }

        public decimal? NonAnimalCosts { get; set; }

        public decimal? Adjustment { get; set; }

        public decimal? ActualExpenditure { get; set; }

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

        public decimal? TotalCosts
        {
            get
            {
                if (Seedcorn is null && PayCosts is null && NonPayOhCosts is null && TestCosts is null
                    && AnimalCosts is null && NonAnimalCosts is null)
                    return null;

                return (Seedcorn ?? 0m)
                    + (PayCosts ?? 0m)
                    + (NonPayOhCosts ?? 0m)
                    + (TestCosts ?? 0m)
                    + (AnimalCosts ?? 0m)
                    + (NonAnimalCosts ?? 0m);
            }
        }
    }
}
