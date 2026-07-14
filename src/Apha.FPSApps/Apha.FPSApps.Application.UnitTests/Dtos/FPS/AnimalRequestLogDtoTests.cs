using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class AnimalRequestLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var now = new DateTime(2024, 5, 15, 9, 0, 0);

            var dto = new AnimalRequestLogDto
            {
                SequenceNo      = 10,
                JobCode         = "JC010",
                AnimalType      = "Bovine",
                NumberOfDays    = 5.5,
                NumberOfAnimals = 12.0,
                DateTime        = now,
                UserId          = "user10",
                InsertDelete    = "D",
                FpsYear         = 2024
            };

            Assert.Equal(10,       dto.SequenceNo);
            Assert.Equal("JC010",  dto.JobCode);
            Assert.Equal("Bovine", dto.AnimalType);
            Assert.Equal(5.5,      dto.NumberOfDays);
            Assert.Equal(12.0,     dto.NumberOfAnimals);
            Assert.Equal(now,      dto.DateTime);
            Assert.Equal("user10", dto.UserId);
            Assert.Equal("D",      dto.InsertDelete);
            Assert.Equal(2024,     dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new AnimalRequestLogDto
            {
                JobCode      = "JC011",
                AnimalType   = "Porcine",
                DateTime     = null,
                UserId       = null,
                InsertDelete = null
            };

            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new AnimalRequestLogDto { JobCode = "OLD", AnimalType = "A" };

            dto.JobCode         = "NEW";
            dto.AnimalType      = "Ovine";
            dto.NumberOfDays    = 3.0;
            dto.NumberOfAnimals = 20.0;
            dto.FpsYear         = 2025;

            Assert.Equal("NEW",   dto.JobCode);
            Assert.Equal("Ovine", dto.AnimalType);
            Assert.Equal(3.0,     dto.NumberOfDays);
            Assert.Equal(20.0,    dto.NumberOfAnimals);
            Assert.Equal(2025,    dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new AnimalRequestLogDto();

            Assert.Equal(0,   dto.SequenceNo);
            Assert.Equal(0.0, dto.NumberOfDays);
            Assert.Equal(0.0, dto.NumberOfAnimals);
            Assert.Equal(0,   dto.FpsYear);
        }

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new AnimalRequestLogDto();

            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void NumberOfDays_AcceptsZero()
        {
            var dto = new AnimalRequestLogDto { JobCode = "JC012", AnimalType = "A", NumberOfDays = 0.0 };

            Assert.Equal(0.0, dto.NumberOfDays);
        }

        [Fact]
        public void NumberOfAnimals_AcceptsLargeValue()
        {
            var dto = new AnimalRequestLogDto { JobCode = "JC013", AnimalType = "A", NumberOfAnimals = 9999.9 };

            Assert.Equal(9999.9, dto.NumberOfAnimals);
        }

        #endregion
    }
}
