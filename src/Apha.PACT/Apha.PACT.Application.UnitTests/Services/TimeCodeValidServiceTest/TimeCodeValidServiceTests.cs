using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.TimeCodeValidServiceTest
{
    public class TimeCodeValidServiceTests
    {
        private readonly ITimeCodeValidRepository _mockRepository;
        private readonly IJobCodeRepository _mockJobCodeRepository;
        private readonly ITestCapabilityRepository _mockTestCapabilityRepository;
        private readonly IProjectRepository _mockProjectRepository;
        private readonly IMonthlyTimeRepository _mockMonthlyTimeRepository;
        private readonly IMapper _mockMapper;
        private readonly TimeCodeValidService _sut;

        public TimeCodeValidServiceTests()
        {
            _mockRepository = Substitute.For<ITimeCodeValidRepository>();
            _mockJobCodeRepository = Substitute.For<IJobCodeRepository>();
            _mockTestCapabilityRepository = Substitute.For<ITestCapabilityRepository>();
            _mockProjectRepository = Substitute.For<IProjectRepository>();
            _mockMonthlyTimeRepository = Substitute.For<IMonthlyTimeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new TimeCodeValidService(
                _mockRepository,
                _mockJobCodeRepository,
                _mockTestCapabilityRepository,
                _mockProjectRepository,
                _mockMonthlyTimeRepository,
                _mockMapper);
        }

        #region GetPagedByProjectAndTestCodeAsync

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_ValidQuery_ReturnsMappedPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TimeCodeValid>(new List<TimeCodeValid>(), new PaginationData());
            var pagedResult = new PaginatedResult<TimeCodeValidDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedByProjectAndTestCodeAsync(mappedParams, "PRJ1", "TST1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedByProjectAndTestCodeAsync(query, "PRJ1", "TST1");

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedByProjectAndTestCodeAsync(mappedParams, "PRJ1", "TST1");
        }

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_RepositoryThrows_PropagatesException()
        {
            var query = new QueryParameters<string>();
            var mappedParams = new PaginationParameters<string>();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedByProjectAndTestCodeAsync(mappedParams, "PRJ1", "TST1")
                .ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.GetPagedByProjectAndTestCodeAsync(query, "PRJ1", "TST1"));
        }

        #endregion

        #region UpdateTimeCodeValidAsync — MonthlyTime dependency path

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WorkGroupChangedWithDependentMonthlyTime_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG_NEW", ParentProject = "PRJ1", JobCode = "JC1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG_OLD", ParentProject = "PRJ1", JobCode = "JC1" };

            _mockRepository.GetTimeCodeValidAsync("WG_NEW", "TC1", "PRJ1").Returns(existing);
            _mockMonthlyTimeRepository.HasMonthlyTimeEntriesAsync("WG_OLD", "TC1", "PRJ1").Returns(true);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Cannot update, existing data in MonthlyTime.");
            await _mockRepository.DidNotReceive().UpdateTimeCodeValidAsync(Arg.Any<TimeCodeValid>());
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_NoFieldChanged_SkipsMonthlyTimeDependencyCheck()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            await _mockMonthlyTimeRepository.DidNotReceive().HasMonthlyTimeEntriesAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_ValidInput_ReturnsMappedDtos()
        {
            var entities = new List<TimeCodeValid> { new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" } };

            _mockRepository.GetByJobCodeAsync("JC1", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.GetByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetByJobCodeAsync("JC1", "PRJ1");
        }

        [Fact]
        public async Task GetByJobCodeAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository.GetByJobCodeAsync("JC1", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.GetByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeEmpty();
        }

        #endregion

        #region GetTimeCodeValidsByWorkGroupAsync

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_ValidInput_ReturnsMappedDtos()
        {
            var entities = new List<TimeCodeValid>
            {
                new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" }
            };
            var dtos = new List<TimeCodeValidDto>
            {
                new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" }
            };

            _mockRepository.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.GetTimeCodeValidsByWorkGroupAsync("WG1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetTimeCodeValidsByWorkGroupAsync("WG1");
        }

        #endregion

        #region GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync

        [Fact]
        public async Task GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync_ValidInput_ReturnsRepositoryResult()
        {
            var projects = new List<string> { "PRJ1", "PRJ2" };
            _mockRepository.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(projects);

            var result = await _sut.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1");

            result.Should().BeEquivalentTo(projects);
            await _mockRepository.Received(1).GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1");
        }

        #endregion

        #region GetAllDistinctTimeCodesAsync

        [Fact]
        public async Task GetAllDistinctTimeCodesAsync_WithDuplicates_ReturnsDistinctOrderedValues()
        {
            var entities = new List<TimeCodeValid>
            {
                new TimeCodeValid { TimeCode = "TC2" },
                new TimeCodeValid { TimeCode = "TC1" },
                new TimeCodeValid { TimeCode = "TC2" }
            };
            _mockRepository.GetTimeCodeValidsAsync().Returns(entities);

            var result = (await _sut.GetAllDistinctTimeCodesAsync()).ToList();

            result.Should().Equal("TC1", "TC2");
            await _mockRepository.Received(1).GetTimeCodeValidsAsync();
        }

        #endregion

        #region GetAllDistinctProjectsAsync

        [Fact]
        public async Task GetAllDistinctProjectsAsync_WithDuplicates_ReturnsDistinctOrderedValues()
        {
            var entities = new List<TimeCodeValid>
            {
                new TimeCodeValid { ParentProject = "PRJ2" },
                new TimeCodeValid { ParentProject = "PRJ1" },
                new TimeCodeValid { ParentProject = "PRJ2" }
            };
            _mockRepository.GetTimeCodeValidsAsync().Returns(entities);

            var result = (await _sut.GetAllDistinctProjectsAsync()).ToList();

            result.Should().Equal("PRJ1", "PRJ2");
            await _mockRepository.Received(1).GetTimeCodeValidsAsync();
        }

        #endregion

        #region GetPagedTimeCodesAsync

        [Fact]
        public async Task GetPagedTimeCodesAsync_ValidQuery_ReturnsPaginatedResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TimeCodeValid>(new List<TimeCodeValid>(), new PaginationData());
            var pagedResult = new PaginatedResult<TimeCodeValidDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTimeCodesAsync(mappedParams, "JC1", "PRJ1").Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedTimeCodesAsync(query, "JC1", "PRJ1");

            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedTimeCodesAsync(mappedParams, "JC1", "PRJ1");
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_NullFilters_PassesNullsToRepository()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData = new PagedData<TimeCodeValid>(new List<TimeCodeValid>(), new PaginationData());
            var pagedResult = new PaginatedResult<TimeCodeValidDto>();

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetPagedTimeCodesAsync(mappedParams, null, null).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<TimeCodeValidDto>>(pagedData).Returns(pagedResult);

            var result = await _sut.GetPagedTimeCodesAsync(query, null, null);

            result.Should().Be(pagedResult);
            await _mockRepository.Received(1).GetPagedTimeCodesAsync(mappedParams, null, null);
        }

        #endregion

        #region GetTimeCodeValidAsync

        [Fact]
        public async Task GetTimeCodeValidAsync_ValidKey_ReturnsMappedDto()
        {
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(dto);

            var result = await _sut.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            result.Should().Be(dto);
            await _mockRepository.Received(1).GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_NotFound_ReturnsNull()
        {
            _mockRepository.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns((TimeCodeValid?)null);

            var result = await _sut.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            result.Should().BeNull();
        }

        #endregion

        #region CreateTimeCodeValidAsync

        [Fact]
        public async Task CreateTimeCodeValidAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var created = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mockJobCodeRepository.GetJobCodeByIdAsync("JC1").Returns(new JobCode { JobCodeId = "JC1" });
            _mockProjectRepository.ExistsAsync("PRJ1").Returns(true);
            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns((TimeCodeValid?)null);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.CreateTimeCodeValidAsync(entity).Returns(created);
            _mockMapper.Map<TimeCodeValidDto>(created).Returns(expected);

            var result = await _sut.CreateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<TimeCodeValid>(dto);
            await _mockRepository.Received(1).CreateTimeCodeValidAsync(entity);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_DuplicateRecord_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            _mockJobCodeRepository.GetJobCodeByIdAsync("JC1").Returns(new JobCode { JobCodeId = "JC1" });
            _mockProjectRepository.ExistsAsync("PRJ1").Returns(true);
            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1")
                .Returns(new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" });

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));

            ex.Message.Should().Contain("WG1").And.Contain("TC1").And.Contain("PRJ1");
            await _mockRepository.DidNotReceive().CreateTimeCodeValidAsync(Arg.Any<TimeCodeValid>());
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WithTestCodeAndPortfolio_ValidatesComboAndReturnsDto()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };

            _mockTestCapabilityRepository.ExistsAsync("TST1", "PF1").Returns(true);
            _mockProjectRepository.ExistsAsync("PRJ1").Returns(true);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.CreateTimeCodeValidAsync(entity).Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(expected);

            var result = await _sut.CreateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            await _mockTestCapabilityRepository.Received(1).ExistsAsync("TST1", "PF1");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_AllFieldsNull_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_TestCodeWithoutPortfolio_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Must fill in Testcode and Portfolio, or Jobcode");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_PortfolioWithoutTestCode_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Portfolio = "PF1" };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Must fill in Testcode and Portfolio, or Jobcode");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_InvalidJobCode_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "INVALID" };
            _mockJobCodeRepository.GetJobCodeByIdAsync("INVALID").Returns((JobCode?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Not a valid jobcode.");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_InvalidTestCodePortfolioCombo_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Portfolio = "PF1" };
            _mockTestCapabilityRepository.ExistsAsync("TST1", "PF1").Returns(false);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Cannot update, this testcode is not in this portfolio.");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_InvalidParentProject_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "INVALID_PRJ", JobCode = "JC1" };
            _mockJobCodeRepository.GetJobCodeByIdAsync("JC1").Returns(new JobCode { JobCodeId = "JC1" });
            _mockProjectRepository.ExistsAsync("INVALID_PRJ").Returns(false);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Not a valid project");
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mockJobCodeRepository.GetJobCodeByIdAsync("JC1").Returns(new JobCode { JobCodeId = "JC1" });
            _mockProjectRepository.ExistsAsync("PRJ1").Returns(true);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.CreateTimeCodeValidAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.CreateTimeCodeValidAsync(dto));
        }

        #endregion

        #region UpdateTimeCodeValidAsync

        [Fact]
        public async Task UpdateTimeCodeValidAsync_ValidInput_ReturnsMappedDto()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true };
            var updated = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            // JobCode unchanged — no FK call needed
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(updated);
            _mockMapper.Map<TimeCodeValidDto>(updated).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            _mockMapper.Received(1).Map<TimeCodeValid>(dto);
            await _mockRepository.Received(1).UpdateTimeCodeValidAsync(entity);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_JobCodeChanged_ValidatesNewJobCode()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_NEW" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_OLD" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_NEW" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_NEW" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockJobCodeRepository.GetJobCodeByIdAsync("JC_NEW").Returns(new JobCode { JobCodeId = "JC_NEW" });
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            await _mockJobCodeRepository.Received(1).GetJobCodeByIdAsync("JC_NEW");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_JobCodeChangedToInvalid_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "INVALID" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_OLD" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockJobCodeRepository.GetJobCodeByIdAsync("INVALID").Returns((JobCode?)null);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Not a valid jobcode.");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_TestCodePortfolioChanged_ValidatesCombo()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_NEW", Portfolio = "PF_NEW" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_OLD", Portfolio = "PF_OLD" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_NEW", Portfolio = "PF_NEW" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_NEW", Portfolio = "PF_NEW" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockTestCapabilityRepository.ExistsAsync("TST_NEW", "PF_NEW").Returns(true);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            await _mockTestCapabilityRepository.Received(1).ExistsAsync("TST_NEW", "PF_NEW");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_InvalidTestCodePortfolioCombo_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_NEW", Portfolio = "PF_NEW" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST_OLD", Portfolio = "PF_OLD" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockTestCapabilityRepository.ExistsAsync("TST_NEW", "PF_NEW").Returns(false);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Cannot update, this testcode is not in this portfolio.");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_ParentProjectChanged_ValidatesProject()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ_NEW", JobCode = "JC1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ_OLD", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ_NEW", JobCode = "JC1" };
            var expected = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ_NEW", JobCode = "JC1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ_NEW").Returns(existing);
            _mockProjectRepository.ExistsAsync("PRJ_NEW").Returns(true);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).Returns(entity);
            _mockMapper.Map<TimeCodeValidDto>(entity).Returns(expected);

            var result = await _sut.UpdateTimeCodeValidAsync(dto);

            result.Should().Be(expected);
            await _mockProjectRepository.Received(1).ExistsAsync("PRJ_NEW");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_InvalidParentProject_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "INVALID_PRJ", JobCode = "JC1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ_OLD", JobCode = "JC1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "INVALID_PRJ").Returns(existing);
            _mockProjectRepository.ExistsAsync("INVALID_PRJ").Returns(false);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTimeCodeValidAsync(dto));
            ex.Message.Should().Be("Not a valid project");
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_AllOptionalFieldsNull_ThrowsInvalidOperationException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateTimeCodeValidAsync(dto));
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_RepositoryThrows_PropagatesException()
        {
            var dto = new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };
            var existing = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1" };

            _mockRepository.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(existing);
            _mockMapper.Map<TimeCodeValid>(dto).Returns(entity);
            _mockRepository.UpdateTimeCodeValidAsync(entity).ThrowsAsync(new Exception("DB error"));

            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateTimeCodeValidAsync(dto));
        }

        #endregion

        #region DeleteTimeCodeValidAsync

        [Fact]
        public async Task DeleteTimeCodeValidAsync_ValidKey_ReturnsTrue()
        {
            _mockRepository.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1").Returns(true);

            var result = await _sut.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1").Returns(false);

            var result = await _sut.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            result.Should().BeFalse();
        }

        #endregion

        #region DeleteAllByJobCodeAsync

        [Fact]
        public async Task DeleteAllByJobCodeAsync_ValidJobCode_ReturnsTrue()
        {
            _mockRepository.DeleteAllByJobCodeAsync("JC1", "PRJ1").Returns(true);

            var result = await _sut.DeleteAllByJobCodeAsync("JC1", "PRJ1");

            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAllByJobCodeAsync("JC1", "PRJ1");
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_NotFound_ReturnsFalse()
        {
            _mockRepository.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1").Returns(false);

            var result = await _sut.DeleteAllByJobCodeAsync("JC_MISSING", "PRJ1");

            result.Should().BeFalse();
        }

        #endregion

        #region CopyWorkGroupAsync

        [Fact]
        public async Task CopyWorkGroupAsync_ValidInput_ReturnsMappedDtos()
        {
            var entities = new List<TimeCodeValid> { new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };
            var dtos = new List<TimeCodeValidDto> { new TimeCodeValidDto { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1" } };

            _mockRepository.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");
        }

        [Fact]
        public async Task CopyWorkGroupAsync_EmptyResult_ReturnsEmptyCollection()
        {
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1").Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            var result = await _sut.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            result.Should().BeEmpty();
        }

        #endregion

        #region DeleteBulkAsync

        [Fact]
        public async Task DeleteBulkAsync_WithValidItems_DelegatesToRepositoryAndReturnsTrue()
        {
            // Arrange
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1"), ("WG2", "TC2") };

            _mockRepository
                .DeleteBulkAsync(items, "PRJ1")
                .Returns(true);

            // Act
            var result = await _sut.DeleteBulkAsync(items, "PRJ1");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteBulkAsync(items, "PRJ1");
        }

        [Fact]
        public async Task DeleteBulkAsync_WithEmptyItems_ReturnsTrue()
        {
            // Arrange — repository always returns true even for an empty list
            var items = Enumerable.Empty<(string WorkGroup, string TimeCode)>();

            _mockRepository
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    "PRJ1")
                .Returns(true);

            // Act
            var result = await _sut.DeleteBulkAsync(items, "PRJ1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteBulkAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1") };

            _mockRepository
                .DeleteBulkAsync(
                    Arg.Any<IEnumerable<(string WorkGroup, string TimeCode)>>(),
                    Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteBulkAsync(items, "PRJ1"));
        }

        #endregion

        #region CopySelectedWorkGroupsAsync

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithValidWorkGroups_ReturnsMappedDtos()
        {
            // Arrange
            var workGroups = new List<string> { "WG1", "WG2" };
            var entities = new List<TimeCodeValid>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_TGT" }
            };
            var dtos = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT" },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_TGT" }
            };

            _mockRepository
                .CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");

            // Assert
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");
            _mockMapper.Received(1).Map<IEnumerable<TimeCodeValidDto>>(entities);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithEmptyWorkGroups_ReturnsEmptyCollection()
        {
            // Arrange — no work groups selected; repository returns empty, mapper returns empty
            var workGroups = new List<string>();
            var entities = new List<TimeCodeValid>();
            var dtos = new List<TimeCodeValidDto>();

            _mockRepository
                .CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")
                .Returns(entities);
            _mockMapper.Map<IEnumerable<TimeCodeValidDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var workGroups = new List<string> { "WG1" };

            _mockRepository
                .CopySelectedWorkGroupsAsync(
                    Arg.Any<IEnumerable<string>>(),
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _sut.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1"));
        }

        #endregion
    }
}
