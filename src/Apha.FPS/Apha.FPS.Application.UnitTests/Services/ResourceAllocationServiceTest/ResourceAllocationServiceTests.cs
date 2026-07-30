using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ResourceAllocationServiceTest
{
    public class ResourceAllocationServiceTests
    {
        private const string DefaultWorkGroupGrade = "WG01";
        private const string DefaultStaffId = "PACT001";

        private readonly IResourceAllocationRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ResourceAllocationService _sut;

        public ResourceAllocationServiceTests()
        {
            _mockRepository = Substitute.For<IResourceAllocationRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ResourceAllocationService(_mockRepository, _mockMapper);
        }

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceAllocationService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceAllocationService(_mockRepository, null!));
        }

        #endregion

        // ── GetPagedStaffAllocationsByWorkGroupGradeAsync Tests ───────────────

        #region GetPagedStaffAllocationsByWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithValidInput_ReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ResourceStaffGeneralSummaryRow>();
            var expected = new PaginatedResult<ResourceStaffAllocationDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, mappedFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ResourceStaffAllocationDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, mappedFilter);
            _mockMapper.Received(1).Map<PaginatedResult<ResourceStaffAllocationDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithBlankWorkGroupGrade_ThrowsArgumentException(
            string workGroupGrade)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(workGroupGrade, query));

            await _mockRepository.DidNotReceive()
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    Arg.Any<string>(), Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithNullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, null!));

            await _mockRepository.DidNotReceive()
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    Arg.Any<string>(), Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWorkGroupGrade, mappedFilter)
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query));
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_MapsQueryBeforeCallingRepository()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "Name" };
            var mappedFilter = new PaginationParameters<string> { Page = 2, PageSize = 5, SortBy = "Name" };
            var pagedData = new PagedData<ResourceStaffGeneralSummaryRow>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                DefaultWorkGroupGrade, mappedFilter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ResourceStaffAllocationDto>>(pagedData)
                .Returns(new PaginatedResult<ResourceStaffAllocationDto>());

            // Act
            await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, mappedFilter);
        }

        #endregion

        // ── GetPagedStaffJobDetailsByStaffIdAsync Tests ───────────────────────

        #region GetPagedStaffJobDetailsByStaffIdAsync Tests

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithValidInput_ReturnsMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<ResourceStaffJobDetailRow>();
            var expected = new PaginatedResult<ResourceStaffJobDetailDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, mappedFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ResourceStaffJobDetailDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, mappedFilter);
            _mockMapper.Received(1).Map<PaginatedResult<ResourceStaffJobDetailDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithBlankStaffId_ThrowsArgumentException(
            string staffId)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetPagedStaffJobDetailsByStaffIdAsync(staffId, query));

            await _mockRepository.DidNotReceive()
                .GetPagedStaffJobDetailsByStaffIdAsync(
                    Arg.Any<string>(), Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithNullQuery_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, null!));

            await _mockRepository.DidNotReceive()
                .GetPagedStaffJobDetailsByStaffIdAsync(
                    Arg.Any<string>(), Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, mappedFilter)
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_MapsQueryBeforeCallingRepository()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20, SortBy = "Hour" };
            var mappedFilter = new PaginationParameters<string> { Page = 1, PageSize = 20, SortBy = "Hour" };
            var pagedData = new PagedData<ResourceStaffJobDetailRow>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, mappedFilter)
                .Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ResourceStaffJobDetailDto>>(pagedData)
                .Returns(new PaginatedResult<ResourceStaffJobDetailDto>());

            // Act
            await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, mappedFilter);
        }

        #endregion
    }
}
