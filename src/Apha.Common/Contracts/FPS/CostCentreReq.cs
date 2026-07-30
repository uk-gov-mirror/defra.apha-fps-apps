namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for CostCentre Create and Update operations.
    /// Contains only the writable ControlSource-bound fields from frmMaintCostCentres.
    /// Maps to the fps.costcentre table (costcentre, profitcentre columns).
    /// </summary>
    public class CostCentreReq
    {
        public double CostCentreNo { get; set; }

        public string ProfitCentre { get; set; } = null!;
    }
}
