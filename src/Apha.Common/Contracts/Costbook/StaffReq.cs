using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Apha.Common.Contracts.Costbook
{
    public class StaffReq
    {
        [Required]
        [MaxLength(50)]
        public string MNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
