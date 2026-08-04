namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// The mutable Staff fields, used for both the source (live) and
    /// effective (target) state of a <see cref="StaffValidationResult"/>. Values are already
    /// normalized (blank/null coalesced to 0) — never the raw staged
    /// value.
    /// </summary>
    public sealed record StaffFieldState
    {
        public required decimal PayRate { get; init; }
        public required decimal Npr { get; init; }
        public required decimal Ohr { get; init; }
    }
}
