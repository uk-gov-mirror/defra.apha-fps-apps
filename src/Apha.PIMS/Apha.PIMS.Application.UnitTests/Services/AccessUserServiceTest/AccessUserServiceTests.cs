using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.AccessUserServiceTest
{
    public class AccessUserServiceTests
    {
        private readonly IAccessUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccessUserService _service;

        public AccessUserServiceTests()
        {
            _repository = Substitute.For<IAccessUserRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new AccessUserService(_repository, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static AccessUser MakeEntity(int systemid = 1, string ntlogin = "DOM\\user1") =>
            new AccessUser { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        private static AccessUserDto MakeDto(int systemid = 1, string ntlogin = "DOM\\user1") =>
            new AccessUserDto { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        // ── Constructor ───────────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessUserService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessUserService(_repository, null!));
        }

        #endregion

        // ── GetPagedAsync ─────────────────────────────────────────────────────────

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Search = "dom", Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>(search: "dom", page: 1, pageSize: 10);
            var entities = new List<AccessUser> { MakeEntity(1, "dom\\u1"), MakeEntity(1, "dom\\u2") };
            var pagedData = new PagedData<AccessUser>(entities, new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });
            var resultDto = new PaginatedResult<AccessUserDto>(
                new List<AccessUserDto> { MakeDto(1, "dom\\u1"), MakeDto(1, "dom\\u2") },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repository.GetPagedAsync(parameters).Returns(pagedData);
            _mapper.Map<PaginatedResult<AccessUserDto>>(pagedData).Returns(resultDto);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalRecords);
            await _repository.Received(1).GetPagedAsync(parameters);
        }

        [Fact]
        public async Task GetPagedAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);
            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repository.GetPagedAsync(parameters).Returns<Task<PagedData<AccessUser>>>(_ => throw new Exception("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPagedAsync(query));
        }

        #endregion

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<AccessUser> { MakeEntity(1, "dom\\u1"), MakeEntity(1, "dom\\u2") };
            var dtos     = new List<AccessUserDto> { MakeDto(1, "dom\\u1"), MakeDto(1, "dom\\u2") };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<AccessUserDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            _repository.GetAllAsync().Returns(new List<AccessUser>());
            _mapper.Map<List<AccessUserDto>>(Arg.Any<List<AccessUser>>()).Returns(new List<AccessUserDto>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // ── GetBySystemIdAsync ────────────────────────────────────────────────────

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_RepositoryReturnsEntities_ReturnsMappedList()
        {
            // Arrange
            var entities = new List<AccessUser> { MakeEntity(2, "dom\\u1") };
            var dtos     = new List<AccessUserDto> { MakeDto(2, "dom\\u1") };
            _repository.GetBySystemIdAsync(2).Returns(entities);
            _mapper.Map<List<AccessUserDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetBySystemIdAsync(2);

            // Assert
            Assert.Single(result);
            await _repository.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetBySystemIdAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetBySystemIdAsync(99).Returns(new List<AccessUser>());
            _mapper.Map<List<AccessUserDto>>(Arg.Any<List<AccessUser>>()).Returns(new List<AccessUserDto>());

            // Act
            var result = await _service.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // ── GetByNtLoginAsync ─────────────────────────────────────────────────────

        #region GetByNtLoginAsync

        [Fact]
        public async Task GetByNtLoginAsync_ValidNtlogin_ReturnsMappedList()
        {
            // Arrange
            const string ntlogin = "dom\\jsmith";
            var entities = new List<AccessUser> { MakeEntity(1, ntlogin) };
            var dtos     = new List<AccessUserDto> { MakeDto(1, ntlogin) };
            _repository.GetByNtLoginAsync(ntlogin).Returns(entities);
            _mapper.Map<List<AccessUserDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByNtLoginAsync(ntlogin);

            // Assert
            Assert.Single(result);
            await _repository.Received(1).GetByNtLoginAsync(ntlogin);
        }

        [Fact]
        public async Task GetByNtLoginAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByNtLoginAsync(""));
        }

        [Fact]
        public async Task GetByNtLoginAsync_WhitespaceNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByNtLoginAsync("   "));
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            const string ntlogin = "dom\\user";
            var entity = MakeEntity(1, ntlogin);
            var dto    = MakeDto(1, ntlogin);
            _repository.GetByIdAsync(1, ntlogin).Returns(entity);
            _mapper.Map<AccessUserDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(1, ntlogin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ntlogin, result!.NtLogin);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>()).Returns((AccessUser?)null);

            // Act
            var result = await _service.GetByIdAsync(99, "dom\\unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(1, ""));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            const string ntlogin = "dom\\newuser";
            var dto     = MakeDto(1, ntlogin);
            var entity  = MakeEntity(1, ntlogin);
            var created = MakeEntity(1, ntlogin);
            var result_dto = MakeDto(1, ntlogin);
            _repository.ExistsAsync(1, ntlogin).Returns(false);
            _repository.GetBySystemIdAsync(1).Returns(new List<AccessUser>());
            _mapper.Map<AccessUser>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<AccessUserDto>(created).Returns(result_dto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ntlogin, result.NtLogin);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_DuplicateUserExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\existing");
            _repository.ExistsAsync(1, "dom\\existing").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DuplicateEmailExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\newuser");
            dto.UserEmail = "user1@example.com";
            _repository.ExistsAsync(1, "dom\\newuser").Returns(false);
            _repository.GetBySystemIdAsync(1).Returns(new List<AccessUser>
            {
                MakeEntity(1, "dom\\existing")
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
            Assert.Equal("UserEmail already exists.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            var dto = new AccessUserDto { SystemId = 1, NtLogin = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallAdd_WhenUserAlreadyExists()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\existing");
            _repository.ExistsAsync(1, "dom\\existing").Returns(true);

            // Act + ignore exception
            try { await _service.CreateAsync(dto); } catch { }

            // Assert
            await _repository.DidNotReceive().AddAsync(Arg.Any<AccessUser>());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_EntityExists_ReturnsMappedUpdatedDto()
        {
            // Arrange
            const string ntlogin = "dom\\user";
            var dto     = MakeDto(1, ntlogin);
            var entity  = MakeEntity(1, ntlogin);
            var updated = MakeEntity(1, ntlogin);
            var result_dto = MakeDto(1, ntlogin);
            _repository.ExistsAsync(1, ntlogin).Returns(true);
            _repository.GetBySystemIdAsync(1).Returns(new List<AccessUser> { MakeEntity(1, ntlogin) });
            _mapper.Map<AccessUser>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<AccessUserDto>(updated).Returns(result_dto);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(MakeDto(99, "dom\\x")));
        }

        [Fact]
        public async Task UpdateAsync_DuplicateEmailExistsOnAnotherUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\user");
            dto.UserEmail = "user1@example.com";
            _repository.ExistsAsync(1, "dom\\user").Returns(true);
            _repository.GetBySystemIdAsync(1).Returns(new List<AccessUser>
            {
                MakeEntity(1, "dom\\user"),
                MakeEntity(1, "dom\\other")
            });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(dto));
            Assert.Equal("UserEmail already exists.", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            var dto = new AccessUserDto { SystemId = 1, NtLogin = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(dto));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_ReturnsTrueAndCallsRepositoryDelete()
        {
            // Arrange
            const string ntlogin = "dom\\user";
            _repository.ExistsAsync(1, ntlogin).Returns(true);
            _repository.DeleteAsync(1, ntlogin).Returns(true);

            // Act
            var result = await _service.DeleteAsync(1, ntlogin);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(1, ntlogin);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(99, "dom\\unknown"));
        }

        [Fact]
        public async Task DeleteAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(1, ""));
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ExistsAsync(1, "dom\\user").Returns(true);

            // Act
            var result = await _service.ExistsAsync(1, "dom\\user");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(false);

            // Act
            var result = await _service.ExistsAsync(99, "dom\\unknown");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExistsAsync(1, ""));
        }

        #endregion
    }
}
