using Apha.FPSApps.Web.Validation;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// DataGrid row model and Add/Edit modal partial model for WorkGroup Maintenance.
    /// Properties mirror <see cref="Apha.FPSApps.Application.Dtos.PACT.WorkGroupDto"/>
    /// for AutoMapper convention-based mapping in FpsViewModelMapper.
    /// Derived from JS <c>initializeWGTable()</c> columns array in fps_workgroup_maintenance.js.
    /// </summary>
    public class WorkgroupMaintenanceItem
    {
        // TRANSFORMENGINE: WorkGroupName — PK component; visible grid column per JS columns[0] { field:'workGroup', header:'WorkGroup', width:150 }
        // Also used as KeyProperty in DataGridConfig. Visible because it appears in JS columns array.
        [Display(Name = "WorkGroup")]
        [Required(ErrorMessage = "WorkGroup is required")]
        [GridColumn(Width = 150, Type = GridColumnType.Text, IsFilterable = true)]
        public string WorkGroupName { get; set; } = null!;

        // TRANSFORMENGINE: ProfitCentre — JS columns[1] { field:'resourceCentre', header:'ResourceCentre', width:170 }
        // HTML label "ResourceCentre"; modal uses AJAX GET /FPS/WorkgroupMaintenance/GetProfitCentres
        [Display(Name = "ResourceCentre")]
        [Required(ErrorMessage = "ResourceCentre is required")]
        [GridColumn(Width = 170, Type = GridColumnType.Text, IsFilterable = true)]
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: CostCentre — JS columns[2] { field:'costCentre', header:'CostCentre', width:150 }
        // Optional; cascading dropdown in modal filtered by ProfitCentre via AJAX GET /FPS/WorkgroupMaintenance/GetCostCentres
        [Display(Name = "CostCentre")]
        [GridColumn(Width = 150, Type = GridColumnType.DecimalNumber, IsFilterable = true)]
        public double? CostCentre { get; set; }

        // TRANSFORMENGINE: Owner — JS columns[3] { field:'owner', header:'Owner', width:180 }
        // Optional; modal uses AJAX GET /FPS/WorkgroupMaintenance/GetOwners → ManagerDto.Name
        [Display(Name = "Owner")]
        [GridColumn(Width = 180, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Owner { get; set; }

        // TRANSFORMENGINE: Description — JS columns[4] { field:'description', header:'Description', width:260 }
        [Display(Name = "Description")]
        [GridColumn(Width = 260, Type = GridColumnType.Text, IsFilterable = true)]
        public string? Description { get; set; }

        // TRANSFORMENGINE: CentralOverhead — JS columns[5] { field:'centralOverhead', header:'CentralOverhead', width:170 }
        // JS prototype formats this as '£N.NN'; GBP value column
        [Display(Name = "CentralOverhead")]
        [GridColumn(Width = 170, Type = GridColumnType.GbpValue, IsFilterable = false)]
        [CurrencyRange]
        public decimal? CentralOverhead { get; set; }

        // ── Audit / non-grid fields — carried for modal round-trip, not displayed in main grid ─────

        // TRANSFORMENGINE: SendEmail — not in JS grid columns; DB smallint; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short? SendEmail { get; set; }

        // TRANSFORMENGINE: Cos90 — not in JS grid columns; DB smallint; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public short? Cos90 { get; set; }

        // TRANSFORMENGINE: CostCentreOld — not in JS grid columns; historical reference; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public double? CostCentreOld { get; set; }

        // TRANSFORMENGINE: EmailRecipient — not in JS grid columns; notification address; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public string? EmailRecipient { get; set; }

        // TRANSFORMENGINE: FpsYear — partition key; auto-resolved server-side; hidden from grid
        [GridColumn(Type = GridColumnType.ReadOnly, IsVisible = false)]
        public int? FpsYear { get; set; }
    }
}
