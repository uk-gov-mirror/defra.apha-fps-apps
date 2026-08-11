namespace Apha.Common.Contracts.PIMS
{
    public class AccessUserLevelRes
    {
        public int SystemId { get; set; }
        public string NtLogin { get; set; } = null!;
        public int AccessLevelId { get; set; }
    }
}
