using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.Costbook
{
    public class AccountGroupReq
    {
        [Required]
        [MaxLength(15)]
        public string Csg7Group { get; set; } = string.Empty;

        public bool UseInflation { get; set; } = true;
    }
}
