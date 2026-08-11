namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class SettingDto
    {
        public string Id { get; set; } = null!;
        public string? SettingValue { get; set; }
        public string? Notes { get; set; }
        public string? Testsetting { get; set; }
        public bool Userupdateable { get; set; }
    }
}
