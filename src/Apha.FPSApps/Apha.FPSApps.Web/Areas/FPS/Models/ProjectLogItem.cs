using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Project Detail Changes audit log tab.
    /// Derives from JS initializeProjectAuditTrailTable() columns array (33 visible columns).
    /// Property names match ProjectLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class ProjectLogItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        [Display(Name = "ParentProject")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ParentProject { get; set; } = null!;

        [Display(Name = "ProjectTitle")]
        [GridColumn(Width = 260, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectTitle { get; set; } = null!;

        [Display(Name = "Program")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Program { get; set; } = null!;

        [Display(Name = "Customer")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Customer { get; set; } = null!;

        [Display(Name = "Manager")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Manager { get; set; }

        [Display(Name = "TransferIncome")]
        [GridColumn(Width = 150, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal TransferIncome { get; set; }

        [Display(Name = "CustIncome")]
        [GridColumn(Width = 140, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal CustIncome { get; set; }

        [Display(Name = "WIP_EOY")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipEoy { get; set; }

        [Display(Name = "WIP_Limit")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipLimit { get; set; }

        [Display(Name = "WIP_Current")]
        [GridColumn(Width = 130, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? WipCurrent { get; set; }

        [Display(Name = "ProjectStatus")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string ProjectStatus { get; set; } = null!;

        [Display(Name = "CostBookNo")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? CostBookNo { get; set; }

        [Display(Name = "DateCreated")]
        [GridColumn(Width = 180, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "FECost")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? FecCost { get; set; }

        [Display(Name = "Profit")]
        [GridColumn(Width = 100, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? Profit { get; set; }

        [Display(Name = "Budget_CVL")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? BudgetCvl { get; set; }

        [Display(Name = "DateCosted")]
        [GridColumn(Width = 120, Type = GridColumnType.Date, IsFilterable = false)]
        public DateTime? DateCosted { get; set; }

        [Display(Name = "Disease")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Disease { get; set; } = null!;

        [Display(Name = "Contract")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Contract { get; set; } = null!;

        [Display(Name = "ProjectParent")]
        [GridColumn(Width = 140, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectParent { get; set; }

        [Display(Name = "ShortTitle")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ShortTitle { get; set; }

        [Display(Name = "CaseworkSub")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CaseWorkSub { get; set; }

        [Display(Name = "PVSIncome")]
        [GridColumn(Width = 120, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? PvsIncome { get; set; }

        [Display(Name = "PlanCaseworkDebit")]
        [GridColumn(Width = 180, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? PlanCaseWorkDebit { get; set; }

        [Display(Name = "Finished")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public short? Finished { get; set; }

        [Display(Name = "OwningRC")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? OwningRc { get; set; }

        [Display(Name = "Comments")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? Comments { get; set; }

        [Display(Name = "CarryOver")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CarryOver { get; set; }

        [Display(Name = "CarryOverSeed")]
        [GridColumn(Width = 140, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? CarryOverSeed { get; set; }

        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "User_ID")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // Populated by JS decorateAuditRowsWithEmail(); requires backend/service to resolve from UserId.
        // JS column field=userEmail, header=User_Email, width=240
        [Display(Name = "User_Email")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string? UserEmail { get; set; }

        [Display(Name = "Insert_Delete")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? InsertDelete { get; set; }
    }
}
