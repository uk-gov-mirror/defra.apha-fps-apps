using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ContributionSummaryRepositoryTest
{
    public class ContributionSummaryRepositoryTests
    {
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultSellingPc = "ENV";

        private static ContributionSummaryRepository CreateRepository(
            IEnumerable<ContributionSummaryView>? views = null,
            string userEmail = DefaultUserEmail)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (views != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(views);
                mockContext.Setup(x => x.VQryFrmTimeSellerPcViews).Returns(mockSet.Object);
            }

            return new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);
        }

        private static ContributionSummaryView MakeView(
            string  sellingPc = DefaultSellingPc,
            string  workGroup = "WG1",
            string  wgGrade   = "G1",
            string? userEmail = DefaultUserEmail)
            => new()
            {
                SellingPc = sellingPc,
                WorkGroup = workGroup,
                WgGrade   = wgGrade,
                UserEmail = userEmail
            };

        #region GetBySellingPcAsync — Happy path

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsRowsForMatchingSellingPcAndUser()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView("ENV", "WG1", "G1"),
                MakeView("ENV", "WG2", "G2"),
                MakeView("ASU", "WG3", "G3")  // different PC — must be excluded
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal(DefaultSellingPc, r.SellingPc));
        }

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsEmpty_WhenNoMatchingSellingPc()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView("ASU", "WG1", "G1"),
                MakeView("DTE", "WG2", "G2")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetBySellingPcAsync_ReturnsEmpty_WhenDataSetIsEmpty()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBySellingPcAsync — User email filter

        [Fact]
        public async Task GetBySellingPcAsync_ExcludesRowsWithDifferentUserEmail()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(userEmail: DefaultUserEmail),
                MakeView(workGroup: "WG2", wgGrade: "G2", userEmail: "other@example.com")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroup);
        }

        [Fact]
        public async Task GetBySellingPcAsync_ExcludesRowsWithNullUserEmail()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(userEmail: DefaultUserEmail),
                MakeView(workGroup: "WG2", wgGrade: "G2", userEmail: null)
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroup);
        }

        #endregion

        #region GetBySellingPcAsync — Ordering

        [Fact]
        public async Task GetBySellingPcAsync_IsOrderedByWorkGroupThenWgGrade()
        {
            // Arrange
            var views = new List<ContributionSummaryView>
            {
                MakeView(workGroup: "WG2", wgGrade: "G1"),
                MakeView(workGroup: "WG1", wgGrade: "G2"),
                MakeView(workGroup: "WG1", wgGrade: "G1")
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetBySellingPcAsync(DefaultSellingPc);

            // Assert
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal("G1",  result[0].WgGrade);
            Assert.Equal("WG1", result[1].WorkGroup);
            Assert.Equal("G2",  result[1].WgGrade);
            Assert.Equal("WG2", result[2].WorkGroup);
        }

        #endregion

        #region GetBySellingPcAsync — Validation

        [Fact]
        public async Task GetBySellingPcAsync_WhenSellingPcIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetBySellingPcAsync(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetBySellingPcAsync_WhenSellingPcIsEmptyOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetBySellingPcAsync(sellingPc));
        }

        #endregion
    }
}
