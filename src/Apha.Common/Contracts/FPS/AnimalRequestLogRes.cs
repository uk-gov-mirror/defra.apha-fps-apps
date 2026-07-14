namespace Apha.Common.Contracts.FPS
{
    // Source: fps.animalreq_log table + initializeAnimalRequirementChangesTable() in projectaudit_trail.js
    public class AnimalRequestLogRes
    {
        public string JobCode { get; set; } = null!;

        public string AnimalType { get; set; } = null!;

        public double NumberOfDays { get; set; }

        public double NumberOfAnimals { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? UserEmail { get; set; }

        public string? InsertDelete { get; set; }
    }
}
