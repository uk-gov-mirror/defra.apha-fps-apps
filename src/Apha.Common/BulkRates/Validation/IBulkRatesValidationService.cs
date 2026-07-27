namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// DR-VAL-01: the shared domain validation service called identically by the API
    /// validator and the worker's revalidation pass (reconciliation §8), so validation
    /// cannot drift between the two call sites. See ValidationContext/ValidationFinding
    /// and plan §3/§3.1/§3.2 for the contract this implements.
    /// </summary>
    public interface IBulkRatesValidationService
    {
        /// <summary>
        /// Deterministic and side-effect free (§3.2): the same context always produces the
        /// same findings, and this never writes staging/audit/status — writing is the
        /// caller's job.
        /// </summary>
        IReadOnlyList<ValidationFinding> Validate(ValidationContext context);
    }
}
