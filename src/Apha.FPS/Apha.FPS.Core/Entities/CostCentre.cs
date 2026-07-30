namespace Apha.FPS.Core.Entities
{
    public partial class CostCentre
    {
        /// <summary>
        /// Cost centre number. Maps to DB column "costcentre" (double precision NOT NULL).
        /// Named CostCentreNo to avoid collision with the class name.
        /// </summary>
        public double CostCentreNo { get; set; }

        /// <summary>
        /// Profit centre code. Maps to DB column "profitcentre" (varchar(50) NOT NULL).
        /// FK → fps.tblkpprofitcentre.
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        /// <summary>
        /// FPS financial year. Maps to DB column "fpsyear" (integer NOT NULL).
        /// Part of composite PK; FK → fps.tblyearmaster.
        /// </summary>
        public int FpsYear { get; set; }
    }
}
