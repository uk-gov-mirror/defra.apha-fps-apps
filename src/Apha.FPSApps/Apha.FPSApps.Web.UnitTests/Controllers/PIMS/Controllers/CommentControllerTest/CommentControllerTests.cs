using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.CommentControllerTest
{
    public class CommentControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectCommentService _commentService;
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _commentService = Substitute.For<IProjectCommentService>();
            _projectListService = Substitute.For<IProjectListService>();
            _projectDetailsService = Substitute.For<IProjectDetailsService>();

            _controller = new CommentController(
                _mapper,
                _commentService,
                _projectListService,
                _projectDetailsService);

            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ── JSON helper ───────────────────────────────────────────────────────

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupDropdownMocks(
            List<ProjectListViewDto>? projects = null,
            List<CommentTopicDto>? topics = null,
            List<YearDto>? years = null)
        {
            _projectListService.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 1)
                .Returns(ApiResponseDto<List<ProjectListViewDto>>.SuccessResponse(projects ?? []));
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse(topics ?? []));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse(years ?? []));
        }

        private void SetupGridMapper(List<CommentDto>? comments = null, PaginationDto? pagination = null)
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns(comments?.Select(c => new ProjectCommentItem { CommentNo = c.CommentNo }).ToList() ?? []);
            if (pagination != null)
            {
                _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                    .Returns(new PaginationModel());
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        #region Index Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResult()
        {
            // Arrange
            SetupDropdownMocks(
                projects: [new ProjectListViewDto { Parentproject = "PP001" }],
                topics: [new CommentTopicDto { Topic = "Budget" }],
                years: [new YearDto { Value = 2024 }]);

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsCommentViewModel()
        {
            // Arrange
            SetupDropdownMocks(
                projects: [new ProjectListViewDto { Parentproject = "PP001" }],
                topics: [new CommentTopicDto { Topic = "Risk" }],
                years: [new YearDto { Value = 2025 }]);

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<CommentViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ServiceReturnsProjects_ViewModelHasProjectOptions()
        {
            // Arrange
            SetupDropdownMocks(
                projects: [new ProjectListViewDto { Parentproject = "PP001" }, new ProjectListViewDto { Parentproject = "PP002" }]);

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            // 2 projects + 1 placeholder = 3
            Assert.Equal(3, model.ProjectOptions.Count);
        }

        [Fact]
        public async Task Index_ServiceReturnsTopics_ViewModelHasTopicOptions()
        {
            // Arrange
            SetupDropdownMocks(topics: [new CommentTopicDto { Topic = "Budget" }, new CommentTopicDto { Topic = "Risk" }]);

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            // 2 topics + 1 placeholder = 3
            Assert.Equal(3, model.TopicOptions.Count);
        }

        [Fact]
        public async Task Index_ServiceReturnsYears_ViewModelHasYearOptions()
        {
            // Arrange
            SetupDropdownMocks(years: [new YearDto { Value = 2024 }, new YearDto { Value = 2025 }]);

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            Assert.Equal(2, model.YearOptions.Count);
        }

        [Fact]
        public async Task Index_Always_CommentsGridIsExplicitlyBuilt()
        {
            // Arrange
            SetupDropdownMocks();

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            Assert.NotNull(model.CommentsGrid);
            Assert.Equal("comments", model.CommentsGrid.GridId);
            Assert.Equal("/PIMS/Comment/LoadCommentsGrid", model.CommentsGrid.BindGridUrl);
            Assert.True(model.CommentsGrid.AllowAdd);
            Assert.True(model.CommentsGrid.AllowEdit);
            Assert.True(model.CommentsGrid.AllowDelete);
        }

        [Fact]
        public async Task Index_ServiceReturnsEmpty_ViewModelHasPlaceholderDropdowns()
        {
            // Arrange
            SetupDropdownMocks();

            // Act
            var result = await _controller.Index(parentproject: null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CommentViewModel>(viewResult.Model);
            // Only placeholder items
            Assert.Single(model.ProjectOptions);
            Assert.Single(model.TopicOptions);
            Assert.Empty(model.YearOptions);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region LoadCommentsGrid Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadCommentsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Invalid page");
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadCommentsGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadCommentsGrid_EmptyProject_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadCommentsGrid(request, project: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadCommentsGrid_EmptyProject_DoesNotCallCommentService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            await _controller.LoadCommentsGrid(request, project: "");

            // Assert
            await _commentService.DidNotReceive().GetCommentsByProjectAsync(
                Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_ValidProject_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var comments = new List<CommentDto>
            {
                new CommentDto { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Budget", Comment = "Test comment" }
            };
            SetupGridMapper(comments: comments);
            _commentService.GetCommentsByProjectAsync("PP001", Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = comments });

            // Act
            var result = await _controller.LoadCommentsGrid(request, project: "PP001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialResult.Model);
            Assert.NotNull(gridConfig);
        }

        [Fact]
        public async Task LoadCommentsGrid_ValidProject_GridHasCorrectBindUrl()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupGridMapper();
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [] });

            // Act
            var result = await _controller.LoadCommentsGrid(request, project: "PP001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialResult.Model);
            Assert.Equal("/PIMS/Comment/LoadCommentsGrid", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task LoadCommentsGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupGridMapper();
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [] });

            // Act
            var result = await _controller.LoadCommentsGrid(request, project: "PP001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithTopicFilter_PassesTopicToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupGridMapper();
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [] });

            // Act
            await _controller.LoadCommentsGrid(request, project: "PP001", topic: "Budget");

            // Assert
            await _commentService.Received(1).GetCommentsByProjectAsync(
                "PP001", Arg.Any<int?>(), "Budget", Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_WithEmptyTopicFilter_PassesNullTopicToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupGridMapper();
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [] });

            // Act
            await _controller.LoadCommentsGrid(request, project: "PP001", topic: "");

            // Assert
            
            await _commentService.Received(1).GetCommentsByProjectAsync(
                "PP001", Arg.Any<int?>(), null, Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_WithYearFilter_ParsesAndPassesYearToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            SetupGridMapper();
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [] });

            // Act
            await _controller.LoadCommentsGrid(request, project: "PP001", year: "2024");

            // Assert
            await _commentService.Received(1).GetCommentsByProjectAsync(
                "PP001", 2024, Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_NullData_ReturnsGridWithEmptyList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns([]);
            _commentService.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = false, Data = null });

            // Act
            var result = await _controller.LoadCommentsGrid(request, project: "PP001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialResult.Model);
            Assert.NotNull(gridConfig.Data);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region GetAddEditCommentPartial Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAddEditCommentPartial_AddMode_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: null, selectedYear: 2024);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditComment", partialResult.ViewName);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            Assert.True(model.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_AddMode_SetsProjectFromParam()
        {
            // Arrange
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: null, selectedYear: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            Assert.Equal("PP001", model.Project);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_AddMode_SetsSelectedYear()
        {
            // Arrange
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: null, selectedYear: 2025);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            Assert.Equal(2025, model.Year);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_EditMode_CallsGetByIdAsync()
        {
            // Arrange
            var commentNo = 42;
            var commentDto = new CommentDto { CommentNo = commentNo, Year = 2024, Topic = "Budget", CommentText = "Test" };
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(commentDto));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: commentNo, selectedYear: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            await _commentService.Received(1).GetByIdAsync(commentNo);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_EditMode_PopulatesModelFromGetById()
        {
            // Arrange
            var commentNo = 42;
            var commentDto = new CommentDto { CommentNo = commentNo, Year = 2024, Topic = "Risk", CommentText = "Existing comment" };
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(commentDto));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: commentNo, selectedYear: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            Assert.Equal(commentNo, model.CommentNo);
            Assert.Equal(2024, model.Year);
            Assert.Equal("Risk", model.Topic);
            Assert.Equal("Existing comment", model.CommentText);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_EditMode_IsAddingNewIsFalse()
        {
            // Arrange
            var commentNo = 5;
            var commentDto = new CommentDto { CommentNo = commentNo };
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(commentDto));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: commentNo, selectedYear: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            Assert.False(model.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_TopicsAvailable_PopulatesTopicOptions()
        {
            // Arrange
            _commentService.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse(
                    [new CommentTopicDto { Topic = "Budget" }, new CommentTopicDto { Topic = "Risk" }]));
            _projectDetailsService.GetAllYearAsync()
                .Returns(ApiResponseDto<List<YearDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", commentNo: null, selectedYear: null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialResult.Model);
            // 2 topics + 1 placeholder = 3
            Assert.Equal(3, model.TopicOptions.Count);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region GetComment Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetComment_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var commentNo = 1;
            var commentDto = new CommentDto { CommentNo = commentNo, Topic = "Budget", Comment = "Test" };
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(commentDto));

            // Act
            var result = await _controller.GetComment(commentNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetComment_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var commentNo = 999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "NOT_FOUND", Message = "Not found" } };
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetComment(commentNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetComment_ServiceReturnsData_JsonContainsData()
        {
            // Arrange
            var commentNo = 7;
            var commentDto = new CommentDto { CommentNo = commentNo, Topic = "Risk" };
            _commentService.GetByIdAsync(commentNo)
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(commentDto));

            // Act
            var result = await _controller.GetComment(commentNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.TryGetProperty("data", out _));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region CreateComment Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateComment_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.CreateComment(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateComment_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = "Budget", Comment = "New comment" };
            _commentService.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateComment_ValidDto_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = "Budget" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "CREATE_ERROR", Message = "Failed" } };
            _commentService.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateComment_ValidDto_CallsCreateCommentAsync()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = "Budget", Comment = "Comment body" };
            _commentService.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(dto));

            // Act
            await _controller.CreateComment(dto);

            // Assert
            await _commentService.Received(1).CreateCommentAsync(Arg.Any<CommentDto>());
        }

        [Fact]
        public async Task CreateComment_ValidDto_SetsMadeByFromIdentity()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = "Budget" };
            CommentDto? captured = null;
            _commentService.CreateCommentAsync(Arg.Do<CommentDto>(d => captured = d))
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(dto));

            // Act
            await _controller.CreateComment(dto);

            // Assert
            
            Assert.NotNull(captured);
            Assert.NotNull(captured.MadeBy); // could be empty string, but must not be null
        }

        [Fact]
        public async Task CreateComment_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto();
            _controller.ModelState.AddModelError("Project", "Project is required");

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region UpdateComment Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateComment_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.UpdateComment(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateComment_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Risk" };
            _commentService.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.UpdateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateComment_ValidDto_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 1, Project = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "UPDATE_ERROR", Message = "Failed" } };
            _commentService.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.UpdateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task UpdateComment_ValidDto_CallsUpdateCommentAsyncWithCommentNo()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 7, Project = "PP001" };
            _commentService.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(ApiResponseDto<CommentDto>.SuccessResponse(dto));

            // Act
            await _controller.UpdateComment(dto);

            // Assert
            await _commentService.Received(1).UpdateCommentAsync(7, Arg.Any<CommentDto>());
        }

        [Fact]
        public async Task UpdateComment_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 1 };
            _controller.ModelState.AddModelError("Project", "Project is required");

            // Act
            var result = await _controller.UpdateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region DeleteComment Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteComment_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var commentNo = 3;
            _commentService.DeleteCommentAsync(commentNo)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteComment(commentNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteComment_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var commentNo = 999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "DELETE_ERROR", Message = "Failed" } };
            _commentService.DeleteCommentAsync(commentNo)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteComment(commentNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteComment_CallsDeleteCommentAsyncWithCorrectId()
        {
            // Arrange
            var commentNo = 15;
            _commentService.DeleteCommentAsync(commentNo)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.DeleteComment(commentNo);

            // Assert
            await _commentService.Received(1).DeleteCommentAsync(commentNo);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region GetForecastSpend Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetForecastSpend_ProjectIsEmpty_ReturnsSuccessWithNullAndSkipsService()
        {
            // Act
            var result = await _controller.GetForecastSpend(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(JsonValueKind.Null, element.GetProperty("forecastSpend").ValueKind);
            await _commentService.DidNotReceive().GetForecastSpendByProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetForecastSpend_ServiceReturnsSuccess_ReturnsForecastSpend()
        {
            // Arrange
            const string project = "PP001";
            _commentService.GetForecastSpendByProjectAsync(project)
                .Returns(ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(
                    new ProjectCommentForecastSpendDto { ForecastSpend = 3456.78 }));

            // Act
            var result = await _controller.GetForecastSpend(project);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(3456.78, element.GetProperty("forecastSpend").GetDouble());
            await _commentService.Received(1).GetForecastSpendByProjectAsync(project);
        }

        [Fact]
        public async Task GetForecastSpend_ServiceReturnsFailure_ReturnsSuccessFalseWithErrors()
        {
            // Arrange
            const string project = "PP001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "FORECAST_ERROR", Message = "Failed" } };
            _commentService.GetForecastSpendByProjectAsync(project)
                .Returns(ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetForecastSpend(project);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal(JsonValueKind.Array, element.GetProperty("errors").ValueKind);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region SaveForecastSpend Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task SaveForecastSpend_ProjectIsEmpty_ReturnsSuccessFalse()
        {
            // Act
            var result = await _controller.SaveForecastSpend(string.Empty, new ProjectCommentForecastSpendDto { ForecastSpend = 10 });

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            await _commentService.DidNotReceive().UpdateForecastSpendByProjectAsync(Arg.Any<string>(), Arg.Any<double?>());
        }

        [Fact]
        public async Task SaveForecastSpend_ServiceReturnsSuccess_ReturnsSuccessTrueWithForecastSpend()
        {
            // Arrange
            const string project = "PP001";
            var dto = new ProjectCommentForecastSpendDto { ForecastSpend = 1200.25 };
            _commentService.UpdateForecastSpendByProjectAsync(project, dto.ForecastSpend)
                .Returns(ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(
                    new ProjectCommentForecastSpendDto { ForecastSpend = dto.ForecastSpend }));

            // Act
            var result = await _controller.SaveForecastSpend(project, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(dto.ForecastSpend, element.GetProperty("forecastSpend").GetDouble());
            await _commentService.Received(1).UpdateForecastSpendByProjectAsync(project, dto.ForecastSpend);
        }

        [Fact]
        public async Task SaveForecastSpend_ServiceReturnsFailure_ReturnsSuccessFalse()
        {
            // Arrange
            const string project = "PP001";
            var dto = new ProjectCommentForecastSpendDto { ForecastSpend = 1200.25 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "SAVE_ERROR", Message = "Save failed" } };
            _commentService.UpdateForecastSpendByProjectAsync(project, dto.ForecastSpend)
                .Returns(ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveForecastSpend(project, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal(JsonValueKind.Array, element.GetProperty("errors").ValueKind);
        }

        #endregion
    }
}
