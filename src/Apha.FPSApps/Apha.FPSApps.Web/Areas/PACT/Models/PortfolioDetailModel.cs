using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    /// <summary>
    /// Represents the editable portfolio fields shown on the Portfolio Maintenance page.
    /// Only these fields are sent to the PATCH api/v1/project/external/portfolio endpoint;
    /// all other Project entity fields (Customer, Disease, Contract, Status, etc.) are
    /// preserved unchanged by the repository.
    /// </summary>
    public class PortfolioDetailModel
    {
        [Display(Name = "Parent Project")]
        [Required(ErrorMessage = "Parent Project is required.")]
        public string? ParentProject { get; set; }

        [Display(Name = "Project Title")]
        [Required(ErrorMessage = "Project Title is required.")]
        [StringLength(255)]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Finished")]
        public bool Finished { get; set; }

        [Display(Name = "Programme")]
        [Required(ErrorMessage = "Programme is required.")]
        public string? Program { get; set; }

        [Display(Name = "Manager")]
        public string? ProjectManager { get; set; }

        [Display(Name = "Budget-cvt")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Budget-cvt must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Transfer Income")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Transfer Income must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        public decimal? TransferIncome { get; set; }

        [Display(Name = "Comments")]
        public string? Comments { get; set; }
    }
}
