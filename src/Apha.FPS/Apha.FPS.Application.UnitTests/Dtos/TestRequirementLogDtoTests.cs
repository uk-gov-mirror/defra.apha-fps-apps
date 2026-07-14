using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class TestRequirementLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange
            var dateTime = new DateTime(2024, 11, 5, 8, 15, 0);

            // Act
            var dto = new TestRequirementLogDto
            {
                SequenceNo       = 7,
                TestCode         = "TC001",
                Buyer            = "BuyerOrg",
                UnitPrice        = 250.50,
                NoRequired       = 12.5,
                ProjectBuyerCode = "PBC100",
                TestBuyerCode    = "TBC200",
                Active           = 1,
                DateTime         = dateTime,
                UserId           = "testUser",
                InsertDelete     = "I",
                JobCode          = "JOB999",
                FpsYear          = 2024
            };

            // Assert
            dto.SequenceNo.Should().Be(7);
            dto.TestCode.Should().Be("TC001");
            dto.Buyer.Should().Be("BuyerOrg");
            dto.UnitPrice.Should().Be(250.50);
            dto.NoRequired.Should().Be(12.5);
            dto.ProjectBuyerCode.Should().Be("PBC100");
            dto.TestBuyerCode.Should().Be("TBC200");
            dto.Active.Should().Be(1);
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("testUser");
            dto.InsertDelete.Should().Be("I");
            dto.JobCode.Should().Be("JOB999");
            dto.FpsYear.Should().Be(2024);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new TestRequirementLogDto
            {
                SequenceNo       = 1,
                TestCode         = null,
                Buyer            = null,
                UnitPrice        = null,
                NoRequired       = null,
                ProjectBuyerCode = null,
                TestBuyerCode    = null,
                Active           = null,
                DateTime         = null,
                UserId           = null,
                InsertDelete     = null,
                JobCode          = null,
                FpsYear          = 2024
            };

            // Assert
            dto.TestCode.Should().BeNull();
            dto.Buyer.Should().BeNull();
            dto.UnitPrice.Should().BeNull();
            dto.NoRequired.Should().BeNull();
            dto.ProjectBuyerCode.Should().BeNull();
            dto.TestBuyerCode.Should().BeNull();
            dto.Active.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
            dto.JobCode.Should().BeNull();
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            // Arrange
            var dto = new TestRequirementLogDto
            {
                SequenceNo = 1,
                FpsYear    = 2023
            };

            // Act
            var updated = new DateTime(2025, 9, 15);
            dto.SequenceNo       = 50;
            dto.TestCode         = "TC999";
            dto.Buyer            = "NewBuyer";
            dto.UnitPrice        = 999.99;
            dto.NoRequired       = 100.0;
            dto.ProjectBuyerCode = "PBC999";
            dto.TestBuyerCode    = "TBC999";
            dto.Active           = 0;
            dto.DateTime         = updated;
            dto.UserId           = "adminUser";
            dto.InsertDelete     = "D";
            dto.JobCode          = "JOBNEW";
            dto.FpsYear          = 2025;

            // Assert
            dto.SequenceNo.Should().Be(50);
            dto.TestCode.Should().Be("TC999");
            dto.Buyer.Should().Be("NewBuyer");
            dto.UnitPrice.Should().Be(999.99);
            dto.NoRequired.Should().Be(100.0);
            dto.ProjectBuyerCode.Should().Be("PBC999");
            dto.TestBuyerCode.Should().Be("TBC999");
            dto.Active.Should().Be(0);
            dto.DateTime.Should().Be(updated);
            dto.UserId.Should().Be("adminUser");
            dto.InsertDelete.Should().Be("D");
            dto.JobCode.Should().Be("JOBNEW");
            dto.FpsYear.Should().Be(2025);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new TestRequirementLogDto();

            dto.SequenceNo.Should().Be(0);
            dto.FpsYear.Should().Be(0);
            dto.TestCode.Should().BeNull();
            dto.Buyer.Should().BeNull();
            dto.UnitPrice.Should().BeNull();
            dto.NoRequired.Should().BeNull();
            dto.ProjectBuyerCode.Should().BeNull();
            dto.TestBuyerCode.Should().BeNull();
            dto.Active.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
            dto.JobCode.Should().BeNull();
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-100.00)]
        [InlineData(99999.99)]
        public void UnitPrice_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = raw;
            var dto = new TestRequirementLogDto { UnitPrice = value };

            dto.UnitPrice.Should().Be(value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.5)]
        [InlineData(1000.0)]
        public void NoRequired_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new TestRequirementLogDto { NoRequired = value };

            dto.NoRequired.Should().Be(value);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)1)]
        public void Active_SetToBoundaryValues_ReturnsCorrectValue(short value)
        {
            var dto = new TestRequirementLogDto { Active = value };

            dto.Active.Should().Be(value);
        }

        #endregion
    }
}
