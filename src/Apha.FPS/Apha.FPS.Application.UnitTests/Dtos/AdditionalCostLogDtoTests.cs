using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class AdditionalCostLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange
            var dateTime = new DateTime(2024, 6, 15, 10, 30, 0);

            // Act
            var dto = new AdditionalCostLogDto
            {
                SequenceNo   = 1,
                JobCode      = "JOB001",
                Account      = "ACC100",
                Description  = "Test Additional Cost",
                ItemCost     = 1500.75m,
                Freq         = "Monthly",
                Supplier     = "ACME Corp",
                DateTime     = dateTime,
                UserId       = "user123",
                InsertDelete = "I",
                FpsYear      = 2024
            };

            // Assert
            dto.SequenceNo.Should().Be(1);
            dto.JobCode.Should().Be("JOB001");
            dto.Account.Should().Be("ACC100");
            dto.Description.Should().Be("Test Additional Cost");
            dto.ItemCost.Should().Be(1500.75m);
            dto.Freq.Should().Be("Monthly");
            dto.Supplier.Should().Be("ACME Corp");
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("user123");
            dto.InsertDelete.Should().Be("I");
            dto.FpsYear.Should().Be(2024);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new AdditionalCostLogDto
            {
                SequenceNo   = 2,
                JobCode      = "JOB002",
                Account      = "ACC200",
                Description  = "No Nullables",
                ItemCost     = 0m,
                Freq         = null,
                Supplier     = null,
                DateTime     = null,
                UserId       = null,
                InsertDelete = null,
                FpsYear      = 2025
            };

            // Assert
            dto.Freq.Should().BeNull();
            dto.Supplier.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            // Arrange
            var dto = new AdditionalCostLogDto
            {
                SequenceNo  = 1,
                JobCode     = "OLDJOB",
                Account     = "OLDACC",
                Description = "Old",
                ItemCost    = 100m,
                FpsYear     = 2023
            };

            // Act
            var updated = new DateTime(2025, 1, 1);
            dto.SequenceNo   = 99;
            dto.JobCode      = "NEWJOB";
            dto.Account      = "NEWACC";
            dto.Description  = "New Description";
            dto.ItemCost     = 9999.99m;
            dto.Freq         = "Annual";
            dto.Supplier     = "NewSupplier";
            dto.DateTime     = updated;
            dto.UserId       = "adminUser";
            dto.InsertDelete = "D";
            dto.FpsYear      = 2025;

            // Assert
            dto.SequenceNo.Should().Be(99);
            dto.JobCode.Should().Be("NEWJOB");
            dto.Account.Should().Be("NEWACC");
            dto.Description.Should().Be("New Description");
            dto.ItemCost.Should().Be(9999.99m);
            dto.Freq.Should().Be("Annual");
            dto.Supplier.Should().Be("NewSupplier");
            dto.DateTime.Should().Be(updated);
            dto.UserId.Should().Be("adminUser");
            dto.InsertDelete.Should().Be("D");
            dto.FpsYear.Should().Be(2025);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new AdditionalCostLogDto();

            dto.SequenceNo.Should().Be(0);
            dto.ItemCost.Should().Be(0m);
            dto.FpsYear.Should().Be(0);
            dto.Freq.Should().BeNull();
            dto.Supplier.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
        }

        [Fact]
        public void JobCode_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new AdditionalCostLogDto { JobCode = string.Empty, Account = "A", Description = "D" };

            dto.JobCode.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-500.00)]
        [InlineData(9999999.99)]
        public void ItemCost_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto = new AdditionalCostLogDto { JobCode = "J", Account = "A", Description = "D", ItemCost = value };

            dto.ItemCost.Should().Be(value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void SequenceNo_SetToBoundaryValues_ReturnsCorrectValue(int value)
        {
            var dto = new AdditionalCostLogDto { SequenceNo = value };

            dto.SequenceNo.Should().Be(value);
        }

        #endregion
    }
}
