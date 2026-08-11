using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class PactProjectViewModel
    {
        [Display(Name = "Project Code")]
        [Required(ErrorMessage = "Project code is required")]
        [StringLength(50)]
        [GridColumn(Width = 296, Type = GridColumnType.Text, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "Title")]
        [Required(ErrorMessage = "Project title is required")]
        [StringLength(255)]
        [GridColumn(Width = 923, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProjectTitle { get; set; } = null!;

        [Display(Name = "Programme")]
        [Required(ErrorMessage = "Programme is required")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string Program { get; set; } = null!;

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required")]
        [StringLength(100)]
        [GridColumn(IsVisible = false)]
        public string Customer { get; set; } = null!;

        [Display(Name = "Manager")]
        [StringLength(100)]
        [GridColumn(IsVisible = false)]
        public string? Manager { get; set; }

        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string ProjectStatus { get; set; } = null!;

        [Display(Name = "Transfer Income")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Transfer Income must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Budget cvl")]
        [Range(-999999999999999.9999, 999999999999999.9999, ErrorMessage = "Budget cvl must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "Budget Ext")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Budget Ext must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? BudgetExt { get; set; }

        [Display(Name = "PVS Income")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "PVS Income must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? PvsIncome { get; set; }

        [Display(Name = "WIP EOY")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "WIP EOY must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? WipEoy { get; set; }

        [Display(Name = "WIP Limit")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "WIP Limit must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? WipLimit { get; set; }

        [Display(Name = "WIP Current")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "WIP Current must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? WipCurrent { get; set; }

        [Display(Name = "FEC Cost")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "FEC Cost must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(IsVisible = false)]
        public decimal? FecCost { get; set; }

        [Display(Name = "Disease")]
        [Required(ErrorMessage = "Disease is required")]
        [StringLength(100)]
        [GridColumn(IsVisible = false)]
        public string Disease { get; set; } = null!;

        [Display(Name = "Contract")]
        [Required(ErrorMessage = "Contract is required")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string Contract { get; set; } = null!;

        [Display(Name = "Project Parent")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? ProjectParent { get; set; }

        [Display(Name = "Finished")]
        [GridColumn(IsVisible = false)]
        public short? Finished { get; set; }

        [Display(Name = "Comments")]
        [StringLength(500)]
        [GridColumn(IsVisible = false)]
        public string? Comments { get; set; }

        [Display(Name = "Is Defra Project")]
        [GridColumn(IsVisible = false)]
        public short IsDefraProject { get; set; }

        [Display(Name = "Oracle Project Code")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? OracleProjectCode { get; set; }

        [Display(Name = "Sub Account Code")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? SubAccountCode { get; set; }

        [Display(Name = "Project Group")]
        [StringLength(50)]
        [GridColumn(IsVisible = false)]
        public string? ProjectGroup { get; set; }
    }
}
