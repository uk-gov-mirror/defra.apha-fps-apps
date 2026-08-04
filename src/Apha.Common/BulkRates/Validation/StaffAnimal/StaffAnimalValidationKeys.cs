namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// Single place that defines how Staff/Animal business keys are normalized for lookup, so
    /// StaffAnimalValidationContext builders (the caller's own repository code) and
    /// StaffAnimalValidationService always agree on what "the same key" means — mirrors
    /// BulkRatesValidationKeys' role for FEC/AGRUP.
    /// </summary>
    public static class StaffAnimalValidationKeys
    {
        public static string PcGrade(string pcGrade) => pcGrade.ToUpperInvariant();

        public static string AnimalType(string animalType) => animalType.ToUpperInvariant();
    }
}
