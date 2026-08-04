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

namespace Apha.FPS.Application.UnitTests.Services.AnimalServiceTest
{
    public class AnimalServiceTests
    {
        private readonly IAnimalRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AnimalService _sut;

        public AnimalServiceTests()
        {
            _mockRepository = Substitute.For<IAnimalRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new AnimalService(_mockRepository, _mockMapper);
        }

        private static AnimalDto BuildDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static Animal BuildEntity(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m, FpsYear = 2025 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimalService(null!, _mockMapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AnimalService(_mockRepository, null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged)

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsMappedDtos()
        {
            var entities = new List<Animal> { BuildEntity() };
            var dtos = new List<AnimalDto> { BuildDto() };

            _mockRepository.GetAllAnimalsAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<AnimalDto>>(entities).Returns(dtos);

            var result = await _sut.GetAllAnimalsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            _mockRepository.GetAllAnimalsAsync().Returns(new List<Animal>());
            _mockMapper.Map<IEnumerable<AnimalDto>>(Arg.Any<IEnumerable<Animal>>())
                .Returns(new List<AnimalDto>());

            var result = await _sut.GetAllAnimalsAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.GetAllAnimalsAsync().ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAnimalsAsync());
        }

        #endregion

        #region GetAllAnimalsAsync (paged)

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllAnimalsAsync(null!));
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PagedData<Animal>
            {
                Data = [BuildEntity()],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expected = new PaginatedResult<AnimalDto>
            {
                Data = [BuildDto()],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllAnimalsAsync(paginationParams).Returns(paged);
            _mockMapper.Map<PaginatedResult<AnimalDto>>(paged).Returns(expected);

            var result = await _sut.GetAllAnimalsAsync(query);

            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var paged = new PagedData<Animal>
            {
                Data = [],
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 0, TotalRecords = 0 }
            };
            var expected = new PaginatedResult<AnimalDto>
            {
                Data = [],
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 0, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllAnimalsAsync(paginationParams).Returns(paged);
            _mockMapper.Map<PaginatedResult<AnimalDto>>(paged).Returns(expected);

            var result = await _sut.GetAllAnimalsAsync(query);

            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetAnimalByIdAsync

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAnimalByIdAsync(""));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenAnimalTypeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAnimalByIdAsync("   "));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsDto_WhenFound()
        {
            var entity = BuildEntity();
            var dto = BuildDto();

            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(entity);
            _mockMapper.Map<AnimalDto?>(entity).Returns(dto);

            var result = await _sut.GetAnimalByIdAsync("CATTLE");

            result.Should().NotBeNull();
            result!.AnimalType.Should().Be("CATTLE");
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository.GetAnimalByIdAsync("NOTEXIST").Returns((Animal?)null);
            _mockMapper.Map<AnimalDto?>(null).Returns((AnimalDto?)null);

            var result = await _sut.GetAnimalByIdAsync("NOTEXIST");

            result.Should().BeNull();
        }

        #endregion

        #region GetAnimalCostAsync

        [Fact]
        public async Task GetAnimalCostAsync_WithValidQueryAndJobCode_ReturnsPaginatedResult()
        {
            // Arrange
            var jobCode = "JOB001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{\"JobCode\":\"JOB001\"}" };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"JobCode\":\"JOB001\"}" };

