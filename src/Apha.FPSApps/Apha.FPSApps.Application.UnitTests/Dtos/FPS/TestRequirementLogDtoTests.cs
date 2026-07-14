using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class TestRequirementLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var now = new DateTime(2024, 4, 20, 11, 0, 0);

            var dto = new TestRequirementLogDto
            {
                SequenceNo       = 3,
                TestCode         = "TC001",
                Buyer            = "BuyerA",
                UnitPrice        = 15.75m,
                NoRequired       = 8.0,
                ProjectBuyerCode = "PBC001",
                TestBuyerCode    = "TBC001",
                Active           = 1,
                DateTime         = now,
                UserId           = "user03",
                InsertDelete     = "I",
                JobCode          = "JC003",
                FpsYear          = 2024
            };

            Assert.Equal(3,         dto.SequenceNo);
            Assert.Equal("TC001",   dto.TestCode);
            Assert.Equal("BuyerA",  dto.Buyer);
            Assert.Equal(15.75m,    dto.UnitPrice);
            Assert.Equal(8.0,       dto.NoRequired);
            Assert.Equal("PBC001",  dto.ProjectBuyerCode);
            Assert.Equal("TBC001",  dto.TestBuyerCode);
            Assert.Equal((short)1,  dto.Active);
            Assert.Equal(now,       dto.DateTime);
            Assert.Equal("user03",  dto.UserId);
            Assert.Equal("I",       dto.InsertDelete);
            Assert.Equal("JC003",   dto.JobCode);
            Assert.Equal(2024,      dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new TestRequirementLogDto
            {
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
                JobCode          = null
            };

            Assert.Null(dto.TestCode);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.UnitPrice);
            Assert.Null(dto.NoRequired);
            Assert.Null(dto.ProjectBuyerCode);
            Assert.Null(dto.TestBuyerCode);
            Assert.Null(dto.Active);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
            Assert.Null(dto.JobCode);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new TestRequirementLogDto { FpsYear = 2023 };

            dto.TestCode     = "TC999";
            dto.UnitPrice    = 99.99m;
            dto.NoRequired   = 4.0;
            dto.FpsYear      = 2025;

            Assert.Equal("TC999", dto.TestCode);
            Assert.Equal(99.99m,  dto.UnitPrice);
            Assert.Equal(4.0,     dto.NoRequired);
            Assert.Equal(2025,    dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new TestRequirementLogDto();

            Assert.Equal(0, dto.SequenceNo);
            Assert.Equal(0, dto.FpsYear);
        }

        [Fact]
        public void DefaultConstructor_AllNullableProperties_AreNull()
        {
            var dto = new TestRequirementLogDto();

            Assert.Null(dto.TestCode);
            Assert.Null(dto.Buyer);
            Assert.Null(dto.UnitPrice);
            Assert.Null(dto.NoRequired);
            Assert.Null(dto.ProjectBuyerCode);
            Assert.Null(dto.TestBuyerCode);
            Assert.Null(dto.Active);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
            Assert.Null(dto.JobCode);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void UnitPrice_AcceptsNegativeValue()
        {
            var dto = new TestRequirementLogDto { UnitPrice = -5.00m };

            Assert.Equal(-5.00m, dto.UnitPrice);
        }

        [Fact]
        public void Active_AcceptsZero()
        {
            var dto = new TestRequirementLogDto { Active = 0 };

            Assert.Equal((short)0, dto.Active);
        }

        #endregion
    }
}
