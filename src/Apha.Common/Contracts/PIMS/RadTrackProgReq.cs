using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.PIMS
{
    public class RadTrackProgReq
    {
        public string Program { get; set; } = null!;

        public bool RadTrackProg { get; set; } = true;

        [MaxLength(5, ErrorMessage = "Publication Prefix cannot exceed 5 characters.")]
        public string? PublicationPrefix { get; set; }
    }
}