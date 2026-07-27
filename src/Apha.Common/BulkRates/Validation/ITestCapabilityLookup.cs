namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// DR-VAL-02: a typed TestCode+WorkGroup capability existence check, querying
    /// fps.tlkptestcapability on (testcode, workgroup) as two real columns — never a
    /// concatenated string, unlike Apha.PACT.Application.Services.TestCapabilityService's
    /// existing ExistsByTestBuyerCodeAsync(dto.TestCode + dto.WorkGroup) call sites, which
    /// this does not replace (plan §3, §7.1). Implemented in Apha.FPS.DataAccess (and, from
    /// D4 onward, Apha.BatchJobs.Infrastructure) by querying fps.tlkptestcapability directly —
    /// no Apha.PACT project reference, per the D2 addendum in the plan doc.
    ///
    /// This single-pair contract is DR-VAL-02's own literal shape (plan §3). Building
    /// ValidationContext.CapabilityLookup in bulk for many rows at once is a separate concern
    /// on the caller's own repository (plan §3.2/§7.6 — bulk, not one query per row); this
    /// interface does not need a bulk method itself to satisfy that.
    /// </summary>
    public interface ITestCapabilityLookup
    {
        Task<bool> ExistsAsync(string testCode, string workGroup, int fpsYear, CancellationToken ct = default);
    }
}
