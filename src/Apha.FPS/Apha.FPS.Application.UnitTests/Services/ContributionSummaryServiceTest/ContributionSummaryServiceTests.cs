using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ContributionSummaryServiceTest
{
    public class ContributionSummaryServiceTests
    {
        private readonly IContributionSummaryRepository _mockRepository;
        private readonly IAnimalRepository       _mockAnimalRepository;
        private readonly ContributionSummaryService     _sut;

        public ContributionSummaryServiceTests()
        {
            _mockRepository       = Substitute.For<IContributionSummaryRepository>();
            _mockAnimalRepository = Substitute.For<IAnimalRepository>();
            _sut                  = new ContributionSummaryService(_mockRepository, _mockAnimalRepository);
        }

        private static ContributionSummaryView MakeView(
            string   sellingPc     = "ENV",
            string   workGroup     = "WG1",
            string   wgGrade       = "G1",
            double?  hrs           = 80,
            double?  avHrs         = 100,
            double?  appHours      = 60,
            decimal  fec           = 1000m,
            decimal  contribution  = 500m,
            decimal  appFec        = 800m,
            decimal? contTarget    = 5000m,
            decimal? sumOfGenBid   = 1000m)
            => new()
            {
                SellingPc         = sellingPc,
                WorkGroup         = workGroup,
                WgGrade           = wgGrade,
                ProfitCentreGrade = "PCG1",
                Hrs               = hrs,
                AvHrs             = avHrs,
                AppHours          = appHours,
                ChargeRate        = 10m,
                Ohr               = 2m,
                Fec               = fec,
                Contribution      = contribution,
                AppFec            = appFec,
                ContTarget        = contTarget,
                SumOfGenBid       = sumOfGenBid
            };

        #region GetRowsAsync — Happy path

        [Fact]
        public async Task GetRowsAsync_WithValidData_ReturnsMappedDtos()
        {
            // Arrange
            var sellingPc = "ENV";
            var views     = new List<ContributionSummaryView> { MakeView(), MakeView(workGroup: "WG2", wgGrade: "G2") };
            _mockRepository.GetBySellingPcAsync(sellingPc).Returns(views);

            // Act
            var result = await _sut.GetRowsAsync(sellingPc);

            // Assert
            result.Should().HaveCount(2);
            result[0].WorkGroup.Should().Be("WG1");
            result[0].WgGrade.Should().Be("G1");
            result[0].Fec.Should().Be(1000m);
            result[0].Contribution.Should().Be(500m);
            await _mockRepository.Received(1).GetBySellingPcAsync(sellingPc);
        }

        [Fact]
        public async Task GetRowsAsync_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result.Should().NotBeNull().And.BeEmpty();
        }

        #endregion

        #region GetRowsAsync — PctPlanned calculation

        [Fact]
        public async Task GetRowsAsync_WhenAvHrsNonZero_CalculatesPctPlanned()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([MakeView(hrs: 80, avHrs: 100)]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result[0].PctPlanned.Should().BeApproximately(0.8, 1e-9);
        }

        [Fact]
        public async Task GetRowsAsync_WhenAvHrsIsZero_PctPlannedIsNull()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([MakeView(hrs: 80, avHrs: 0)]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result[0].PctPlanned.Should().BeNull();
        }

        [Fact]
        public async Task GetRowsAsync_WhenAvHrsIsNull_PctPlannedIsNull()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([MakeView(hrs: 80, avHrs: null)]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result[0].PctPlanned.Should().BeNull();
        }

        [Fact]
        public async Task GetRowsAsync_WhenAvHrsNonZero_CalculatesPctAssuredPlanned()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([MakeView(appHours: 40, avHrs: 100)]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result[0].PctAssuredPlanned.Should().BeApproximately(0.4, 1e-9);
        }

        [Fact]
        public async Task GetRowsAsync_WhenAvHrsIsZero_PctAssuredPlannedIsNull()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([MakeView(appHours: 40, avHrs: 0)]);

            // Act
            var result = await _sut.GetRowsAsync("ENV");

            // Assert
            result[0].PctAssuredPlanned.Should().BeNull();
        }

        #endregion

        #region GetRowsAsync — Validation

        [Fact]
        public async Task GetRowsAsync_WhenSellingPcIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetRowsAsync(null!));
            await _mockRepository.DidNotReceive().GetBySellingPcAsync(Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetRowsAsync_WhenSellingPcIsEmptyOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetRowsAsync(sellingPc));
            await _mockRepository.DidNotReceive().GetBySellingPcAsync(Arg.Any<string>());
        }

        #endregion

        #region GetTotalsAsync — Non-ASU

        [Fact]
        public async Task GetTotalsAsync_ForNonAsuPc_ReturnsTotalsWithoutAnimalCosts()
        {
            // Arrange
            var sellingPc = "ENV";
            var views = new List<ContributionSummaryView>
            {
                MakeView(fec: 200m, contribution: 100m, appFec: 180m, contTarget: 1000m, sumOfGenBid: 200m),
                MakeView(fec: 300m, contribution: 150m, appFec: 270m, contTarget: 1000m, sumOfGenBid: 200m)
            };
            _mockRepository.GetBySellingPcAsync(sellingPc).Returns(views);

            // Act
            var result = await _sut.GetTotalsAsync(sellingPc);

            // Assert
            result.SellingPc.Should().Be(sellingPc);
            result.TotalFec.Should().Be(500m);
            result.TotalContribution.Should().Be(250m);
            result.TotalAppFec.Should().Be(450m);
            result.ContTarget.Should().Be(1000m);
            result.SumOfGenBid.Should().Be(200m);
            result.TotalToRecover.Should().Be(1200m);
            result.Surplus.Should().Be(-700m);
            result.AssuredSurplus.Should().Be(-750m);
            result.AnimalCosts.Should().Be(0m);
            result.IsAsuMode.Should().BeFalse();
            await _mockAnimalRepository.DidNotReceive().GetGlobalAnimalCostAsync();
        }

        [Fact]
        public async Task GetTotalsAsync_ForAsuPc_IncludesAnimalCostsInSurplus()
        {
            // Arrange
            var sellingPc = "ASU";
            var views = new List<ContributionSummaryView>
            {
                MakeView(sellingPc: sellingPc, fec: 400m, appFec: 360m, contTarget: 1000m, sumOfGenBid: 200m)
            };
            _mockRepository.GetBySellingPcAsync(sellingPc).Returns(views);
            _mockAnimalRepository.GetGlobalAnimalCostAsync().Returns(150m);

            // Act
            var result = await _sut.GetTotalsAsync(sellingPc);

            // Assert
            result.IsAsuMode.Should().BeTrue();
            result.AnimalCosts.Should().Be(150m);
            result.Surplus.Should().Be(400m - 1200m + 150m);
            result.AssuredSurplus.Should().Be(360m - 1200m);
            await _mockAnimalRepository.Received(1).GetGlobalAnimalCostAsync();
        }

        [Fact]
        public async Task GetTotalsAsync_AsuIsCaseInsensitive()
        {
            // Arrange
            var views = new List<ContributionSummaryView> { MakeView(sellingPc: "asu") };
            _mockRepository.GetBySellingPcAsync("asu").Returns(views);
            _mockAnimalRepository.GetGlobalAnimalCostAsync().Returns(100m);

            // Act
            var result = await _sut.GetTotalsAsync("asu");

            // Assert
            result.IsAsuMode.Should().BeTrue();
            await _mockAnimalRepository.Received(1).GetGlobalAnimalCostAsync();
        }

        [Fact]
        public async Task GetTotalsAsync_WithEmptyRows_ReturnsZeroTotals()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Returns([]);

            // Act
            var result = await _sut.GetTotalsAsync("ENV");

            // Assert
            result.TotalFec.Should().Be(0m);
            result.TotalContribution.Should().Be(0m);
            result.TotalAppFec.Should().Be(0m);
            result.TotalToRecover.Should().Be(0m);
            result.Surplus.Should().Be(0m);
            result.AssuredSurplus.Should().Be(0m);
        }

        #endregion

        #region GetTotalsAsync — Validation

        [Fact]
        public async Task GetTotalsAsync_WhenSellingPcIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetTotalsAsync(null!));
            await _mockRepository.DidNotReceive().GetBySellingPcAsync(Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalsAsync_WhenSellingPcIsEmptyOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetTotalsAsync(sellingPc));
            await _mockRepository.DidNotReceive().GetBySellingPcAsync(Arg.Any<string>());
        }

        #endregion

        #region GetTotalsAsync — Exception propagation

        [Fact]
        public async Task GetTotalsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetBySellingPcAsync("ENV").Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetTotalsAsync("ENV"));
        }

        #endregion
    }
}
