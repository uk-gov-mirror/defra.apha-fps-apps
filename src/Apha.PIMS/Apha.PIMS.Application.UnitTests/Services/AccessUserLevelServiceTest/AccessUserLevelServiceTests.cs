using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.AccessUserLevelServiceTest
{
    public class AccessUserLevelServiceTests
    {
        private readonly IAccessUserLevelRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccessUserLevelService _service;

        public AccessUserLevelServiceTests()
        {
            _repository = Substitute.For<IAccessUserLevelRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new AccessUserLevelService(_repository, _mapper);
        }

        private static AccessUserLevel MakeEntity(int systemid = 1, string ntlogin = "DOM\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        private static AccessUserLevelDto MakeDto(int systemid = 1, string ntlogin = "DOM\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessUserLevelService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessUserLevelService(_repository, null!));
        }

        #endregion

        #region GetPagedAccessUserLevelAllAsync

        [Fact]
        public async Task GetPagedAccessUserLevelAllAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Search = "dom", Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string>(search: "dom", page: 1, pageSize: 10);
            var entities = new List<AccessUserLevel> { MakeEntity(1, "dom\\u1", 1), MakeEntity(1, "dom\\u2", 2) };
            var pagedData = new PagedData<AccessUserLevel>(entities, new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });
            var resultDto = new PaginatedResult<AccessUserLevelDto>(
                new List<AccessUserLevelDto> { MakeDto(1, "dom\\u1", 1), MakeDto(1, "dom\\u2", 2) },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 });

            _mapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _repository.GetPagedAccessUserLevelAllAsync(parameters).Returns(pagedData);
            _mapper.Map<PaginatedResult<AccessUserLevelDto>>(pagedData).Returns(resultDto);

            // Act
            var result = await _service.GetPagedAccessUserLevelAllAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalRecords);
            await _repository.Received(1).GetPagedAccessUserLevelAllAsync(parameters);
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_RepositoryReturnsEntities_ReturnsMappedList()
        {
            // Arrange
            var entities = new List<AccessUserLevel> { MakeEntity(2, "dom\\u1", 1) };
            var dtos     = new List<AccessUserLevelDto> { MakeDto(2, "dom\\u1", 1) };
            _repository.GetBySystemIdAsync(2).Returns(entities);
            _mapper.Map<List<AccessUserLevelDto>>(entities).Returns(dtos);

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
            _repository.GetBySystemIdAsync(99).Returns(new List<AccessUserLevel>());
            _mapper.Map<List<AccessUserLevelDto>>(Arg.Any<List<AccessUserLevel>>()).Returns(new List<AccessUserLevelDto>());

            // Act
            var result = await _service.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByUserAsync

        [Fact]
        public async Task GetByUserAsync_ValidNtlogin_ReturnsMappedList()
        {
            // Arrange
            const string ntlogin = "dom\\jsmith";
            var entities = new List<AccessUserLevel> { MakeEntity(1, ntlogin, 3) };
            var dtos     = new List<AccessUserLevelDto> { MakeDto(1, ntlogin, 3) };
            _repository.GetByUserAsync(1, ntlogin).Returns(entities);
            _mapper.Map<List<AccessUserLevelDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetByUserAsync(1, ntlogin);

            // Assert
            Assert.Single(result);
            await _repository.Received(1).GetByUserAsync(1, ntlogin);
        }

        [Fact]
        public async Task GetByUserAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByUserAsync(1, ""));
        }

        [Fact]
        public async Task GetByUserAsync_WhitespaceNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByUserAsync(1, "   "));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            const string ntlogin = "dom\\user";
            var entity = MakeEntity(1, ntlogin, 2);
            var dto    = MakeDto(1, ntlogin, 2);
            _repository.GetByIdAsync(1, ntlogin, 2).Returns(entity);
            _mapper.Map<AccessUserLevelDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(1, ntlogin, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.AccessLevelId);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns((AccessUserLevel?)null);

            // Act
            var result = await _service.GetByIdAsync(99, "dom\\unknown", 88);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(1, "", 1));
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto       = MakeDto(1, "dom\\newuser", 7);
            var entity    = MakeEntity(1, "dom\\newuser", 7);
            var created   = MakeEntity(1, "dom\\newuser", 7);
            var resultDto = MakeDto(1, "dom\\newuser", 7);
            _repository.ExistsAsync(1, "dom\\newuser", 7).Returns(false);
            _mapper.Map<AccessUserLevel>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<AccessUserLevelDto>(created).Returns(resultDto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("dom\\newuser", result.NtLogin);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_DuplicateAssignment_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\existing", 7);
            _repository.ExistsAsync(1, "dom\\existing", 7).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_InvalidSystemId_ThrowsArgumentException()
        {
            var dto = MakeDto(0, "dom\\user", 1);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_InvalidAccessLevelId_ThrowsArgumentException()
        {
            var dto = MakeDto(1, "dom\\user", 0);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            var dto = MakeDto(1, "", 1);
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallAdd_WhenAssignmentAlreadyExists()
        {
            // Arrange
            var dto = MakeDto(1, "dom\\existing", 7);
            _repository.ExistsAsync(1, "dom\\existing", 7).Returns(true);

            // Act + ignore exception
            try { await _service.CreateAsync(dto); } catch { }

            // Assert
            await _repository.DidNotReceive().AddAsync(Arg.Any<AccessUserLevel>());
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_ReturnsTrueAndCallsRepositoryDelete()
        {
            // Arrange
            const string ntlogin = "dom\\user";
            _repository.ExistsAsync(1, ntlogin, 2).Returns(true);
            _repository.DeleteAsync(1, ntlogin, 2).Returns(true);

            // Act
            var result = await _service.DeleteAsync(1, ntlogin, 2);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteAsync(1, ntlogin, 2);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(99, "dom\\unknown", 88));
        }

        [Fact]
        public async Task DeleteAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(1, "", 1));
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ExistsAsync(1, "dom\\user", 2).Returns(true);

            // Act
            var result = await _service.ExistsAsync(1, "dom\\user", 2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(false);

            // Act
            var result = await _service.ExistsAsync(99, "dom\\unknown", 88);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyNtlogin_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExistsAsync(1, "", 1));
        }

        #endregion
    }
}
