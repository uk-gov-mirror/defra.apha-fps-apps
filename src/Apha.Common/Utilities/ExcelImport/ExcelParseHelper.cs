using System.Globalization;

namespace Apha.Common.Utilities.ExcelImport
{    
    public static class ExcelParseHelper
    {       
        public static double? TryParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        public static decimal? TryParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        public static int? TryParseInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null;
        }

        public static DateTime? TryParseDateTime(string? value, string? format = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!string.IsNullOrEmpty(format))
            {
                return DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
                    ? parsed
                    : null;
            }

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDefault)
                ? parsedDefault
                : null;
        }

        public static bool? TryParseBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Handle common representations
            var normalized = value.Trim().ToLowerInvariant();
            return normalized switch
            {
                "true" or "1" or "yes" or "y" => true,
                "false" or "0" or "no" or "n" => false,
                _ => bool.TryParse(value, out var parsed) ? parsed : null
            };
        }
    }
}
