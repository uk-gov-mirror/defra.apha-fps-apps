namespace Apha.Common.Utilities.ExcelImport
{
    public static class ExcelValidationHelper
    {        
        public static void ValidateRequired(string? value, string fieldName, List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(value))
                failures.Add($"{fieldName} is required.");
        }

        public static void ValidateRequiredDecimal(string? value, string fieldName, List<string> failures, bool required = true)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    failures.Add($"{fieldName} is required.");
                return;
            }

            if (!ExcelParseHelper.TryParseDecimal(value).HasValue)
                failures.Add($"{fieldName} must be a valid decimal number.");
        }

        public static void ValidateNonNegativeInteger(string? value, string fieldName, List<string> failures, bool required = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    failures.Add($"{fieldName} is required.");
                return;
            }

            var parsed = ExcelParseHelper.TryParseInt(value);
            if (!parsed.HasValue)
            {
                failures.Add($"{fieldName} must be a valid whole number.");
                return;
            }

            if (parsed.Value < 0)
                failures.Add($"{fieldName} cannot be negative.");
        }

        public static void ValidateNonNegativeDecimal(string? value, string fieldName, List<string> failures, bool required = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    failures.Add($"{fieldName} is required.");
                return;
            }

            var parsed = ExcelParseHelper.TryParseDecimal(value);
            if (!parsed.HasValue)
            {
                failures.Add($"{fieldName} must be a valid decimal number.");
                return;
            }

            if (parsed.Value < 0)
                failures.Add($"{fieldName} cannot be negative.");
        }

        public static void ValidateDecimal(string? value, string fieldName, List<string> failures, bool required = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (required)
                    failures.Add($"{fieldName} is required.");
                return;
            }

            var parsed = ExcelParseHelper.TryParseDecimal(value);
            if (!parsed.HasValue)
            {
                failures.Add($"{fieldName} must be a valid decimal number.");
                return;
            }
        }

        public static void ValidateMonth(string? value, List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                failures.Add("Month is required.");
                return;
            }

            var month = ExcelParseHelper.TryParseDouble(value);
            if (!month.HasValue)
            {
                failures.Add("Month must be a valid number.");
                return;
            }

            if (month.Value < 1 || month.Value > 12)
                failures.Add("Month must be between 1 and 12.");
        }

        public static void ValidateInSet<T>(T? value, HashSet<T> validSet, string fieldName, List<string> failures) where T : notnull
        {
            if (value == null)
            {
                failures.Add($"{fieldName} is required.");
                return;
            }

            if (!validSet.Contains(value))
                failures.Add($"{fieldName} '{value}' does not exist.");
        }

        public static void ValidateStringInSet(string? value, HashSet<string> validSet, string fieldName, List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                failures.Add($"{fieldName} is required.");
                return;
            }

            if (!validSet.Contains(value))
                failures.Add($"{fieldName} '{value}' does not exist.");
        }

        public static void ValidateRange<T>(T? value, T min, T max, string fieldName, List<string> failures) where T : struct, IComparable<T>
        {
            if (!value.HasValue)
                return;

            if (value.Value.CompareTo(min) < 0 || value.Value.CompareTo(max) > 0)
                failures.Add($"{fieldName} must be between {min} and {max}.");
        }

        public static void ValidateMaxLength(string? value, int maxLength, string fieldName, List<string> failures)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (value.Length > maxLength)
                failures.Add($"{fieldName} cannot exceed {maxLength} characters.");
        }
    }
}
