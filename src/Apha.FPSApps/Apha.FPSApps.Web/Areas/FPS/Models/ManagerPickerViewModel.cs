using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// View model for the shared searchable Manager picker partial (_ManagerPicker).
    /// Extracted to remove duplicated markup/script across maintenance and project views.
    /// </summary>
    public class ManagerPickerViewModel
    {
        public List<SelectListItem> ManagerList { get; set; } = new();

        /// <summary>The bound field/select name (defaults to "Manager").</summary>
        public string FieldName { get; set; } = "Manager";

        /// <summary>When true a third "Grade" column is rendered in the dropdown.</summary>
        public bool ShowGrade { get; set; }

        /// <summary>When true the hidden select is marked as required.</summary>
        public bool Required { get; set; }

        /// <summary>Css classes applied to the hidden select element.</summary>
        public string SelectCssClass { get; set; } = "govuk-select govuk-!-font-size-16 sup_width_100";
    }
}
