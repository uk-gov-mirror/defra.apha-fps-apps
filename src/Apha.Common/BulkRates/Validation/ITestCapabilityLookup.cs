namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// A typed TestCode+WorkGroup capability existence check, querying
    /// fps.tlkptestcapability on (testcode, workgroup) as two real columns — never a
    /// concatenated string, unlike Apha.PACT.Application.Services.TestCapabilityService's
    /// existing ExistsByTestBuyerCodeAsync(dto.TestCode + dto.WorkGroup) call sites, which
    /// this does not replace. Implemented in Apha.FPS.DataAccess (and, from
    /// D4 onward, Apha.BatchJobs.Infrastructure) by querying fps.tlkptestcapability directly —
    /// no Apha.PACT project reference.
    ///
    /// This single-pair contract is its own literal shape. Building
    /// ValidationContext.CapabilityLookup in bulk for many rows at once is a separate concern
    /// on the caller's own repository (bulk, not one query per row); this
    /// interface does not need a bulk method itself to satisfy that.
    /// </summary>
    public interface ITestCapabilityLookup
    {
        Task<bool> ExistsAsync(string testCode, string workGroup, int fpsYear, CancellationToken ct = default);
    }
}
