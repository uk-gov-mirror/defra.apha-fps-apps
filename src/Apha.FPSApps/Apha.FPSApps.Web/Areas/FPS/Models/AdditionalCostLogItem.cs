using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Exceptional Cost Changes audit log tab.
    /// Derives from JS initializeExceptionalCostChangesTable() columns array (10 visible columns).
    /// Property names match AdditionalCostLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class AdditionalCostLogItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        [Display(Name = "JobCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "Account")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string Account { get; set; } = null!;

        [Display(Name = "Description")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string Description { get; set; } = null!;

        [Display(Name = "ItemCost")]
        [GridColumn(Width = 140, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal ItemCost { get; set; }

        [Display(Name = "Freq")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Freq { get; set; }

        [Display(Name = "Supplier")]
        [GridColumn(Width = 130, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Supplier { get; set; }

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
