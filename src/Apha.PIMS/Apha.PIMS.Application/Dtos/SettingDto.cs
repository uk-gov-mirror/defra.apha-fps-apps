namespace Apha.PIMS.Application.Dtos
{
    public class SettingDto
    {
        public string Id { get; set; } = null!;

        public string? Setting { get; set; }

        public string? Notes { get; set; }

        public string? TestSetting { get; set; }

        public bool UserUpdateable { get; set; }
    }
}
