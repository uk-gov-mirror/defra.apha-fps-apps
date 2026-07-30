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

namespace Apha.FPS.Application.UnitTests.Services.ProjectStaffPlanDetailsServiceTest
{
    public class ProjectStaffPlanDetailsServiceTests
    {
        private readonly IProjectStaffPlanDetailsRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProjectStaffPlanDetailsService _sut;

        public ProjectStaffPlanDetailsServiceTests()
        {
            _mockRepository = Substitute.For<IProjectStaffPlanDetailsRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ProjectStaffPlanDetailsService(_mockRepository, _mockMapper);
        }

        private static QueryParameters<string> DefaultQuery() => new() { Page = 1, PageSize = 10 };
        private static PaginationParameters<string> DefaultFilter() => new() { Page = 1, PageSize = 10 };

        private static PagedData<ProjectStaffPlanDetailsView> MakePagedData(IEnumerable<ProjectStaffPlanDetailsView> items)
        {
            var list = items.ToList();
            return new PagedData<ProjectStaffPlanDetailsView>(list,
                new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        private static PaginatedResult<ProjectStaffPlanDetailsViewDto> MakePaginatedResult(
            IEnumerable<ProjectStaffPlanDetailsViewDto> items)
        {
            var list = items.ToList();
            return new PaginatedResult<ProjectStaffPlanDetailsViewDto>(list,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });
        }

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            var entities = new List<ProjectStaffPlanDetailsView>
            {
                new() { ProfitCentre = "PC_A", Program = "PROG1", Name = "Alice" },
                new() { ProfitCentre = "PC_B", Program = "PROG2", Name = "Bob" }
            };
            var pagedData = MakePagedData(entities);
            var expectedDtos = new List<ProjectStaffPlanDetailsViewDto>
            {
                new() { ProfitCentre = "PC_A", Program = "PROG1", Name = "Alice" },
                new() { ProfitCentre = "PC_B", Program = "PROG2", Name = "Bob" }
            };
            var expectedResult = MakePaginatedResult(expectedDtos);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanDetailsViewDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            await _mockRepository.Received(1).GetPagedAsync(filter);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyData_ReturnsMappedEmptyList()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            var pagedData   = MakePagedData(Enumerable.Empty<ProjectStaffPlanDetailsView>());
            var emptyResult = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanDetailsViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanDetailsViewDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedAsync_MapsQueryParametersToPaginationParameters()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 2, PageSize = 5, Filter = "{\"ProfitCentre\":\"PC_A\"}" };
            var filter = new PaginationParameters<string> { Page = 2, PageSize = 5, Filter = "{\"ProfitCentre\":\"PC_A\"}" };
            var pagedData   = MakePagedData(Enumerable.Empty<ProjectStaffPlanDetailsView>());
            var emptyResult = MakePaginatedResult(Enumerable.Empty<ProjectStaffPlanDetailsViewDto>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanDetailsViewDto>>(pagedData).Returns(emptyResult);

            // Act
            await _sut.GetPagedAsync(query);

            // Assert
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetPagedAsync(filter);
        }

        #endregion

        #region GetPagedAsync — Error cases

        [Fact]
        public async Task GetPagedAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query  = DefaultQuery();
            var filter = DefaultFilter();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).ThrowsAsync(new Exception("DB error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
        }

        [Fact]
        public async Task GetPagedAsync_WhenMapperThrowsOnQuery_PropagatesException()
        {
            // Arrange
            var query = DefaultQuery();
            _mockMapper.Map<PaginationParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Mapping error");
        }

        [Fact]
        public async Task GetPagedAsync_WhenMapperThrowsOnResult_PropagatesException()
        {
            // Arrange
            var query    = DefaultQuery();
            var filter   = DefaultFilter();
            var pagedData = MakePagedData(Enumerable.Empty<ProjectStaffPlanDetailsView>());

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _mockRepository.GetPagedAsync(filter).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ProjectStaffPlanDetailsViewDto>>(pagedData)
                .Throws(new Exception("Result mapping error"));

            // Act
            var act = async () => await _sut.GetPagedAsync(query);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Result mapping error");
        }

        #endregion
    }
}
