using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public class YearlyFinancialDataItem
    {
        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? Project { get; set; }

       
        [Required(ErrorMessage = "Year is required")]
        [Display(Name = "Year")]
        [GridColumn(Order = 1, Width = 60, Type = GridColumnType.Text, IsFilterable = false)]
        public short Year { get; set; }

       
        [Display(Name = "PP/Acc")]
        [GridColumn(Order = 2, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? BfBudget { get; set; }

      
        [Display(Name = "Customer Income")]
        [GridColumn(Order = 3, Width = 115, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? PyBudget { get; set; }

        
        [Display(Name = "VLA Budget")]
        [GridColumn(Order = 4, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? VlaBudget { get; set; }

        
        [Display(Name = "Actual Exp")]
        [GridColumn(Order = 5, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? ActualExpenditure { get; set; }

        
        [Display(Name = "Seedcorn")]
        [GridColumn(Order = 6, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false)]
        public decimal? Seedcorn { get; set; }

        
        [Display(Name = "Man Hours")]
        [GridColumn(Order = 7, Width = 90, Type = GridColumnType.DecimalNumber, IsFilterable = false)]
        public double? ManHours { get; set; }

        
        [Display(Name = "Pay Costs")]
        [GridColumn(Order = 8, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? PayCosts { get; set; }

        
        [Display(Name = "Non-Pay & OH")]
        [GridColumn(Order = 9, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? NonPayOhCosts { get; set; }

        
        [Display(Name = "Test Costs")]
        [GridColumn(Order = 10, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? TestCosts { get; set; }

        
        [Display(Name = "Project Specific")]
        [GridColumn(Order = 11, Width = 100, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? NonAnimalCosts { get; set; }

        
        [Display(Name = "Animal Costs")]
        [GridColumn(Order = 12, Width = 90, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? AnimalCosts { get; set; }

        
        [Display(Name = "Exc/Adj")]
        [GridColumn(Order = 13, Width = 80, Type = GridColumnType.GbpValue, IsFilterable = false, CssClass = "sup_text_right_align")]
        public decimal? Adjustment { get; set; }

       
        [Display(Name = "Adj Comment")]
        [GridColumn(Order = 14, Width = 120, Type = GridColumnType.ReadOnly, IsFilterable = false)]
        public string? AdjustmentComment { get; set; }

        
        [Display(Name = "Total Costs")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false, CssClass = "sup_text_right_align")]
        public decimal? TotalCosts { get; set; }

        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManDays { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ManYears { get; set; }

       
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? ActualManYears { get; set; }

        
        [Display(Name = "Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short Locked { get; set; }

        
        [Display(Name = "Date Fixed")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public DateTime? DateCosted { get; set; }

        
        [Display(Name = "Fixed By")]
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? CostedBy { get; set; }

        
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short ManHoursChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short PayCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short NonPayOhCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short TestCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short AnimalCostsChanged { get; set; }

        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false, CssClass = "sup_text_right_align")]
        public short NonAnimalCostsChanged { get; set; }
    }
}
