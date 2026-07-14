namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public static class GridHelpers
    {
        private const string GridReadonlyCssClass = "grid-readonly";

        public static object? GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                if (obj == null) return null;
                var type = obj.GetType();
                var prop = type.GetProperty(propertyName);
                return prop?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }

        public static string GetTypeCssClass(DataGridColumn column)
        {
            return column.ColumnType switch
            {
                GridColumnType.Date => GridReadonlyCssClass,
                GridColumnType.DateTime => GridReadonlyCssClass,
                GridColumnType.Number => GridReadonlyCssClass,
                GridColumnType.Text => GridReadonlyCssClass,
                GridColumnType.Dropdown => GridReadonlyCssClass,
                GridColumnType.Checkbox => "grid-input grid-checkbox",
                _ => string.Empty
            };
        }

        public static string FormatValue(object? value, DataGridColumn column)
        {
            if (value == null) return string.Empty;

            switch (column.ColumnType)
            {
                case GridColumnType.DecimalNumber:
                    if (value is decimal decValue)
                        return decValue.ToString("F2");
                    break;
                case GridColumnType.Date:
                    if (value is DateTime dateValue)
                        return dateValue.ToString(column.DateFormat ?? "yyyy-MM-dd");
                    break;
                case GridColumnType.DateTime:
                    if (value is DateTime dateTimeValue)
                        return dateTimeValue.ToString(column.DateTimeFormatHhMm ?? "yyyy-MM-dd HH:mm");
                    break;
                case GridColumnType.UsdValue:
                    if (value is decimal usdValue)
                        return usdValue.ToString("C", new System.Globalization.CultureInfo("en-US"));
                    break;
                case GridColumnType.GbpValue:
                    if (value is decimal gbpValue)
                        return gbpValue.ToString("£#,##0.00;-£#,##0.00");
                    if (value is double gbpDouble)
                        return gbpDouble.ToString("£#,##0.00;-£#,##0.00");
                    break;
                case GridColumnType.GbpValueRounded:
                    if (value is decimal gbpRounded)
                        return Math.Round(gbpRounded, MidpointRounding.AwayFromZero).ToString("£#,##0;-£#,##0");
                    if (value is double gbpRoundedDouble)
                        return Math.Round(gbpRoundedDouble, MidpointRounding.AwayFromZero).ToString("£#,##0;-£#,##0");
                    break;
                case GridColumnType.DoubleNumber:
                    if (value is double doubleValue)
                        return doubleValue.ToString("F2");
                    break;
            }

            return value.ToString() ?? string.Empty;
        }
    }
}
