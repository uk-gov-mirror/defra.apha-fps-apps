using Apha.FPSApps.Web.Models.Components.DataGrid;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestListVlaViewModel
    {
        public int FpsYear { get; set; }

        // AllowAdd/Edit/Delete = true (CRUD modals present in HTML prototype: vlaTestListModal, vlaDeleteModal)
        public DataGridConfig<TestListVlaItem> TestListGrid { get; set; } = new();

        // CRUD sub-resource: TestRequirements per selected TestListVla item
        public DataGridConfig<TestRequirementItem> TestRequirementsGrid { get; set; } = new();

        // CRUD sub-resource: TestRCCost per selected TestListVla item
        public DataGridConfig<TestRCCostItem> ComponentChargesGeneralGrid { get; set; } = new();

        // CRUD sub-resource: TestRequirementRCCost per selected TestListVla item
        public DataGridConfig<TestRequirementRCCostItem> ComponentChargesProjectGrid { get; set; } = new();

        // CRUD sub-resource: TestCapability per selected TestListVla item
        public DataGridConfig<TestCapabilityItem> SuppliersGrid { get; set; } = new();
    }
}
