using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.WorkGroupGradeServiceTest
{
    public class WorkGroupGradeServiceTests
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IWorkGroupGradeRepository _mockRepository;
        private readonly IWorkGroupEmployeeRepository _mockEmployeeRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupGradeService _sut;

        public WorkGroupGradeServiceTests()
        {
            _mockRepository         = Substitute.For<IWorkGroupGradeRepository>();
            _mockEmployeeRepository  = Substitute.For<IWorkGroupEmployeeRepository>();
            _mockMapper             = Substitute.For<IMapper>();
            _sut                    = new WorkGroupGradeService(_mockRepository, _mockEmployeeRepository, _mockMapper);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<WorkGroupGradeView>();
            var expected     = new PaginatedResult<WorkgroupGradeDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupGradesAsync(mappedParams, profitCentreGrade: DefaultPcGrade).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<WorkgroupGradeDto>>(pagedData).Returns(expected);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(query, profitCentreGrade: DefaultPcGrade);

            // Assert
            result.Should().Be(expected);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetWorkGroupGradesAsync(mappedParams, profitCentreGrade: DefaultPcGrade);
            _mockMapper.Received(1).Map<PaginatedResult<WorkgroupGradeDto>>(pagedData);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWorkGroupGradeAsync_WithNullOrWhitespacePcGrade_ThrowsArgumentException(string pcGrade)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetWorkGroupGradeAsync(query, profitCentreGrade: pcGrade));

            await _mockRepository.DidNotReceive()
                .GetWorkGroupGradesAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetWorkGroupGradesAsync(mappedParams, profitCentreGrade: DefaultPcGrade)
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetWorkGroupGradeAsync(query, profitCentreGrade: DefaultPcGrade));
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithValidWgGrade_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(true);

            // Act
            var result = await _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteWorkGroupGradeAsync(DefaultWgGrade);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteWorkGroupGradeAsync_WithNullOrWhitespaceWgGrade_ThrowsArgumentException(string wgGrade)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.DeleteWorkGroupGradeAsync(wgGrade));

            await _mockRepository.DidNotReceive().DeleteWorkGroupGradeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.DeleteWorkGroupGradeAsync(DefaultWgGrade)
                .ThrowsAsync(new KeyNotFoundException("WG grade not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade));
        }

        #endregion

        #region GetWorkgroupGradesByWorkGroupAsync Tests

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithValidInputs_ReturnsMappedResult()
        {
            // Arrange
            var entities = new List<WorkgroupGrade> { new() { WgGrade = DefaultWgGrade } };
            var expected  = new List<WorkgroupGradeDto> { new() { WgGrade = DefaultWgGrade } };

            _mockRepository.GetWorkgroupGradesByWorkGroupAsync("TeamA").Returns(entities);
            _mockMapper.Map<List<WorkgroupGradeDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetWorkgroupGradesByWorkGroupAsync("TeamA");

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetWorkgroupGradesByWorkGroupAsync("TeamA");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithNullOrWhitespaceWorkGroup_ThrowsArgumentException(string wg)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _sut.GetWorkgroupGradesByWorkGroupAsync(wg));

            await _mockRepository.DidNotReceive().GetWorkgroupGradesByWorkGroupAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetWorkgroupGradesByWorkGroupAsync("TeamA")
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetWorkgroupGradesByWorkGroupAsync("TeamA"));
        }

        #endregion
    }
}
