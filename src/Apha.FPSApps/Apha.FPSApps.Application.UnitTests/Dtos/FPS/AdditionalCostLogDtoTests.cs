using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class AdditionalCostLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var now = new DateTime(2024, 6, 1, 10, 30, 0);

            var dto = new AdditionalCostLogDto
            {
                SequenceNo   = 1,
                JobCode      = "JC001",
                Account      = "ACC01",
                Description  = "Lab consumables",
                ItemCost     = 250.50m,
                Freq         = "Monthly",
                Supplier     = "SupplierX",
                DateTime     = now,
                UserId       = "user01",
                InsertDelete = "I",
                FpsYear      = 2024
            };

            Assert.Equal(1,               dto.SequenceNo);
            Assert.Equal("JC001",         dto.JobCode);
            Assert.Equal("ACC01",         dto.Account);
            Assert.Equal("Lab consumables", dto.Description);
            Assert.Equal(250.50m,         dto.ItemCost);
            Assert.Equal("Monthly",       dto.Freq);
            Assert.Equal("SupplierX",     dto.Supplier);
            Assert.Equal(now,             dto.DateTime);
            Assert.Equal("user01",        dto.UserId);
            Assert.Equal("I",             dto.InsertDelete);
            Assert.Equal(2024,            dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new AdditionalCostLogDto
            {
                JobCode      = "JC002",
                Account      = "ACC02",
                Description  = "Desc",
                Freq         = null,
                Supplier     = null,
                DateTime     = null,
                UserId       = null,
                InsertDelete = null
            };

            Assert.Null(dto.Freq);
            Assert.Null(dto.Supplier);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new AdditionalCostLogDto { JobCode = "OLD", Account = "A", Description = "D" };

            dto.JobCode      = "NEW";
            dto.Account      = "ACC99";
            dto.Description  = "Updated";
            dto.ItemCost     = 999.99m;
            dto.FpsYear      = 2025;

            Assert.Equal("NEW",     dto.JobCode);
            Assert.Equal("ACC99",   dto.Account);
            Assert.Equal("Updated", dto.Description);
            Assert.Equal(999.99m,   dto.ItemCost);
            Assert.Equal(2025,      dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new AdditionalCostLogDto();

            Assert.Equal(0,    dto.SequenceNo);
            Assert.Equal(0m,   dto.ItemCost);
            Assert.Equal(0,    dto.FpsYear);
        }

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new AdditionalCostLogDto();

            Assert.Null(dto.Freq);
            Assert.Null(dto.Supplier);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void ItemCost_AcceptsNegativeValue()
        {
            var dto = new AdditionalCostLogDto
            {
                JobCode     = "JC003",
                Account     = "ACC03",
                Description = "Credit",
                ItemCost    = -100.00m
            };

            Assert.Equal(-100.00m, dto.ItemCost);
        }

        [Fact]
        public void SequenceNo_AcceptsMaxInt()
        {
            var dto = new AdditionalCostLogDto
            {
                JobCode      = "JC004",
                Account      = "A",
                Description  = "D",
                SequenceNo   = int.MaxValue
            };

            Assert.Equal(int.MaxValue, dto.SequenceNo);
        }

        #endregion
    }
}
