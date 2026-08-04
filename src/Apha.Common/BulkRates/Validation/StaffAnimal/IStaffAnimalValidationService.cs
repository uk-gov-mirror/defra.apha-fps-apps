namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// The shared, deterministic Staff/Animal validation service — a parallel
    /// service to <see cref="IBulkRatesValidationService"/>, deliberately not forced into that
    /// FEC/AGRUP-shaped context (different action vocabulary, no Insert/ZeroRateWithdrawal, no
    /// project/capability routing concerns). Intended to be called identically by the API
    /// validator and the worker's revalidation pass once wired in, so validation
    /// cannot drift between the two call sites.
    /// </summary>
    public interface IStaffAnimalValidationService
    {
        /// <summary>
        /// Deterministic and side-effect free: the same context always produces the same
        /// result, and this never writes staging/audit/status — writing is the caller's job.
        /// </summary>
        StaffAnimalValidationResult Validate(StaffAnimalValidationContext context);
    }
}
