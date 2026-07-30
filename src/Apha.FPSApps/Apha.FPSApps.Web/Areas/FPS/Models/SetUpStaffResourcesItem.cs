using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class SetUpStaffResourcesItem
    {
        [GridColumn(IsVisible = false)]
        public string PactId { get; set; } = null!;

        [Display(Name = "SP No")]
        [GridColumn(Width = 100, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string SpNumber { get; set; } = null!;

        [GridColumn(IsVisible = false)]
        public string WorkGroupGrade { get; set; } = null!;

        [Display(Name = "Name")]
        [GridColumn(Width = 200, Type = GridColumnType.ReadOnly, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "Hrs Paid")]
        [GridColumn(Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double HrsPaid { get; set; }

        [Display(Name = "Leave")]
        [GridColumn(Width = 70, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double Leave { get; set; }

        [Display(Name = "Sick Sp")]
        [GridColumn(Width = 80, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double SickSpecial { get; set; }

        //   Displayed as readonly in modal (ssrEditAtWork); JS recalculates on input change
        [Display(Name = "At Work")]
        [GridColumn(Width = 90, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public double HrsAvail { get; set; }

        //   int type matches WorkGroupEmployeeStaffDto.MakeAvailable (0/1 from backend)
        [Display(Name = "Planable")]
        [GridColumn(Width = 80, Type = GridColumnType.Checkbox, IsFilterable = false)]
        public int MakeAvailable { get; set; }
    }
}