            var repositoryResult = new PagedData<AnimalCostView>
            {
                Data = new List<AnimalCostView>
                {
                    new AnimalCostView { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 },
                    new AnimalCostView { IndCounter = 2, JobCode = "JOB001", AnimalType = "DOG", NumberOfDays = 3, NumberOfAnimals = 5 }
                },
                PaginationData = new PaginationData { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            var expectedResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data = new List<AnimalCostViewDto>
                {
                    new AnimalCostViewDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 },
                    new AnimalCostViewDto { IndCounter = 2, JobCode = "JOB001", AnimalType = "DOG", NumberOfDays = 3, NumberOfAnimals = 5 }
                },
                PaginationData = new PaginationDto { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostAsync(mappedParams, jobCode).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAnimalCostAsync(query, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.PaginationData.PageNumber.Should().Be(1);
            result.Data.First().AnimalType.Should().Be("CAT");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAnimalCostAsync(mappedParams, jobCode);
            _mockMapper.Received(1).Map<PaginatedResult<AnimalCostViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetAnimalCostAsync_WithNoMatchingRecords_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var jobCode = "NONEXISTENT";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var emptyRepositoryResult = new PagedData<AnimalCostView>
            {
                Data = new List<AnimalCostView>(),
                PaginationData = new PaginationData { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            var emptyExpectedResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data = new List<AnimalCostViewDto>(),
                PaginationData = new PaginationDto { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostAsync(mappedParams, jobCode).Returns(emptyRepositoryResult);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(emptyRepositoryResult).Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetAnimalCostAsync(query, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAnimalCostAsync(mappedParams, jobCode);
        }

        [Fact]
        public async Task GetAnimalCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var jobCode = "JOB001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostAsync(mappedParams, jobCode)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetAnimalCostAsync(query, jobCode)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.DidNotReceive().Map<PaginatedResult<AnimalCostViewDto>>(Arg.Any<PagedData<AnimalCostView>>());
        }

        #endregion

        #region GetAnimalCostByAnimalTypeAsync

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithValidQueryAndAnimalType_ReturnsPaginatedResult()
        {
            // Arrange
            var animalType = "CATTLE";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var repositoryResult = new PagedData<AnimalCostView>
            {
                Data = new List<AnimalCostView>
                {
                    new AnimalCostView { IndCounter = 1, JobCode = "JOB001", AnimalType = "CATTLE", NumberOfDays = 5, NumberOfAnimals = 10 },
                    new AnimalCostView { IndCounter = 2, JobCode = "JOB002", AnimalType = "CATTLE", NumberOfDays = 3, NumberOfAnimals = 5 }
                },
                PaginationData = new PaginationData { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            var expectedResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data = new List<AnimalCostViewDto>
                {
                    new AnimalCostViewDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "CATTLE", NumberOfDays = 5, NumberOfAnimals = 10 },
                    new AnimalCostViewDto { IndCounter = 2, JobCode = "JOB002", AnimalType = "CATTLE", NumberOfDays = 3, NumberOfAnimals = 5 }
                },
                PaginationData = new PaginationDto { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, animalType).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.Data.First().AnimalType.Should().Be("CATTLE");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAnimalCostByAnimalTypeAsync(mappedParams, animalType);
            _mockMapper.Received(1).Map<PaginatedResult<AnimalCostViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WithNoMatchingRecords_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var animalType = "NONEXISTENT";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var emptyRepositoryResult = new PagedData<AnimalCostView>
            {
                Data = new List<AnimalCostView>(),
                PaginationData = new PaginationData { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            var emptyExpectedResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data = new List<AnimalCostViewDto>(),
                PaginationData = new PaginationDto { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, animalType).Returns(emptyRepositoryResult);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(emptyRepositoryResult).Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAnimalCostByAnimalTypeAsync(mappedParams, animalType);
        }

        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var animalType = "CATTLE";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, animalType)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetAnimalCostByAnimalTypeAsync(query, animalType)
            );

            exception.Message.Should().Be("Database connection failed");
            _mockMapper.DidNotReceive().Map<PaginatedResult<AnimalCostViewDto>>(Arg.Any<PagedData<AnimalCostView>>());
        }

        #endregion

        #region GetAnimalLookupAsync

        [Fact]
        public async Task GetAnimalLookupAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var animalEntities = new List<Animal>
            {
                new Animal { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m },
                new Animal { AnimalType = "DOG", Species = "Domestic", DailyRate = 60.00m }
            };

            var expectedDtos = new List<AnimalDto>
            {
                new AnimalDto { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m },
                new AnimalDto { AnimalType = "DOG", Species = "Domestic", DailyRate = 60.00m }
            };

            _mockRepository.GetAnimalLookup().Returns(Task.FromResult(animalEntities));
            _mockMapper.Map<List<AnimalDto>>(animalEntities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAnimalLookupAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].AnimalType.Should().Be("CAT");
            result[1].AnimalType.Should().Be("DOG");

            await _mockRepository.Received(1).GetAnimalLookup();
            _mockMapper.Received(1).Map<List<AnimalDto>>(animalEntities);
        }

        [Fact]
        public async Task GetAnimalLookupAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyEntities = new List<Animal>();
            var emptyDtos = new List<AnimalDto>();

            _mockRepository.GetAnimalLookup().Returns(Task.FromResult(emptyEntities));
            _mockMapper.Map<List<AnimalDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAnimalLookupAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAnimalLookup();
        }

        [Fact]
        public async Task GetAnimalLookupAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAnimalLookup()
                .Returns(Task.FromException<List<Animal>>(new Exception("Database connection failed")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAnimalLookupAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAnimalLookup();
            _mockMapper.DidNotReceive().Map<List<AnimalDto>>(Arg.Any<List<Animal>>());
        }

        #endregion

        #region GetAnimalRateByIdAsync

        [Fact]
        public async Task GetAnimalRateByIdAsync_WithValidAnimalType_ReturnsRate()
        {
            // Arrange
            var animalType = "CAT";
            var jobCode = "JOB001";
            var expectedRate = 75.50m;

            _mockRepository.GetAnimalRateByIdAsync(animalType, jobCode)
                .Returns(Task.FromResult<decimal?>(expectedRate));

            // Act
            var result = await _sut.GetAnimalRateByIdAsync(animalType, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedRate);

            await _mockRepository.Received(1).GetAnimalRateByIdAsync(animalType, jobCode);
        }

        [Fact]
        public async Task GetAnimalRateByIdAsync_WhenAnimalTypeNotFound_ReturnsNull()
        {
            // Arrange
            var animalType = "UNKNOWN";
            var jobCode = "JOB001";

            _mockRepository.GetAnimalRateByIdAsync(animalType, jobCode)
                .Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _sut.GetAnimalRateByIdAsync(animalType, jobCode);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAnimalRateByIdAsync(animalType, jobCode);
        }

        [Fact]
        public async Task GetAnimalRateByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var animalType = "CAT";
            var jobCode = "JOB001";

            _mockRepository.GetAnimalRateByIdAsync(animalType, jobCode)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetAnimalRateByIdAsync(animalType, jobCode)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAnimalRateByIdAsync(animalType, jobCode);
        }

        #endregion

        #region AddAnimalCostAsync

        [Fact]
        public async Task AddAnimalCostAsync_WithValidRequest_ReturnsMappedDto()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var mappedEntity = new AnimalRequest { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var repositoryResult = new AnimalRequest { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var expectedDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };

            _mockMapper.Map<AnimalRequest>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAnimalCostAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<AnimalRequestDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddAnimalCostAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.JobCode.Should().Be("JOB001");
            result.AnimalType.Should().Be("CAT");

            _mockMapper.Received(1).Map<AnimalRequest>(inputDto);
            await _mockRepository.Received(1).AddAnimalCostAsync(mappedEntity);
            _mockMapper.Received(1).Map<AnimalRequestDto>(repositoryResult);
        }

        [Fact]
        public async Task AddAnimalCostAsync_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.AddAnimalCostAsync(null!)
            );

            await _mockRepository.DidNotReceive().AddAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task AddAnimalCostAsync_WhenNumberOfDaysIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = -1, NumberOfAnimals = 5 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.AddAnimalCostAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().AddAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task AddAnimalCostAsync_WhenNumberOfAnimalsIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = -1 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.AddAnimalCostAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().AddAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task AddAnimalCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };
            var mappedEntity = new AnimalRequest { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 10 };

            _mockMapper.Map<AnimalRequest>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAnimalCostAsync(mappedEntity)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _sut.AddAnimalCostAsync(inputDto);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Database connection failed");

            await _mockRepository.Received(1).AddAnimalCostAsync(mappedEntity);
        }

