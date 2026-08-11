namespace Apha.Common.Contracts.PIMS
{
    public class AccessUserLevelReq
    {
        public int SystemId { get; set; }       
        public string NtLogin { get; set; } = null!;
        public int AccessLevelId { get; set; }
    }
}
