using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class TestOrProductViewModel
    {
        [Display(Name = "Item Code")]
        [Required(ErrorMessage = "Item Code is required")]
        [StringLength(20)]
        [GridColumn(Order = 1, Width = 100, Type = GridColumnType.Text,IsFilterable =true)]
        public string ItemCode { get; set; } = null!;
       
        [Display(Name = "Item Description")]
        [StringLength(200)]
        [GridColumn(Order = 2, Width = 200, Type = GridColumnType.Text, IsFilterable = true)]
        public string? ItemDescription { get; set; }

        [Display(Name = "Short Description")]
        [StringLength(18)]
        [GridColumn(Order = 3, Width = 150, Type = GridColumnType.Text)]
        public string? ShortDescription { get; set; }


        [Display(Name = "Owner")]
        [Required(ErrorMessage = "Owner is required")]
        [StringLength(2)]
        [GridColumn(Order = 4, Width = 80, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Owner { get; set; }

        [Display(Name = "Test Manager")]
        [StringLength(50)]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.Text, IsFilterable = true)]
        public string? TestManager { get; set; }
       
        [Display(Name = "Non Defra Unit Price")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Non Defra Unit Price must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal? UnitPriceVla { get; set; }

        [Display(Name = "Defra Unit Price")]
        [Required(ErrorMessage = "Defra Unit Price is required")]
        [Range(typeof(decimal), "-999999999999999.9999", "999999999999999.9999", ErrorMessage = "Defra Unit Price must be between -999,999,999,999,999.9999 and 999,999,999,999,999.9999.")]
        [GridColumn(Order = 8, Width = 100, Type = GridColumnType.GbpValue)]
        public decimal DefraUnitPrice { get; set; }
        [GridColumn(IsVisible = false)]
        public int FpsYear { get; set; }
    }
}