        #endregion

        #region UpdateAnimalCostAsync

        [Fact]
        public async Task UpdateAnimalCostAsync_WithValidRequest_ReturnsMappedDto()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var mappedEntity = new AnimalRequest { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var updatedEntity = new AnimalRequest { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };
            var expectedDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 7, NumberOfAnimals = 12 };

            _mockMapper.Map<AnimalRequest>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAnimalCostAsync(mappedEntity).Returns(updatedEntity);
            _mockMapper.Map<AnimalRequestDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAnimalCostAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.JobCode.Should().Be("JOB001");
            result.NumberOfDays.Should().Be(7);

            _mockMapper.Received(1).Map<AnimalRequest>(inputDto);
            await _mockRepository.Received(1).UpdateAnimalCostAsync(mappedEntity);
            _mockMapper.Received(1).Map<AnimalRequestDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenRequestIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.UpdateAnimalCostAsync(null!)
            );

            await _mockRepository.DidNotReceive().UpdateAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenNumberOfDaysIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = -3, NumberOfAnimals = 5 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.UpdateAnimalCostAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().UpdateAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenNumberOfAnimalsIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = -2 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.UpdateAnimalCostAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().UpdateAnimalCostAsync(Arg.Any<AnimalRequest>());
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new AnimalRequestDto { JobCode = "JOB999", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 5 };
            var mappedEntity = new AnimalRequest { JobCode = "JOB999", AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 5 };

            _mockMapper.Map<AnimalRequest>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAnimalCostAsync(mappedEntity)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.UpdateAnimalCostAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).UpdateAnimalCostAsync(mappedEntity);
            _mockMapper.DidNotReceive().Map<AnimalRequestDto>(Arg.Any<AnimalRequest>());
        }

        #endregion

        #region DeleteAnimalCostAsync

        [Fact]
        public async Task DeleteAnimalCostAsync_WithValidIndCounter_ReturnsTrue()
        {
            // Arrange
            var indCounter = 1;
            _mockRepository.DeleteJobAnimalCostAsync(indCounter).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.DeleteAnimalCostAsync(indCounter);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteJobAnimalCostAsync(indCounter);
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WithNonExistentRecord_ReturnsFalse()
        {
            // Arrange
            var indCounter = 9999;
            _mockRepository.DeleteJobAnimalCostAsync(indCounter).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAnimalCostAsync(indCounter);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteJobAnimalCostAsync(indCounter);
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WhenIndCounterIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var indCounter = -1;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.DeleteAnimalCostAsync(indCounter)
            );

            await _mockRepository.DidNotReceive().DeleteJobAnimalCostAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WhenIndCounterIsZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var indCounter = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.DeleteAnimalCostAsync(indCounter)
            );

            await _mockRepository.DidNotReceive().DeleteJobAnimalCostAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var indCounter = 1;
            _mockRepository.DeleteJobAnimalCostAsync(indCounter)
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.DeleteAnimalCostAsync(indCounter)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).DeleteJobAnimalCostAsync(indCounter);
        }

        #endregion

        #region GetTotalAnimalCostAsync

        [Fact]
        public async Task GetTotalAnimalCostAsync_WithValidJobCode_ReturnsTotalFromRepository()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedTotal = 220m;

            _mockRepository.GetTotalAnimalCostAsync(jobCode)
                .Returns(Task.FromResult(expectedTotal));

            // Act
            var result = await _sut.GetTotalAnimalCostAsync(jobCode);

            // Assert
            result.Should().Be(expectedTotal);
            await _mockRepository.Received(1).GetTotalAnimalCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_WithNoMatchingRecords_ReturnsZero()
        {
            // Arrange
            var jobCode = "NONEXISTENT";

            _mockRepository.GetTotalAnimalCostAsync(jobCode)
                .Returns(Task.FromResult(0m));

            // Act
            var result = await _sut.GetTotalAnimalCostAsync(jobCode);

            // Assert
            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalAnimalCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var jobCode = "JOB001";

            _mockRepository.GetTotalAnimalCostAsync(jobCode)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetTotalAnimalCostAsync(jobCode)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetTotalAnimalCostAsync(jobCode);
        }

        #endregion

        #region GetAnimalCostViewByIdAsync

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WithValidParameters_ReturnsMappedDto()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";

            var entity = new AnimalCostView
            {
                IndCounter     = indCounter,
                JobCode        = jobCode,
                AnimalType     = "CAT",
                NumberOfDays   = 5,
                NumberOfAnimals = 2,
                AnimalCost     = 100m
            };
            var expectedDto = new AnimalCostViewDto
            {
                IndCounter     = indCounter,
                JobCode        = jobCode,
                AnimalType     = "CAT",
                NumberOfDays   = 5,
                NumberOfAnimals = 2,
                AnimalCost     = 100m
            };

            _mockRepository.GetAnimalCostViewByIdAsync(indCounter, jobCode)
                .Returns(Task.FromResult<AnimalCostView?>(entity));
            _mockMapper.Map<AnimalCostViewDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.IndCounter.Should().Be(indCounter);
            result.JobCode.Should().Be(jobCode);
            result.AnimalCost.Should().Be(100m);

            await _mockRepository.Received(1).GetAnimalCostViewByIdAsync(indCounter, jobCode);
            _mockMapper.Received(1).Map<AnimalCostViewDto>(entity);
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            var indCounter = 999;
            var jobCode = "JOB001";

            _mockRepository.GetAnimalCostViewByIdAsync(indCounter, jobCode)
                .Returns(Task.FromResult<AnimalCostView?>(null));

            // Act
            var result = await _sut.GetAnimalCostViewByIdAsync(indCounter, jobCode);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAnimalCostViewByIdAsync(indCounter, jobCode);
            _mockMapper.DidNotReceive().Map<AnimalCostViewDto>(Arg.Any<AnimalCostView>());
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var indCounter = 1;
            var jobCode = "JOB001";

            _mockRepository.GetAnimalCostViewByIdAsync(indCounter, jobCode)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetAnimalCostViewByIdAsync(indCounter, jobCode)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAnimalCostViewByIdAsync(indCounter, jobCode);
            _mockMapper.DidNotReceive().Map<AnimalCostViewDto>(Arg.Any<AnimalCostView>());
        }

        #endregion

        #region AddAnimalAsync

        [Fact]
        public async Task AddAnimalAsync_WhenAnimalDoesNotExist_ReturnsCreatedDto()
        {
            // Arrange
            var inputDto = new AnimalDto { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };
            var mappedEntity = new Animal { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };
            var savedEntity = new Animal { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };
            var expectedDto = new AnimalDto { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };

            _mockRepository.GetAnimalByIdAsync(inputDto.AnimalType).Returns(Task.FromResult<Animal?>(null));
            _mockMapper.Map<Animal>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAnimalAsync(mappedEntity).Returns(savedEntity);
            _mockMapper.Map<AnimalDto>(savedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddAnimalAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.AnimalType.Should().Be("CAT");
            result.DailyRate.Should().Be(50.00m);

            await _mockRepository.Received(1).GetAnimalByIdAsync(inputDto.AnimalType);
            _mockMapper.Received(1).Map<Animal>(inputDto);
            await _mockRepository.Received(1).AddAnimalAsync(mappedEntity);
            _mockMapper.Received(1).Map<AnimalDto>(savedEntity);
        }

        [Fact]
        public async Task AddAnimalAsync_WhenAnimalAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = new AnimalDto { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };
            var existingEntity = new Animal { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };

            _mockRepository.GetAnimalByIdAsync(inputDto.AnimalType).Returns(Task.FromResult<Animal?>(existingEntity));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.AddAnimalAsync(inputDto)
            );

            exception.Message.Should().Be($"Animal '{inputDto.AnimalType}' already exists.");

            await _mockRepository.Received(1).GetAnimalByIdAsync(inputDto.AnimalType);
            await _mockRepository.DidNotReceive().AddAnimalAsync(Arg.Any<Animal>());
            _mockMapper.DidNotReceive().Map<Animal>(Arg.Any<AnimalDto>());
        }

        [Fact]
        public async Task AddAnimalAsync_WhenDtoIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.AddAnimalAsync(null!)
            );

            await _mockRepository.DidNotReceive().GetAnimalByIdAsync(Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddAnimalAsync(Arg.Any<Animal>());
        }

        [Fact]
        public async Task AddAnimalAsync_WhenAnimalTypeIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = new AnimalDto { AnimalType = "", Species = "Domestic", DailyRate = 50.00m };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.AddAnimalAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().GetAnimalByIdAsync(Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddAnimalAsync(Arg.Any<Animal>());
        }

        [Fact]
        public async Task AddAnimalAsync_WhenAnimalTypeIsWhiteSpace_ThrowsArgumentException()
        {
            // Arrange
            var inputDto = new AnimalDto { AnimalType = "   ", Species = "Domestic", DailyRate = 50.00m };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.AddAnimalAsync(inputDto)
            );

            await _mockRepository.DidNotReceive().GetAnimalByIdAsync(Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddAnimalAsync(Arg.Any<Animal>());
        }

        [Fact]
        public async Task AddAnimalAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new AnimalDto { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };
            var mappedEntity = new Animal { AnimalType = "CAT", Species = "Domestic", DailyRate = 50.00m };

            _mockRepository.GetAnimalByIdAsync(inputDto.AnimalType).Returns(Task.FromResult<Animal?>(null));
            _mockMapper.Map<Animal>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAnimalAsync(mappedEntity)
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AddAnimalAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAnimalByIdAsync(inputDto.AnimalType);
            await _mockRepository.Received(1).AddAnimalAsync(mappedEntity);
            _mockMapper.DidNotReceive().Map<AnimalDto>(Arg.Any<Animal>());
        }

        #endregion

        #region UpdateAnimalAsync

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAnimalAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentException_WhenAnimalTypeIsEmpty()
        {
            var dto = new AnimalDto { AnimalType = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAnimalAsync(dto));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsKeyNotFoundException_WhenAnimalNotFound()
        {
            var dto = BuildDto("NOTEXIST");
            _mockRepository.GetAnimalByIdAsync("NOTEXIST").Returns((Animal?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAnimalAsync(dto));
        }

        [Fact]
        public async Task UpdateAnimalAsync_ReturnsUpdatedDto()
        {
            var dto = BuildDto();
            var existing = BuildEntity();
            var updatedEntity = BuildEntity();

            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(existing);
            _mockMapper.Map(dto, existing);
            _mockRepository.UpdateAnimalAsync(existing).Returns(updatedEntity);
            _mockMapper.Map<AnimalDto>(updatedEntity).Returns(dto);

            var result = await _sut.UpdateAnimalAsync(dto);

            result.Should().NotBeNull();
            result.AnimalType.Should().Be("CATTLE");
        }

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsException_WhenRepositoryThrows()
        {
            var dto = BuildDto();
            var existing = BuildEntity();
            _mockRepository.GetAnimalByIdAsync("CATTLE").Returns(existing);
            _mockMapper.Map(dto, existing);
            _mockRepository.UpdateAnimalAsync(Arg.Any<Animal>()).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAnimalAsync(dto));
        }

        #endregion

        #region DeleteAnimalAsync

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAnimalAsync(""));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAnimalAsync("   "));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsTrue_WhenDeleted()
        {
            _mockRepository.DeleteAnimalAsync("CATTLE").Returns(true);

            var result = await _sut.DeleteAnimalAsync("CATTLE");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepository.DeleteAnimalAsync("NOTEXIST").Returns(false);

            var result = await _sut.DeleteAnimalAsync("NOTEXIST");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsException_WhenRepositoryThrows()
        {
            _mockRepository.DeleteAnimalAsync("CATTLE").ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAnimalAsync("CATTLE"));
        }

        #endregion

        #region GetAnimalSnapshotAsync

        [Fact]
        public async Task GetAnimalSnapshotAsync_WithValidQuery_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "{\"AnimalType\":\"CAT\"}" };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"AnimalType\":\"CAT\"}" };

            var repositoryResult = new PagedData<AnimalSnapshotView>
            {
                Data = new List<AnimalSnapshotView>
                {
                    new() { AnimalType = "CAT", JobCode = "JOB001", NumberOfDays = 5, NumberOfAnimals = 10, Cost = 750m }
                },
                PaginationData = new PaginationData { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            var expectedResult = new PaginatedResult<AnimalSnapshotViewDto>
            {
                Data = new List<AnimalSnapshotViewDto>
                {
                    new() { AnimalType = "CAT", JobCode = "JOB001", NumberOfDays = 5, NumberOfAnimals = 10, Cost = 750m }
                },
                PaginationData = new PaginationDto { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalSnapshotAsync(mappedParams).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<AnimalSnapshotViewDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetAnimalSnapshotAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.PaginationData.TotalRecords.Should().Be(1);
            result.Data.First().AnimalType.Should().Be("CAT");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAnimalSnapshotAsync(mappedParams);
            _mockMapper.Received(1).Map<PaginatedResult<AnimalSnapshotViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_WithNullQuery_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAnimalSnapshotAsync(null!));
        }

        [Fact]
        public async Task GetAnimalSnapshotAsync_ThrowsException_WhenRepositoryThrows()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalSnapshotAsync(mappedParams).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetAnimalSnapshotAsync(query));
        }

        #endregion
    }
}