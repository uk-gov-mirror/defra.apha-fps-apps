namespace Apha.FPSApps.Web.Handler
{
    public interface IFpsYearContext
    {
        int Year { get; set; }
        bool IsReadOnly { get; set; }

        /// <summary>fps.tblyearmaster yearstatus for <see cref="Year"/> (e.g. "Open", "Planned", "Closed").</summary>
        string? YearStatus { get; set; }
    }


    public class FpsYearContext : IFpsYearContext
    {
        public int Year { get; set; }
        public bool IsReadOnly { get; set; }
        public string? YearStatus { get; set; }
    }
}
