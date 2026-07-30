using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupPeopleItem
    {
        [Display(Name = "PACT ID")]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? PactId { get; set; }

        [Display(Name = "SP Number")]
        [GridColumn(Order = 2, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? SpNumber { get; set; }

        [Display(Name = "Name")]
        [GridColumn(Order = 3, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Name { get; set; }

        [Display(Name = "WorkGroup Grade")]
        [GridColumn(Order = 4, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? WorkGroupGrade { get; set; }

        [Display(Name = "Title")]
        [GridColumn(Order = 5, Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Title { get; set; }

        [Display(Name = "Person Status")]
        [GridColumn(Order = 6, Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        public string? PersonStatus { get; set; }

        [Display(Name = "Person Class")]
        [GridColumn(IsVisible = false)]
        public string? PersonClass { get; set; }

        [Display(Name = "Hrs Paid")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.Text, IsFilterable = true)]
        public decimal? HrsPaid { get; set; }

        [Display(Name = "Leave")]
        [GridColumn(Order = 8, Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public decimal? Leave { get; set; }

        [Display(Name = "Sick / Special")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public decimal? SickSpecial { get; set; }

        [Display(Name = "Hrs Avail")]
        [GridColumn(Order = 10, Width = 90, Type = GridColumnType.Text, IsFilterable = true)]
        public decimal? HrsAvail { get; set; }
    }
}
