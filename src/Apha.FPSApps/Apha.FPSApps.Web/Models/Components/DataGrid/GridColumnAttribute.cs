namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GridColumnAttribute : Attribute
    {
        public int Order { get; set; } = int.MaxValue;
        public int Width { get; set; } = 100;
        public GridColumnType Type { get; set; } = GridColumnType.Text;
        public bool IsVisible { get; set; } = true;
        public bool IsFilterable { get; set; } = false;
        public string CssClass { get; set; } = string.Empty;

        /// <summary>
        /// For Type = Badge: name of a sibling property on the same row holding the per-row
        /// govuk-tag colour modifier (e.g. "govuk-tag--yellow"), computed by the mapping layer.
        /// The sibling property carries no GridColumn attribute of its own — it's a data
        /// carrier only, never rendered as its own column.
        /// </summary>
        public string? CssClassSourceProperty { get; set; }
    }
}
