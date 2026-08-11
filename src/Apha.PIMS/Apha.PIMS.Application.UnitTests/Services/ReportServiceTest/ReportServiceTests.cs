using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.ReportServiceTest
{
    public class ReportServiceTests
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _repository = Substitute.For<IReportRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new ReportService(_repository, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static Report MakeEntity(int id = 1) => new Report { Id = id, ReportName = $"R{id}", Type = "R" };
        private static ReportDto MakeDto(int id = 1) => new ReportDto { Id = id, ReportName = $"R{id}", Type = "R" };

        // ── Constructor ───────────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReportService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReportService(_repository, null!));
        }

        #endregion

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<Report> { MakeEntity(1), MakeEntity(2) };
            var dtos     = new List<ReportDto> { MakeDto(1), MakeDto(2) };
            _repository.GetAllReportsAsync().Returns(entities);
            _mapper.Map<List<ReportDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllReportsAsync();
            _mapper.Received(1).Map<List<ReportDto>>(entities);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            _repository.GetAllReportsAsync().Returns(new List<Report>());
            _mapper.Map<List<ReportDto>>(Arg.Any<List<Report>>()).Returns(new List<ReportDto>());

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetAllReportsAsync().ThrowsAsync(new InvalidOperationException("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllReportsAsync());
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            var entity = MakeEntity(3);
            var dto    = MakeDto(3);
            _repository.GetReportByIdAsync(3).Returns(entity);
            _mapper.Map<ReportDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetReportByIdAsync(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result!.Id);
            await _repository.Received(1).GetReportByIdAsync(3);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetReportByIdAsync(99).Returns((Report?)null);

            // Act
            var result = await _service.GetReportByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto     = MakeDto(0);
            var entity  = MakeEntity(0);
            var created = MakeEntity(10);
            var result_dto = MakeDto(10);
            _mapper.Map<Report>(dto).Returns(entity);
            _repository.AddReportAsync(entity).Returns(created);
            _mapper.Map<ReportDto>(created).Returns(result_dto);

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            await _repository.Received(1).AddReportAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateReportAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyReportname_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ReportDto { ReportName = "", Type = "R" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateReportAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_WhitespaceReportname_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ReportDto { ReportName = "   ", Type = "R" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateReportAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_EntityExists_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto     = MakeDto(5);
            var entity  = MakeEntity(5);
            var updated = MakeEntity(5);
            var result_dto = MakeDto(5);
            _repository.ReportExistsAsync(5).Returns(true);
            _mapper.Map<Report>(dto).Returns(entity);
            _repository.UpdateReportAsync(entity).Returns(updated);
            _mapper.Map<ReportDto>(updated).Returns(result_dto);

            // Act
            var result = await _service.UpdateReportAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            await _repository.Received(1).ReportExistsAsync(5);
            await _repository.Received(1).UpdateReportAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ReportExistsAsync(99).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateReportAsync(MakeDto(99)));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateReportAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_EmptyReportname_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ReportDto { Id = 1, ReportName = "", Type = "R" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateReportAsync(dto));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_ReturnsTrueAndCallsRepositoryDelete()
        {
            // Arrange
            _repository.ReportExistsAsync(7).Returns(true);
            _repository.DeleteReportAsync(7).Returns(true);

            // Act
            var result = await _service.DeleteReportAsync(7);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteReportAsync(7);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ReportExistsAsync(99).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteReportAsync(99));
        }

        [Fact]
        public async Task DeleteAsync_DoesNotCallDelete_WhenEntityNotFound()
        {
            // Arrange
            _repository.ReportExistsAsync(99).Returns(false);

            // Act + ignore exception
            try { await _service.DeleteReportAsync(99); } catch { }

            // Assert — repository.DeleteAsync should never be called
            await _repository.DidNotReceive().DeleteReportAsync(Arg.Any<int>());
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ReportExistsAsync(1).Returns(true);

            // Act
            var result = await _service.ReportExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ReportExistsAsync(99).Returns(false);

            // Act
            var result = await _service.ReportExistsAsync(99);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
