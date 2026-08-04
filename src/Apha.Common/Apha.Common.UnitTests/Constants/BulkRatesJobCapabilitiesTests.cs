using Apha.Common.Constants;
using FluentAssertions;

namespace Apha.Common.UnitTests.Constants
{
    /// <summary>
    /// Staff/Animal joined FEC in RequiresDownloadVersion once their request-scoped download
    /// route shipped — see BulkRatesJobCapabilities' own doc comment for why this was deferred
    /// until then.
    /// </summary>
    public class BulkRatesJobCapabilitiesTests
    {
        [Fact]
        public void RequiresDownloadVersion_ForFec_ReturnsTrue()
        {
            BulkRatesJobCapabilities.RequiresDownloadVersion(BulkRatesJobNames.Fec).Should().BeTrue();
        }

        [Fact]
        public void RequiresDownloadVersion_ForStaff_ReturnsTrue()
        {
            BulkRatesJobCapabilities.RequiresDownloadVersion(BulkRatesJobNames.Staff).Should().BeTrue();
        }

        [Fact]
        public void RequiresDownloadVersion_ForAnimal_ReturnsTrue()
        {
            BulkRatesJobCapabilities.RequiresDownloadVersion(BulkRatesJobNames.Animal).Should().BeTrue();
        }

        [Fact]
        public void RequiresDownloadVersion_IsCaseInsensitive()
        {
            BulkRatesJobCapabilities.RequiresDownloadVersion(BulkRatesJobNames.Fec.ToLowerInvariant()).Should().BeTrue();
        }

        [Fact]
        public void RequiresDownloadVersion_ForUnknownJobName_ReturnsFalse()
        {
            BulkRatesJobCapabilities.RequiresDownloadVersion("SomeOtherJob").Should().BeFalse();
        }
    }
}
