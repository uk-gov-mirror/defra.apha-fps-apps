using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class ProjectCreateEditViewModel
    {
        public string ProjectId { get; set; } = string.Empty;
        public string? PlanCategory { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string ProjectTitle { get; set; } = string.Empty;    

        public string? Programme { get; set; }
        public string? ProjectWorkgroup { get; set; }
        public double? ContractPrice { get; set; }

        [Required(ErrorMessage = "Start Date is required.")]
        public DateTime? StartDate { get; set; }

        public string? Disease { get; set; }
        public double? StartFYear { get; set; }
        public string? CustomerName { get; set; }
        public string? ContractNumber { get; set; }
        public string? SubmittedByFName { get; set; }
        public string? SubmittedByLName { get; set; }
        public DateTime? DateOfSubmission { get; set; }
        [Required(ErrorMessage = "Prepared By is required.")]
        public string? PreparedBy { get; set; }
        public int? Inflation { get; set; }
        public int? FinancialYears { get; set; }
        public string? Notes { get; set; }
        public double? Euroconvrate { get; set; }

        [Required(ErrorMessage = "Please select Defra/Non-Defra")]
        public short? IsDefraProject { get; set; }        
        public decimal? BudgetAmount { get; set; }
        public decimal? ActualCost { get; set; }
       
        [BindNever] public List<SelectListItem> AvailablePrograms { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableCustomers { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableDiseases { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableStaff { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableContracts { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableFinancialYears { get; set; } = new();
        [BindNever] public List<SelectListItem> AvailableDefraProjectOptions { get; set; } = new();
    }
}
