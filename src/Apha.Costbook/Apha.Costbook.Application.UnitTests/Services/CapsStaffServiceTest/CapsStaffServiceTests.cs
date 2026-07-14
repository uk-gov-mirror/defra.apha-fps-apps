using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.CapsStaffServiceTest
{
    public class CapsStaffServiceTests
    {
        private readonly ICapsStaffRepository _repository;
        private readonly IMapper _mapper;
        private readonly CapsStaffService _service;

        public CapsStaffServiceTests()
        {
            _repository = Substitute.For<ICapsStaffRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new CapsStaffService(_repository, _mapper);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice" },
                new Staff { Mnumber = "M002", Name = "Bob" }
            };
            var dtos = new List<StaffDto>
            {
                new StaffDto { Mnumber = "M001", Name = "Alice" },
                new StaffDto { Mnumber = "M002", Name = "Bob" }
            };
            _repository.GetAllStaffAsync().Returns(entities);
            _mapper.Map<List<StaffDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllStaffAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<Staff>();
            var dtos = new List<StaffDto>();
            _repository.GetAllStaffAsync().Returns(entities);
            _mapper.Map<List<StaffDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── GetPaginatedAsync ─────────────────────────────────────────────────

        #region GetPaginatedAsync Tests

        [Fact]
        public async Task GetPaginatedAsync_ValidParameters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var coreParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var staffEntities = new List<Staff> { new Staff { Mnumber = "M001", Name = "Alice" } };
            var staffDtos = new List<StaffDto> { new StaffDto { Mnumber = "M001", Name = "Alice" } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var pagedData = new PagedData<Staff>(staffEntities, paginationData);

            _mapper.Map<PaginationParameters<string>>(queryParameters).Returns(coreParams);
            _repository.GetPaginatedAsync(coreParams).Returns(pagedData);
            _mapper.Map<List<StaffDto>>(pagedData.Data).Returns(staffDtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _service.GetPaginatedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            _mapper.Received(1).Map<List<StaffDto>>(pagedData.Data);
            _mapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
            await _repository.Received(1).GetPaginatedAsync(coreParams);
        }

        [Fact]
        public async Task GetPaginatedAsync_NullParameters_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPaginatedAsync(null!));
            await _repository.DidNotReceive().GetPaginatedAsync(Arg.Any<PaginationParameters<string>>());
        }

        #endregion

        // ── GetByMNumberAsync ─────────────────────────────────────────────────

        #region GetByMNumberAsync Tests

        [Fact]
        public async Task GetByMNumberAsync_ExistingMNumber_ReturnsMappedDto()
        {
            // Arrange
            var mNumber = "M001";
            var entity = new Staff { Mnumber = mNumber, Name = "Alice" };
            var dto = new StaffDto { Mnumber = mNumber, Name = "Alice" };
            _repository.GetByMNumberAsync(mNumber).Returns(entity);
            _mapper.Map<StaffDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByMNumberAsync(mNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mNumber, result!.Mnumber);
        }

        [Fact]
        public async Task GetByMNumberAsync_NonExistentMNumber_ReturnsNull()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _repository.GetByMNumberAsync(mNumber).Returns((Staff?)null);

            // Act
            var result = await _service.GetByMNumberAsync(mNumber);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByMNumberAsync_NullMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByMNumberAsync(null!));
            await _repository.DidNotReceive().GetByMNumberAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetByMNumberAsync_WhitespaceMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByMNumberAsync("   "));
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidDto_AddsAndReturnsMappedDto()
        {
            // Arrange
            var dto = new StaffDto { Mnumber = "M003", Name = "Charlie" };
            var entity = new Staff { Mnumber = "M003", Name = "Charlie" };
            var created = new Staff { Mnumber = "M003", Name = "Charlie" };
            var createdDto = new StaffDto { Mnumber = "M003", Name = "Charlie" };
            _repository.ExistsAsync("M003").Returns(false);
            _mapper.Map<Staff>(dto).Returns(entity);
            _repository.AddStaffAsync(entity).Returns(created);
            _mapper.Map<StaffDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddStaffAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M003", result.Mnumber);
            await _repository.Received(1).AddStaffAsync(entity);
        }

        [Fact]
        public async Task AddAsync_DuplicateMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new StaffDto { Mnumber = "M001", Name = "Duplicate" };
            _repository.ExistsAsync("M001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddStaffAsync(dto));
            await _repository.DidNotReceive().AddStaffAsync(Arg.Any<Staff>());
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddStaffAsync(null!));
        }

        [Fact]
        public async Task AddAsync_EmptyMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new StaffDto { Mnumber = "", Name = "Alice" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddStaffAsync(dto));
        }

        [Fact]
        public async Task AddAsync_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var dto = new StaffDto { Mnumber = "M003", Name = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.AddStaffAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingMNumber_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var mNumber = "M001";
            var dto = new StaffDto { Mnumber = mNumber, Name = "Alice Updated" };   
            var entity = new Staff { Mnumber = mNumber, Name = "Alice Updated" };
            var updated = new Staff { Mnumber = mNumber, Name = "Alice Updated" };
            var updatedDto = new StaffDto { Mnumber = mNumber, Name = "Alice Updated" };
            _repository.ExistsAsync(mNumber).Returns(true);
            _mapper.Map<Staff>(dto).Returns(entity);
            _repository.UpdateStaffAsync(entity).Returns(updated);
            _mapper.Map<StaffDto>(updated).Returns(updatedDto);

            // Act
            var result = await _service.UpdateStaffAsync(mNumber, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mNumber, result.Mnumber);
            await _repository.Received(1).UpdateStaffAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentMNumber_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            var dto = new StaffDto { Mnumber = mNumber, Name = "Ghost" };
            _repository.ExistsAsync(mNumber).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateStaffAsync(mNumber, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullMNumber_ThrowsArgumentException()
        {
            // Arrange
            var dto = new StaffDto { Mnumber = "M001", Name = "Alice" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateStaffAsync(null!, dto));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Arrange
            var mNumber = "M001";
            _repository.ExistsAsync(mNumber).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateStaffAsync(mNumber, null!));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ExistingMNumber_CallsRepositoryDelete()
        {
            // Arrange
            var mNumber = "M001";
            _repository.ExistsAsync(mNumber).Returns(true);
            _repository.DeleteStaffAsync(mNumber).Returns(true);

            // Act
            await _service.DeleteStaffAsync(mNumber);

            // Assert
            await _repository.Received(1).DeleteStaffAsync(mNumber);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentMNumber_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mNumber = "NOTEXIST";
            _repository.ExistsAsync(mNumber).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteStaffAsync(mNumber));
            await _repository.DidNotReceive().DeleteStaffAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_NullMNumber_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteStaffAsync(null!));
        }

        #endregion
    }
}
