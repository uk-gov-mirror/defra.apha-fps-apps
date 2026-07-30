using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Application.Validation;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.AccountGroupServiceTest
{
    public class AccountGroupServiceTests
    {
        private readonly IAccountGroupRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccountGroupService _service;

        public AccountGroupServiceTests()
        {
            _repository = Substitute.For<IAccountGroupRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new AccountGroupService(_repository, _mapper);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true },
                new AccountGroup { Csg7group = "CSG002", Useinflation = false }
            };
            var dtos = new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7group = "CSG001", Useinflation = true },
                new AccountGroupDto { Csg7group = "CSG002", Useinflation = false }
            };
            _repository.GetAllAccountGroupAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAccountGroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAccountGroupAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<AccountGroup>();
            var dtos = new List<AccountGroupDto>();
            _repository.GetAllAccountGroupAsync().Returns(entities);
            _mapper.Map<List<AccountGroupDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAccountGroupAsync();

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
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true }
            };
            var accountGroupDtos = new List<AccountGroupDto>
            {
                new AccountGroupDto { Csg7group = "CSG001", Useinflation = true }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var pagedData = new PagedData<AccountGroup>(accountGroups, paginationData);

            _mapper.Map<PaginationParameters<string>>(queryParameters).Returns(coreParams);
            _repository.GetPaginatedAsync(coreParams).Returns(pagedData);
            _mapper.Map<List<AccountGroupDto>>(pagedData.Data).Returns(accountGroupDtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _service.GetPaginatedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            _mapper.Received(1).Map<List<AccountGroupDto>>(pagedData.Data);
            _mapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
            await _repository.Received(1).GetPaginatedAsync(coreParams);
        }

        #endregion

        // ── GetByCsg7GroupAsync ───────────────────────────────────────────────

        #region GetByCsg7GroupAsync Tests

        [Fact]
        public async Task GetByCsg7GroupAsync_ExistingKey_ReturnsMappedDto()
        {
            // Arrange
            var key = "CSG001";
            var entity = new AccountGroup { Csg7group = key, Useinflation = true };
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = true };
            _repository.GetByCsg7GroupAsync(key).Returns(entity);
            _mapper.Map<AccountGroupDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByCsg7GroupAsync(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result!.Csg7group);
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_NonExistentKey_ReturnsNull()
        {
            // Arrange
            var key = "NOTEXIST";
            _repository.GetByCsg7GroupAsync(key).Returns((AccountGroup?)null);

            // Act
            var result = await _service.GetByCsg7GroupAsync(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_NullKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.GetByCsg7GroupAsync(null!));
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_WhitespaceKey_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.GetByCsg7GroupAsync("   "));
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidDto_AddsAndReturnsMappedDto()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            var entity = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var created = new AccountGroup { Csg7group = "CSG003", Useinflation = true };
            var createdDto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = true };
            _repository.ExistsAsync("CSG003").Returns(false);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.AddAccountGroupAsync(entity).Returns(created);
            _mapper.Map<AccountGroupDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddAccountGroupAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CSG003", result.Csg7group);
            await _repository.Received(1).AddAccountGroupAsync(entity);
        }

        [Fact]
        public async Task AddAsync_KeyWithWhitespace_TrimsBeforeSave()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "  CSG003  ", Useinflation = false };
            var entity = new AccountGroup { Csg7group = "CSG003", Useinflation = false };
            var created = new AccountGroup { Csg7group = "CSG003", Useinflation = false };
            var createdDto = new AccountGroupDto { Csg7group = "CSG003", Useinflation = false };
            _repository.ExistsAsync("CSG003").Returns(false);
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.AddAccountGroupAsync(entity).Returns(created);
            _mapper.Map<AccountGroupDto>(created).Returns(createdDto);

            // Act
            var result = await _service.AddAccountGroupAsync(dto);

            // Assert
            Assert.Equal("CSG003", dto.Csg7group);
            await _repository.Received(1).ExistsAsync("CSG003");
            await _repository.Received(1).AddAccountGroupAsync(entity);
        }

        [Fact]
        public async Task AddAsync_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "CSG001", Useinflation = true };
            _repository.ExistsAsync("CSG001").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.AddAccountGroupAsync(dto));
            await _repository.DidNotReceive().AddAccountGroupAsync(Arg.Any<AccountGroup>());
        }

        [Fact]
        public async Task AddAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.AddAccountGroupAsync(null!));
        }

        [Fact]
        public async Task AddAsync_EmptyKey_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7group = "", Useinflation = true };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.AddAccountGroupAsync(dto));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ValidKeyAndDto_UpdatesAndReturnsMappedDto()
        {
            // Arrange
            var key = "CSG001";
            var dto = new AccountGroupDto { Csg7group = key, Useinflation = false };
            var entity = new AccountGroup { Csg7group = key, Useinflation = false };
            var updated = new AccountGroup { Csg7group = key, Useinflation = false };
            var updatedDto = new AccountGroupDto { Csg7group = key, Useinflation = false };
            _mapper.Map<AccountGroup>(dto).Returns(entity);
            _repository.UpdateAccountGroupAsync(entity).Returns(updated);
            _mapper.Map<AccountGroupDto>(updated).Returns(updatedDto);

            // Act
            var result = await _service.UpdateAccountGroupAsync(key, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(key, result.Csg7group);
            await _repository.Received(1).UpdateAccountGroupAsync(entity);
            await _repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentException()
        {
            // Arrange
            var key = "CSG001";

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.UpdateAccountGroupAsync(key, null!));
            await _repository.DidNotReceive().UpdateAccountGroupAsync(Arg.Any<AccountGroup>());
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ValidKey_CallsRepositoryDelete()
        {
            // Arrange
            var key = "CSG001";
            _repository.DeleteAccountGroupAsync(key).Returns(true);

            // Act
            await _service.DeleteAccountGroupAsync(key);

            // Assert
            await _repository.Received(1).DeleteAccountGroupAsync(key);
            await _repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_AnyKey_DelegatesToRepositoryWithoutExistenceCheck()
        {
            // Arrange
            var key = "NOTEXIST";
            _repository.DeleteAccountGroupAsync(key).Returns(false);

            // Act
            await _service.DeleteAccountGroupAsync(key);

            // Assert
            await _repository.Received(1).DeleteAccountGroupAsync(key);
            await _repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        }

        #endregion
    }
}
