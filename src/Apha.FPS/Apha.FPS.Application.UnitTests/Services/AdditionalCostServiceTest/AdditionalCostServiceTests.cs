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

namespace Apha.FPS.Application.UnitTests.Services.AdditionalCostServiceTest
{
    public class AdditionalCostServiceTests
    {
        private readonly IAdditionalCostRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AdditionalCostService _sut;

        public AdditionalCostServiceTests()
        {
            _mockRepository = Substitute.For<IAdditionalCostRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new AdditionalCostService(_mockRepository, _mockMapper);
        }

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_WithValidQueryAndJobCode_ReturnsPaginatedResult()
        {
            // Arrange
            var jobCode = "JOB001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var repositoryResult = new PagedData<AdditionalCost>
            {
                Data = new List<AdditionalCost>
                {
                    new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m },
                    new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", ItemCost = 200m }
                },
                PaginationData = new PaginationData { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            var expectedResult = new PaginatedResult<AdditionalCostDto>
            {
                Data = new List<AdditionalCostDto>
                {
                    new() { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m },
                    new() { JobCode = "JOB001", Account = "ACC2", Description = "Desc2", ItemCost = 200m }
                },
                PaginationData = new PaginationDto { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetByJobCodeAsync(mappedParams, jobCode).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<AdditionalCostDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetByJobCodeAsync(query, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.Data.First().Account.Should().Be("ACC1");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetByJobCodeAsync(mappedParams, jobCode);
            _mockMapper.Received(1).Map<PaginatedResult<AdditionalCostDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WithNoMatchingRecords_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var jobCode = "NONEXISTENT";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var emptyResult = new PagedData<AdditionalCost>
            {
                Data = new List<AdditionalCost>(),
                PaginationData = new PaginationData { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var emptyExpected = new PaginatedResult<AdditionalCostDto>
            {
                Data = new List<AdditionalCostDto>(),
                PaginationData = new PaginationDto { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetByJobCodeAsync(mappedParams, jobCode).Returns(emptyResult);
            _mockMapper.Map<PaginatedResult<AdditionalCostDto>>(emptyResult).Returns(emptyExpected);

            // Act
            var result = await _sut.GetByJobCodeAsync(query, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetByJobCodeAsync(mappedParams, Arg.Any<string>()).Throws(new Exception("DB error"));

            // Act & Assert
            await _sut.Invoking(s => s.GetByJobCodeAsync(query, "JOB001"))
                .Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        #endregion

        #region GetTotalItemCostAsync

        [Fact]
        public async Task GetTotalItemCostAsync_WithValidJobCode_ReturnsTotal()
        {
            // Arrange
            _mockRepository.GetTotalItemCostAsync("JOB001").Returns(350m);

            // Act
            var result = await _sut.GetTotalItemCostAsync("JOB001");

            // Assert
            result.Should().Be(350m);
            await _mockRepository.Received(1).GetTotalItemCostAsync("JOB001");
        }

        [Fact]
        public async Task GetTotalItemCostAsync_WithNoRecords_ReturnsZero()
        {
            // Arrange
            _mockRepository.GetTotalItemCostAsync("EMPTY").Returns(0m);

            // Act
            var result = await _sut.GetTotalItemCostAsync("EMPTY");

            // Assert
            result.Should().Be(0m);
        }

        #endregion

        #region GetAccountCategoriesAsync

        [Fact]
        public async Task GetAccountCategoriesAsync_ReturnsListOfCategories()
        {
            // Arrange
            var entities = new List<AccountCategory>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Account One" },
                new() { AccShortName = "ACC2", AccountDescription = "Account Two" }
            };
            var dtos = new List<AccountCategoryDto>
            {
                new() { AccShortName = "ACC1", AccountDescription = "Account One" },
                new() { AccShortName = "ACC2", AccountDescription = "Account Two" }
            };
            _mockRepository.GetAccountCategoriesAsync().Returns(entities);
            _mockMapper.Map<List<AccountCategoryDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().AccShortName.Should().Be("ACC1");
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAccountCategoriesAsync().Returns(new List<AccountCategory>());
            _mockMapper.Map<List<AccountCategoryDto>>(Arg.Any<List<AccountCategory>>()).Returns(new List<AccountCategoryDto>());

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithExistingKey_ReturnsDto()
        {
            // Arrange
            var entity = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "Desc1").Returns(entity);
            _mockMapper.Map<AdditionalCostDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync("JOB001", "ACC1", "Desc1");

            // Assert
            result.Should().NotBeNull();
            result!.JobCode.Should().Be("JOB001");
            result.Account.Should().Be("ACC1");
            result.Description.Should().Be("Desc1");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetByIdAsync("JOB999", "ACC999", "NoExist").Returns((AdditionalCost?)null);
            _mockMapper.Map<AdditionalCostDto>((AdditionalCost?)null).Returns((AdditionalCostDto?)null);

            // Act
            var result = await _sut.GetByIdAsync("JOB999", "ACC999", "NoExist");

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_WithNewRecord_ReturnsCreatedDto()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "NewDesc", ItemCost = 150m };
            var entity = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "NewDesc", ItemCost = 150m };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "NewDesc").Returns((AdditionalCost?)null);
            _mockMapper.Map<AdditionalCost>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(entity);
            _mockMapper.Map<AdditionalCostDto>(entity).Returns(dto);

            // Act
            var result = await _sut.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Description.Should().Be("NewDesc");
            await _mockRepository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task AddAsync_WithDuplicateCompositeKey_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Existing", ItemCost = 100m };
            var existing = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Existing" };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "Existing").Returns(existing);

            // Act & Assert
            await _sut.Invoking(s => s.AddAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*JOB001*ACC1*Existing*");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<AdditionalCost>());
        }

        [Fact]
        public async Task AddAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.AddAsync(null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddAsync_WithNegativeItemCost_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = -1m };

            // Act & Assert
            await _sut.Invoking(s => s.AddAsync(dto))
                .Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_WithExistingRecord_ReturnsUpdatedDto()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };
            var entity = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 200m };
            var existing = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = 100m };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "Desc1").Returns(existing);
            _mockMapper.Map<AdditionalCost>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity, "ACC1", "Desc1").Returns(entity);
            _mockMapper.Map<AdditionalCostDto>(entity).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.ItemCost.Should().Be(200m);
            await _mockRepository.Received(1).UpdateAsync(entity, "ACC1", "Desc1");
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingRecord_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Ghost", ItemCost = 100m };
            _mockRepository.GetByIdAsync("JOB001", "ACC1", "Ghost").Returns((AdditionalCost?)null);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*JOB001*ACC1*Ghost*");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<AdditionalCost>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.UpdateAsync(null!))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_WithNegativeItemCost_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var dto = new AdditionalCostDto { JobCode = "JOB001", Account = "ACC1", Description = "Desc1", ItemCost = -5m };

            // Act & Assert
            await _sut.Invoking(s => s.UpdateAsync(dto))
                .Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task UpdateAsync_WithOriginalDescription_UsesOriginalDescriptionAsLookupKey()
        {
            // Arrange — OriginalDescription differs from Description (rename scenario)
            var dto = new AdditionalCostDto
            {
                JobCode = "JOB001",
                Account = "ACC1",
                Description = "NewDesc",
                OriginalDescription = "OldDesc",
                ItemCost = 100m
            };
            var entity = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "NewDesc", ItemCost = 100m };
            var existing = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "OldDesc", ItemCost = 100m };

            // GetByIdAsync must be called with the ORIGINAL description
            _mockRepository.GetByIdAsync("JOB001", "ACC1", "OldDesc").Returns(existing);
            // No duplicate exists under the new description
            _mockRepository.GetByIdAsync("JOB001", "ACC1", "NewDesc").Returns((AdditionalCost?)null);
            _mockMapper.Map<AdditionalCost>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity, "ACC1", "OldDesc").Returns(entity);
            _mockMapper.Map<AdditionalCostDto>(entity).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetByIdAsync("JOB001", "ACC1", "OldDesc");
            await _mockRepository.Received(1).UpdateAsync(entity, "ACC1", "OldDesc");
        }

