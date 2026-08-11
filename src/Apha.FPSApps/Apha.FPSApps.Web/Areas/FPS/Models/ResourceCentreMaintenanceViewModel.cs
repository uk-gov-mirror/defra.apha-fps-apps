using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for Resource Centre maintenance page.
    /// </summary>
    public class ResourceCentreMaintenanceViewModel
    {
        /// <summary>
        /// DataGrid configuration for resource centres list.
        /// </summary>
        public DataGridConfig<ResourceCentreMaintenanceItem> ResourceCentreGrid { get; set; } = new DataGridConfig<ResourceCentreMaintenanceItem>();
    }

    /// <summary>
    /// ViewModel for individual Resource Centre records in the grid and modal.
    /// </summary>
    public class ResourceCentreMaintenanceItem
    {
        /// <summary>
        /// Profit centre identifier (primary key, case-insensitive text).
        /// Editable in Add mode, read-only in Edit mode.
        /// </summary>
        [Display(Name = "Centre")]
        [GridColumn(Width = 120, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "Centre ID is required")]
        [StringLength(50, ErrorMessage = "Centre ID cannot exceed 50 characters")]
        public string ProfitCentreId { get; set; } = null!;

        /// <summary>
        /// Profit centre display name.
        /// </summary>
        [Display(Name = "RC Name")]
        [GridColumn(Width = 250, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "RC Name is required")]
        [StringLength(40, ErrorMessage = "RC Name cannot exceed 40 characters")]
        public string ProfitCentreName { get; set; } = null!;

        /// <summary>
        /// Division name (foreign key).
        /// </summary>
        [Display(Name = "Division")]
        [GridColumn(Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        [Required(ErrorMessage = "Division is required")]
        public string Division { get; set; } = null!;

        /// <summary>
        /// Contribution target monetary value.
        /// </summary>
        [Display(Name = "Contribution Target")]
        [GridColumn(Width = 160, Type = GridColumnType.GbpValue)]
        [CurrencyRange]
        public decimal? ContTarget { get; set; }

        /// <summary>
        /// Name of the profit centre head (manager).
        /// </summary>
        [Display(Name = "RC Head")]
        [GridColumn(Width = 200, Type = GridColumnType.Text)]
        public string? ProfitCentreHead { get; set; }

        /// <summary>
        /// Division identifier.
        /// </summary>
        [Display(Name = "Division ID")]
        [GridColumn(Width = 100, Type = GridColumnType.Number, IsVisible = false)]
        public int? DivisionId { get; set; }

        /// <summary>
        /// Email recipient for notifications.
        /// </summary>
        [Display(Name = "Email Recipient")]
        [GridColumn(Width = 220, Type = GridColumnType.Text)]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? EmailRecipient { get; set; }

            }
        }
