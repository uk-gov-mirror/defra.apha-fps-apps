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

        public static bool IsNumericColumn(GridColumnType columnType)
        {
            return columnType switch
            {
                GridColumnType.Number => true,
                GridColumnType.DecimalNumber => true,
                GridColumnType.DoubleNumber => true,
                GridColumnType.UsdValue => true,
                GridColumnType.GbpValue => true,
                GridColumnType.GbpValueRounded => true,
                GridColumnType.Percentage => true,
                GridColumnType.RoundTwoDecimal => true,
                _ => false
            };
        }

        public static string GetAlignmentCssClass(DataGridColumn column)
        {
            // A custom CssClass specified at the property level takes precedence
            // and overrides the default numeric right-alignment.
            if (!string.IsNullOrWhiteSpace(column.CssClass))
                return string.Empty;

            return IsNumericColumn(column.ColumnType) ? "govuk-table__cell--numeric" : string.Empty;
        }

        public static string GetHeaderAlignmentCssClass(DataGridColumn column)
        {
            // A custom CssClass specified at the property level takes precedence
            // and overrides the default numeric right-alignment.
            if (!string.IsNullOrWhiteSpace(column.CssClass))
                return string.Empty;

            return IsNumericColumn(column.ColumnType) ? "govuk-table__header--numeric" : string.Empty;
        }

        /// <summary>
        /// Overload for dynamic dictionary-backed rows (e.g. Plan Test CrossTab).
        /// The DLR picks this overload at runtime when the row is a Dictionary,
        /// leaving the existing reflection-based overload untouched for all other grids.
        /// </summary>
        public static object? GetPropertyValue(IDictionary<string, string?> dict, string propertyName)
        {
            if (dict == null || string.IsNullOrEmpty(propertyName))
                return null;

            return dict.TryGetValue(propertyName, out var value) ? value : null;
        }

        /// <summary>
        /// Retrieves a cell value from a dynamic dictionary-backed row (e.g. CrossTab).
        /// Returns an empty string when the key is absent or the value is null.
        /// </summary>
        public static string GetPropertyCrossTab(Dictionary<string, string?> row, string propertyName)
        {
            if (row == null || string.IsNullOrEmpty(propertyName))
                return string.Empty;

            return row.TryGetValue(propertyName, out var value) ? value ?? string.Empty : string.Empty;
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
                case GridColumnType.Percentage:
                    if (value is double pctDouble)
                        return (pctDouble < 1 ? pctDouble * 100 : pctDouble).ToString("F2") + "%";
                    if (value is decimal pctDecimal)
                        return (pctDecimal < 1 ? pctDecimal * 100 : pctDecimal).ToString("F2") + "%";
                    if (value is float pctFloat)
                        return (((double)pctFloat) < 1 ? ((double)pctFloat) * 100 : ((double)pctFloat)).ToString("F2") + "%";
                    break;
                case GridColumnType.RoundTwoDecimal:
                    if (value is decimal rtdDecimal)
                        return rtdDecimal.ToString("£#,##0.00;-£#,##0.00");
                    if (value is double rtdDouble)
                        return rtdDouble.ToString("£#,##0.00;-£#,##0.00");
                    var raw = value.ToString();
                    if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var parsed))
                        return parsed.ToString("£#,##0.00;-£#,##0.00");
                    return raw ?? string.Empty;
                }

            return value.ToString() ?? string.Empty;
        }
    }
}
