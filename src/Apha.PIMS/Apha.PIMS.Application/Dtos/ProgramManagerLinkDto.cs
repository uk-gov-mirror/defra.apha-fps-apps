namespace Apha.PIMS.Application.Dtos
{
    public class ProgramManagerLinkDto
    {
        public string Program { get; set; } = null!;

        public string Manager { get; set; } = null!;
    }

    public class ProgramLookupDto
    {
        public string ProgramNo { get; set; } = null!;

        public short LatestYear { get; set; }
    }
}
