using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.PACT.Api.UnitTests.Controller.RecreateAndReleaseSummaryControllerTest
{
    public class RecreateAndReleaseSummaryControllerTests
    {
        private readonly IRecreateAndReleaseSummaryService _mockService;
        private readonly IBatchJobService _mockBatchJobService;
        private readonly IFpsRequestContext _mockfpsRequestContext;
        private readonly IMapper _mockMapper;
        private readonly RecreateAndReleaseSummaryController _controller;

        private const string TestUserId = "TestUser1";
        private const short TestPeriod = 1;

        public RecreateAndReleaseSummaryControllerTests()
        {
            _mockService = Substitute.For<IRecreateAndReleaseSummaryService>();
            _mockBatchJobService = Substitute.For<IBatchJobService>();
            _mockfpsRequestContext = Substitute.For<IFpsRequestContext>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new RecreateAndReleaseSummaryController(_mockService, _mockBatchJobService, _mockfpsRequestContext, _mockMapper);
        }

        #region GetRecreateSummariesLogs

        [Fact]
        public async Task GetRecreateSummariesLogs_WithExistingLogs_ReturnsOkWithPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "datedone",
                Descending = true
            };

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            var responses = new List<RecreateSummaryLogRes>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = TestUserId, Comments = "Test User", Period = 2, DateDone = DateTime.UtcNow.AddDays(-1) }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count());
            Assert.Equal(1, returnValue.PaginationData.PageNumber);
            Assert.Equal(10, returnValue.PaginationData.PageSize);
            Assert.Equal(1, returnValue.PaginationData.TotalPages);
            Assert.Equal(2, returnValue.PaginationData.TotalRecords);

            await _mockService.Received(1).GetRecreateSummaryLogAsync(query);
            _mockMapper.Received(1).Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithNoLogs_ReturnsOkWithEmptyPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var emptyDtos = new List<RecreateSummaryLogDto>();
            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(emptyDtos, paginationDto);

            var emptyResponses = new List<RecreateSummaryLogRes>();
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 0,
                TotalRecords = 0
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(emptyResponses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
            Assert.Equal(0, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            _mockService.GetRecreateSummaryLogAsync(query)
                .Returns(Task.FromException<PaginatedResult<RecreateSummaryLogDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummaryLog(query));
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "period",
                Descending = false
            };

            var dtos = Enumerable.Range(6, 5)
                .Select(i => new RecreateSummaryLogDto
                {
                    Id = i,
                    UserId = TestUserId,
                    Comments = "Test User",
                    Period = (short)i,
                    DateDone = DateTime.UtcNow.AddDays(-i)
                })
                .ToList();

            var paginationDto = new PaginationDto
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            var responses = dtos.Select(dto => new RecreateSummaryLogRes
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Comments = dto.Comments,
                Period = dto.Period,
                DateDone = dto.DateDone
            }).ToList();

            var pagination = new Pagination
            {
                PageNumber = 2,
                PageSize = 5,
                TotalPages = 4,
                TotalRecords = 20
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<RecreateSummaryLogRes>>(okResult.Value);
            Assert.Equal(5, returnValue.Data.Count());
            Assert.Equal(2, returnValue.PaginationData.PageNumber);
            Assert.Equal(5, returnValue.PaginationData.PageSize);
            Assert.Equal(4, returnValue.PaginationData.TotalPages);
            Assert.Equal(20, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_WithSortParameters_PassesCorrectQueryToService()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "userid",
                Descending = false
            };

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = "UserA", Comments = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", Comments = "User B", Period = 2, DateDone = DateTime.UtcNow }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            var responses = new List<RecreateSummaryLogRes>
            {
                new() { Id = 1, UserId = "UserA", Comments = "User A", Period = 1, DateDone = DateTime.UtcNow },
                new() { Id = 2, UserId = "UserB", Comments = "User B", Period = 2, DateDone = DateTime.UtcNow }
            };

            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 2
            };

            var paginationRes = new PaginationRes<RecreateSummaryLogRes>(responses, pagination);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetRecreateSummaryLog(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _mockService.Received(1).GetRecreateSummaryLogAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 &&
                    q.PageSize == 10 &&
                    q.SortBy == "userid" &&
                    q.Descending == false
                )
            );
        }

        [Fact]
        public async Task GetRecreateSummariesLogs_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var dtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = "Test User", Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            var paginationDto = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1,
                TotalRecords = 1
            };

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(dtos, paginationDto);

            _mockService.GetRecreateSummaryLogAsync(query).Returns(paginatedResult);
            _mockMapper.When(m => m.Map<PaginationRes<RecreateSummaryLogRes>>(paginatedResult))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetRecreateSummaryLog(query));
        }

        #endregion

        #region GetReleaseSummaries

        [Fact]
        public async Task GetReleaseSummaries_WithExistingPeriods_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var responses = new List<ReleasePeriodRes>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            };

            var summaryDto = new ReleaseSummaryDto { ReleasePeriods = dtos.AsReadOnly() };

            var summaryRes = new ReleaseSummaryRes { ReleasePeriods = responses.AsReadOnly() };

            _mockService.GetReleaseSummariesAsync().Returns(summaryDto);
            _mockMapper.Map<ReleaseSummaryRes>(Arg.Any<ReleaseSummaryDto>()).Returns(summaryRes);

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleaseSummaryRes>(okResult.Value);
            Assert.Equal(2, returnValue.ReleasePeriods.Count);
            Assert.Equal("Period1", returnValue.ReleasePeriods[0].PeriodName);
            Assert.Equal("Period2", returnValue.ReleasePeriods[1].PeriodName);

            await _mockService.Received(1).GetReleaseSummariesAsync();
            _mockMapper.Received(1).Map<ReleaseSummaryRes>(Arg.Any<ReleaseSummaryDto>());
        }

        [Fact]
        public async Task GetReleaseSummaries_WithNoPeriods_ReturnsOkWithEmptyList()
        {
            // Arrange
            var summaryDto    = new ReleaseSummaryDto { ReleasePeriods = new List<ReleasePeriodDto>().AsReadOnly() };
            var summaryRes    = new ReleaseSummaryRes { ReleasePeriods = new List<ReleasePeriodRes>().AsReadOnly() };

            _mockService.GetReleaseSummariesAsync().Returns(summaryDto);
            _mockMapper.Map<ReleaseSummaryRes>(Arg.Any<ReleaseSummaryDto>()).Returns(summaryRes);

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleaseSummaryRes>(okResult.Value);
            Assert.Empty(returnValue.ReleasePeriods);

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummaries_MapsAllFieldsCorrectly()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new()
                {
                    PeriodName  = "P1",
                    PeriodType  = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod   = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            var responses = new List<ReleasePeriodRes>
            {
                new()
                {
                    PeriodName  = "P1",
                    PeriodType  = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod   = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            var summaryDto = new ReleaseSummaryDto { ReleasePeriods = dtos };
            var summaryRes = new ReleaseSummaryRes { ReleasePeriods = responses };

            _mockService.GetReleaseSummariesAsync().Returns(summaryDto);
            _mockMapper.Map<ReleaseSummaryRes>(Arg.Any<ReleaseSummaryDto>()).Returns(summaryRes);

            // Act
            var result = await _controller.GetReleaseSummaries();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleaseSummaryRes>(okResult.Value);
            Assert.Single(returnValue.ReleasePeriods);
            var res = returnValue.ReleasePeriods[0];
            Assert.Equal("P1",      res.PeriodName);
            Assert.Equal("Quarter", res.PeriodType);
            Assert.Equal(1.0,       res.StartPeriod);
            Assert.Equal(3.0,       res.EndPeriod);
            Assert.Equal((short)2,  res.FinalSummariesRun);
            Assert.Equal((short)1,  res.PeriodLocked);
        }

        [Fact]
        public async Task GetReleaseSummaries_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.GetReleaseSummariesAsync()
                .Returns(Task.FromException<ReleaseSummaryDto>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleaseSummaries());

            await _mockService.Received(1).GetReleaseSummariesAsync();
        }

        [Fact]
        public async Task GetReleaseSummaries_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var summaryDto = new ReleaseSummaryDto
            {
                ReleasePeriods = new List<ReleasePeriodDto> { new() { PeriodName = "Period1" } }.AsReadOnly()
            };

            _mockService.GetReleaseSummariesAsync().Returns(summaryDto);
            _mockMapper.When(m => m.Map<ReleaseSummaryRes>(Arg.Any<ReleaseSummaryDto>()))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleaseSummaries());
        }

        #endregion

        #region GetReleasePeriods

        [Fact]
        public async Task GetReleasePeriods_WithExistingPeriods_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            }.AsReadOnly();

            var responses = new List<ReleasePeriodRes>
            {
                new() { PeriodName = "Period1", PeriodType = "Month", StartPeriod = 0.5, EndPeriod = 1.0, FinalSummariesRun = 0, PeriodLocked = 0 },
                new() { PeriodName = "Period2", PeriodType = "Month", StartPeriod = 1.5, EndPeriod = 2.0, FinalSummariesRun = 1, PeriodLocked = 0 }
            }.AsReadOnly();

            _mockService.GetReleasePeriodsAsync().Returns(dtos);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(responses);

            // Act
            var result = await _controller.GetReleasePeriods();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Equal("Period1", returnValue[0].PeriodName);
            Assert.Equal("Period2", returnValue[1].PeriodName);

            await _mockService.Received(1).GetReleasePeriodsAsync();
            _mockMapper.Received(1).Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>());
        }

        [Fact]
        public async Task GetReleasePeriods_WithNoPeriods_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>().AsReadOnly();
            var responses = new List<ReleasePeriodRes>().AsReadOnly();

            _mockService.GetReleasePeriodsAsync().Returns(dtos);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(responses);

            // Act
            var result = await _controller.GetReleasePeriods();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Empty(returnValue);

            await _mockService.Received(1).GetReleasePeriodsAsync();
        }

        [Fact]
        public async Task GetReleasePeriods_MapsAllFieldsCorrectly()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto>
            {
                new()
                {
                    PeriodName = "P1",
                    PeriodType = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            var responses = new List<ReleasePeriodRes>
            {
                new()
                {
                    PeriodName = "P1",
                    PeriodType = "Quarter",
                    StartPeriod = 1.0,
                    EndPeriod = 3.0,
                    FinalSummariesRun = 2,
                    PeriodLocked = 1
                }
            }.AsReadOnly();

            _mockService.GetReleasePeriodsAsync().Returns(dtos);
            _mockMapper.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()).Returns(responses);

            // Act
            var result = await _controller.GetReleasePeriods();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsAssignableFrom<IReadOnlyList<ReleasePeriodRes>>(okResult.Value);
            Assert.Single(returnValue);
            var res = returnValue[0];
            Assert.Equal("P1", res.PeriodName);
            Assert.Equal("Quarter", res.PeriodType);
            Assert.Equal(1.0, res.StartPeriod);
            Assert.Equal(3.0, res.EndPeriod);
            Assert.Equal((short)2, res.FinalSummariesRun);
            Assert.Equal((short)1, res.PeriodLocked);
        }

        [Fact]
        public async Task GetReleasePeriods_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _mockService.GetReleasePeriodsAsync()
                .Returns(Task.FromException<IReadOnlyList<ReleasePeriodDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleasePeriods());

            await _mockService.Received(1).GetReleasePeriodsAsync();
        }

        [Fact]
        public async Task GetReleasePeriods_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var dtos = new List<ReleasePeriodDto> { new() { PeriodName = "Period1" } }.AsReadOnly();

            _mockService.GetReleasePeriodsAsync().Returns(dtos);
            _mockMapper.When(m => m.Map<IReadOnlyList<ReleasePeriodRes>>(Arg.Any<IReadOnlyList<ReleasePeriodDto>>()))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetReleasePeriods());
        }

        #endregion

        #region SetFinalSummaryRun

        [Fact]
        public async Task SetFinalSummaryRun_WithExistingPeriod_ReturnsOkWithMappedResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "TestPeriod", FinalSummariesRun = 1, SendEmail = "1" };

            var dto = new ReleasePeriodDto
            {
                PeriodName = "TestPeriod",
                FinalSummariesRun = 1,
                EndPeriod = 1.0
            };

            var response = new ReleasePeriodRes
            {
                PeriodName = "TestPeriod",
                FinalSummariesRun = 1,
                EndPeriod = 1.0
            };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleasePeriodRes>(okResult.Value);
            Assert.Equal("TestPeriod", returnValue.PeriodName);
            Assert.Equal((short)1, returnValue.FinalSummariesRun);

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail);
            _mockMapper.Received(1).Map<ReleasePeriodRes>(dto);
        }

        [Fact]
        public async Task SetFinalSummaryRun_WithNonExistingPeriod_ReturnsOkWithNullMappedResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "NonExistentPeriod", FinalSummariesRun = 1, SendEmail = "0" };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail)
                .Returns((ReleasePeriodDto?)null);
            _mockMapper.Map<ReleasePeriodRes>((ReleasePeriodDto?)null).Returns((ReleasePeriodRes?)null);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail);
            _mockMapper.Received(1).Map<ReleasePeriodRes>((ReleasePeriodDto?)null);
        }

        [Fact]
        public async Task SetFinalSummaryRun_PassesCorrectArgumentsToService()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3, SendEmail = "1" };

            var dto      = new ReleasePeriodDto { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3 };
            var response = new ReleasePeriodRes { PeriodName = "ArgCheckPeriod", FinalSummariesRun = 3 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            await _controller.SetFinalSummaryRun(request);

            // Assert
            await _mockService.Received(1).SetFinalSummaryRunAsync(
                Arg.Is<string>(p  => p  == "ArgCheckPeriod"),
                Arg.Is<short>(f   => f  == (short)3),
                Arg.Is<string>(s  => s  == "1")
            );
        }

        [Fact]
        public async Task SetFinalSummaryRun_MapsAllFieldsFromDtoToResponse()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "FieldMapPeriod", FinalSummariesRun = 2, SendEmail = "0" };

            var dto = new ReleasePeriodDto
            {
                PeriodName       = "FieldMapPeriod",
                PeriodType       = "Month",
                StartPeriod      = 1.5,
                EndPeriod        = 2.5,
                FinalSummariesRun = 2,
                PeriodLocked     = 0
            };

            var response = new ReleasePeriodRes
            {
                PeriodName       = "FieldMapPeriod",
                PeriodType       = "Month",
                StartPeriod      = 1.5,
                EndPeriod        = 2.5,
                FinalSummariesRun = 2,
                PeriodLocked     = 0
            };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail).Returns(dto);
            _mockMapper.Map<ReleasePeriodRes>(dto).Returns(response);

            // Act
            var result = await _controller.SetFinalSummaryRun(request);

            // Assert
            var okResult    = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ReleasePeriodRes>(okResult.Value);
            Assert.Equal("FieldMapPeriod", returnValue.PeriodName);
            Assert.Equal("Month",          returnValue.PeriodType);
            Assert.Equal(1.5,              returnValue.StartPeriod);
            Assert.Equal(2.5,              returnValue.EndPeriod);
            Assert.Equal((short)2,         returnValue.FinalSummariesRun);
            Assert.Equal((short)0,         returnValue.PeriodLocked);
        }

        [Fact]
        public async Task SetFinalSummaryRun_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "ErrorPeriod", FinalSummariesRun = 1, SendEmail = "1" };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail)
                .Returns(Task.FromException<ReleasePeriodDto?>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.SetFinalSummaryRun(request));

            await _mockService.Received(1).SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail);
        }

        [Fact]
        public async Task SetFinalSummaryRun_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var request = new ReleasePeriodReq { PeriodName = "MapperErrorPeriod", FinalSummariesRun = 1, SendEmail = "0" };
            var dto = new ReleasePeriodDto { PeriodName = "MapperErrorPeriod", FinalSummariesRun = 1 };

            _mockService.SetFinalSummaryRunAsync(request.PeriodName, request.FinalSummariesRun, request.SendEmail).Returns(dto);
            _mockMapper.When(m => m.Map<ReleasePeriodRes>(dto))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.SetFinalSummaryRun(request));
        }

        #endregion

        #region GetBatchJobHistory

        [Fact]
        public async Task GetBatchJobHistory_WithResults_ReturnsOkWithPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string jobName = "RecreateSummary";

            var dtos = new List<BatchJobHistoryDto>
            {
                new() { JobId = 1, JobName = jobName, Status = "Completed", RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow },
                new() { JobId = 1, JobName = jobName, Status = "Running",   RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow.AddMinutes(-5) }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var paginatedResult = new PaginatedResult<BatchJobHistoryDto>(dtos, paginationDto);

            var responses = dtos.Select(d => new BatchJobHistoryRes
            {
                JobId = d.JobId, JobName = d.JobName, Status = d.Status, RequestedBy = d.RequestedBy, StartDateTime = d.StartDateTime
            }).ToList();
            var pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var paginationRes = new PaginationRes<BatchJobHistoryRes>(responses, pagination);

            _mockBatchJobService.GetBatchJobsHistoryAsync(query, jobName).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<BatchJobHistoryRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetBatchJobHistory(query, jobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<BatchJobHistoryRes>>(okResult.Value);
            Assert.Equal(2, returnValue.Data.Count());
            await _mockBatchJobService.Received(1).GetBatchJobsHistoryAsync(query, jobName);
            _mockMapper.Received(1).Map<PaginationRes<BatchJobHistoryRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetBatchJobHistory_EmptyResults_ReturnsOkWithEmptyPaginatedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string jobName = "RecreateSummary";

            var paginatedResult = new PaginatedResult<BatchJobHistoryDto>(
                [], new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var paginationRes = new PaginationRes<BatchJobHistoryRes>(
                [], new Pagination { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockBatchJobService.GetBatchJobsHistoryAsync(query, jobName).Returns(paginatedResult);
            _mockMapper.Map<PaginationRes<BatchJobHistoryRes>>(paginatedResult).Returns(paginationRes);

            // Act
            var result = await _controller.GetBatchJobHistory(query, jobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<BatchJobHistoryRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
            Assert.Equal(0, returnValue.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetBatchJobHistory_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            const string jobName = "RecreateSummary";

            _mockBatchJobService.GetBatchJobsHistoryAsync(query, jobName)
                .Returns(Task.FromException<PaginatedResult<BatchJobHistoryDto>>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetBatchJobHistory(query, jobName));
        }

        #endregion

        #region CanRunBatchJob

        [Fact]
        public async Task CanRunBatchJob_WhenJobCanRun_ReturnsOkWithTrue()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            _mockBatchJobService.CanRunBatchJobAsync(jobName).Returns(true);

            // Act
            var result = await _controller.CanRunBatchJob(jobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _mockBatchJobService.Received(1).CanRunBatchJobAsync(jobName);
        }

        [Fact]
        public async Task CanRunBatchJob_WhenJobIsRunning_ReturnsOkWithFalse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            _mockBatchJobService.CanRunBatchJobAsync(jobName).Returns(false);

            // Act
            var result = await _controller.CanRunBatchJob(jobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value!);
            await _mockBatchJobService.Received(1).CanRunBatchJobAsync(jobName);
        }

        [Fact]
        public async Task CanRunBatchJob_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            _mockBatchJobService.CanRunBatchJobAsync(jobName)
                .Returns(Task.FromException<bool>(new InvalidOperationException("Service error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CanRunBatchJob(jobName));
        }

        #endregion

        #region TriggerRecreateSummariesJob

        [Fact]
        public async Task TriggerRecreateSummariesJob_ValidRequest_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new RecreateSummariesReq { Month = 6 };
            const string correlationId = "corr-123";
            const int fpsYear = 2024;
            const string userEmail = "user@test.com";

            _mockfpsRequestContext.FpsYear.Returns(fpsYear);
            _mockfpsRequestContext.UserEmailId.Returns(userEmail);

            var dto = new BatchJobEventTriggerDto { EventId = "event-abc" };
            var response = new BatchJobEventTriggerRes { EventId = "event-abc" };

            _mockBatchJobService
                .TriggerRecreateSummariesJobAsync(request.Month, fpsYear, userEmail, correlationId)
                .Returns(dto);
            _mockMapper.Map<BatchJobEventTriggerRes>(dto).Returns(response);

            // Act
            var result = await _controller.TriggerRecreateSummariesJob(request, correlationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<BatchJobEventTriggerRes>(okResult.Value);
            Assert.Equal("event-abc", returnValue.EventId);

            await _mockBatchJobService.Received(1).TriggerRecreateSummariesJobAsync(
                request.Month, fpsYear, userEmail, correlationId);
            _mockMapper.Received(1).Map<BatchJobEventTriggerRes>(dto);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJob_PassesFpsYearAndUserEmailFromContext()
        {
            // Arrange
            var request = new RecreateSummariesReq { Month = 3 };
            const string correlationId = "corr-456";
            const int expectedYear = 2025;
            const string expectedEmail = "admin@test.com";

            _mockfpsRequestContext.FpsYear.Returns(expectedYear);
            _mockfpsRequestContext.UserEmailId.Returns(expectedEmail);

            var dto = new BatchJobEventTriggerDto { EventId = "ev" };
            _mockBatchJobService
                .TriggerRecreateSummariesJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(dto);
            _mockMapper.Map<BatchJobEventTriggerRes>(dto).Returns(new BatchJobEventTriggerRes());

            // Act
            await _controller.TriggerRecreateSummariesJob(request, correlationId);

            // Assert – ensure context values were forwarded to the service
            await _mockBatchJobService.Received(1).TriggerRecreateSummariesJobAsync(
                request.Month,
                Arg.Is<int>(y => y == expectedYear),
                Arg.Is<string>(u => u == expectedEmail),
                correlationId);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJob_ServiceThrowsBusinessValidation_PropagatesException()
        {
            // Arrange
            var request = new RecreateSummariesReq { Month = 0 }; // invalid month
            const string correlationId = "corr-789";

            _mockfpsRequestContext.FpsYear.Returns(2024);
            _mockfpsRequestContext.UserEmailId.Returns("user@test.com");

            _mockBatchJobService
                .TriggerRecreateSummariesJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<BatchJobEventTriggerDto>(
                    new BusinessValidationErrorException([new BusinessValidationError("Month must be between 1 and 12.", "INVALID_MONTH")])));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _controller.TriggerRecreateSummariesJob(request, correlationId));
        }

        [Fact]
        public async Task TriggerRecreateSummariesJob_ServiceThrowsGenericException_PropagatesException()
        {
            // Arrange
            var request = new RecreateSummariesReq { Month = 6 };
            const string correlationId = "corr-999";

            _mockfpsRequestContext.FpsYear.Returns(2024);
            _mockfpsRequestContext.UserEmailId.Returns("user@test.com");

            _mockBatchJobService
                .TriggerRecreateSummariesJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromException<BatchJobEventTriggerDto>(new InvalidOperationException("Unexpected error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.TriggerRecreateSummariesJob(request, correlationId));
        }

        [Fact]
        public async Task TriggerRecreateSummariesJob_MapperThrowsException_PropagatesException()
        {
            // Arrange
            var request = new RecreateSummariesReq { Month = 6 };
            const string correlationId = "corr-map";
            var dto = new BatchJobEventTriggerDto { EventId = "ev" };

            _mockfpsRequestContext.FpsYear.Returns(2024);
            _mockfpsRequestContext.UserEmailId.Returns("user@test.com");

            _mockBatchJobService
                .TriggerRecreateSummariesJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(dto);
            _mockMapper.When(m => m.Map<BatchJobEventTriggerRes>(dto))
                .Do(_ => throw new InvalidOperationException("Mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _controller.TriggerRecreateSummariesJob(request, correlationId));
        }

        #endregion
    }
}
