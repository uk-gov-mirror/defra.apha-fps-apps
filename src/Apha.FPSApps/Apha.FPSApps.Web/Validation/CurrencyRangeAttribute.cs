using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Validation
{
    /// <summary>
    /// Reusable range validation for currency/decimal fields.
    /// Enforces -99999999999999.9999 to 99999999999999.9999 and builds the error message
    /// from the property's display name, e.g. "Budget must be between -99999999999999.9999 and 99999999999999.9999.".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CurrencyRangeAttribute : RangeAttribute
    {
        private const string MinValue = "-99999999999999.9999";
        private const string MaxValue = "99999999999999.9999";

        public CurrencyRangeAttribute() : base(typeof(decimal), MinValue, MaxValue)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {MinValue} and {MaxValue}.";
        }
    }
}
