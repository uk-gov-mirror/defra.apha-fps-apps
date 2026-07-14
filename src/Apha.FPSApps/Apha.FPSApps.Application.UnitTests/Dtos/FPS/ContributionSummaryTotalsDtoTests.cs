using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ContributionSummaryTotalsDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new ContributionSummaryTotalsDto
            {
                SellingPc         = "ASU",
                ContTarget        = 50000m,
                SumOfGenBid       = 40000m,
                TotalFec          = 95000m,
                TotalContribution = 90000m,
                TotalAppFec       = 80000m,
                TotalToRecover    = 90000m,
                Surplus           = 5000m,
                AssuredSurplus    = -10000m,
                AnimalCosts       = 1500m,
                IsAsuMode         = true
            };

            Assert.Equal("ASU",    dto.SellingPc);
            Assert.Equal(50000m,   dto.ContTarget);
            Assert.Equal(40000m,   dto.SumOfGenBid);
            Assert.Equal(95000m,   dto.TotalFec);
            Assert.Equal(90000m,   dto.TotalContribution);
            Assert.Equal(80000m,   dto.TotalAppFec);
            Assert.Equal(90000m,   dto.TotalToRecover);
            Assert.Equal(5000m,    dto.Surplus);
            Assert.Equal(-10000m,  dto.AssuredSurplus);
            Assert.Equal(1500m,    dto.AnimalCosts);
            Assert.True(dto.IsAsuMode);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ContributionSummaryTotalsDto
            {
                SellingPc   = "ENV",
                ContTarget  = null,
                SumOfGenBid = null
            };

            Assert.Equal("ENV", dto.SellingPc);
            Assert.Null(dto.ContTarget);
            Assert.Null(dto.SumOfGenBid);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ContributionSummaryTotalsDto { SellingPc = "ENV" };

            dto.SellingPc         = "ASU";
            dto.ContTarget        = 60000m;
            dto.SumOfGenBid       = 45000m;
            dto.TotalFec          = 100000m;
            dto.TotalContribution = 95000m;
            dto.TotalAppFec       = 85000m;
            dto.TotalToRecover    = 105000m;
            dto.Surplus           = -5000m;
            dto.AssuredSurplus    = -20000m;
            dto.AnimalCosts       = 2000m;
            dto.IsAsuMode         = true;

            Assert.Equal("ASU",   dto.SellingPc);
            Assert.Equal(60000m,  dto.ContTarget);
            Assert.Equal(45000m,  dto.SumOfGenBid);
            Assert.Equal(100000m, dto.TotalFec);
            Assert.Equal(95000m,  dto.TotalContribution);
            Assert.Equal(85000m,  dto.TotalAppFec);
            Assert.Equal(105000m, dto.TotalToRecover);
            Assert.Equal(-5000m,  dto.Surplus);
            Assert.Equal(-20000m, dto.AssuredSurplus);
            Assert.Equal(2000m,   dto.AnimalCosts);
            Assert.True(dto.IsAsuMode);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new ContributionSummaryTotalsDto();

            Assert.Null(dto.ContTarget);
            Assert.Null(dto.SumOfGenBid);
            Assert.Equal(0m,    dto.TotalFec);
            Assert.Equal(0m,    dto.TotalContribution);
            Assert.Equal(0m,    dto.TotalAppFec);
            Assert.Equal(0m,    dto.TotalToRecover);
            Assert.Equal(0m,    dto.Surplus);
            Assert.Equal(0m,    dto.AssuredSurplus);
            Assert.Equal(0m,    dto.AnimalCosts);
            Assert.False(dto.IsAsuMode);
        }

        [Fact]
        public void IsAsuMode_DefaultsToFalse()
        {
            var dto = new ContributionSummaryTotalsDto();

            Assert.False(dto.IsAsuMode);
        }

        [Fact]
        public void IsAsuMode_SetToTrue_ReturnsTrue()
        {
            var dto = new ContributionSummaryTotalsDto { IsAsuMode = true };

            Assert.True(dto.IsAsuMode);
        }

        [Fact]
        public void AnimalCosts_DefaultsToZero()
        {
            var dto = new ContributionSummaryTotalsDto();

            Assert.Equal(0m, dto.AnimalCosts);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-99999.99)]
        [InlineData(999999.99)]
        public void Surplus_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new ContributionSummaryTotalsDto { Surplus = value };

            Assert.Equal(value, dto.Surplus);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-99999.99)]
        [InlineData(999999.99)]
        public void AssuredSurplus_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new ContributionSummaryTotalsDto { AssuredSurplus = value };

            Assert.Equal(value, dto.AssuredSurplus);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(500.0)]
        [InlineData(999999.99)]
        public void AnimalCosts_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new ContributionSummaryTotalsDto { AnimalCosts = value };

            Assert.Equal(value, dto.AnimalCosts);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-50000.0)]
        [InlineData(200000.0)]
        public void TotalFec_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new ContributionSummaryTotalsDto { TotalFec = value };

            Assert.Equal(value, dto.TotalFec);
        }

        #endregion

        #region Non-ASU Scenario Tests

        [Fact]
        public void NonAsuMode_AnimalCostsZeroAndIsAsuModeFalse()
        {
            var dto = new ContributionSummaryTotalsDto
            {
                SellingPc         = "ENV",
                ContTarget        = 30000m,
                SumOfGenBid       = 20000m,
                TotalFec          = 55000m,
                TotalContribution = 52000m,
                TotalAppFec       = 48000m,
                TotalToRecover    = 50000m,
                Surplus           = 5000m,
                AssuredSurplus    = -2000m,
                AnimalCosts       = 0m,
                IsAsuMode         = false
            };

            Assert.Equal("ENV", dto.SellingPc);
            Assert.Equal(0m,    dto.AnimalCosts);
            Assert.False(dto.IsAsuMode);
        }

        [Fact]
        public void AsuMode_WithAnimalCostsAndIsAsuModeTrue()
        {
            var dto = new ContributionSummaryTotalsDto
            {
                SellingPc         = "ASU",
                ContTarget        = 80000m,
                SumOfGenBid       = 60000m,
                TotalFec          = 150000m,
                TotalContribution = 140000m,
                TotalAppFec       = 130000m,
                TotalToRecover    = 140000m,
                Surplus           = 10000m,
                AssuredSurplus    = -10000m,
                AnimalCosts       = 3500m,
                IsAsuMode         = true
            };

            Assert.Equal("ASU", dto.SellingPc);
            Assert.Equal(3500m, dto.AnimalCosts);
            Assert.True(dto.IsAsuMode);
        }

        #endregion
    }
}
