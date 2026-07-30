namespace Apha.PIMS.Core.Entities
{
    public class ProjectMonthFinal
    {
        public short Year { get; set; }

        public string Project { get; set; } = null!;

        public double Monthno { get; set; }

        public string? Periodname { get; set; }

        public double? Cumflag { get; set; }

        public decimal? Costprofile { get; set; }

        public decimal? Subcontracts { get; set; }

        public decimal? Animals { get; set; }

        public decimal? Nonanimals { get; set; }

        public decimal? Timecosts { get; set; }

        public decimal? Transfercosts { get; set; }

        public decimal? Totalcost { get; set; }

        public decimal? Invoices { get; set; }

        public decimal? Coiw { get; set; }

        public decimal? Portsales { get; set; }

        public decimal? Cumcost { get; set; }

        public decimal? Cumprofile { get; set; }

        public decimal? Sumofcostprofile { get; set; }

        public decimal? Cuminvoices { get; set; }

        public decimal? Cumcoiw { get; set; }

        public decimal? Cumportsales { get; set; }

        public int? Mstonedue { get; set; }

        public double? DueDone { get; set; }

        public double? Ontime { get; set; }

        public double? Sumofmstonedue { get; set; }

        public double? SumofdueDone { get; set; }

        public double? Sumofontime { get; set; }

        public decimal? Cwdebit { get; set; }

        public decimal? Cwcredit { get; set; }

        public decimal? Cumcwdebit { get; set; }

        public decimal? Cumcwcredit { get; set; }

        public double? Totalhours { get; set; }

        public double? Cumtotalhours { get; set; }

        public double? Cumsubcontracts { get; set; }

        public double? Cumtestcosts { get; set; }

        public double? Paycosts { get; set; }

        public double? Cumpaycosts { get; set; }
    }
}
