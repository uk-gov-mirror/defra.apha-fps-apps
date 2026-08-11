using System.ComponentModel.DataAnnotations;

namespace Apha.PIMS.Application.Dtos
{
    public class RadTrackProgDto
    {
        public string Program { get; set; } = null!;

        public bool RadTrackProg { get; set; }

        [MaxLength(5, ErrorMessage = "Publication Prefix cannot exceed 5 characters.")]
        public string? PublicationPrefix { get; set; }
    }
}
