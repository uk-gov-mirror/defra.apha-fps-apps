using Apha.FPSApps.Web.Models.Components.DataGrid;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.Components.DataGrid
{
    public class GridHelpersTests
    {
        private sealed class SimpleRow
        {
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        private sealed class DynamicRow
        {
            public string Key { get; set; } = string.Empty;
            public IDictionary<string, object?> Values { get; } = new Dictionary<string, object?>();
        }

        private sealed class NonDictionaryValuesRow
        {
            public string Values { get; set; } = "not-a-dictionary";
        }

        [Fact]
        public void GetPropertyValue_ReturnsValue_WhenPropertyExists()
        {
            var row = new SimpleRow { Name = "Acme", Amount = 42m };

            Assert.Equal("Acme", GridHelpers.GetPropertyValue(row, nameof(SimpleRow.Name)));
            Assert.Equal(42m, GridHelpers.GetPropertyValue(row, nameof(SimpleRow.Amount)));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_ForMissingProperty_EvenWhenValuesDictionaryPresent()
        {
            // Dynamic/pivot rows are now dictionary-backed and resolved via the
            // IDictionary overload; the reflection-based "Values" fallback is not used.
            var row = new DynamicRow { Key = "K1" };
            row.Values["WG1"] = 100m;

            Assert.Null(GridHelpers.GetPropertyValue(row, "WG1"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsDynamicValue_FromDictionaryRow()
        {
            var row = new Dictionary<string, string?>
            {
                ["Key"] = "K1",
                ["WG1"] = "100",
                ["WG2"] = null
            };

            Assert.Equal("100", GridHelpers.GetPropertyValue(row, "WG1"));
            Assert.Null(GridHelpers.GetPropertyValue(row, "WG2"));
            Assert.Null(GridHelpers.GetPropertyValue(row, "DoesNotExist"));
        }

        [Fact]
        public void GetPropertyValue_PrefersRealProperty_OverValuesDictionary()
        {
            var row = new DynamicRow { Key = "K1" };
            row.Values["Key"] = "from-dictionary";

            // "Key" is a real property, so the property value wins over the dictionary entry.
            Assert.Equal("K1", GridHelpers.GetPropertyValue(row, "Key"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenPropertyMissingAndNotInValues()
        {
            var row = new DynamicRow { Key = "K1" };

            Assert.Null(GridHelpers.GetPropertyValue(row, "DoesNotExist"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenPropertyMissingAndNoValuesDictionary()
        {
            var row = new SimpleRow { Name = "Acme" };

            Assert.Null(GridHelpers.GetPropertyValue(row, "Missing"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenValuesPropertyIsNotDictionary()
        {
            var row = new NonDictionaryValuesRow();

            Assert.Null(GridHelpers.GetPropertyValue(row, "Missing"));
        }
    }
}
