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

namespace Apha.PIMS.Application.UnitTests.Services.MilestoneServiceTest
{
    public class MilestoneServiceTests
    {
        private readonly IMilestoneRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MilestoneService _sut;

        public MilestoneServiceTests()
        {
            _mockRepository = Substitute.For<IMilestoneRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new MilestoneService(_mockRepository, _mockMapper);
        }

        /// <summary>Returns a <see cref="MilestoneDto"/> that passes all business-rule validation.</summary>
        private static MilestoneDto ValidMilestoneDto() => new()
        {
            Project = "PP001",
            Number  = "M1",
            IdType  = "D",
            DateDue = DateTime.Today.AddDays(30)
        };

        /// <summary>Returns a <see cref="MilestoneFormDatesDto"/> that passes all business-rule validation.</summary>
        private static MilestoneFormDatesDto ValidFormDatesDto() => new()
        {
            ParentProject = "PP001",
            Year          = 2024
        };

        #region GetAllMilestonesAsync

        [Fact]
        public async Task GetAllMilestonesAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var milestones     = new List<Milestone> { new() { Project = project, Number = "M1" }, new() { Project = project, Number = "M2" } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<Milestone>(milestones, paginationData);

            var dtos         = new List<MilestoneDto> { new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(10) }, new() { Project = project, Number = "M2", DateDue = DateTime.Today.AddDays(20) } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Number.Should().Be("M1");
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllMilestonesAsync(paginationParams, project);
            _mockMapper.Received(1).Map<List<MilestoneDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WithEmptyData_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<MilestoneDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_SetsIsLateTrue_WhenDateDueIsInPastAndDateCompletedIsNull()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = null, IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_IsLateIsFalse_WhenDateDueIsInFuture()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(10), DateCompleted = null, IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_IsLateIsFalse_WhenDateCompletedIsSet()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            var pagedData     = new PagedData<Milestone>(new List<Milestone>(), new PaginationData());
            var paginationDto = new PaginationDto();
            var dtos          = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = DateTime.Today.AddDays(-2), IsLate = false }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project).Returns(pagedData);
            _mockMapper.Map<List<MilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestonesAsync(query, project);

