using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.RecreateSummariesLogControllerTest
{
    public class RecreateSummaryLogControllerTests
    {
        private const string EmptyFilterJson = "{}";
        private const string GridId = "releaseLogsGrid";
        private const string BindGridUrl = "/PACT/RecreateSummaryLog/LoadRecreateSummariesLogGrid";
        private const string PartialViewName = "_DataGrid";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        private readonly IMapper _mapper;
        private readonly IRecreateSummaryService _logService;
        private readonly RecreateSummaryLogController _controller;

        public RecreateSummaryLogControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _logService = Substitute.For<IRecreateSummaryService>();
            _controller = new RecreateSummaryLogController(_mapper, _logService);
        }

        #region Helper Methods

        private void SetupQueryParametersMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(callInfo =>
                {
                    var filter = callInfo.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string>
                    {
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        Search = filter.Search,
                        Filter = filter.Filter
                    };
                });
        }

        private void SetupLogItemMapper(List<RecreateSummaryLogDto> dtos, List<RecreateSummaryLogItem> items)
        {
            _mapper.Map<List<RecreateSummaryLogItem>>(Arg.Any<IEnumerable<RecreateSummaryLogDto>>()).Returns(items);
        }

        private void SetupPaginationMapper(PaginationDto? paginationDto = null)
        {
            var pagination = paginationDto ?? new PaginationDto
            {
                PageNumber = DefaultPageNumber,
                PageSize = DefaultPageSize,
                TotalRecords = 0,
                TotalPages = 0
            };

            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel
                {
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalRecords = pagination.TotalRecords
                });
        }

        private static ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>> BuildSuccessResponse(int count = 2)
        {
            var data = Enumerable.Range(1, count)
                .Select(i => new RecreateSummaryLogDto
                {
                    Id = i,
                    UserId = $"User{i}",
                    Period = (short)(202401 + i),
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    Comments = $"Comment {i}"
                })
                .ToList();

            var paginatedResult = new PaginatedResult<RecreateSummaryLogDto>(data, count, DefaultPageNumber, DefaultPageSize);

            return new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = paginatedResult,
                Pagination = new PaginationDto
                {
                    PageNumber = DefaultPageNumber,
                    PageSize = DefaultPageSize,
                    TotalRecords = count,
                    TotalPages = (int)Math.Ceiling(count / (double)DefaultPageSize)
                }
            };
        }

        private static ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>> BuildFailureResponse(string errorMessage = "API error")
        {
            return new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = false,
                Data = null,
                Pagination = null,
                Errors = new List<ApiErrorDto> { new() { Message = errorMessage } }
            };
        }

        private static ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>> BuildEmptySuccessResponse()
        {
            return new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>([], 0, DefaultPageNumber, DefaultPageSize),
                Pagination = new PaginationDto
                {
                    PageNumber = DefaultPageNumber,
                    PageSize = DefaultPageSize,
                    TotalRecords = 0,
                    TotalPages = 0
                }
            };
        }

        private static List<RecreateSummaryLogItem> BuildMappedItems(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new RecreateSummaryLogItem
                {
                    Id = i,
                    UserId = $"User{i}",
                    Period = (short)(202401 + i),
                    DateDone = DateTime.UtcNow.AddDays(-i),
                    User = $"Comment {i}"
                })
                .ToList();
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulResponse_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            var apiResponse = BuildSuccessResponse(3);
            var mappedItems = BuildMappedItems(3);

            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), mappedItems);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Equal(GridId, model.LogsGrid.GridId);
            Assert.Equal(mappedItems.Count, model.LogsGrid.Data.Count());
            Assert.NotNull(model.LogsGrid.Pagination);
        }

        [Fact]
        public async Task Index_WithEmptyResponse_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var apiResponse = BuildEmptySuccessResponse();

            SetupQueryParametersMapper();
            SetupLogItemMapper([], []);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Empty(model.LogsGrid.Data);
        }

        [Fact]
        public async Task Index_WithFailedApiResponse_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var apiResponse = BuildFailureResponse("Service unavailable");

            SetupQueryParametersMapper();
            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Empty(model.LogsGrid.Data);
        }

        [Fact]
        public async Task Index_WithNullDataInResponse_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = null,
                Pagination = null
            };

            SetupQueryParametersMapper();
            SetupPaginationMapper();

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummaryLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogsGrid);
            Assert.Empty(model.LogsGrid.Data);
        }

        [Fact]
        public async Task Index_CallsMapperWithDefaultFilter()
        {
            // Arrange
            var apiResponse = BuildSuccessResponse(1);
            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), BuildMappedItems(1));
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            await _controller.Index();

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(
                Arg.Is<PaginationFilter<string>>(f => f.Filter == EmptyFilterJson));
        }

        [Fact]
        public async Task Index_CallsLogServiceWithMappedQuery()
        {
            // Arrange
            var apiResponse = BuildSuccessResponse(1);
            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), BuildMappedItems(1));
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            await _controller.Index();

            // Assert
            await _logService.Received(1).GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region LoadRecreateSummariesLogGrid

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2,
                PageSize = 20,
                Filter = EmptyFilterJson,
                SortBy = "DateDone",
                Descending = true
            };

            var apiResponse = BuildSuccessResponse(5);
            var mappedItems = BuildMappedItems(5);

            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), mappedItems);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal(PartialViewName, partialViewResult.ViewName);

            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Equal(mappedItems.Count, model.Data.Count());
            Assert.Equal("DateDone", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithEmptyResponse_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = EmptyFilterJson };
            var apiResponse = BuildEmptySuccessResponse();

            SetupQueryParametersMapper();
            SetupLogItemMapper([], []);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithFailedResponse_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = EmptyFilterJson };
            var apiResponse = BuildFailureResponse("Database timeout");

            SetupQueryParametersMapper();
            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_WithNullPagination_CreatesNewPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = EmptyFilterJson,
                SortBy = "Id",
                Descending = false
            };

            var apiResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>([], 0),
                Pagination = null
            };

            SetupQueryParametersMapper();
            SetupLogItemMapper([], []);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns((PaginationModel)null!);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
            Assert.Equal("Id", model.Pagination.SortColumn);
            Assert.False(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_PreservesSortingFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = EmptyFilterJson,
                SortBy = "Period",
                Descending = true
            };

            var apiResponse = BuildSuccessResponse(2);
            var mappedItems = BuildMappedItems(2);

            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), mappedItems);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Equal("Period", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_CallsMapperWithProvidedRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 3,
                PageSize = 50,
                Filter = EmptyFilterJson
            };

            var apiResponse = BuildSuccessResponse(1);
            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), BuildMappedItems(1));
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(
                Arg.Is<PaginationFilter<string>>(f =>
                    f.Page == 3 &&
                    f.PageSize == 50 &&
                    f.Filter == EmptyFilterJson));
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_MapsDataCollectionCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = EmptyFilterJson };
            var apiResponse = BuildSuccessResponse(4);
            var mappedItems = BuildMappedItems(4);

            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), mappedItems);
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            _mapper.Received(1).Map<List<RecreateSummaryLogItem>>(
                Arg.Is<IEnumerable<RecreateSummaryLogDto>>(list => list.Count() == 4));

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);
            Assert.Equal(4, model.Data.Count());
        }

        [Fact]
        public async Task LoadRecreateSummariesLogGrid_ConfiguresGridPropertiesCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = EmptyFilterJson };
            var apiResponse = BuildSuccessResponse(1);

            SetupQueryParametersMapper();
            SetupLogItemMapper(apiResponse.Data!.data.ToList(), BuildMappedItems(1));
            SetupPaginationMapper(apiResponse.Pagination);

            _logService.GetRecreateSummaryLogAsync(Arg.Any<QueryParameters<string>>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadRecreateSummariesLogGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<RecreateSummaryLogItem>>(partialViewResult.Model);

            Assert.Equal(GridId, model.GridId);
            Assert.Equal(BindGridUrl, model.BindGridUrl);
            Assert.False(model.ShowCheckboxColumn);
            Assert.False(model.AllowAdd);
            Assert.False(model.AllowEdit);
            Assert.False(model.AllowDelete);
            Assert.False(model.AllowExport);
            Assert.False(model.AllowRowSelection);
            Assert.True(model.ShowPagination);
            Assert.NotNull(model.Columns);
        }

        #endregion
    }
}
