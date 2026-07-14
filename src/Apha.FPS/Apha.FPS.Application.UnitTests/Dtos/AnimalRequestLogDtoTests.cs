using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class AnimalRequestLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange
            var dateTime = new DateTime(2024, 3, 10, 9, 0, 0);

            // Act
            var dto = new AnimalRequestLogDto
            {
                SequenceNo      = 5,
                JobCode         = "JOBANI01",
                AnimalType      = "Cattle",
                NumberOfDays    = 14.5,
                NumberOfAnimals = 25.0,
                DateTime        = dateTime,
                UserId          = "user456",
                InsertDelete    = "I",
                FpsYear         = 2024
            };

            // Assert
            dto.SequenceNo.Should().Be(5);
            dto.JobCode.Should().Be("JOBANI01");
            dto.AnimalType.Should().Be("Cattle");
            dto.NumberOfDays.Should().Be(14.5);
            dto.NumberOfAnimals.Should().Be(25.0);
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("user456");
            dto.InsertDelete.Should().Be("I");
            dto.FpsYear.Should().Be(2024);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new AnimalRequestLogDto
            {
                SequenceNo      = 1,
                JobCode         = "JOB001",
                AnimalType      = "Sheep",
                NumberOfDays    = 7.0,
                NumberOfAnimals = 10.0,
                DateTime        = null,
                UserId          = null,
                InsertDelete    = null,
                FpsYear         = 2024
            };

            // Assert
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            // Arrange
            var dto = new AnimalRequestLogDto
            {
                SequenceNo      = 1,
                JobCode         = "OLDJOB",
                AnimalType      = "Pig",
                NumberOfDays    = 5.0,
                NumberOfAnimals = 3.0,
                FpsYear         = 2022
            };

            // Act
            var updated = new DateTime(2025, 6, 1);
            dto.SequenceNo      = 42;
            dto.JobCode         = "NEWJOB";
            dto.AnimalType      = "Poultry";
            dto.NumberOfDays    = 30.0;
            dto.NumberOfAnimals = 500.0;
            dto.DateTime        = updated;
            dto.UserId          = "updater";
            dto.InsertDelete    = "D";
            dto.FpsYear         = 2025;

            // Assert
            dto.SequenceNo.Should().Be(42);
            dto.JobCode.Should().Be("NEWJOB");
            dto.AnimalType.Should().Be("Poultry");
            dto.NumberOfDays.Should().Be(30.0);
            dto.NumberOfAnimals.Should().Be(500.0);
            dto.DateTime.Should().Be(updated);
            dto.UserId.Should().Be("updater");
            dto.InsertDelete.Should().Be("D");
            dto.FpsYear.Should().Be(2025);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new AnimalRequestLogDto();

            dto.SequenceNo.Should().Be(0);
            dto.NumberOfDays.Should().Be(0.0);
            dto.NumberOfAnimals.Should().Be(0.0);
            dto.FpsYear.Should().Be(0);
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
        }

        [Fact]
        public void AnimalType_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new AnimalRequestLogDto { JobCode = "J", AnimalType = string.Empty };

            dto.AnimalType.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.5)]
        [InlineData(365.0)]
        public void NumberOfDays_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new AnimalRequestLogDto { JobCode = "J", AnimalType = "Cattle", NumberOfDays = value };

            dto.NumberOfDays.Should().Be(value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(10000.0)]
        public void NumberOfAnimals_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new AnimalRequestLogDto { JobCode = "J", AnimalType = "Cattle", NumberOfAnimals = value };

            dto.NumberOfAnimals.Should().Be(value);
        }

        #endregion
    }
}
