namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for CostCentre CRUD endpoints.
    /// Exposes the full RecordSource surface of fps.costcentre required by list, get, create, and update responses.
    /// FpsYear is included to provide the partition context needed by frontend consumers.
    /// </summary>
    public class CostCentreRes
    {
        public double CostCentreNo { get; set; }

        public string ProfitCentre { get; set; } = null!;

        public int FpsYear { get; set; }
    }
}
