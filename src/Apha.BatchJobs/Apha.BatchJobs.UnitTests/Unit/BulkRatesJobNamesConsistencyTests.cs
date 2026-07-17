using Apha.BatchJobs.Domain.Constants;
using Apha.Common.Constants;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Guards against drift between the worker's BatchJobNames and the API/Web-side
/// BulkRatesJobNames — both define the same three job name strings independently
/// because the worker project intentionally does not reference Apha.Common.
/// </summary>
public sealed class BulkRatesJobNamesConsistencyTests
{
    [Fact]
    public void Fec_MatchesAcrossBothConstantSources()
        => Assert.Equal(BulkRatesJobNames.Fec, BatchJobNames.BulkTestRatesUpdate);

    [Fact]
    public void Staff_MatchesAcrossBothConstantSources()
        => Assert.Equal(BulkRatesJobNames.Staff, BatchJobNames.BulkStaffRatesUpdate);

    [Fact]
    public void Animal_MatchesAcrossBothConstantSources()
        => Assert.Equal(BulkRatesJobNames.Animal, BatchJobNames.BulkAnimalRatesUpdate);
}
