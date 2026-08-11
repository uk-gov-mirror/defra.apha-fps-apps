using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Grid row item for the Snapshot Projects DataGrid.
    /// Property names match <c>ProjectSnapshotViewDto</c> for AutoMapper convention mapping.
    /// </summary>
    public class ProjectSnapshotItem
    {
        [Display(Name = "Program")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Program { get; set; }

        [Display(Name = "Parent")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "ProjectTitle")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectTitle { get; set; } = null!;

        [Display(Name = "Contract")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Contract { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectStatus { get; set; }

        [Display(Name = "Customer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Customer { get; set; }

        [Display(Name = "Manager")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "Transfer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "Customer Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue)]
        public decimal CustIncome { get; set; }

        [Display(Name = "Budget CVL")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? BudgetCvl { get; set; }


        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? CaseWorkSub { get; set; }


        [Display(Name = "PVS Income")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? PvsIncome { get; set; }

        [Display(Name = "PlanCaseWorkDebit")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [GridColumn(Width = 110, Type = GridColumnType.GbpValue)]
        public decimal? PlanCaseWorkDebit { get; set; }
    }
}
