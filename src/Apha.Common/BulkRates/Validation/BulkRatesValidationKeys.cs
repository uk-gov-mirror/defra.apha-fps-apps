namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// Single place that defines how composite (TestCode, Buyer)/(TestCode, WorkGroup) keys
    /// are normalized for lookup, so ValidationContext builders (the caller's own repository
    /// code) and BulkRatesValidationService always agree on what "the same key" means —
    /// mirroring §3.1's "exactly one rule, not two" principle for key comparison instead of
    /// just rate classification. Case-insensitive to match the citext columns these values
    /// come from (fps.tblstagingtestorproduct/tblstagingtlkptestreqmt); not trimmed, since
    /// the underlying citext columns are not trimmed either (plan comparison note).
    /// </summary>
    public static class BulkRatesValidationKeys
    {
        public static string TestCode(string testCode) => testCode.ToUpperInvariant();

        public static (string TestCode, string Buyer) AgrupKey(string testCode, string buyer)
            => (testCode.ToUpperInvariant(), buyer.ToUpperInvariant());

        public static (string TestCode, string WorkGroup) CapabilityKey(string testCode, string workGroup)
            => (testCode.ToUpperInvariant(), workGroup.ToUpperInvariant());

        public static (string TestCode, string? Buyer) SnapshotKey(string testCode, string? buyer)
            => (testCode.ToUpperInvariant(), buyer?.ToUpperInvariant());
    }
}
