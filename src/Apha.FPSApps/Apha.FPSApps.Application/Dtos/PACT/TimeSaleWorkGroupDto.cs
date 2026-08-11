using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class TimeSaleWorkGroupDto
    {
        public string? SellingWg { get; set; }
        public string? Name { get; set; }
        public double? Time { get; set; }
        public double? Cost { get; set; }
        public double Month { get; set; }
        [Display(Name = "PlanCat")]
        public string? PlanCategory { get; set; }
        public string? Program { get; set; }
        public string? Project { get; set; }
        public string? JobCode { get; set; }
        public string? Manager { get; set; }
    }
}
