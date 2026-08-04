using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.BudgetBidsControllerTest
{
    public class BudgetBidsControllerTests
    {
        private readonly IBudgetBidsService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly BudgetBidsController _controller;

        public BudgetBidsControllerTests()
        {
            _serviceMock = Substitute.For<IBudgetBidsService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new BudgetBidsController(_serviceMock, _mapperMock);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsController(_serviceMock, null!));
        }

        #endregion

        #region GetGenericBidsPagedAsync Tests

        [Fact]
        public async Task GetGenericBidsPagedAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<GenericBidViewDto>();
            var mappedResult = new PaginationRes<GenericBidViewRes>();

            _serviceMock.GetGenericBidsPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<GenericBidViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetGenericBidsPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetGenericBidsPagedAsync(query);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<GenericBidViewDto>();
            var mappedResult = new PaginationRes<GenericBidViewRes>();

            _serviceMock.GetGenericBidsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<GenericBidViewRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetGenericBidsPagedAsync(query).Throws(new InvalidOperationException("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.GetGenericBidsPagedAsync(query));
        }

        #endregion
    }
}
