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

namespace Apha.FPS.Application.UnitTests.Services.StaffJobServiceTest
{
    public class StaffJobServiceTests
    {
        private readonly IStaffJobRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly StaffJobService _sut;

        public StaffJobServiceTests()
        {
            _mockRepository = Substitute.For<IStaffJobRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new StaffJobService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WithValidQueryFilter_ReturnsSuccessfulPaginatedResult()
        {
            string jobCode = "JOB001";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"JOB001\"}"
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"JOB001\"}"
            };

            var repositoryResult = new PagedData<StaffJobView>
            {

                Data = new List<StaffJobView>
                        {
                        new StaffJobView { StaffID = "S001", JobCode = "JOB001", ChargeRate = 75.50m },
                        new StaffJobView { StaffID = "S002", JobCode = "JOB001", ChargeRate = 80.00m }
                        },
                PaginationData = new PaginationData
                {
                    TotalPages = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            var expectedResult = new PaginatedResult<StaffJobViewDto>
            {
                Data = new List<StaffJobViewDto>
                        {
                        new StaffJobViewDto { StaffID = "S001", JobCode = "JOB001", ChargeRate = 75.50m },
                        new StaffJobViewDto { StaffID = "S002", JobCode = "JOB001", ChargeRate = 80.00m }
                        },
                PaginationData = new PaginationDto
                {
                    TotalPages = 2,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Returns(repositoryResult);

            _mockMapper.Map<PaginatedResult<StaffJobViewDto>>(repositoryResult)
            .Returns(expectedResult);

            // Act
            var result = await _sut.GetJobStaffCostAsync(queryFilter, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.PaginationData.PageNumber.Should().Be(1);
            result.PaginationData.PageSize.Should().Be(10);
            result.Data.First().StaffID.Should().Be("S001");
            result.Data.First().ChargeRate.Should().Be(75.50m);

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            await _mockRepository.Received(1).GetJobStaffCostAsync(mappedPaginationParams, jobCode);
            _mockMapper.Received(1).Map<PaginatedResult<StaffJobViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WithValidQueryFilter_ReturnsEmptyPaginatedResult()
        {
            string jobCode = "NONEXISTENT";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,                
                Filter = "{\"JobCode\":\"NONEXISTENT\"}"
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"JobCode\":\"NONEXISTENT\"}"
            };

            var emptyRepositoryResult = new PagedData<StaffJobView>
            {

                Data = new List<StaffJobView>(),
                PaginationData = new PaginationData
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            var emptyExpectedResult = new PaginatedResult<StaffJobViewDto>
            {
                Data = new List<StaffJobViewDto>(),
                PaginationData = new PaginationDto
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Returns(emptyRepositoryResult);

            _mockMapper.Map<PaginatedResult<StaffJobViewDto>>(emptyRepositoryResult)
            .Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetJobStaffCostAsync(queryFilter, jobCode);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
            result.PaginationData.PageNumber.Should().Be(1);
            result.PaginationData.PageSize.Should().Be(10);

            await _mockRepository.Received(1).GetJobStaffCostAsync(mappedPaginationParams, jobCode);
        }

        [Fact]
        public async Task GetJobStaffCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            string jobCode = "";
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
            .Returns(mappedPaginationParams);

            _mockRepository.GetJobStaffCostAsync(mappedPaginationParams, jobCode)
            .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.GetJobStaffCostAsync(queryFilter, jobCode)
            );

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            _mockMapper.DidNotReceive().Map<PaginatedResult<StaffJobViewDto>>(Arg.Any<PaginatedResult<StaffJobView>>());
        }


        [Fact]
        public async Task GetStaffWorkgroupLookup_WithValidData_ReturnsMapperDtoList()
        {
            // Arrange
            var staffWorkgroupEntities = new List<StaffWorkgroupLookup>
            {
                new StaffWorkgroupLookup
                {
                     StaffID = "S001",
                    WorkGroupGrade = "WG001",
                    Name = "Engineering"
                },
                new StaffWorkgroupLookup
                {
                     StaffID = "S002",
                    WorkGroupGrade = "WG002",
                    Name = "Design"
                }
            };

            var expectedDtos = new List<StaffWorkgroupLookupDto>
            {
                new StaffWorkgroupLookupDto
                {
                    StaffID = "S001",
                    WorkGroupGrade = "WG001",
                    Name = "Engineering"
                },
                new StaffWorkgroupLookupDto
                {
                    StaffID = "S002",
                    WorkGroupGrade = "WG002",
                    Name = "Design"
                }
            };

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromResult(staffWorkgroupEntities));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupEntities)
            .Returns(expectedDtos);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("S001", result[0].StaffID);
            Assert.Equal("Engineering", result[0].Name);
            Assert.Equal("S002", result[1].StaffID);
            Assert.Equal("Design", result[1].Name);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>>(staffWorkgroupEntities);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyEntityList = new List<StaffWorkgroupLookup>();
            var emptyDtoList = new List<StaffWorkgroupLookupDto>();

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromResult(emptyEntityList));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>>(emptyEntityList)
            .Returns(emptyDtoList);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>>(emptyEntityList);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(await Task.FromResult<List<StaffWorkgroupLookup>>(null!));

            _mockMapper.Map<List<StaffWorkgroupLookupDto>?>(null)
            .Returns((List<StaffWorkgroupLookupDto>?)null);

            // Act
            var result = await _sut.GetStaffWorkgroupLookup();

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.Received(1).Map<List<StaffWorkgroupLookupDto>?>(null); // Explicitly mark the type as nullable
        }

        [Fact]
        public async Task GetStaffWorkgroupLookup_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetStaffWorkgroupLookup()
            .Returns(Task.FromException<List<StaffWorkgroupLookup>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
            async () => await _sut.GetStaffWorkgroupLookup()
            );

            Assert.Equal("Database connection failed", exception.Message);
            await _mockRepository.Received(1).GetStaffWorkgroupLookup();
            _mockMapper.DidNotReceive().Map<List<StaffWorkgroupLookupDto>>(Arg.Any<List<StaffWorkgroupLookup>>());
        }

        [Fact]
        public async Task GetStaffChargeRate_WithValidStaffIdAndJobCode_ReturnsChargeRate()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobcode = "JOB001";
            var expectedChargeRate = 150.50m;

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(expectedChargeRate));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedChargeRate, result.Value);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenNoChargeRateExists_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobcode = "JOBNOTFOUND";

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WithEmptyStrings_PassesToRepositoryAndReturnsResult()
        {
            // Arrange
            var staffId = string.Empty;
            var jobcode = string.Empty;

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .Returns(Task.FromResult<decimal?>(null));

            // Act
            var result = await _sut.GetStaffChargeRate(staffId, jobcode);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobcode = "JOB001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository
            .GetStaffChargeRate(staffId, jobcode)
            .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
            () => _sut.GetStaffChargeRate(staffId, jobcode)
            );

