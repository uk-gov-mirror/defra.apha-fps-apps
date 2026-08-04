namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// One staged Staff row's classification: "business key, source state, effective
    /// state, action, errors, validation_version" — bundled per row here, unlike FEC/AGRUP's
    /// separate classification/error findings, since this validation is deliberately not forced into that
    /// shape.
    /// </summary>
    public sealed record StaffValidationResult
    {
        public required string PcGrade { get; init; }

        /// <summary>One of <see cref="StaffAnimalCalculatedAction"/>.</summary>
        public required string Action { get; init; }

        /// <summary>The live state at validation time. Null when <see cref="Action"/> is <see cref="StaffAnimalCalculatedAction.NotFound"/> — there is no live row to describe.</summary>
        public StaffFieldState? Source { get; init; }

        /// <summary>The normalized target state this row requests, regardless of whether it could be resolved/applied.</summary>
        public StaffFieldState? Effective { get; init; }

        public required int ValidationVersion { get; init; }

        public IReadOnlyList<ValidationFinding> Errors { get; init; } = [];
    }
}
