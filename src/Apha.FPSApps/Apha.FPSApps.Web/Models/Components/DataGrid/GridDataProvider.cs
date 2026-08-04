using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public static class GridDataProvider
    {
        public static List<DataGridColumn> GetColumnsDefination<T>(
            Dictionary<string, List<SelectListItem>>? filterOptionsSource = null)
        {
            var columns = new List<DataGridColumn>();
            foreach (var prop in typeof(T).GetProperties()
                         .OrderBy(p => p.GetCustomAttribute<GridColumnAttribute>()?.Order ?? int.MaxValue))
            {
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var columnAttr = prop.GetCustomAttribute<GridColumnAttribute>();
                var isFilterable = columnAttr?.IsFilterable ?? false;
                Dictionary<string, string>? filterOptions = null;

                if (filterOptionsSource != null && filterOptionsSource.TryGetValue(prop.Name, out var selectList))
                {
                    filterOptions = selectList.ToDictionary(x => x.Value ?? "", x => x.Text ?? "");
                }

                columns.Add(new DataGridColumn
                {
                    PropertyName = prop.Name,
                    DisplayName = displayAttr?.Name ?? prop.Name,
                    IsVisible = columnAttr?.IsVisible ?? true,
                    Width = columnAttr?.Width ?? 100,
                    ColumnType = columnAttr?.Type ?? GridColumnType.Text,
                    IsFilterable = isFilterable,
                    FilterType = columnAttr?.Type,
                    FilterOptions = filterOptions,
                    CssClass = columnAttr?.CssClass ?? string.Empty,
                    CssClassSourceProperty = columnAttr?.CssClassSourceProperty
                });
            }
            return columns;
        }
    }
}