        [Fact]
        public async Task UpdateAsync_WhenDescriptionChangedAndNewDescriptionAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange — renaming to a description that is already taken
            var dto = new AdditionalCostDto
            {
                JobCode = "JOB001",
                Account = "ACC1",
                Description = "TakenDesc",
                OriginalDescription = "OldDesc",
                ItemCost = 100m
            };
            var existing = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "OldDesc" };
            var duplicate = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "TakenDesc" };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "OldDesc").Returns(existing);
            _mockRepository.GetByIdAsync("JOB001", "ACC1", "TakenDesc").Returns(duplicate);

            // Act & Assert
            await _sut.Invoking(s => s.UpdateAsync(dto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*JOB001*ACC1*TakenDesc*already exists*");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<AdditionalCost>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateAsync_WhenOriginalDescriptionIsWhitespace_FallsBackToDescription()
        {
            // Arrange — OriginalDescription is whitespace; service should fall back to Description
            var dto = new AdditionalCostDto
            {
                JobCode = "JOB001",
                Account = "ACC1",
                Description = "Desc1",
                OriginalDescription = "   ",
                ItemCost = 50m
            };
            var entity = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Desc1" };
            var existing = new AdditionalCost { JobCode = "JOB001", Account = "ACC1", Description = "Desc1" };

            _mockRepository.GetByIdAsync("JOB001", "ACC1", "Desc1").Returns(existing);
            _mockMapper.Map<AdditionalCost>(dto).Returns(entity);
            _mockRepository.UpdateAsync(entity, "ACC1", "Desc1").Returns(entity);
            _mockMapper.Map<AdditionalCostDto>(entity).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetByIdAsync("JOB001", "ACC1", "Desc1");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            _mockRepository.DeleteAsync("JOB001", "ACC1", "Desc1").Returns(true);

            // Act
            var result = await _sut.DeleteAsync("JOB001", "ACC1", "Desc1");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync("JOB001", "ACC1", "Desc1");
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            _mockRepository.DeleteAsync("JOB999", "ACC999", "NoExist").Returns(false);

            // Act
            var result = await _sut.DeleteAsync("JOB999", "ACC999", "NoExist");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WithNullOrEmptyJobCode_ThrowsArgumentException()
        {
            // Act & Assert
            await _sut.Invoking(s => s.DeleteAsync(string.Empty, "ACC1", "Desc1"))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.DeleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("DB error"));

            // Act & Assert
            await _sut.Invoking(s => s.DeleteAsync("JOB001", "ACC1", "Desc1"))
                .Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        #endregion
    }
}
