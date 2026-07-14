using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class StaffJobLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange
            var dateTime = new DateTime(2024, 8, 20, 14, 45, 0);

            // Act
            var dto = new StaffJobLogDto
            {
                SequenceNo   = 10,
                StaffId      = "STAFF001",
                JobCode      = "JOB100",
                PlannedHours = 37.5,
                DateTime     = dateTime,
                UserId       = "manager01",
                InsertDelete = "I",
                FpsYear      = 2024
            };

            // Assert
            dto.SequenceNo.Should().Be(10);
            dto.StaffId.Should().Be("STAFF001");
            dto.JobCode.Should().Be("JOB100");
            dto.PlannedHours.Should().Be(37.5);
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("manager01");
            dto.InsertDelete.Should().Be("I");
            dto.FpsYear.Should().Be(2024);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new StaffJobLogDto
            {
                SequenceNo   = 3,
                StaffId      = "STAFF002",
                JobCode      = "JOB200",
                PlannedHours = 40.0,
                DateTime     = null,
                UserId       = null,
                InsertDelete = null,
                FpsYear      = 2025
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
            var dto = new StaffJobLogDto
            {
                SequenceNo   = 1,
                StaffId      = "OLD001",
                JobCode      = "OLDJOB",
                PlannedHours = 20.0,
                FpsYear      = 2022
            };

            // Act
            var updated = new DateTime(2025, 4, 1);
            dto.SequenceNo   = 99;
            dto.StaffId      = "NEW001";
            dto.JobCode      = "NEWJOB";
            dto.PlannedHours = 80.0;
            dto.DateTime     = updated;
            dto.UserId       = "superUser";
            dto.InsertDelete = "D";
            dto.FpsYear      = 2025;

            // Assert
            dto.SequenceNo.Should().Be(99);
            dto.StaffId.Should().Be("NEW001");
            dto.JobCode.Should().Be("NEWJOB");
            dto.PlannedHours.Should().Be(80.0);
            dto.DateTime.Should().Be(updated);
            dto.UserId.Should().Be("superUser");
            dto.InsertDelete.Should().Be("D");
            dto.FpsYear.Should().Be(2025);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new StaffJobLogDto();

            dto.SequenceNo.Should().Be(0);
            dto.PlannedHours.Should().Be(0.0);
            dto.FpsYear.Should().Be(0);
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
        }

        [Fact]
        public void StaffId_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new StaffJobLogDto { StaffId = string.Empty, JobCode = "J" };

            dto.StaffId.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(7.5)]
        [InlineData(2080.0)]
        public void PlannedHours_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new StaffJobLogDto { StaffId = "S", JobCode = "J", PlannedHours = value };

            dto.PlannedHours.Should().Be(value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(int.MaxValue)]
        public void SequenceNo_SetToBoundaryValues_ReturnsCorrectValue(int value)
        {
            var dto = new StaffJobLogDto { SequenceNo = value };

            dto.SequenceNo.Should().Be(value);
        }

        #endregion
    }
}
