using Apha.FPSApps.Web.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class ProgramProjectEditViewModel
    {
        [Display(Name = "Project")]
        [Required(ErrorMessage = "Project code is required.")]
        public string ParentProject { get; set; } = null!;

        /// <summary>
        /// Holds the original project code loaded from the database.
        /// Used on POST to detect whether the user changed the code
        /// so a dependency check can be performed before allowing the rename.
        /// </summary>
        public string? OriginalParentProject { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required.")]
        public string? ProjectTitle { get; set; }

        [Display(Name = "Programme")]
        [Required(ErrorMessage = "Program is required.")]
        public string? Program { get; set; }

        [Display(Name = "Manager")]
        [Required(ErrorMessage = "Manager is required.")]
        public string? Manager { get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required.")]
        public string? Customer { get; set; }

        public string? ProjectGroup { get; set; }

        [Display(Name = "Contract")]
        [Required(ErrorMessage = "Contract is required.")]
        public string? Contract { get; set; }

        [Display(Name = "Disease")]
        [Required(ErrorMessage = "Disease is required.")]
        public string? Disease { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required.")]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Budget")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [CurrencyRange]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Cost Inc")]
        [Required(ErrorMessage = "Cost Inc is required.")]
        [CurrencyRange]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "Trans Inc")]
        [Required(ErrorMessage = "Trans Inc is required.")]
        [CurrencyRange]
        public decimal? TransferIncome { get; set; }

        [CurrencyRange]
        public decimal? PlanCaseWorkDebit { get; set; }

        public short IsDefraProject { get; set; }

        // Dropdown lists — populated in PopulateDropdownsAsync
        public List<SelectListItem> ManagerList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProgramList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CustomerList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProjectGroupList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ContractList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DiseaseList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StatusList { get; set; } = new List<SelectListItem>();
    }
}
