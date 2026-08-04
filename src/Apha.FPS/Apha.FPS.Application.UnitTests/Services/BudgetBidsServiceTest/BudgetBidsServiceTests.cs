using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using Moq;

namespace Apha.FPS.Application.UnitTests.Services.BudgetBidsServiceTest
{
    public class BudgetBidsServiceTests
    {
        private const string DefaultWorkGroup = "WG01";
        private const string DefaultAccount   = "ACC1";

        private readonly Mock<IBudgetBidsRepository> _repositoryMock;
        private readonly Mock<IMapper>               _mapperMock;
        private readonly BudgetBidsService           _sut;

        public BudgetBidsServiceTests()
        {
            _repositoryMock = new Mock<IBudgetBidsRepository>();
            _mapperMock     = new Mock<IMapper>();
            _sut = new BudgetBidsService(
                _repositoryMock.Object,
                _mapperMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsService(null!, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BudgetBidsService(_repositoryMock.Object, null!));
        }

        #endregion

        #region DeleteBidAsync — related purchases validation

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_ThrowsInvalidOperationException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            Assert.Equal(
                "This record cannot be deleted as it has a related entry in the Purchase table.",
                ex.Message);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenNoRelatedPurchases_CallsRepositoryDelete()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(r => r.DeleteBidAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount);

            // Assert
            Assert.True(result);
            _repositoryMock.Verify(r => r.DeleteBidAsync(DefaultWorkGroup, DefaultAccount), Times.Once);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_DoesNotCallRepositoryDelete()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.DeleteBidAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenRepositoryThrowsUnauthorized_PropagatesException()
        {
            // Arrange — repository now owns the ownership check; service propagates the exception
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ThrowsAsync(new UnauthorizedAccessException("User does not have access to workgroup 'WG01'."));

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.DeleteBidAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBidAsync_WithNullOrWhiteSpaceWorkGroupName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBidAsync("", DefaultAccount));
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteBidAsync("   ", DefaultAccount));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenRelatedPurchasesExist_HasRelatedPurchasesCalledOnce()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.DeleteBidAsync(DefaultWorkGroup, DefaultAccount));

            _repositoryMock.Verify(r => r.HasRelatedPurchasesAsync(DefaultWorkGroup, DefaultAccount), Times.Once);
        }

        #endregion

        #region GetGenericBidsPagedAsync Tests

        [Fact]
        public async Task GetGenericBidsPagedAsync_MapsQueryCallsRepositoryAndMapsResult()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParameters = new Apha.FPS.Core.Pagination.PaginationParameters<string>();
            var pagedData = new Apha.FPS.Core.Pagination.PagedData<Apha.FPS.Core.Entities.GenericBidView>(
                new List<Apha.FPS.Core.Entities.GenericBidView>
                {
                    new() { ProfitCentre = "PC1", WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, AccountType = "TYPE1" }
                },
                new Apha.FPS.Core.Pagination.PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var expected = new Apha.FPS.Application.Pagination.PaginatedResult<GenericBidViewDto>();

            _mapperMock
                .Setup(m => m.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query))
                .Returns(mappedParameters);
            _repositoryMock
                .Setup(r => r.GetGenericBidsPagedAsync(mappedParameters))
                .ReturnsAsync(pagedData);
            _mapperMock
                .Setup(m => m.Map<Apha.FPS.Application.Pagination.PaginatedResult<GenericBidViewDto>>(pagedData))
                .Returns(expected);

            // Act
            var result = await _sut.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Same(expected, result);
            _repositoryMock.Verify(r => r.GetGenericBidsPagedAsync(mappedParameters), Times.Once);
            _mapperMock.Verify(m => m.Map<Apha.FPS.Application.Pagination.PaginatedResult<GenericBidViewDto>>(pagedData), Times.Once);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WhenRepositoryReturnsEmpty_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParameters = new Apha.FPS.Core.Pagination.PaginationParameters<string>();
            var pagedData = new Apha.FPS.Core.Pagination.PagedData<Apha.FPS.Core.Entities.GenericBidView>(
                new List<Apha.FPS.Core.Entities.GenericBidView>(),
                new Apha.FPS.Core.Pagination.PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            var expected = new Apha.FPS.Application.Pagination.PaginatedResult<GenericBidViewDto>();

            _mapperMock
                .Setup(m => m.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query))
                .Returns(mappedParameters);
            _repositoryMock
                .Setup(r => r.GetGenericBidsPagedAsync(mappedParameters))
                .ReturnsAsync(pagedData);
            _mapperMock
                .Setup(m => m.Map<Apha.FPS.Application.Pagination.PaginatedResult<GenericBidViewDto>>(pagedData))
                .Returns(expected);

            // Act
            var result = await _sut.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Same(expected, result);
            _repositoryMock.Verify(r => r.GetGenericBidsPagedAsync(mappedParameters), Times.Once);
        }

        #endregion
    }
}
