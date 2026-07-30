using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.TestRCCostServiceTest
{
    public class TestRCCostServiceTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly ITestRCCostRepository _repository;
        private readonly IMapper _mapper;
        private readonly TestRCCostService _service;

        public TestRCCostServiceTests()
        {
            _repository = Substitute.For<ITestRCCostRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new TestRCCostService(_repository, _mapper);
        }

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_ValidRequest_ReturnsPagedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<TestRCCost>
            {
                Data = new List<TestRCCost> { CreateTestEntity() },
                PaginationData = new PaginationData { TotalRecords = 1 }
            };
            var expectedResult = new PaginatedResult<TestRCCostDto>
            {
                Data = new List<TestRCCostDto> { CreateTestDto() }
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetPagedByTestCodeAsync(paginationParams, DefaultTestCode).Returns(pagedData);
            _mapper.Map<PaginatedResult<TestRCCostDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _service.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            await _repository.Received(1).GetPagedByTestCodeAsync(paginationParams, DefaultTestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NullQuery_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.GetPagedByTestCodeAsync(null!, DefaultTestCode));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhitespaceTestCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetPagedByTestCodeAsync(new QueryParameters<string>(), "   "));
        }

        #endregion

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_ValidInput_ReturnsDtoList()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateTestEntity() };
            var dtos = new List<TestRCCostDto> { CreateTestDto() };

            _repository.GetByTestCodeAsync(DefaultTestCode).Returns(entities);
            _mapper.Map<IEnumerable<TestRCCostDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByTestCodeAsync(DefaultTestCode);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            await _repository.Received(1).GetByTestCodeAsync(DefaultTestCode);
        }

        [Fact]
        public async Task GetByTestCodeAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<TestRCCost>();
            var dtos = new List<TestRCCostDto>();

            _repository.GetByTestCodeAsync(DefaultTestCode).Returns(entities);
            _mapper.Map<IEnumerable<TestRCCostDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByTestCodeAsync(DefaultTestCode);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTestCodeAsync_WhitespaceTestCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByTestCodeAsync("   "));
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ExistingRecord_ReturnsDto()
        {
            // Arrange
            var entity = CreateTestEntity();
            var dto = CreateTestDto();

            _repository.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre).Returns(entity);
            _mapper.Map<TestRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByKeyAsync_RecordNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByKeyAsync("NOTEXIST", "PC999")
                .Returns((TestRCCost?)null);

            // Act
            var result = await _service.GetByKeyAsync("NOTEXIST", "PC999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WhitespaceProfitCentre_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetByKeyAsync(DefaultTestCode, "  "));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_CreatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.ExistsAsync(DefaultTestCode, DefaultProfitCentre).Returns(false);
            _mapper.Map<TestRCCost>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(entity);
            _mapper.Map<TestRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyTestCode_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.TestCode = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_EmptyProfitCentre_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.ProfitCentre = string.Empty;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_InvalidFpsYear_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.FpsYear = 0;
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DuplicatePrimaryKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.ExistsAsync(DefaultTestCode, DefaultProfitCentre).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Contains("already exists", ex.Message);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidDto_UpdatesSuccessfully()
        {
            // Arrange
            var dto = CreateTestDto();
            var entity = CreateTestEntity();

            _repository.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre).Returns(entity);
            _mapper.Map<TestRCCost>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(entity);
            _mapper.Map<TestRCCostDto>(entity).Returns(dto);

            // Act
            var result = await _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, null!));
        }

        [Fact]
        public async Task UpdateAsync_TestCodeMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.TestCode = "DIFFERENT";
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, dto));
        }

        [Fact]
        public async Task UpdateAsync_ProfitCentreMismatch_ThrowsArgumentException()
        {
            var dto = CreateTestDto();
            dto.ProfitCentre = "DIFFERENT";
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, dto));
        }


        [Fact]
        public async Task UpdateAsync_RecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = CreateTestDto();
            _repository.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre)
                .Returns((TestRCCost?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateAsync(DefaultTestCode, DefaultProfitCentre, dto));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            _repository.DeleteAsync(DefaultTestCode, DefaultProfitCentre).Returns(true);

            // Act
            var result = await _service.DeleteAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(DefaultTestCode, DefaultProfitCentre);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.DeleteAsync("NOTEXIST", "PC999").Returns(false);

            // Act
            var result = await _service.DeleteAsync("NOTEXIST", "PC999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceTestCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync("  ", DefaultProfitCentre));
        }

        [Fact]
        public async Task DeleteAsync_WhitespaceProfitCentre_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.DeleteAsync(DefaultTestCode, "  "));
        }

        #endregion

        #region Helper Methods

        private static TestRCCost CreateTestEntity() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        private static TestRCCostDto CreateTestDto() =>
            new()
            {
                TestCode = DefaultTestCode,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        #endregion
    }
}
