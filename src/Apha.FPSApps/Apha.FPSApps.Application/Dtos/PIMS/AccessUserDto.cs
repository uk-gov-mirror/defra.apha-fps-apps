namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class AccessUserDto
    {
        public int SystemId { get; set; }
        public string NtLogin { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Dt2Login { get; set; }
        public string? UserEmail { get; set; }
    }
}
