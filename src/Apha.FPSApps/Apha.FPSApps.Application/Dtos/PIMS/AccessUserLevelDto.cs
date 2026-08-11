namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class AccessUserLevelDto
    {
        public int SystemId { get; set; }
        public string NtLogin { get; set; } = null!;
        public int AccessLevelId { get; set; }
    }
}
