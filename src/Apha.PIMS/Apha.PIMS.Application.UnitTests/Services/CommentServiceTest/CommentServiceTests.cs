using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.CommentServiceTest
{
    public class CommentServiceTests
    {
        private readonly ICommentRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly CommentService _sut;

        public CommentServiceTests()
        {
            _mockRepository = Substitute.For<ICommentRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new CommentService(_mockRepository, _mockMapper);
        }

        #region GetCommentsByProjectAsync

        [Fact]
        public async Task GetCommentsByProjectAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var commentEntities = new List<Comment>
            {
                new Comment { CommentNo = 1, Project = project, Year = 2024, Topic = "Topic1", CommentText = "Text1", MadeBy = "User1" },
                new Comment { CommentNo = 2, Project = project, Year = 2024, Topic = "Topic2", CommentText = "Text2", MadeBy = "User2" }
            };

            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<Comment>(commentEntities, paginationData);

            var expectedDtos = new List<CommentDto>
            {
                new CommentDto { CommentNo = 1, Project = project, Year = 2024, Topic = "Topic1", CommentText = "Text1", MadeBy = "User1" },
                new CommentDto { CommentNo = 2, Project = project, Year = 2024, Topic = "Topic2", CommentText = "Text2", MadeBy = "User2" }
            };

            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<CommentDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<CommentDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetCommentsByProjectAsync(project, year, query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().CommentNo.Should().Be(1);
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
            _mockMapper.Received(1).Map<PaginatedResult<CommentDto>>(pagedData);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithNullYear_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var project = "PP001";
            int? year = null;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var emptyPagedData = new PagedData<Comment>(
                new List<Comment>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            var emptyResult = new PaginatedResult<CommentDto>(
                Enumerable.Empty<CommentDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 }
            );

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<CommentDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetCommentsByProjectAsync(project, year, query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var expectedException = new Exception("Database connection failed");

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams)
                .Returns(Task.FromException<PagedData<Comment>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetCommentsByProjectAsync(project, year, query)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams);
            _mockMapper.DidNotReceive().Map<PaginatedResult<CommentDto>>(Arg.Any<PagedData<Comment>>());
        }

        
        [Fact]
        public async Task GetCommentsByProjectAsync_WithTopicFilter_ForwardsTopicToRepository()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var topic = "Risk";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var commentEntities = new List<Comment>
            {
                new Comment { CommentNo = 5, Project = project, Year = 2024, Topic = "Risk", CommentText = "Risk comment", MadeBy = "User1" }
            };

            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData = new PagedData<Comment>(commentEntities, paginationData);

            var expectedDtos = new List<CommentDto>
            {
                new CommentDto { CommentNo = 5, Project = project, Year = 2024, Topic = "Risk", CommentText = "Risk comment", MadeBy = "User1" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var expectedResult = new PaginatedResult<CommentDto>(expectedDtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetCommentsByProjectAsync(project, year, paginationParams, topic).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<CommentDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetCommentsByProjectAsync(project, year, query, topic);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().Topic.Should().Be("Risk");

            await _mockRepository.Received(1).GetCommentsByProjectAsync(project, year, paginationParams, topic);
            _mockMapper.Received(1).Map<PaginatedResult<CommentDto>>(pagedData);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithValidCommentNo_ReturnsMappedDto()
        {
            // Arrange
            var CommentNo = 1;

            var entity = new Comment
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 15)
            };

            var expectedDto = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 15)
            };

            _mockRepository.GetByIdAsync(CommentNo)
                .Returns(Task.FromResult<Comment?>(entity));

            _mockMapper.Map<CommentDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync(CommentNo);

            // Assert
            result.Should().NotBeNull();
            result!.CommentNo.Should().Be(1);
            result.Project.Should().Be("PP001");
            result.CommentText.Should().Be("Some comment text");

            await _mockRepository.Received(1).GetByIdAsync(CommentNo);
            _mockMapper.Received(1).Map<CommentDto>(entity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCommentNotFound_ReturnsNull()
        {
            // Arrange
            var CommentNo = 999;

            _mockRepository.GetByIdAsync(CommentNo)
                .Returns(Task.FromResult<Comment?>(null));

            // Act
            var result = await _sut.GetByIdAsync(CommentNo);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetByIdAsync(CommentNo);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var CommentNo = 1;
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetByIdAsync(CommentNo)
                .Returns(Task.FromException<Comment?>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetByIdAsync(CommentNo)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetByIdAsync(CommentNo);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region AddAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddAsync_WithInvalidProject_ThrowsBusinessValidationErrorException(string? project)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Year = 2024, Topic = "Topic1", CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task AddAsync_WithInvalidYear_ThrowsBusinessValidationErrorException(int? year)
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = year, Topic = "Topic1", CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("YEAR_REQUIRED");
            exception.Errors.First().Message.Should().Be("Year is required.");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddAsync_WithInvalidTopic_ThrowsBusinessValidationErrorException(string? topic)
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = topic, CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("TOPIC_REQUIRED");
            exception.Errors.First().Message.Should().Be("Topic is required.");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task AddAsync_WithAllRequiredFieldsInvalid_ThrowsWithMultipleErrors()
        {
            // Arrange
            var dto = new CommentDto { Project = null, Year = null, Topic = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().HaveCount(3);
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "YEAR_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "TOPIC_REQUIRED");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task AddAsync_WhenDuplicateExists_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1"
            };

            _mockRepository.ExistsAsync(dto.Project!, (short)dto.Year!.Value, dto.Topic!)
                .Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("COMMENT_DUPLICATE");

            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task AddAsync_WithValidDto_SetsDateEnteredAndReturnsMappedCreatedDto()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1"
            };

            var entity = new Comment
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1"
            };

            var createdEntity = new Comment
            {
                CommentNo = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 6, 1)
            };

            var expectedDto = new CommentDto
            {
                CommentNo = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text",
                MadeBy = "User1",
                DateEntered = createdEntity.DateEntered
            };

            _mockRepository.ExistsAsync(dto.Project!, (short)dto.Year!.Value, dto.Topic!).Returns(false);
            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.AddAsync(entity).Returns(Task.FromResult(createdEntity));
            _mockMapper.Map<CommentDto>(createdEntity).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.CommentNo.Should().Be(42);
            result.Project.Should().Be("PP001");
            result.CommentText.Should().Be("Some comment text");

            entity.DateEntered.Should().NotBeNull();
            entity.DateEntered!.Value.Kind.Should().Be(DateTimeKind.Unspecified);

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).AddAsync(entity);
            _mockMapper.Received(1).Map<CommentDto>(createdEntity);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text"
            };

            var entity = new Comment
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text"
            };

            var expectedException = new Exception("Database connection failed");

            _mockRepository.ExistsAsync(dto.Project!, (short)dto.Year!.Value, dto.Topic!).Returns(false);
            _mockMapper.Map<Comment>(dto).Returns(entity);
            _mockRepository.AddAsync(entity)
                .Returns(Task.FromException<Comment>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AddAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<Comment>(dto);
            await _mockRepository.Received(1).AddAsync(entity);
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region UpdateAsync

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_WithInvalidProject_ThrowsBusinessValidationErrorException(string? project)
        {
            // Arrange
            var dto = new CommentDto { Project = project, Year = 2024, Topic = "Topic1", CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("PROJECT_REQUIRED");
            exception.Errors.First().Message.Should().Be("Project is required.");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task UpdateAsync_WithInvalidYear_ThrowsBusinessValidationErrorException(int? year)
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = year, Topic = "Topic1", CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("YEAR_REQUIRED");
            exception.Errors.First().Message.Should().Be("Year is required.");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_WithInvalidTopic_ThrowsBusinessValidationErrorException(string? topic)
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = topic, CommentText = "Some comment text" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().ContainSingle();
            exception.Errors.First().Code.Should().Be("TOPIC_REQUIRED");
            exception.Errors.First().Message.Should().Be("Topic is required.");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task UpdateAsync_WithAllRequiredFieldsInvalid_ThrowsWithMultipleErrors()
        {
            // Arrange
            var dto = new CommentDto { Project = null, Year = null, Topic = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Errors.Should().HaveCount(3);
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "YEAR_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "TOPIC_REQUIRED");

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task UpdateAsync_WhenCommentNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new CommentDto
            {
                CommentNo = 999,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text"
            };

            _mockRepository.GetByIdAsync(dto.CommentNo).Returns(Task.FromResult<Comment?>(null));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _sut.UpdateAsync(dto)
            );

            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>());
        }

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto = new CommentDto
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                CommentText = "Updated comment text",
                MadeBy = "User1"
            };

            var existingEntity = new Comment
            {
                CommentNo = 1,
                Project = "OldProject",
                Year = 2023,
                Topic = "Old Topic",
                CommentText = "Old text",
                MadeBy = "OldUser",
                DateEntered = new DateTime(2024, 1, 15)
            };

            var updatedEntity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                CommentText = "Updated comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 15)
            };

            var expectedDto = new CommentDto
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Updated Topic",
                CommentText = "Updated comment text",
                MadeBy = "User1",
                DateEntered = new DateTime(2024, 1, 15)
            };

            _mockRepository.GetByIdAsync(dto.CommentNo).Returns(Task.FromResult<Comment?>(existingEntity));
            _mockRepository.UpdateAsync(existingEntity).Returns(Task.FromResult(updatedEntity));
            _mockMapper.Map<CommentDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.CommentNo.Should().Be(1);
            result.Project.Should().Be("PP001");
            result.CommentText.Should().Be("Updated comment text");

            await _mockRepository.Received(1).GetByIdAsync(dto.CommentNo);
            await _mockRepository.Received(1).UpdateAsync(existingEntity);
            _mockMapper.Received(1).Map<CommentDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new CommentDto
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text"
            };

            var existingEntity = new Comment
            {
                CommentNo = 1,
                Project = "PP001",
                Year = 2024,
                Topic = "Topic1",
                CommentText = "Some comment text"
            };

            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetByIdAsync(dto.CommentNo).Returns(Task.FromResult<Comment?>(existingEntity));
            _mockRepository.UpdateAsync(Arg.Any<Comment>())
                .Returns(Task.FromException<Comment>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateAsync(dto)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetByIdAsync(dto.CommentNo);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<Comment>());
            _mockMapper.DidNotReceive().Map<CommentDto>(Arg.Any<Comment>());
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenCommentExists_ReturnsTrue()
        {
            // Arrange
            var CommentNo = 1;

            _mockRepository.DeleteAsync(CommentNo).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.DeleteAsync(CommentNo);

            // Assert
            result.Should().BeTrue();

            await _mockRepository.Received(1).DeleteAsync(CommentNo);
        }

        [Fact]
        public async Task DeleteAsync_WhenCommentNotFound_ReturnsFalse()
        {
            // Arrange
            var CommentNo = 999;

            _mockRepository.DeleteAsync(CommentNo).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync(CommentNo);

            // Assert
            result.Should().BeFalse();

            await _mockRepository.Received(1).DeleteAsync(CommentNo);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var CommentNo = 1;
            var expectedException = new Exception("Database connection failed");

            _mockRepository.DeleteAsync(CommentNo)
                .Returns(Task.FromException<bool>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.DeleteAsync(CommentNo)
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).DeleteAsync(CommentNo);
        }

        #endregion

        #region GetCommentTopicsAsync

       
        [Fact]
        public async Task GetCommentTopicsAsync_WhenTopicsExist_ReturnsMappedDtos()
        {
            // Arrange
            var topicEntities = new List<CommentTopic>
            {
                new CommentTopic { Topic = "Risk" },
                new CommentTopic { Topic = "Progress" },
                new CommentTopic { Topic = "Financial" }
            };

            var expectedDtos = new List<CommentTopicDto>
            {
                new CommentTopicDto { Topic = "Risk" },
                new CommentTopicDto { Topic = "Progress" },
                new CommentTopicDto { Topic = "Financial" }
            };

            _mockRepository.GetCommentTopicsAsync().Returns(Task.FromResult<IEnumerable<CommentTopic>>(topicEntities));
            _mockMapper.Map<IEnumerable<CommentTopicDto>>(topicEntities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetCommentTopicsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(t => t.Topic == "Risk");
            result.Should().Contain(t => t.Topic == "Progress");

            await _mockRepository.Received(1).GetCommentTopicsAsync();
            _mockMapper.Received(1).Map<IEnumerable<CommentTopicDto>>(topicEntities);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_WhenNoTopicsExist_ReturnsEmptyCollection()
        {
            // Arrange
            var emptyTopics = new List<CommentTopic>();
            var emptyDtos = new List<CommentTopicDto>();

            _mockRepository.GetCommentTopicsAsync().Returns(Task.FromResult<IEnumerable<CommentTopic>>(emptyTopics));
            _mockMapper.Map<IEnumerable<CommentTopicDto>>(emptyTopics).Returns(emptyDtos);

            // Act
            var result = await _sut.GetCommentTopicsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetCommentTopicsAsync();
            _mockMapper.Received(1).Map<IEnumerable<CommentTopicDto>>(emptyTopics);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetCommentTopicsAsync()
                .Returns(Task.FromException<IEnumerable<CommentTopic>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetCommentTopicsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetCommentTopicsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<CommentTopicDto>>(Arg.Any<IEnumerable<CommentTopic>>());
        }

        #endregion

        #region GetForecastSpendByProjectAsync

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WhenRepositoryReturnsValue_ReturnsForecastSpend()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 12345.67;
            _mockRepository.GetForecastSpendByProjectAsync(project).Returns(Task.FromResult(forecastSpend));

            // Act
            var result = await _sut.GetForecastSpendByProjectAsync(project);

            // Assert
            result.Should().Be(forecastSpend);
            await _mockRepository.Received(1).GetForecastSpendByProjectAsync(project);
        }

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.GetForecastSpendByProjectAsync(project)
                .Returns(Task.FromException<double?>(expectedException));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetForecastSpendByProjectAsync(project)
            );

            // Assert
            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetForecastSpendByProjectAsync(project);
        }

        #endregion

        #region UpdateForecastSpendByProjectAsync

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WithValidInput_ReturnsUpdatedForecastSpend()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 9876.54;
            _mockRepository.UpdateForecastSpendByProjectAsync(project, forecastSpend).Returns(Task.FromResult(forecastSpend));

            // Act
            var result = await _sut.UpdateForecastSpendByProjectAsync(project, forecastSpend);

            // Assert
            result.Should().Be(forecastSpend);
            await _mockRepository.Received(1).UpdateForecastSpendByProjectAsync(project, forecastSpend);
        }

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 9876.54;
            var expectedException = new Exception("Database connection failed");
            _mockRepository.UpdateForecastSpendByProjectAsync(project, forecastSpend)
                .Returns(Task.FromException<double?>(expectedException));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.UpdateForecastSpendByProjectAsync(project, forecastSpend)
            );

            // Assert
            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).UpdateForecastSpendByProjectAsync(project, forecastSpend);
        }

        #endregion
    }
}
