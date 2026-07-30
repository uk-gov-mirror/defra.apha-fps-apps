namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class CostCentreDto
    {
        /// <summary>
        /// Cost centre number. Mirrors backend CostCentreDto.CostCentreNo (double precision NOT NULL).
        /// Named CostCentreNo to avoid collision with the class name.
        /// </summary>
        public double CostCentreNo { get; set; }

        /// <summary>
        /// Profit centre code. Mirrors backend CostCentreDto.ProfitCentre (varchar NOT NULL).
        /// FK → fps.tblkpprofitcentre.
        /// </summary>
        public string ProfitCentre { get; set; } = null!;

        /// <summary>
        /// FPS financial year. Mirrors backend CostCentreDto.FpsYear (integer NOT NULL).
        /// Part of composite PK; set server-side via X-FPS-Year header / request context.
        /// </summary>
        public int FpsYear { get; set; }
    }
}
