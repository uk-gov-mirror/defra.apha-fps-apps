namespace Apha.Common.Contracts.FPS
{
    public class YearEndMonthHourRes
    {
        public short Year { get; set; }
        public short Month { get; set; }
        public decimal? Days { get; set; }
        public decimal? CvlHours { get; set; }
        public decimal? VidHours { get; set; }
        public short? Fmonth { get; set; }
        public int FpsYear { get; set; }
        public string ExistsForPlannedYear { get; set; } = string.Empty;
    }
}
