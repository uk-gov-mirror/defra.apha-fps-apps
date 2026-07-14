using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ContributionSummaryControllerTest
{
    public class ContributionSummaryControllerTests
    {
        private readonly IContributionSummaryService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ContributionSummaryController _controller;

        public ContributionSummaryControllerTests()
        {
            _serviceMock = Substitute.For<IContributionSummaryService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ContributionSummaryController(_serviceMock, _mapperMock);
        }

        private static List<ContributionSummaryRowDto> MakeRowDtos(int count = 2)
            => Enumerable.Range(1, count)
                .Select(i => new ContributionSummaryRowDto
                {
                    WorkGroup    = $"WG{i}",
                    WgGrade      = $"G{i}",
                    Fec          = 100m * i,
                    Contribution = 50m * i
                })
                .ToList();

        private static ContributionSummaryTotalsDto MakeTotalsDto(string sellingPc = "ENV")
            => new()
            {
                SellingPc         = sellingPc,
                ContTarget        = 1000m,
                SumOfGenBid       = 200m,
                TotalFec          = 500m,
                TotalContribution = 300m,
                TotalAppFec       = 400m,
                TotalToRecover    = 1200m,
                Surplus           = -700m,
                AssuredSurplus    = -800m,
                AnimalCosts       = sellingPc == "ASU" ? 150m : 0m,
                IsAsuMode         = sellingPc == "ASU"
            };

        #region GetRowsAsync — Happy path

        [Fact]
        public async Task GetRowsAsync_WithValidSellingPc_ReturnsOk()
        {
            // Arrange
            var sellingPc    = "ENV";
            var dtos         = MakeRowDtos(2);
            var mappedResult = new List<ContributionSummaryRowRes>
            {
                new() { WorkGroup = "WG1", WgGrade = "G1" },
                new() { WorkGroup = "WG2", WgGrade = "G2" }
            };

            _serviceMock.GetRowsAsync(sellingPc, Arg.Any<CancellationToken>()).Returns(dtos);
            _mapperMock.Map<List<ContributionSummaryRowRes>>(dtos).Returns(mappedResult);

            // Act
            var result = await _controller.GetRowsAsync(sellingPc, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetRowsAsync(sellingPc, Arg.Any<CancellationToken>());
            _mapperMock.Received(1).Map<List<ContributionSummaryRowRes>>(dtos);
        }

        [Fact]
        public async Task GetRowsAsync_WithNoRows_ReturnsOkWithEmptyList()
        {
            // Arrange
            var sellingPc    = "ENV";
            var dtos         = MakeRowDtos(0);
            var mappedResult = new List<ContributionSummaryRowRes>();

            _serviceMock.GetRowsAsync(sellingPc, Arg.Any<CancellationToken>()).Returns(dtos);
            _mapperMock.Map<List<ContributionSummaryRowRes>>(dtos).Returns(mappedResult);

            // Act
            var result = await _controller.GetRowsAsync(sellingPc, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value    = Assert.IsType<List<ContributionSummaryRowRes>>(okResult.Value);
            Assert.Empty(value);
        }

        #endregion

        #region GetRowsAsync — Validation

        [Fact]
        public async Task GetRowsAsync_WhenSellingPcIsNull_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetRowsAsync(null!, CancellationToken.None));
            Assert.Equal("sellingPc is required. (Parameter 'sellingPc')", ex.Message);
            await _serviceMock.DidNotReceive().GetRowsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetRowsAsync_WhenSellingPcIsNullOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetRowsAsync(sellingPc, CancellationToken.None));
            await _serviceMock.DidNotReceive().GetRowsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region GetRowsAsync — Exception propagation

        [Fact]
        public async Task GetRowsAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var sellingPc = "ENV";
            _serviceMock.GetRowsAsync(sellingPc, Arg.Any<CancellationToken>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetRowsAsync(sellingPc, CancellationToken.None));
        }

        #endregion

        #region GetTotalsAsync — Happy path

        [Fact]
        public async Task GetTotalsAsync_WithValidSellingPc_ReturnsOk()
        {
            // Arrange
            var sellingPc    = "ENV";
            var dto          = MakeTotalsDto(sellingPc);
            var mappedResult = new ContributionSummaryTotalsRes
            {
                SellingPc         = sellingPc,
                TotalFec          = 500m,
                TotalContribution = 300m
            };

            _serviceMock.GetTotalsAsync(sellingPc, Arg.Any<CancellationToken>()).Returns(dto);
            _mapperMock.Map<ContributionSummaryTotalsRes>(dto).Returns(mappedResult);

            // Act
            var result = await _controller.GetTotalsAsync(sellingPc, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetTotalsAsync(sellingPc, Arg.Any<CancellationToken>());
            _mapperMock.Received(1).Map<ContributionSummaryTotalsRes>(dto);
        }

        [Fact]
        public async Task GetTotalsAsync_ForAsuSellingPc_ReturnsOk()
        {
            // Arrange
            var sellingPc    = "ASU";
            var dto          = MakeTotalsDto(sellingPc);
            var mappedResult = new ContributionSummaryTotalsRes
            {
                SellingPc   = sellingPc,
                AnimalCosts = 150m,
                IsAsuMode   = true
            };

            _serviceMock.GetTotalsAsync(sellingPc, Arg.Any<CancellationToken>()).Returns(dto);
            _mapperMock.Map<ContributionSummaryTotalsRes>(dto).Returns(mappedResult);

            // Act
            var result = await _controller.GetTotalsAsync(sellingPc, CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetTotalsAsync(sellingPc, Arg.Any<CancellationToken>());
        }

        #endregion

        #region GetTotalsAsync — Validation

        [Fact]
        public async Task GetTotalsAsync_WhenSellingPcIsNull_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetTotalsAsync(null!, CancellationToken.None));
            Assert.Equal("sellingPc is required. (Parameter 'sellingPc')", ex.Message);
            await _serviceMock.DidNotReceive().GetTotalsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalsAsync_WhenSellingPcIsNullOrWhitespace_ThrowsArgumentException(string sellingPc)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetTotalsAsync(sellingPc, CancellationToken.None));
            await _serviceMock.DidNotReceive().GetTotalsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region GetTotalsAsync — Exception propagation

        [Fact]
        public async Task GetTotalsAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var sellingPc = "ENV";
            _serviceMock.GetTotalsAsync(sellingPc, Arg.Any<CancellationToken>())
                .Throws(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetTotalsAsync(sellingPc, CancellationToken.None));
        }

        #endregion
    }
}
