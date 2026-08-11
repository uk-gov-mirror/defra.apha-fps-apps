namespace Apha.PIMS.Core.Entities
{
    public partial class AccessUserLevel
    {
        public int SystemId { get; set; }

        public string NtLogin { get; set; } = null!;

        public int AccessLevelId { get; set; }
    }
}
