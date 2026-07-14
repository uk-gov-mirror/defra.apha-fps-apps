using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Test Requirement Changes audit log tab.
    /// Derives from JS initializeTestRequirementChangesTable() columns array (11 visible columns).
    /// Property names match TestRequirementLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class TestRequirementLogItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        [Display(Name = "TestCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestCode { get; set; }

        [Display(Name = "Buyer")]
        [GridColumn(Width = 110, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Buyer { get; set; }

        [Display(Name = "UnitPrice")]
        [GridColumn(Width = 110, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "NoRequired")]
        [GridColumn(Width = 120, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? NoRequired { get; set; }

        [Display(Name = "ProjectBuyerCode")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? ProjectBuyerCode { get; set; }

        [Display(Name = "TestBuyerCode")]
        [GridColumn(Width = 160, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? TestBuyerCode { get; set; }

        [Display(Name = "Active")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public short? Active { get; set; }

        [Display(Name = "Date_Time")]
        [GridColumn(Width = 180, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "User_ID")]
        [GridColumn(Width = 170, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? UserId { get; set; }

        // JS column field=userEmail, header=User_Email, width=240
        [Display(Name = "User_Email")]
        [GridColumn(Width = 240, Type = GridColumnType.ReadOnly, IsFilterable = false, IsVisible = false)]
        public string? UserEmail { get; set; }

        [Display(Name = "Insert_Delete")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? InsertDelete { get; set; }
    }
}