            // Assert
            result.Data.First().IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestonesAsync(paginationParams, project)
                .Returns(Task.FromException<PagedData<Milestone>>(new Exception("DB error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllMilestonesAsync(query, project));

            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetMilestoneAsync

        [Fact]
        public async Task GetMilestoneAsync_ReturnsMappedDto_WhenMilestoneExists()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";

            var entity = new Milestone { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };
            var dto    = new MilestoneDto { Project = project, Number = number, DateDue = DateTime.Today.AddDays(10) };

            _mockRepository.GetMilestoneAsync(project, number).Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync(project, number);

            // Assert
            result.Should().NotBeNull();
            result!.Project.Should().Be(project);
            result.Number.Should().Be(number);

            await _mockRepository.Received(1).GetMilestoneAsync(project, number);
            _mockMapper.Received(1).Map<MilestoneDto>(entity);
        }

        [Fact]
        public async Task GetMilestoneAsync_ReturnsNull_WhenMilestoneNotFound()
        {
            // Arrange
            _mockRepository.GetMilestoneAsync("PP001", "UNKNOWN").Returns((Milestone?)null);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "UNKNOWN");

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<MilestoneDto>(Arg.Any<Milestone>());
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateTrue_WhenDateDueIsInPastAndDateCompletedIsNull()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = null };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeTrue();
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateFalse_WhenDateCompletedIsSet()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(-1), DateCompleted = DateTime.Today.AddDays(-2) };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeFalse();
        }

        [Fact]
        public async Task GetMilestoneAsync_SetsIsLateFalse_WhenDateDueIsInFuture()
        {
            // Arrange
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(10) };
            var dto    = new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(10) };

            _mockRepository.GetMilestoneAsync("PP001", "M1").Returns(entity);
            _mockMapper.Map<MilestoneDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneAsync("PP001", "M1");

            // Assert
            result!.IsLate.Should().BeFalse();
        }

        #endregion

        #region SaveMilestoneAsync — validation

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenProjectIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Project = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenNumberIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Number = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenIdTypeIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.IdType = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "TYPE_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenDateDueIsDefault()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue = default;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_DUE_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenDateCompletedIsInFuture()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(1);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsBusinessValidationError_WhenOnTargetSetAndDateDueHasPassed()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue  = DateTime.Today.AddDays(-1);
            dto.OnTarget = 1;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "ON_TARGET_PAST_DUE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_CollectsAllValidationErrors_WhenAllFieldsInvalid()
        {
            // Arrange — every validated field is invalid at once
            var dto = new MilestoneDto
            {
                Project       = string.Empty,
                Number        = string.Empty,
                IdType        = null,
                DateDue       = default,
                DateCompleted = DateTime.Today.AddDays(1)
            };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "NUMBER_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "TYPE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DATE_DUE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task SaveMilestoneAsync_DoesNotCallRepository_WhenValidationFails()
        {
            // Arrange
            var dto = new MilestoneDto { Project = string.Empty, Number = "M1", IdType = "D", DateDue = DateTime.Today.AddDays(10) };

            // Act
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            await _mockRepository.DidNotReceive().GetMilestoneAsync(Arg.Any<string>(), Arg.Any<string>());
            await _mockRepository.DidNotReceive().AddMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task SaveMilestoneAsync_ThrowsNumberExists_WhenMilestoneAlreadyExists()
        {
            // Arrange
            var dto      = ValidMilestoneDto();
            var existing = new Milestone { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns(existing);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_EXISTS");
            await _mockRepository.DidNotReceive().AddMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>());
        }

        #endregion

        #region SaveMilestoneAsync — mutual exclusions (ApplyMutualExclusions)

        [Fact]
        public async Task SaveMilestoneAsync_WhenDateCompletedIsSet_ClearsOnTargetAndUnderSdReview()
        {
            // Arrange — ApplyMutualExclusions mutates dto in place, so we inspect dto after the call.
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(-1);
            dto.OnTarget      = 1;
            dto.UnderSdReview = 1;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.OnTarget.Should().Be(0);
            dto.UnderSdReview.Should().Be(0);
            dto.DateCompleted.Should().NotBeNull();
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenOnTargetIsSet_ClearsUnderSdReviewAndDateCompleted()
        {
            // Arrange — no DateCompleted so the first exclusion block does not fire first.
            var dto = ValidMilestoneDto();
            dto.OnTarget      = 1;
            dto.UnderSdReview = 1;
            dto.DateCompleted = null;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.UnderSdReview.Should().Be(0);
            dto.DateCompleted.Should().BeNull();
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenUnderSdReviewIsSet_ClearsOnTargetAndDateCompleted()
        {
            // Arrange — OnTarget is 0 so the second exclusion block does not fire.
            var dto = ValidMilestoneDto();
            dto.UnderSdReview = 1;
            dto.OnTarget      = 0;
            dto.DateCompleted = null;

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(new Milestone());
            _mockRepository.AddMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>()).Returns(new Milestone());
            _mockMapper.Map<MilestoneDto>(Arg.Any<Milestone>()).Returns(new MilestoneDto { Project = "PP001", Number = "M1", DateDue = DateTime.Today.AddDays(30) });

            // Act
            await _sut.SaveMilestoneAsync(dto);

            // Assert
            dto.OnTarget.Should().Be(0);
            dto.DateCompleted.Should().BeNull();
        }

        #endregion

        #region SaveMilestoneAsync — happy path

        [Fact]
        public async Task SaveMilestoneAsync_CallsAddAndReturnsMappedDto_WhenValid()
        {
            // Arrange
            var dto       = ValidMilestoneDto();
            var entity    = new Milestone { Project = dto.Project, Number = dto.Number };
            var created   = new Milestone { Project = dto.Project, Number = dto.Number };
            var resultDto = new MilestoneDto { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);
            _mockMapper.Map<Milestone>(Arg.Any<object>()).Returns(entity);
            _mockRepository.AddMilestoneAsync(entity, Arg.Any<string?>()).Returns(created);
            _mockMapper.Map<MilestoneDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Project.Should().Be("PP001");
            result.Number.Should().Be("M1");
            await _mockRepository.Received(1).AddMilestoneAsync(entity, Arg.Any<string?>());
        }

        #endregion

        #region UpdateMilestoneAsync — validation

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenProjectIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Project = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenNumberIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.Number = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NUMBER_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenIdTypeIsEmpty()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.IdType = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "TYPE_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenDateDueIsDefault()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue = default;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_DUE_REQUIRED");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenDateCompletedIsInFuture()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateCompleted = DateTime.Today.AddDays(1);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "DATE_COMPLETED_FUTURE");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsBusinessValidationError_WhenOnTargetSetAndDateDueHasPassed()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            dto.DateDue  = DateTime.Today.AddDays(-1);
            dto.OnTarget = 1;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "ON_TARGET_PAST_DUE");
        }

        [Fact]
        public async Task UpdateMilestoneAsync_DoesNotCallRepository_WhenValidationFails()
        {
            // Arrange
            var dto = new MilestoneDto { Project = string.Empty, Number = "M1", IdType = "D", DateDue = DateTime.Today.AddDays(10) };

            // Act
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            await _mockRepository.DidNotReceive().GetMilestoneAsync(Arg.Any<string>(), Arg.Any<string>());
            await _mockRepository.DidNotReceive().UpdateMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task UpdateMilestoneAsync_ThrowsNotFound_WhenMilestoneDoesNotExist()
        {
            // Arrange
            var dto = ValidMilestoneDto();
            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns((Milestone?)null);

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.UpdateMilestoneAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "NOT_FOUND");
            await _mockRepository.DidNotReceive().UpdateMilestoneAsync(Arg.Any<Milestone>(), Arg.Any<string?>());
        }

        #endregion

        #region UpdateMilestoneAsync — happy path

        [Fact]
        public async Task UpdateMilestoneAsync_CallsUpdateAndReturnsMappedDto_WhenValid()
        {
            // Arrange
            var dto       = ValidMilestoneDto();
            var existing  = new Milestone { Project = dto.Project, Number = dto.Number };
            var updated   = new Milestone { Project = dto.Project, Number = dto.Number };
            var resultDto = new MilestoneDto { Project = dto.Project, Number = dto.Number };

            _mockRepository.GetMilestoneAsync(dto.Project, dto.Number).Returns(existing);
            _mockRepository.UpdateMilestoneAsync(existing, Arg.Any<string?>()).Returns(updated);
            _mockMapper.Map<MilestoneDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateMilestoneAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Project.Should().Be("PP001");
            result.Number.Should().Be("M1");
            await _mockRepository.Received(1).UpdateMilestoneAsync(existing, Arg.Any<string?>());
        }

        #endregion

        #region DeleteMilestoneAsync

        [Fact]
        public async Task DeleteMilestoneAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            _mockRepository.DeleteMilestoneAsync("PP001", "M1").Returns(true);

            // Act
            var result = await _sut.DeleteMilestoneAsync("PP001", "M1");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteMilestoneAsync("PP001", "M1");
        }

        [Fact]
        public async Task DeleteMilestoneAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            _mockRepository.DeleteMilestoneAsync("PP001", "UNKNOWN").Returns(false);

            // Act
            var result = await _sut.DeleteMilestoneAsync("PP001", "UNKNOWN");

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteMilestoneAsync("PP001", "UNKNOWN");
        }

        #endregion

        #region UpdateFormRequiredAsync

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequiredAsync_DelegatesToRepository_AndReturnsResult(bool formRequired)
        {
            // Arrange
            _mockRepository.UpdateFormRequiredAsync("PP001", formRequired).Returns(true);

            // Act
            var result = await _sut.UpdateFormRequiredAsync("PP001", formRequired);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).UpdateFormRequiredAsync("PP001", formRequired);
        }

        #endregion

        #region GetMilestoneTypesAsync

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsMappedDtoList_WhenNoFilterProvided()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };
            var expectedDtos = new List<MilestoneTypeDto>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };

            _mockRepository.GetMilestoneTypesAsync(null).Returns(types);
            _mockMapper.Map<List<MilestoneTypeDto>>(types).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().IdType.Should().Be('A');

            await _mockRepository.Received(1).GetMilestoneTypesAsync(null);
            _mockMapper.Received(1).Map<List<MilestoneTypeDto>>(types);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_PassesFilterToRepository()
        {
            // Arrange
            const string filter = "M";
            var types = new List<MilestoneType>
            {
                new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' }
            };
            var expectedDtos = new List<MilestoneTypeDto>
            {
                new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' }
            };

            _mockRepository.GetMilestoneTypesAsync(filter).Returns(types);
            _mockMapper.Map<List<MilestoneTypeDto>>(types).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync(filter);

            // Assert
            result.Should().ContainSingle(t => t.MilestoneDeliverable == 'M');
            await _mockRepository.Received(1).GetMilestoneTypesAsync(filter);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsEmpty_WhenNoTypes()
        {
            // Arrange
            var emptyTypes = new List<MilestoneType>();
            var emptyDtos  = new List<MilestoneTypeDto>();

            _mockRepository.GetMilestoneTypesAsync(null).Returns(emptyTypes);
            _mockMapper.Map<List<MilestoneTypeDto>>(emptyTypes).Returns(emptyDtos);

            // Act
            var result = await _sut.GetMilestoneTypesAsync();

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetAllMilestoneFormDatesAsync

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string parent  = "PP001";

            var formDatesList  = new List<MilestoneFormDates> { new() { Year = 2024, ParentProject = parent }, new() { Year = 2023, ParentProject = parent } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<MilestoneFormDates>(formDatesList, paginationData);

            var dtos          = new List<MilestoneFormDatesDto> { new() { Year = 2024, ParentProject = parent }, new() { Year = 2023, ParentProject = parent } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, parent).Returns(pagedData);
            _mockMapper.Map<List<MilestoneFormDatesDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(query, parent);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Year.Should().Be(2024);
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllMilestoneFormDatesAsync(paginationParams, parent);
            _mockMapper.Received(1).Map<List<MilestoneFormDatesDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithEmptyData_ReturnsEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string parent  = "PP001";

            var pagedData     = new PagedData<MilestoneFormDates>(new List<MilestoneFormDates>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<MilestoneFormDatesDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, parent).Returns(pagedData);
            _mockMapper.Map<List<MilestoneFormDatesDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllMilestoneFormDatesAsync(query, parent);

            // Assert
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllMilestoneFormDatesAsync(paginationParams, "PP001")
                .Returns(Task.FromException<PagedData<MilestoneFormDates>>(new Exception("DB error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllMilestoneFormDatesAsync(query, "PP001"));

            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetMilestoneFormDatesAsync

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsMappedDto_WhenExists()
        {
            // Arrange
            const short year    = 2024;
            const string parent = "PP001";

            var entity = new MilestoneFormDates { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var dto    = new MilestoneFormDatesDto { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };

            _mockRepository.GetMilestoneFormDatesAsync(year, parent).Returns(entity);
            _mockMapper.Map<MilestoneFormDatesDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(year, parent);

            // Assert
            result.Should().NotBeNull();
            result!.Year.Should().Be(year);
            result.ParentProject.Should().Be(parent);
            result.Jan.Should().Be(new DateTime(2024, 1, 31));

            await _mockRepository.Received(1).GetMilestoneFormDatesAsync(year, parent);
            _mockMapper.Received(1).Map<MilestoneFormDatesDto>(entity);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _mockRepository.GetMilestoneFormDatesAsync(2024, "PP001").Returns((MilestoneFormDates?)null);

            // Act
            var result = await _sut.GetMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<MilestoneFormDatesDto>(Arg.Any<MilestoneFormDates>());
        }

        #endregion

        #region SaveMilestoneFormDatesAsync — validation

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_ThrowsBusinessValidationError_WhenParentProjectIsEmpty()
        {
            // Arrange
            var dto = ValidFormDatesDto();
            dto.ParentProject = string.Empty;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_ThrowsBusinessValidationError_WhenYearIsZero()
        {
            // Arrange
            var dto = ValidFormDatesDto();
            dto.Year = 0;

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().ContainSingle(e => e.Code == "YEAR_REQUIRED");
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CollectsAllValidationErrors()
        {
            // Arrange
            var dto = new MilestoneFormDatesDto { ParentProject = string.Empty, Year = 0 };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                async () => await _sut.SaveMilestoneFormDatesAsync(dto));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "YEAR_REQUIRED");
        }

        #endregion

        #region SaveMilestoneFormDatesAsync — add vs update

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CallsAdd_WhenNoExistingRecord()
        {
            // Arrange
            var dto       = ValidFormDatesDto();
            var entity    = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var created   = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var resultDto = new MilestoneFormDatesDto { Year = dto.Year, ParentProject = dto.ParentProject };

            _mockRepository.GetMilestoneFormDatesAsync(dto.Year, dto.ParentProject).Returns((MilestoneFormDates?)null);
            _mockMapper.Map<MilestoneFormDates>(dto).Returns(entity);
            _mockRepository.AddMilestoneFormDatesAsync(entity).Returns(created);
            _mockMapper.Map<MilestoneFormDatesDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Year.Should().Be(dto.Year);
            result.ParentProject.Should().Be(dto.ParentProject);

            await _mockRepository.Received(1).AddMilestoneFormDatesAsync(entity);
            await _mockRepository.DidNotReceive().UpdateMilestoneFormDatesAsync(Arg.Any<MilestoneFormDates>());
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CallsUpdate_WhenExistingRecordFound()
        {
            // Arrange
            var dto       = ValidFormDatesDto();
            var existing  = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var updated   = new MilestoneFormDates { Year = dto.Year, ParentProject = dto.ParentProject };
            var resultDto = new MilestoneFormDatesDto { Year = dto.Year, ParentProject = dto.ParentProject };

            _mockRepository.GetMilestoneFormDatesAsync(dto.Year, dto.ParentProject).Returns(existing);
            _mockRepository.UpdateMilestoneFormDatesAsync(existing).Returns(updated);
            _mockMapper.Map<MilestoneFormDatesDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.SaveMilestoneFormDatesAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).UpdateMilestoneFormDatesAsync(existing);
            await _mockRepository.DidNotReceive().AddMilestoneFormDatesAsync(Arg.Any<MilestoneFormDates>());
        }

        #endregion

        #region DeleteMilestoneFormDatesAsync

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            // Arrange
            _mockRepository.DeleteMilestoneFormDatesAsync(2024, "PP001").Returns(true);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteMilestoneFormDatesAsync(2024, "PP001");
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            _mockRepository.DeleteMilestoneFormDatesAsync(9999, "PP001").Returns(false);

            // Act
            var result = await _sut.DeleteMilestoneFormDatesAsync(9999, "PP001");

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteMilestoneFormDatesAsync(9999, "PP001");
        }

        #endregion

        #region GetLogMilestonesAsync

        [Fact]
        public async Task GetLogMilestonesAsync_WithAllParams_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            const string project = "PP001";
            const string part1   = "M";
            const string part2   = "1";

            var entities = new List<LogMilestone>
            {
                new() { Project = project, Number = "M1", Description = "Log Entry 1" },
                new() { Project = project, Number = "M2", Description = "Log Entry 2" }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData      = new PagedData<LogMilestone>(entities, paginationData);

            var dtos          = new List<LogMilestoneDto>
            {
                new() { Project = project, Number = "M1", Description = "Log Entry 1" },
                new() { Project = project, Number = "M2", Description = "Log Entry 2" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, project, part1, part2).Returns(pagedData);
            _mockMapper.Map<List<LogMilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetLogMilestonesAsync(query, project, part1, part2);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Number.Should().Be("M1");
            result.PaginationData.TotalRecords.Should().Be(2);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetLogMilestonesAsync(paginationParams, project, part1, part2);
            _mockMapper.Received(1).Map<List<LogMilestoneDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_DelegatesToRepository_WithMappedParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var entities = new List<LogMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "M1", UpdateType = 'I' }
            };
            var pagedData = new PagedData<LogMilestone>(entities, new PaginationData { TotalRecords = 1 });
            var dtos = new List<LogMilestoneDto>
            {
                new() { Project = "PP001", Number = "M1", UpdateType = "I" }
            };
            var paginationDto = new PaginationDto { TotalRecords = 1 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, "PP001", "M", "1").Returns(pagedData);
            _mockMapper.Map<List<LogMilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetLogMilestonesAsync(query, "PP001", "M", "1");

            // Assert
            result.Data.Should().ContainSingle();
            result.PaginationData.TotalRecords.Should().Be(1);
            await _mockRepository.Received(1).GetLogMilestonesAsync(paginationParams, "PP001", "M", "1");
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WithNullOptionalParams_PassesNullsToRepository()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var pagedData     = new PagedData<LogMilestone>(new List<LogMilestone>(), new PaginationData());
            var emptyDtos     = new List<LogMilestoneDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, null, null, null).Returns(pagedData);
            _mockMapper.Map<List<LogMilestoneDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            await _sut.GetLogMilestonesAsync(query, null, null, null);

            // Assert
            await _mockRepository.Received(1).GetLogMilestonesAsync(
                paginationParams,
                Arg.Is<string?>(p  => p  == null),
                Arg.Is<string?>(n1 => n1 == null),
                Arg.Is<string?>(n2 => n2 == null));
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WithEmptyData_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            var pagedData     = new PagedData<LogMilestone>(new List<LogMilestone>(), new PaginationData { TotalRecords = 0 });
            var emptyDtos     = new List<LogMilestoneDto>();
            var paginationDto = new PaginationDto { TotalRecords = 0 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, null, null, null).Returns(pagedData);
            _mockMapper.Map<List<LogMilestoneDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetLogMilestonesAsync(query, null, null, null);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 3, PageSize = 25 };
            var paginationParams = new PaginationParameters<string>(page: 3, pageSize: 25);

            var pagedData     = new PagedData<LogMilestone>(new List<LogMilestone>(), new PaginationData());
            var emptyDtos     = new List<LogMilestoneDto>();
            var paginationDto = new PaginationDto();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, null, null, null).Returns(pagedData);
            _mockMapper.Map<List<LogMilestoneDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            await _sut.GetLogMilestonesAsync(query, null, null, null);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(
                Arg.Is<QueryParameters<string>>(q => q.Page == 3 && q.PageSize == 25));
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetLogMilestonesAsync(paginationParams, null, null, null)
                .Returns(Task.FromException<PagedData<LogMilestone>>(new Exception("DB error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetLogMilestonesAsync(query, null, null, null));

            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetAllStagingRowsAsync

        [Fact]
        public async Task GetAllStagingRowsAsync_WithValidData_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var entities = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "M1" },
                new() { Id = 2, Project = "PP001", Number = "M2" }
            };
            var pagedData = new PagedData<StagingMilestone>(entities, new PaginationData { TotalRecords = 2 });
            var dtos = new List<StagingMilestoneDto>
            {
                new() { Id = 1, Project = "PP001", Number = "M1" },
                new() { Id = 2, Project = "PP001", Number = "M2" }
            };
            var paginationDto = new PaginationDto { TotalRecords = 2 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllStagingRowsAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<List<StagingMilestoneDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllStagingRowsAsync(query);

            // Assert
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            await _mockRepository.Received(1).GetAllStagingRowsAsync(paginationParams);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetAllStagingRowsAsync(paginationParams)
                .Returns(Task.FromException<PagedData<StagingMilestone>>(new Exception("DB error")));

            // Act
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllStagingRowsAsync(query));

            // Assert
            ex.Message.Should().Be("DB error");
        }

        #endregion

        #region GetStagingRowsAsync

        [Fact]
        public async Task GetStagingRowsAsync_ReturnsMappedList_WhenProjectProvided()
        {
            // Arrange
            var entities = new List<StagingMilestone> { new() { Id = 1, Project = "PP001", Number = "M1" } };
            var dtos = new List<StagingMilestoneDto> { new() { Id = 1, Project = "PP001", Number = "M1" } };

            _mockRepository.GetStagingRowsAsync(1).Returns(entities);
            _mockMapper.Map<List<StagingMilestoneDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetStagingRowsAsync(1);

            // Assert
            result.Should().ContainSingle(r => r.Number == "M1");
            await _mockRepository.Received(1).GetStagingRowsAsync(1);
        }

        [Fact]
        public async Task GetStagingRowsAsync_ReturnsEmpty_WhenNoRowsFound()
        {
            // Arrange
            _mockRepository.GetStagingRowsAsync(99).Returns(new List<StagingMilestone>());
            _mockMapper.Map<List<StagingMilestoneDto>>(Arg.Any<List<StagingMilestone>>()).Returns(new List<StagingMilestoneDto>());

            // Act
            var result = await _sut.GetStagingRowsAsync(99);

            // Assert
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetStagingRowsAsync(99);
        }

        #endregion

        #region AddStagingRowAsync

        [Fact]
        public async Task AddStagingRowAsync_AssignsNextNumber_WhenNumberMissingAndProjectProvided()
        {
            // Arrange
            var dto = new StagingMilestoneDto
            {
                Project = "PP001",
                Number = null,
                Description = "Desc",
                DateDue = new DateTime(2025, 1, 31)
            };
            var entity = new StagingMilestone { Project = "PP001", Number = "M9", Description = "Desc", DateDue = dto.DateDue };
            var created = new StagingMilestone { Project = "PP001", Number = "M9", Description = "Desc", DateDue = dto.DateDue };
            var resultDto = new StagingMilestoneDto { Project = "PP001", Number = "M9", Description = "Desc", DateDue = dto.DateDue };

            // Mock program retrieval to return a program ending in "surv"
            _mockRepository.GetProgramByProjectAsync("PP001").Returns("TestSurv");
            _mockRepository.GetNextMilestoneNumberAsync("PP001", 2025).Returns("M9");
            _mockMapper.Map<StagingMilestone>(dto).Returns(entity);
            _mockRepository.AddStagingRowAsync(entity).Returns(created);
            _mockMapper.Map<StagingMilestoneDto>(created).Returns(resultDto);

            // Act
            var result = await _sut.AddStagingRowAsync(dto, 2025);

            // Assert
            result.Number.Should().Be("M9");
            dto.Number.Should().Be("M9");
            await _mockRepository.Received(1).GetProgramByProjectAsync("PP001");
            await _mockRepository.Received(1).GetNextMilestoneNumberAsync("PP001", 2025);
            await _mockRepository.Received(1).AddStagingRowAsync(entity);
        }

        [Fact]
        public async Task AddStagingRowAsync_ThrowsBusinessValidationError_WhenRequiredFieldsMissing()
        {
            // Arrange
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "", Description = "", DateDue = default };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddStagingRowAsync(dto, 2025));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "DATE_DUE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "NUMBER_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DESCRIPTION_REQUIRED");
            await _mockRepository.DidNotReceive().AddStagingRowAsync(Arg.Any<StagingMilestone>());
        }

        [Fact]
        public async Task AddStagingRowAsync_SetsEntityDateDueKindToUnspecified()
        {
            // Arrange
            var dto = new StagingMilestoneDto
            {
                Project = "PP001",
                Number = "M1",
                Description = "Desc",
                DateDue = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc)
            };
            var entity = new StagingMilestone { Project = "PP001", Number = "M1", Description = "Desc", DateDue = dto.DateDue };
            var created = new StagingMilestone { Project = "PP001", Number = "M1", Description = "Desc", DateDue = dto.DateDue };
            var resultDto = new StagingMilestoneDto { Project = "PP001", Number = "M1", Description = "Desc", DateDue = dto.DateDue };

            _mockMapper.Map<StagingMilestone>(dto).Returns(entity);
            _mockRepository.AddStagingRowAsync(entity).Returns(created);
            _mockMapper.Map<StagingMilestoneDto>(created).Returns(resultDto);

            // Act
            await _sut.AddStagingRowAsync(dto, 2025);

            // Assert
            entity.DateDue.Kind.Should().Be(DateTimeKind.Unspecified);
        }

        #endregion

        #region UpdateStagingRowAsync

        [Fact]
        public async Task UpdateStagingRowAsync_ThrowsBusinessValidationError_WhenRequiredFieldsMissing()
        {
            // Arrange
            var dto = new StagingMilestoneDto { Id = 0, Number = "", Description = "", DateDue = default };

            // Act
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateStagingRowAsync(dto));

            // Assert
            ex.Errors.Should().Contain(e => e.Code == "ID_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "NUMBER_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DATE_DUE_REQUIRED");
            ex.Errors.Should().Contain(e => e.Code == "DESCRIPTION_REQUIRED");
            await _mockRepository.DidNotReceive().UpdateStagingRowAsync(Arg.Any<StagingMilestone>());
        }

        [Fact]
        public async Task UpdateStagingRowAsync_SetsDateDueToUnspecifiedAndClearsNote()
        {
            // Arrange
            var dto = new StagingMilestoneDto
            {
                Id = 10,
                Number = "M1",
                Description = "Updated",
                DateDue = DateTime.SpecifyKind(new DateTime(2025, 2, 2), DateTimeKind.Utc),
                Note = "Old note"
            };
            var entity = new StagingMilestone { Id = 10, Number = "M1", Description = "Updated", DateDue = dto.DateDue, Note = "Old note" };
            var updated = new StagingMilestone { Id = 10, Number = "M1", Description = "Updated", DateDue = dto.DateDue, Note = null };
            var resultDto = new StagingMilestoneDto { Id = 10, Number = "M1", Description = "Updated" };

            _mockMapper.Map<StagingMilestone>(dto).Returns(entity);
            _mockRepository.UpdateStagingRowAsync(entity).Returns(updated);
            _mockMapper.Map<StagingMilestoneDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateStagingRowAsync(dto);

            // Assert
            result.Id.Should().Be(10);
            entity.DateDue.Kind.Should().Be(DateTimeKind.Unspecified);
            entity.Note.Should().BeNull();
            await _mockRepository.Received(1).UpdateStagingRowAsync(entity);
        }

        #endregion

        #region Staging and Import Delegation

        [Fact]
        public async Task DeleteStagingRowAsync_DelegatesToRepository()
        {
            _mockRepository.DeleteStagingRowAsync(7).Returns(true);

            var result = await _sut.DeleteStagingRowAsync(7);

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteStagingRowAsync(7);
        }

        [Fact]
        public async Task ClearStagingAsync_DelegatesToRepository()
        {
            _mockRepository.ClearStagingAsync("PP001").Returns(4);

            var result = await _sut.ClearStagingAsync("PP001");

            result.Should().Be(4);
            await _mockRepository.Received(1).ClearStagingAsync("PP001");
        }

        [Fact]
        public async Task ValidateStagingAsync_DelegatesToRepository()
        {
            _mockRepository.ValidateStagingAsync("PP001", "M", true).Returns(Task.CompletedTask);

            await _sut.ValidateStagingAsync("PP001", "M", true);

            await _mockRepository.Received(1).ValidateStagingAsync("PP001", "M", true);
        }

        [Fact]
        public async Task ImportStagingAsync_DelegatesToRepository()
        {
            _mockRepository.ImportStagingAsync("PP001", "USER1").Returns(3);

            var result = await _sut.ImportStagingAsync("PP001", "USER1");

            result.Should().Be(3);
            await _mockRepository.Received(1).ImportStagingAsync("PP001", "USER1");
        }

        [Fact]
        public async Task ImportWithOverwriteAsync_DelegatesToRepository()
        {
            _mockRepository.ImportWithOverwriteAsync("PP001", "USER1").Returns(2);

            var result = await _sut.ImportWithOverwriteAsync("PP001", "USER1");

            result.Should().Be(2);
            await _mockRepository.Received(1).ImportWithOverwriteAsync("PP001", "USER1");
        }

        [Fact]
        public async Task GetNextMilestoneNumberAsync_DelegatesToRepository()
        {
            _mockRepository.GetNextMilestoneNumberAsync("PP001", 2025).Returns("M42");

            var result = await _sut.GetNextMilestoneNumberAsync("PP001", 2025);

            result.Should().Be("M42");
            await _mockRepository.Received(1).GetNextMilestoneNumberAsync("PP001", 2025);
        }

        #endregion
    }
}

