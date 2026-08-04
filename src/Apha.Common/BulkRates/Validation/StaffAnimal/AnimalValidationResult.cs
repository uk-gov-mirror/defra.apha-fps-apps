namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// One staged Animal row's classification — see <see cref="StaffValidationResult"/> for the
    /// per-row shape rationale.
    /// </summary>
    public sealed record AnimalValidationResult
    {
        public required string AnimalType { get; init; }

        /// <summary>One of <see cref="StaffAnimalCalculatedAction"/>.</summary>
        public required string Action { get; init; }

        /// <summary>The live state at validation time. Null when <see cref="Action"/> is <see cref="StaffAnimalCalculatedAction.NotFound"/> — there is no live row to describe.</summary>
        public AnimalFieldState? Source { get; init; }

        /// <summary>The normalized target state this row requests, regardless of whether it could be resolved/applied.</summary>
        public AnimalFieldState? Effective { get; init; }

        public required int ValidationVersion { get; init; }

        public IReadOnlyList<ValidationFinding> Errors { get; init; } = [];
    }
}
