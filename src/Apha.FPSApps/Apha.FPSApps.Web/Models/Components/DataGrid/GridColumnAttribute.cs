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
    }
}
