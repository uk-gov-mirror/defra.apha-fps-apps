using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ContributionSummaryRowDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new ContributionSummaryRowDto
            {
                WgGrade             = "SG01",
                WorkGroup           = "WG-ENV",
                ProfitCentreGrade   = "PCG-A",
                Hrs                 = 120.5,
                AvHrs               = 1840.0,
                ChargeRate          = 55.50m,
                Ohr                 = 12.25m,
                Fec                 = 8500.00m,
                Contribution        = 7200.00m,
                AppHours            = 95.0,
                AppFec              = 6700.00m,
                PctPlanned          = 0.0654,
                PctAssuredPlanned   = 0.0516
            };

            Assert.Equal("SG01",     dto.WgGrade);
            Assert.Equal("WG-ENV",   dto.WorkGroup);
            Assert.Equal("PCG-A",    dto.ProfitCentreGrade);
            Assert.Equal(120.5,      dto.Hrs);
            Assert.Equal(1840.0,     dto.AvHrs);
            Assert.Equal(55.50m,     dto.ChargeRate);
            Assert.Equal(12.25m,     dto.Ohr);
            Assert.Equal(8500.00m,   dto.Fec);
            Assert.Equal(7200.00m,   dto.Contribution);
            Assert.Equal(95.0,       dto.AppHours);
            Assert.Equal(6700.00m,   dto.AppFec);
            Assert.Equal(0.0654,     dto.PctPlanned);
            Assert.Equal(0.0516,     dto.PctAssuredPlanned);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ContributionSummaryRowDto
            {
                WgGrade             = null,
                WorkGroup           = null,
                ProfitCentreGrade   = null,
                Hrs                 = null,
                AvHrs               = null,
                ChargeRate          = null,
                Ohr                 = null,
                Fec                 = null,
                Contribution        = null,
                AppHours            = null,
                AppFec              = null,
                PctPlanned          = null,
                PctAssuredPlanned   = null
            };

            Assert.Null(dto.WgGrade);
            Assert.Null(dto.WorkGroup);
            Assert.Null(dto.ProfitCentreGrade);
            Assert.Null(dto.Hrs);
            Assert.Null(dto.AvHrs);
            Assert.Null(dto.ChargeRate);
            Assert.Null(dto.Ohr);
            Assert.Null(dto.Fec);
            Assert.Null(dto.Contribution);
            Assert.Null(dto.AppHours);
            Assert.Null(dto.AppFec);
            Assert.Null(dto.PctPlanned);
            Assert.Null(dto.PctAssuredPlanned);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ContributionSummaryRowDto { WgGrade = "OLD" };

            dto.WgGrade           = "NEW";
            dto.WorkGroup         = "WG-BIO";
            dto.Hrs               = 200.0;
            dto.ChargeRate        = 60.00m;
            dto.Fec               = 9000.00m;
            dto.PctPlanned        = 0.1087;
            dto.PctAssuredPlanned = 0.0815;

            Assert.Equal("NEW",    dto.WgGrade);
            Assert.Equal("WG-BIO", dto.WorkGroup);
            Assert.Equal(200.0,    dto.Hrs);
            Assert.Equal(60.00m,   dto.ChargeRate);
            Assert.Equal(9000.00m, dto.Fec);
            Assert.Equal(0.1087,   dto.PctPlanned);
            Assert.Equal(0.0815,   dto.PctAssuredPlanned);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new ContributionSummaryRowDto();

            Assert.Null(dto.WgGrade);
            Assert.Null(dto.WorkGroup);
            Assert.Null(dto.ProfitCentreGrade);
            Assert.Null(dto.Hrs);
            Assert.Null(dto.AvHrs);
            Assert.Null(dto.ChargeRate);
            Assert.Null(dto.Ohr);
            Assert.Null(dto.Fec);
            Assert.Null(dto.Contribution);
            Assert.Null(dto.AppHours);
            Assert.Null(dto.AppFec);
            Assert.Null(dto.PctPlanned);
            Assert.Null(dto.PctAssuredPlanned);
        }

        [Fact]
        public void WgGrade_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new ContributionSummaryRowDto { WgGrade = string.Empty };

            Assert.Equal(string.Empty, dto.WgGrade);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-8.5)]
        [InlineData(2080.0)]
        public void Hrs_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new ContributionSummaryRowDto { Hrs = value };

            Assert.Equal(value, dto.Hrs);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-500.0)]
        [InlineData(99999.99)]
        public void Fec_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new ContributionSummaryRowDto { Fec = value };

            Assert.Equal(value, dto.Fec);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.5)]
        public void PctPlanned_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new ContributionSummaryRowDto { PctPlanned = value };

            Assert.Equal(value, dto.PctPlanned);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(0.75)]
        public void PctAssuredPlanned_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new ContributionSummaryRowDto { PctAssuredPlanned = value };

            Assert.Equal(value, dto.PctAssuredPlanned);
        }

        #endregion
    }
}
