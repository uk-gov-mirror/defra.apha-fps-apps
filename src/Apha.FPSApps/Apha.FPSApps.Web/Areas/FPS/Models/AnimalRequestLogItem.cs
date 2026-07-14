using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Read-only DataGrid row model for the Animal Requirement Changes audit log tab.
    /// Derives from JS initializeAnimalRequirementChangesTable() columns array (8 visible columns).
    /// Property names match AnimalRequestLogDto exactly for AutoMapper convention mapping.
    /// </summary>
    public class AnimalRequestLogItem
    {
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int SequenceNo { get; set; }

        [Display(Name = "JobCode")]
        [GridColumn(Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string JobCode { get; set; } = null!;

        [Display(Name = "AnimalType")]
        [GridColumn(Width = 180, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string AnimalType { get; set; } = null!;

        [Display(Name = "NumberOfDays")]
        [GridColumn(Width = 150, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double NumberOfDays { get; set; }

        [Display(Name = "NumberOfAnimals")]
        [GridColumn(Width = 160, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double NumberOfAnimals { get; set; }

        [Display(Name = "Date_Time")]
        [GridColumn(Width = 170, Type = GridColumnType.DateTime, IsFilterable = false)]
        public DateTime? DateTime { get; set; }

        [Display(Name = "User_ID")]
        [GridColumn(Width = 150, Type = GridColumnType.ReadOnly, IsFilterable = true)]
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