            Assert.Equal(expectedException.Message, exception.Message);
            await _mockRepository.Received(1).GetStaffChargeRate(staffId, jobcode);
        }


        [Fact]
        public async Task GetByIdAsync_WhenValidStaffIdAndJobCode_ReturnsStaffJobDto()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobEntity = new StaffJob
            {
                StaffId = staffId,
                JobCode = jobCode
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = staffId,
                JobCode = jobCode
            };                      

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(staffJobEntity));            
            _mockMapper.Map<StaffJobDto>(staffJobEntity).Returns(expectedDto);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.StaffId, result.StaffId);
            Assert.Equal(expectedDto.JobCode, result.JobCode);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(staffJobEntity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRecordNotFound_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "JOB999";

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(null)); 
            _mockMapper.Map<StaffJobDto>(Arg.Any<StaffJob?>()).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(null);
        }

        [Theory]       
        [InlineData("", "JOB001")]
        [InlineData("STAFF001", "")]
        public async Task GetByIdAsync_WhenInvalidInputParameters_ReturnsNull(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(null)); 
            _mockMapper.Map<StaffJobDto>(Arg.Any<StaffJob?>()).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobEntity = new StaffJob
            {
                StaffId = staffId,
                JobCode = jobCode
            };

            _mockRepository.GetByIdAsync(staffId, jobCode).Returns(Task.FromResult<StaffJob?>(staffJobEntity));            
            _mockMapper.Map<StaffJobDto>(staffJobEntity).Returns((StaffJobDto?)null);

            // Act
            var result = await _sut.GetByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobDto>(staffJobEntity);
        }

        #region GetViewByStaffIdAsync Tests

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenValidStaffIdAndJobCode_ReturnsStaffJobViewDto()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m,
                StaffCost = 6000.00m
            };

            var expectedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe",
                PlannedHours = 40,
                ChargeRate = 150.00m,
                StaffCost = 6000.00m
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be(expectedDto.StaffID);
            result.JobCode.Should().Be(expectedDto.JobCode);
            result.Name.Should().Be(expectedDto.Name);
            result.PlannedHours.Should().Be(expectedDto.PlannedHours);
            result.ChargeRate.Should().Be(expectedDto.ChargeRate);
            result.StaffCost.Should().Be(expectedDto.StaffCost);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenRecordNotFound_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "JOB999";

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(null));
            _mockMapper.Map<StaffJobViewDto>(Arg.Any<StaffJobView?>())
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(null);
        }

        [Theory]       
        [InlineData("", "JOB001")]
        [InlineData("STAFF001", "")]
        public async Task GetViewByStaffIdAsync_WhenInvalidInputParameters_ReturnsNull(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(null));
            _mockMapper.Map<StaffJobViewDto>(Arg.Any<StaffJobView?>())
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenMapperReturnsNull_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "John Doe"
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns((StaffJobViewDto?)null);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB001";
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetViewByStaffIdAsync(staffId, jobCode)
            );

            Assert.Equal(expectedException.Message, exception.Message);
            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.DidNotReceive().Map<StaffJobViewDto>(Arg.Any<StaffJobView>());
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WithCompleteData_MapsAllProperties()
        {
            // Arrange
            var staffId = "STAFF002";
            var jobCode = "JOB002";

            var staffJobViewEntity = new StaffJobView
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            var expectedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                Name = "Jane Smith",
                PlannedHours = 80,
                ChargeRate = 200.00m,
                StaffCost = 16000.00m,
                WorkGroupGrade = "WG01",
                GradeCode = "G01",
                WorkGroup = "Engineering",
                SectorName = "charge",
                Days = 10
            };

            _mockRepository.GetViewByStaffIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobView?>(staffJobViewEntity));
            _mockMapper.Map<StaffJobViewDto>(staffJobViewEntity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be(expectedDto.StaffID);
            result.JobCode.Should().Be(expectedDto.JobCode);
            result.Name.Should().Be(expectedDto.Name);
            result.PlannedHours.Should().Be(expectedDto.PlannedHours);
            result.ChargeRate.Should().Be(expectedDto.ChargeRate);
            result.StaffCost.Should().Be(expectedDto.StaffCost);
            result.WorkGroupGrade.Should().Be(expectedDto.WorkGroupGrade);
            result.GradeCode.Should().Be(expectedDto.GradeCode);
            result.WorkGroup.Should().Be(expectedDto.WorkGroup);
            result.SectorName.Should().Be(expectedDto.SectorName);
            result.Days.Should().Be(expectedDto.Days);

            await _mockRepository.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobViewDto>(staffJobViewEntity);
        }

        #endregion

        [Fact]
        public async Task AddAsync_WithValidStaffJob_ShouldReturnStaffJobDto()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var repositoryResult = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.GetByIdAsync(inputDto.StaffId, inputDto.JobCode).Returns((StaffJob?)null);
            _mockRepository.AddAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<StaffJobDto>(repositoryResult).Returns(expectedDto);

            var result = await _sut.AddAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be("STAFF001");
            result.JobCode.Should().Be("JOB001");
            result.PlannedHours.Should().Be(75.50);

            _mockMapper.Received(1).Map<StaffJob>(inputDto);
            await _mockRepository.Received(1).AddAsync(mappedEntity);
            _mockMapper.Received(1).Map<StaffJobDto>(repositoryResult);
        }


        [Fact]
        public async Task AddAsync_WithMinimalData_ShouldProcessSuccessfully()
        {
            // Arrange
            var minimalDto = new StaffJobDto
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var repositoryResult = new StaffJob
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF002",
                JobCode = "JOB002"
            };

            _mockMapper.Map<StaffJob>(minimalDto).Returns(mappedEntity);
            _mockRepository.GetByIdAsync(minimalDto.StaffId, minimalDto.JobCode).Returns((StaffJob?)null);
            _mockRepository.AddAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<StaffJobDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(minimalDto);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be("STAFF002");
            result.JobCode.Should().Be("JOB002");

            await _mockRepository.Received(1).AddAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF003",
                JobCode = "JOB003"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF003",
                JobCode = "JOB003"
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.GetByIdAsync(inputDto.StaffId, inputDto.JobCode).Returns((StaffJob?)null);
            _mockRepository.AddAsync(mappedEntity)
            .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            Func<Task> act = async () => await _sut.AddAsync(inputDto);

            // Assert
            await act.Should().ThrowAsync<Exception>()
            .WithMessage("Database connection failed");

            await _mockRepository.Received(1).AddAsync(mappedEntity);
        }

        [Fact]
        public async Task AddAsync_WhenStaffJobIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.AddAsync(null!));
            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task AddAsync_WhenPlannedHoursIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = -5
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.AddAsync(inputDto));
            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task AddAsync_WhenEntryAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 40
            };

            var existingEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001"
            };

            _mockRepository.GetByIdAsync(inputDto.StaffId, inputDto.JobCode).Returns(existingEntity);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddAsync(inputDto));
            ex.Message.Should().Contain("STAFF001").And.Contain("JOB001");
            await _mockRepository.DidNotReceive().AddAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task AddAsync_WhenNoExistingEntry_ProceedsToInsert()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 40
            };

            var mappedEntity = new StaffJob { StaffId = "STAFF001", JobCode = "JOB001", PlannedHours = 40 };
            var repositoryResult = new StaffJob { StaffId = "STAFF001", JobCode = "JOB001", PlannedHours = 40 };
            var expectedDto = new StaffJobDto { StaffId = "STAFF001", JobCode = "JOB001", PlannedHours = 40 };

            _mockRepository.GetByIdAsync(inputDto.StaffId, inputDto.JobCode).Returns((StaffJob?)null);
            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.AddAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<StaffJobDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.StaffId.Should().Be("STAFF001");
            result.JobCode.Should().Be("JOB001");
            await _mockRepository.Received(1).GetByIdAsync(inputDto.StaffId, inputDto.JobCode);
            await _mockRepository.Received(1).AddAsync(mappedEntity);
        }

        [Fact]
        public async Task UpdateAsync_WithValidStaffJob_ShouldReturnUpdatedStaffJobDto()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 75.50
            };

            var updatedEntity = new StaffJob
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 85.00
            };

            var expectedDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = 85.00
            };

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAsync(mappedEntity).Returns(updatedEntity);
            _mockMapper.Map<StaffJobDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateAsync(inputDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.StaffId, result.StaffId);
            Assert.Equal(expectedDto.JobCode, result.JobCode);
            Assert.Equal(expectedDto.PlannedHours, result.PlannedHours);

            _mockMapper.Received(1).Map<StaffJob>(inputDto);
            await _mockRepository.Received(1).UpdateAsync(mappedEntity);
            _mockMapper.Received(1).Map<StaffJobDto>(updatedEntity);
        }        

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_ShouldPropagateException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF999",
                JobCode = "INVALID"
            };

            var mappedEntity = new StaffJob
            {
                StaffId = "STAFF999",
                JobCode = "INVALID"
            };

            var exceptionMessage = "Database connection failed";

            _mockMapper.Map<StaffJob>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateAsync(mappedEntity)
            .Throws(new InvalidOperationException(exceptionMessage));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sut.UpdateAsync(inputDto)
            );

            Assert.Equal(exceptionMessage, exception.Message);

            await _mockRepository.Received(1).UpdateAsync(mappedEntity);
            _mockMapper.DidNotReceive().Map<StaffJobDto>(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task UpdateAsync_WhenStaffJobIsNull_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync(null!));
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task UpdateAsync_WhenPlannedHoursIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var inputDto = new StaffJobDto
            {
                StaffId = "STAFF001",
                JobCode = "JOB001",
                PlannedHours = -5
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _sut.UpdateAsync(inputDto));
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<StaffJob>());
        }

        [Fact]
        public async Task DeleteAsync_WithValidParameters_ReturnsTrue()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB123";
            _mockRepository.DeleteAsync(staffId, jobCode).Returns(Task.FromResult(true));

            // Act
            var result = await _sut.DeleteAsync(staffId, jobCode);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentRecord_ReturnsFalse()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "NONEXISTENT";
            _mockRepository.DeleteAsync(staffId, jobCode).Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync(staffId, jobCode);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyStaffId_CallsRepository()
        {
            // Arrange
            _mockRepository.DeleteAsync("", "JOB123").Returns(Task.FromResult(false));

            // Act
            var result = await _sut.DeleteAsync("", "JOB123");

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync("", "JOB123");
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyJobCode_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync("STAFF001", ""));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "JOB123";
            var expectedException = new Exception("Database connection failed");
            _mockRepository.DeleteAsync(staffId, jobCode).Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(staffId, jobCode));
            Assert.Equal("Database connection failed", exception.Message);
            await _mockRepository.Received(1).DeleteAsync(staffId, jobCode);
        }

        #region GetTotalStaffCostAsync Tests

        [Fact]
        public async Task GetTotalStaffCostAsync_WithValidJobCode_ReturnsTotalFromRepository()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedTotal = 4500m;
            _mockRepository.GetTotalStaffCostAsync(jobCode).Returns(expectedTotal);

            // Act
            var result = await _sut.GetTotalStaffCostAsync(jobCode);

            // Assert
            result.Should().Be(expectedTotal);
            await _mockRepository.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_WhenRepositoryReturnsZero_ReturnsZero()
        {
            // Arrange
            var jobCode = "JOB001";
            _mockRepository.GetTotalStaffCostAsync(jobCode).Returns(0m);

            // Act
            var result = await _sut.GetTotalStaffCostAsync(jobCode);

            // Assert
            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_WithEmptyJobCode_PassesToRepository()
        {
            // Arrange
            var jobCode = string.Empty;
            _mockRepository.GetTotalStaffCostAsync(jobCode).Returns(0m);

            // Act
            var result = await _sut.GetTotalStaffCostAsync(jobCode);

            // Assert
            result.Should().Be(0m);
            await _mockRepository.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var jobCode = "JOB001";
            _mockRepository.GetTotalStaffCostAsync(jobCode)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetTotalStaffCostAsync(jobCode));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        #endregion

        #region GetZtTotalHoursByStaffIdAsync Tests

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WithValidStaffId_ReturnsTotalHours()
        {
            // Arrange
            var staffId = "STAFF001";
            var expectedTotal = 120.5;
            _mockRepository.GetZtTotalHoursByStaffIdAsync(staffId).Returns(expectedTotal);

            // Act
            var result = await _sut.GetZtTotalHoursByStaffIdAsync(staffId);

            // Assert
            result.Should().Be(expectedTotal);
            await _mockRepository.Received(1).GetZtTotalHoursByStaffIdAsync(staffId);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WithNoZtJobs_ReturnsZero()
        {
            // Arrange
            var staffId = "STAFF999";
            _mockRepository.GetZtTotalHoursByStaffIdAsync(staffId).Returns(0.0);

            // Act
            var result = await _sut.GetZtTotalHoursByStaffIdAsync(staffId);

            // Assert
            result.Should().Be(0.0);
            await _mockRepository.Received(1).GetZtTotalHoursByStaffIdAsync(staffId);
        }

        [Fact]
        public async Task GetZtTotalHoursByStaffIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            _mockRepository.GetZtTotalHoursByStaffIdAsync(staffId)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetZtTotalHoursByStaffIdAsync(staffId));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetZtTotalHoursByStaffIdAsync(staffId);
        }

        #endregion

        #region GetZtStaffJobsByStaffIdPagedAsync Tests

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WithValidParams_ReturnsPagedResult()
        {
            // Arrange
            var staffId = "STAFF001";
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var repositoryResult = new PagedData<StaffJobZtView>
            {
                Data = new List<StaffJobZtView>
                {
                    new StaffJobZtView { StaffID = staffId, JobCode = "ZT001", PlannedHours = 40, Name = "Admin" },
                    new StaffJobZtView { StaffID = staffId, JobCode = "ZT002", PlannedHours = 20, Name = "Training" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            var expectedResult = new PaginatedResult<StaffJobZtViewDto>
            {
                Data = new List<StaffJobZtViewDto>
                {
                    new StaffJobZtViewDto { StaffID = staffId, JobCode = "ZT001", PlannedHours = 40, Name = "Admin" },
                    new StaffJobZtViewDto { StaffID = staffId, JobCode = "ZT002", PlannedHours = 20, Name = "Training" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetZtStaffJobsByStaffIdPagedAsync(mappedPaginationParams, staffId).Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<StaffJobZtViewDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetZtStaffJobsByStaffIdPagedAsync(queryFilter, staffId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.Data.First().JobCode.Should().Be("ZT001");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            await _mockRepository.Received(1).GetZtStaffJobsByStaffIdPagedAsync(mappedPaginationParams, staffId);
            _mockMapper.Received(1).Map<PaginatedResult<StaffJobZtViewDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var staffId = "STAFF999";
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var emptyRepositoryResult = new PagedData<StaffJobZtView>
            {
                Data = new List<StaffJobZtView>(),
                PaginationData = new PaginationData
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            var emptyExpectedResult = new PaginatedResult<StaffJobZtViewDto>
            {
                Data = new List<StaffJobZtViewDto>(),
                PaginationData = new PaginationDto
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetZtStaffJobsByStaffIdPagedAsync(mappedPaginationParams, staffId).Returns(emptyRepositoryResult);
            _mockMapper.Map<PaginatedResult<StaffJobZtViewDto>>(emptyRepositoryResult).Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetZtStaffJobsByStaffIdPagedAsync(queryFilter, staffId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetZtStaffJobsByStaffIdPagedAsync(mappedPaginationParams, staffId);
        }

        [Fact]
        public async Task GetZtStaffJobsByStaffIdPagedAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetZtStaffJobsByStaffIdPagedAsync(mappedPaginationParams, staffId)
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetZtStaffJobsByStaffIdPagedAsync(queryFilter, staffId));

            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetZtStaffJobDetailsByIdAsync Tests

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_WhenFound_ReturnsDto()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "ZT001";

            var entity = new StaffJobZtView
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Admin Work"
            };

            var expectedDto = new StaffJobZtViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Admin Work"
            };

            _mockRepository.GetZtStaffJobDetailsByIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobZtView?>(entity));
            _mockMapper.Map<StaffJobZtViewDto>(entity).Returns(expectedDto);

            // Act
            var result = await _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be(staffId);
            result.JobCode.Should().Be(jobCode);
            result.PlannedHours.Should().Be(40);

            await _mockRepository.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
            _mockMapper.Received(1).Map<StaffJobZtViewDto>(entity);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var staffId = "STAFF999";
            var jobCode = "ZT999";

            _mockRepository.GetZtStaffJobDetailsByIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobZtView?>(null));

            // Act
            var result = await _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetZtStaffJobDetailsByIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var staffId = "STAFF001";
            var jobCode = "ZT001";

            _mockRepository.GetZtStaffJobDetailsByIdAsync(staffId, jobCode)
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode));

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        [Theory]
        [InlineData("S001", "ZT001")]
        [InlineData("S002", "ZT002")]
        [InlineData("", "ZT001")]
        public async Task GetZtStaffJobDetailsByIdAsync_WithVariousIds_CallsRepository(string staffId, string jobCode)
        {
            // Arrange
            _mockRepository.GetZtStaffJobDetailsByIdAsync(staffId, jobCode)
                .Returns(Task.FromResult<StaffJobZtView?>(null));

            // Act
            await _sut.GetZtStaffJobDetailsByIdAsync(staffId, jobCode);

            // Assert
            await _mockRepository.Received(1).GetZtStaffJobDetailsByIdAsync(staffId, jobCode);
        }

        #endregion

        #region GetStaffSummaryByIdAsync Tests

        [Fact]
        public async Task GetStaffSummaryByIdAsync_WhenFound_ReturnsMappedDto()
        {
            // Arrange
            var entity = new StaffWorkgroupLookup
            {
                StaffID = "S001",
                Name = "John Doe",
                WorkGroupGrade = "Grade A",
                HrsAvail = 1500,
                HrsPaid = 1800,
                Leave = 200,
                SickSpecial = 50
            };
            var expectedDto = new StaffWorkgroupLookupDto
            {
                StaffID = "S001",
                Name = "John Doe",
                WorkGroupGrade = "Grade A",
                HrsAvail = 1500,
                HrsPaid = 1800,
                Leave = 200,
                SickSpecial = 50
            };

            _mockRepository.GetStaffSummaryByIdAsync("S001")
                .Returns(Task.FromResult<StaffWorkgroupLookup?>(entity));
            _mockMapper.Map<StaffWorkgroupLookupDto>(entity)
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetStaffSummaryByIdAsync("S001");

            // Assert
            result.Should().NotBeNull();
            result!.StaffID.Should().Be("S001");
            result.Name.Should().Be("John Doe");
            result.WorkGroupGrade.Should().Be("Grade A");

            await _mockRepository.Received(1).GetStaffSummaryByIdAsync("S001");
            _mockMapper.Received(1).Map<StaffWorkgroupLookupDto>(entity);
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetStaffSummaryByIdAsync("UNKNOWN")
                .Returns(Task.FromResult<StaffWorkgroupLookup?>(null));

            // Act
            var result = await _sut.GetStaffSummaryByIdAsync("UNKNOWN");

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetStaffSummaryByIdAsync("UNKNOWN");
            _mockMapper.DidNotReceive().Map<StaffWorkgroupLookupDto>(Arg.Any<StaffWorkgroupLookup>());
        }

        [Fact]
        public async Task GetStaffSummaryByIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetStaffSummaryByIdAsync("S001")
                .Returns(Task.FromException<StaffWorkgroupLookup?>(new Exception("DB error")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetStaffSummaryByIdAsync("S001"));

            ex.Message.Should().Be("DB error");
            await _mockRepository.Received(1).GetStaffSummaryByIdAsync("S001");
        }

        [Theory]
        [InlineData("S001")]
        [InlineData("EMP_123")]
        [InlineData("")]
        public async Task GetStaffSummaryByIdAsync_WithVariousIds_CallsRepository(string staffId)
        {
            // Arrange
            _mockRepository.GetStaffSummaryByIdAsync(staffId)
                .Returns(Task.FromResult<StaffWorkgroupLookup?>(null));

            // Act
            await _sut.GetStaffSummaryByIdAsync(staffId);

            // Assert
            await _mockRepository.Received(1).GetStaffSummaryByIdAsync(staffId);
        }

        #endregion

        #region GetStaffResourceUtilisationAsync

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithValidWorkgroup_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            const string workgroup = "WG01";

            var repoResult = new PagedData<StaffResourceUtilisationView>
            {
                Data = new List<StaffResourceUtilisationView>
                {
                    new() { WorkGroup = workgroup, StaffId = "S001", Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5, ApprovedSoct = 20.0, NotApprovedSoct = 5.0 },
                    new() { WorkGroup = workgroup, StaffId = "S002", Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0, ApprovedSoct = 15.0, NotApprovedSoct = 3.0 }
                },
                PaginationData = new PaginationData { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            var expectedResult = new PaginatedResult<StaffResourceUtilisationDto>
            {
                Data = new List<StaffResourceUtilisationDto>
                {
                    new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5 },
                    new() { WorkGroup = workgroup, Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0 }
                },
                PaginationData = new PaginationDto { TotalPages = 1, PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetStaffResourceUtilisationAsync(mappedFilter, workgroup).Returns(repoResult);
            _mockMapper.Map<PaginatedResult<StaffResourceUtilisationDto>>(repoResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.Data.First().WorkGroup.Should().Be(workgroup);
            result.Data.First().Name.Should().Be("John Doe");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetStaffResourceUtilisationAsync(mappedFilter, workgroup);
            _mockMapper.Received(1).Map<PaginatedResult<StaffResourceUtilisationDto>>(repoResult);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithNoData_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedFilter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            const string workgroup = "WG_EMPTY";

            var emptyRepoResult = new PagedData<StaffResourceUtilisationView>
            {
                Data = new List<StaffResourceUtilisationView>(),
                PaginationData = new PaginationData { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            var emptyExpected = new PaginatedResult<StaffResourceUtilisationDto>
            {
                Data = new List<StaffResourceUtilisationDto>(),
                PaginationData = new PaginationDto { TotalPages = 0, PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetStaffResourceUtilisationAsync(mappedFilter, workgroup).Returns(emptyRepoResult);
            _mockMapper.Map<PaginatedResult<StaffResourceUtilisationDto>>(emptyRepoResult).Returns(emptyExpected);

            // Act
            var result = await _sut.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetStaffResourceUtilisationAsync(mappedFilter, workgroup);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_CallsMapperForFilterAndResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var mappedFilter = new PaginationParameters<string> { Page = 2, PageSize = 5 };
            const string workgroup = "WG02";

            var repoResult = new PagedData<StaffResourceUtilisationView>
            {
                Data = new List<StaffResourceUtilisationView>(),
                PaginationData = new PaginationData()
            };
            var mappedResult = new PaginatedResult<StaffResourceUtilisationDto>
            {
                Data = new List<StaffResourceUtilisationDto>(),
                PaginationData = new PaginationDto()
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedFilter);
            _mockRepository.GetStaffResourceUtilisationAsync(mappedFilter, workgroup).Returns(repoResult);
            _mockMapper.Map<PaginatedResult<StaffResourceUtilisationDto>>(repoResult).Returns(mappedResult);

            // Act
            await _sut.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.Received(1).Map<PaginatedResult<StaffResourceUtilisationDto>>(repoResult);
        }

        #endregion

    }
}
