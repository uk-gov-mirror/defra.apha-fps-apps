using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.ProjectDetailsControllerTest
{
    public class ProjectDetailsControllerTests
    {
        private readonly IMapper _mapperMock;
        private readonly IProjectListService _projectListServiceMock;
        private readonly IProjectDetailsService _projectDetailsServiceMock;
        private readonly IProjectCommentService _commentServiceMock;
        private readonly ProjectDetailsController _controller;

        public ProjectDetailsControllerTests()
        {
            _mapperMock = Substitute.For<IMapper>();
            _projectListServiceMock = Substitute.For<IProjectListService>();
            _projectDetailsServiceMock = Substitute.For<IProjectDetailsService>();
            _commentServiceMock = Substitute.For<IProjectCommentService>();
            _controller = new ProjectDetailsController(
                _mapperMock,
                _projectListServiceMock,
                _projectDetailsServiceMock,
                _commentServiceMock);
        }

        /// <summary>
        /// Sets up the common mocks required for BuildViewModelAsync (and the nested BuildCommentsGridAsync) to complete successfully.
        /// </summary>
        private void SetupSuccessfulIndexMocks(
            ProjectDto? fpsProject = null,
            ProposedProjectDto? proposedProject = null,
            List<ProjectsDto>? yearlyDetails = null,
            ProjectDetailDto? pimsDetail = null,
            List<ProjectListViewDto>? allProjects = null,
            List<RiskDto>? risks = null,
            List<CommentDto>? comments = null,
            PaginationDto? commentPagination = null,
            List<YearDto>? years = null)
        {
            _projectDetailsServiceMock.GetFpsProjectAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProjectDto> { Success = true, Data = fpsProject });

            _projectDetailsServiceMock.GetProposedProjectAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = proposedProject ?? new ProposedProjectDto() });

            _projectListServiceMock.GetYearlyDetailsByProjectAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<List<ProjectsDto>> { Success = true, Data = yearlyDetails ?? [] });

            _projectDetailsServiceMock.GetPimsDetailAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = true, Data = pimsDetail });

            _projectListServiceMock.GetAllProjectsListAsync()
                .Returns(new ApiResponseDto<List<ProjectListViewDto>> { Success = true, Data = allProjects ?? [] });

            _projectDetailsServiceMock.GetAllRiskAsync()
                .Returns(new ApiResponseDto<List<RiskDto>> { Success = true, Data = risks ?? [] });

            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = years ?? [] });

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = comments ?? [], Pagination = commentPagination });

            _mapperMock.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns(new List<ProjectCommentItem>());

            if (commentPagination != null)
            {
                _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                    .Returns(new PaginationModel());
            }
        }

        /// <summary>
        /// Sets up the common mocks required for BuildCommentsGridAsync to complete successfully.
        /// </summary>
        private void SetupSuccessfulCommentsGridMocks(
            List<CommentDto>? comments = null,
            PaginationDto? pagination = null)
        {
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = comments ?? [], Pagination = pagination });

            _mapperMock.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns(new List<ProjectCommentItem>());

            if (pagination != null)
            {
                _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                    .Returns(new PaginationModel());
            }
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsProjectDetailsViewModel()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_SetsParentprojectOnViewModel()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Index_CallsGetFpsProjectAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectDetailsServiceMock.Received(1).GetFpsProjectAsync("PP001");
        }

        [Fact]
        public async Task Index_CallsGetProposedProjectAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectDetailsServiceMock.Received(1).GetProposedProjectAsync("PP001");
        }

        [Fact]
        public async Task Index_CallsGetYearlyDetailsByProjectAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectListServiceMock.Received(1).GetYearlyDetailsByProjectAsync("PP001");
        }

        [Fact]
        public async Task Index_CallsGetPimsDetailAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectDetailsServiceMock.Received(1).GetPimsDetailAsync("PP001");
        }

        [Fact]
        public async Task Index_CallsGetAllProjectsListAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectListServiceMock.Received(1).GetAllProjectsListAsync();
        }

        [Fact]
        public async Task Index_CallsGetAllRiskAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectDetailsServiceMock.Received(1).GetAllRiskAsync();
        }

        [Fact]
        public async Task Index_CallsGetCommentsByProjectAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _commentServiceMock.Received(1).GetCommentsByProjectAsync(
                Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_CallsMapperToMapQueryParameters()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            _mapperMock.Received(1).Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>());
        }

        [Fact]
        public async Task Index_WithProposedProjectData_MapsProposedProjectDetailsToViewModel()
        {
            // Arrange
            var proposed = new ProposedProjectDto
            {
                Parentproject = "PP001",
                Projecttitle = "Test Title",
                Program = "Program A",
                Customer = "Customer A",
                Manager = "Manager A",
                Costbookno = "CB001",
                Disease = "Disease A"
            };
            SetupSuccessfulIndexMocks(proposedProject: proposed);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotNull(model.ProposedProjectDetails);
            Assert.Equal("Test Title", model.ProposedProjectDetails.Projecttitle);
            Assert.Equal("Program A", model.ProposedProjectDetails.Program);
            Assert.Equal("Customer A", model.ProposedProjectDetails.Customer);
            Assert.Equal("Manager A", model.ProposedProjectDetails.Manager);
        }

        [Fact]
        public async Task Index_WithPimsDetailData_MapsProjectDetailsToViewModel()
        {
            // Arrange
            var pimsDetail = new ProjectDetailDto { Parentproject = "PP001", Version = "2.0", Riskid = 3 };
            SetupSuccessfulIndexMocks(pimsDetail: pimsDetail);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectDetails);
            Assert.Equal("2.0", model.ProjectDetails.Version);
            Assert.Equal(3, model.ProjectDetails.Riskid);
        }

        [Fact]
        public async Task Index_WithRisks_PopulatesRiskRatingOptions()
        {
            // Arrange
            var risks = new List<RiskDto>
            {
                new RiskDto { Riskid = 1, Riskrating = "Low" },
                new RiskDto { Riskid = 2, Riskrating = "High" }
            };
            SetupSuccessfulIndexMocks(risks: risks);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotEmpty(model.RiskRatingOptions);
            Assert.Equal(3, model.RiskRatingOptions.Count);
            Assert.Equal("-- Select --", model.RiskRatingOptions[0].Text);
        }

        [Fact]
        public async Task Index_WithNullRisks_ReturnsEmptyRiskRatingOptions()
        {
            // Arrange
            SetupSuccessfulIndexMocks(risks: null);
            _projectDetailsServiceMock.GetAllRiskAsync()
                .Returns(new ApiResponseDto<List<RiskDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Single(model.RiskRatingOptions);
            Assert.Equal("-- Select --", model.RiskRatingOptions[0].Text);
        }

        [Fact]
        public async Task Index_WithAllProjects_PopulatesTransferToOptions()
        {
            // Arrange
            var allProjects = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP002" },
                new ProjectListViewDto { Parentproject = "PP003" }
            };
            SetupSuccessfulIndexMocks(allProjects: allProjects);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotEmpty(model.TransferToOptions);
            Assert.Equal(3, model.TransferToOptions.Count);
            Assert.Equal("-- Select --", model.TransferToOptions[0].Text);
        }

        [Fact]
        public async Task Index_WithNullAllProjects_ReturnsEmptyTransferToOptions()
        {
            // Arrange
            SetupSuccessfulIndexMocks();
            _projectListServiceMock.GetAllProjectsListAsync()
                .Returns(new ApiResponseDto<List<ProjectListViewDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Single(model.TransferToOptions);
            Assert.Equal("-- Select --", model.TransferToOptions[0].Text);
        }

        [Fact]
        public async Task Index_YearOptionsArePopulated()
        {
            // Arrange
            var years = new List<YearDto>
            {
                new YearDto { Value = 2023 },
                new YearDto { Value = 2024 }
            };
            SetupSuccessfulIndexMocks(years: years);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotEmpty(model.YearOptions);
            Assert.Equal(2, model.YearOptions.Count);
        }

        [Fact]
        public async Task Index_CallsGetAllYearAsync_Once()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            await _controller.Index("PP001");

            // Assert
            await _projectDetailsServiceMock.Received(1).GetAllYearAsync();
        }

        [Fact]
        public async Task Index_WithYears_PopulatesYearOptionsInDescendingOrder()
        {
            // Arrange
            var years = new List<YearDto>
            {
                new YearDto { Value = 2020 },
                new YearDto { Value = 2024 },
                new YearDto { Value = 2022 }
            };
            SetupSuccessfulIndexMocks(years: years);

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal(3, model.YearOptions.Count);
            Assert.Equal("2024", model.YearOptions[0].Value);
            Assert.Equal("2022", model.YearOptions[1].Value);
            Assert.Equal("2020", model.YearOptions[2].Value);
        }

        [Fact]
        public async Task Index_WithNullYears_ReturnsEmptyYearOptions()
        {
            // Arrange
            SetupSuccessfulIndexMocks(years: null);
            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Empty(model.YearOptions);
        }

        [Fact]
        public async Task Index_WithNullCommentPagination_DoesNotCallMapperForPaginationModel()
        {
            // Arrange
            SetupSuccessfulIndexMocks(commentPagination: null);

            // Act
            await _controller.Index("PP001");

            // Assert
            _mapperMock.DidNotReceive().Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        [Fact]
        public async Task Index_WithCommentPagination_CallsMapperForPaginationModel()
        {
            // Arrange
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 5 };
            SetupSuccessfulIndexMocks(commentPagination: paginationDto);

            // Act
            await _controller.Index("PP001");

            // Assert
            _mapperMock.Received(1).Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        [Fact]
        public async Task Index_CommentsGridHasCorrectGridId()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("projectCommentsGrid", model.CommentsGrid.GridId);
        }

        [Fact]
        public async Task Index_CommentsGridHasCorrectBindUrl()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("/PIMS/ProjectDetails/LoadCommentsGrid", model.CommentsGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_CommentsGridShowPaginationIsTrue()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.True(model.CommentsGrid.ShowPagination);
        }

        [Fact]
        public async Task Index_CommentsGridShowCheckboxColumnIsFalse()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.False(model.CommentsGrid.ShowCheckboxColumn);
        }

        [Fact]
        public async Task Index_CommentsGridKeyPropertyIsCommentNo()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("CommentNo", model.CommentsGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_CommentsGridEditFunctionIsEditComment()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("editComment", model.CommentsGrid.EditFunction);
        }

        [Fact]
        public async Task Index_CommentsGridDeleteFunctionIsDeleteComment()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("deleteComment", model.CommentsGrid.DeleteFunction);
        }


        [Fact]
        public async Task Index_CommentsGridExtraFilterMethodIsCorrect()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Equal("getProjectDetailsExtraFilters", model.CommentsGrid.ExtraFilterMethod);
        }

        [Fact]
        public async Task Index_CommentsGridColumnsArePopulated()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.NotNull(model.CommentsGrid.Columns);
            Assert.NotEmpty(model.CommentsGrid.Columns);
        }

        [Fact]
        public async Task Index_CommentsGridDefaultSortColumnIsNull()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.Null(model.CommentsGrid.Pagination.SortColumn);
        }

        [Fact]
        public async Task Index_CommentsGridDefaultSortDirectionIsFalse()
        {
            // Arrange
            SetupSuccessfulIndexMocks();

            // Act
            var result = await _controller.Index("PP001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectDetailsViewModel>(viewResult.Model);
            Assert.False(model.CommentsGrid.Pagination.SortDirection);
        }

        #endregion

        #region LoadCommentsGrid Tests

        [Fact]
        public async Task LoadCommentsGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithInvalidModelState_JsonContainsSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Invalid filter");
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithInvalidModelState_DoesNotCallService()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Invalid");
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            await _commentServiceMock.DidNotReceive()
                .GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_WithValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithValidRequest_ReturnsDataGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithValidRequest_ReturnsDataGridConfigModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithValidRequest_CallsGetCommentsByProjectAsync()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            await _commentServiceMock.Received(1)
                .GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_WithYear_PassesYearToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            await _controller.LoadCommentsGrid("PP001", 2023, request);

            // Assert
            await _commentServiceMock.Received(1)
                .GetCommentsByProjectAsync("PP001", 2023, Arg.Any<string?>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_WithSuccessfulData_PopulatesGridItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var commentData = new List<CommentDto>
            {
                new CommentDto { CommentNo = 1, Project = "PP001", Topic = "General Comment", Comment = "Test comment" }
            };
            var mappedItems = new List<ProjectCommentItem>
            {
                new ProjectCommentItem { CommentNo = 1, Topic = "General Comment", Comment = "Test comment" }
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = commentData, Pagination = null });
            _mapperMock.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns(mappedItems);

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Single(model.Data);
            Assert.Equal(1, model.Data[0].CommentNo);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithNullData_ReturnsEmptyDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = null, Pagination = null });

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithNullData_DoesNotCallMapperForCommentItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = null, Pagination = null });

            // Act
            await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            _mapperMock.DidNotReceive().Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>());
        }

        [Fact]
        public async Task LoadCommentsGrid_GridIdIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Equal("projectCommentsGrid", model.GridId);
        }

        [Fact]
        public async Task LoadCommentsGrid_TitleIsEmpty()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Equal(string.Empty, model.Title);
        }

        [Fact]
        public async Task LoadCommentsGrid_ShowCheckboxColumnIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.False(model.ShowCheckboxColumn);
        }

        [Fact]
        public async Task LoadCommentsGrid_ShowPaginationIsTrue()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.True(model.ShowPagination);
        }

        [Fact]
        public async Task LoadCommentsGrid_KeyPropertyIsCommentNo()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Equal("CommentNo", model.KeyProperty);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithFilterValues_PopulatesCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{\"Topic\":\"General Comment\"}",
                Page = 1,
                PageSize = 10
            };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
            Assert.True(model.CurrentFilters.ContainsKey("Topic"));
            Assert.Equal("General Comment", model.CurrentFilters["Topic"]);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithEmptyFilter_ReturnsEmptyCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
            Assert.Empty(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null, Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadCommentsGrid_SetsSortColumnFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                Page = 1,
                PageSize = 10,
                SortBy = "Year"
            };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.Equal("Year", model.Pagination.SortColumn);
        }

        [Fact]
        public async Task LoadCommentsGrid_SetsSortDirectionFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                Page = 1,
                PageSize = 10,
                Descending = true
            };
            SetupSuccessfulCommentsGridMocks();

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithPaginationData_MapsPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 2, PageSize = 20 };
            var paginationDto = new PaginationDto { PageNumber = 2, PageSize = 20, TotalRecords = 50 };
            var paginationModel = new PaginationModel { PageNumber = 2, PageSize = 20, TotalRecords = 50 };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _commentServiceMock.GetCommentsByProjectAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<string?>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<CommentDto>> { Success = true, Data = [], Pagination = paginationDto });
            _mapperMock.Map<List<ProjectCommentItem>>(Arg.Any<List<CommentDto>>())
                .Returns(new List<ProjectCommentItem>());
            _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(paginationModel);

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            _mapperMock.Received(1).Map<PaginationModel>(Arg.Any<PaginationDto>());
            Assert.Equal(50, model.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadCommentsGrid_WithNullPagination_UsesDefaultPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulCommentsGridMocks(pagination: null);

            // Act
            var result = await _controller.LoadCommentsGrid("PP001", null, request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectCommentItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
            _mapperMock.DidNotReceive().Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        #endregion

        #region SavePimsDetail Tests

        [Fact]
        public async Task SavePimsDetail_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new ProjectDetailDto { Version = "1.0" };
            _projectDetailsServiceMock.SavePimsDetailAsync(Arg.Any<string>(), Arg.Any<ProjectDetailDto>())
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.SavePimsDetail("PP001", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task SavePimsDetail_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new ProjectDetailDto { Version = "1.0" };
            _projectDetailsServiceMock.SavePimsDetailAsync(Arg.Any<string>(), Arg.Any<ProjectDetailDto>())
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Save failed", Code = "ERROR" } }
                });

            // Act
            var result = await _controller.SavePimsDetail("PP001", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task SavePimsDetail_SetsParentprojectOnDto()
        {
            // Arrange
            var dto = new ProjectDetailDto();
            _projectDetailsServiceMock.SavePimsDetailAsync(Arg.Any<string>(), Arg.Any<ProjectDetailDto>())
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = true, Data = dto });

            // Act
            await _controller.SavePimsDetail("PP001", dto);

            // Assert
            Assert.Equal("PP001", dto.Parentproject);
        }

        [Fact]
        public async Task SavePimsDetail_CallsServiceOnce()
        {
            // Arrange
            var dto = new ProjectDetailDto();
            _projectDetailsServiceMock.SavePimsDetailAsync(Arg.Any<string>(), Arg.Any<ProjectDetailDto>())
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = true, Data = dto });

            // Act
            await _controller.SavePimsDetail("PP001", dto);

            // Assert
            await _projectDetailsServiceMock.Received(1).SavePimsDetailAsync("PP001", dto);
        }

        #endregion

        #region UpdateProposedProject Tests

        [Fact]
        public async Task UpdateProposedProject_WhenServiceReturnsSuccess_ReturnsRedirectToActionResult()
        {
            // Arrange
            var dto = new ProposedProjectDto { Projecttitle = "Updated Title" };
            var viewModel = new ProjectDetailsViewModel { ProposedProjectDetails = dto };
            _projectDetailsServiceMock.UpdateProposedProjectAsync(Arg.Any<string>(), Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.UpdateProposedProject("PP001", viewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task UpdateProposedProject_WhenProposedProjectDetailsIsNull_ReturnsRedirectToIndex()
        {
            // Arrange
            var viewModel = new ProjectDetailsViewModel { ProposedProjectDetails = null };

            // Act
            var result = await _controller.UpdateProposedProject("PP001", viewModel);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Index), redirectResult.ActionName);
            Assert.Equal("PP001", redirectResult.RouteValues?["parentproject"]);
        }

        [Fact]
        public async Task UpdateProposedProject_SetsParentprojectOnDto()
        {
            // Arrange
            var dto = new ProposedProjectDto();
            var viewModel = new ProjectDetailsViewModel { ProposedProjectDetails = dto };
            _projectDetailsServiceMock.UpdateProposedProjectAsync(Arg.Any<string>(), Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _controller.UpdateProposedProject("PP001", viewModel);

            // Assert
            Assert.Equal("PP001", dto.Parentproject);
        }

        [Fact]
        public async Task UpdateProposedProject_CallsServiceOnce()
        {
            // Arrange
            var dto = new ProposedProjectDto();
            var viewModel = new ProjectDetailsViewModel { ProposedProjectDetails = dto };
            _projectDetailsServiceMock.UpdateProposedProjectAsync(Arg.Any<string>(), Arg.Any<ProposedProjectDto>())
                .Returns(new ApiResponseDto<ProposedProjectDto> { Success = true, Data = dto });

            // Act
            await _controller.UpdateProposedProject("PP001", viewModel);

            // Assert
            await _projectDetailsServiceMock.Received(1).UpdateProposedProjectAsync("PP001", dto);
        }

        #endregion

        #region GetComment Tests

        [Fact]
        public async Task GetComment_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var commentDto = new CommentDto { CommentNo = 1, Comment = "Test comment" };
            _commentServiceMock.GetByIdAsync(1)
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = commentDto });

            // Act
            var result = await _controller.GetComment(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task GetComment_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(Arg.Any<int>())
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } }
                });

            // Act
            var result = await _controller.GetComment(999);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task GetComment_CallsGetByIdAsync_WithCorrectCommentNo()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(42)
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = new CommentDto { CommentNo = 42 } });

            // Act
            await _controller.GetComment(42);

            // Assert
            await _commentServiceMock.Received(1).GetByIdAsync(42);
        }

        #endregion

        #region CreateComment Tests

        [Fact]
        public async Task CreateComment_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Comment = "New comment" };
            _commentServiceMock.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task CreateComment_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Comment = "New comment" };
            _commentServiceMock.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Create failed", Code = "ERROR" } }
                });

            // Act
            var result = await _controller.CreateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task CreateComment_TrimsCommentText()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Comment = "  Trimmed comment  " };
            _commentServiceMock.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = dto });

            // Act
            await _controller.CreateComment(dto);

            // Assert
            Assert.Equal("Trimmed comment", dto.CommentText);
        }

        [Fact]
        public async Task CreateComment_CallsCreateCommentAsync_Once()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Comment = "New comment" };
            _commentServiceMock.CreateCommentAsync(Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = dto });

            // Act
            await _controller.CreateComment(dto);

            // Assert
            await _commentServiceMock.Received(1).CreateCommentAsync(dto);
        }

        #endregion

        #region UpdateComment Tests

        [Fact]
        public async Task UpdateComment_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 1, Comment = "Updated comment" };
            _commentServiceMock.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.UpdateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task UpdateComment_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 1, Comment = "Updated comment" };
            _commentServiceMock.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "ERROR" } }
                });

            // Act
            var result = await _controller.UpdateComment(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task UpdateComment_CallsUpdateCommentAsync_WithCorrectCommentNo()
        {
            // Arrange
            var dto = new CommentDto { CommentNo = 5, Comment = "Updated comment" };
            _commentServiceMock.UpdateCommentAsync(Arg.Any<int>(), Arg.Any<CommentDto>())
                .Returns(new ApiResponseDto<CommentDto> { Success = true, Data = dto });

            // Act
            await _controller.UpdateComment(dto);

            // Assert
            await _commentServiceMock.Received(1).UpdateCommentAsync(5, dto);
        }

        #endregion

        #region DeleteComment Tests

        [Fact]
        public async Task DeleteComment_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _commentServiceMock.DeleteCommentAsync(Arg.Any<int>())
                .Returns(new ApiResponseDto<bool> { Success = true, Data = true });

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task DeleteComment_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _commentServiceMock.DeleteCommentAsync(Arg.Any<int>())
                .Returns(new ApiResponseDto<bool>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "ERROR" } }
                });

            // Act
            var result = await _controller.DeleteComment(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task DeleteComment_CallsDeleteCommentAsync_WithCorrectCommentNo()
        {
            // Arrange
            _commentServiceMock.DeleteCommentAsync(Arg.Any<int>())
                .Returns(new ApiResponseDto<bool> { Success = true, Data = true });

            // Act
            await _controller.DeleteComment(7);

            // Assert
            await _commentServiceMock.Received(1).DeleteCommentAsync(7);
        }

        #endregion

        #region GetAddEditCommentPartial Tests

        [Fact]
        public async Task GetAddEditCommentPartial_WithNullCommentNo_ReturnsPartialViewResult()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, 2024);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithNullCommentNo_ReturnsAddEditCommentPartialView()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditComment", partialViewResult.ViewName);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithNullCommentNo_SetsIsAddingNewTrue()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.True(model.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithZeroCommentNo_SetsIsAddingNewTrue()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", 0, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.True(model.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithNullCommentNo_DoesNotCallGetByIdAsync()
        {
            // Arrange & Act
            await _controller.GetAddEditCommentPartial("PP001", null, 2024);

            // Assert
            await _commentServiceMock.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithZeroCommentNo_DoesNotCallGetByIdAsync()
        {
            // Arrange & Act
            await _controller.GetAddEditCommentPartial("PP001", 0, 2024);

            // Assert
            await _commentServiceMock.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithValidCommentNo_CallsGetByIdAsync()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(5)
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Data = new CommentDto { CommentNo = 5, Year = 2024, Topic = "General Comment", CommentText = "Test" }
                });

            // Act
            await _controller.GetAddEditCommentPartial("PP001", 5, 2024);

            // Assert
            await _commentServiceMock.Received(1).GetByIdAsync(5);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithValidCommentNo_SetsIsAddingNewFalse()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(5)
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Data = new CommentDto { CommentNo = 5, Year = 2024, Topic = "General Comment", CommentText = "Test" }
                });

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", 5, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.False(model.IsAddingNew);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithValidCommentNo_MapsCommentDataToModel()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(5)
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = true,
                    Data = new CommentDto { CommentNo = 5, Year = 2023, Topic = "Contracts", CommentText = "Some text" }
                });

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", 5, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.Equal(5, model.CommentNo);
            Assert.Equal(2023, model.Year);
            Assert.Equal("Contracts", model.Topic);
            Assert.Equal("Some text", model.CommentText);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_WithValidCommentNo_ServiceFailure_DoesNotMapCommentData()
        {
            // Arrange
            _commentServiceMock.GetByIdAsync(5)
                .Returns(new ApiResponseDto<CommentDto>
                {
                    Success = false,
                    Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } }
                });

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", 5, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.Equal(0, model.CommentNo);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_SetsProjectOnModel()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, 2024);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.Equal("PP001", model.Project);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_SetsSelectedYearOnModel()
        {
            // Arrange & Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, 2023);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.Equal(2023, model.Year);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_PopulatesYearOptions()
        {
            // Arrange
            var years = new List<YearDto>
            {
                new() { Value = 2023 },
                new() { Value = 2024 },
                new() { Value = 2025 }
            };
            _projectDetailsServiceMock.GetAllYearAsync()
                .Returns(new ApiResponseDto<List<YearDto>> { Success = true, Data = years });

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.NotEmpty(model.YearOptions);
        }

        [Fact]
        public async Task GetAddEditCommentPartial_PopulatesTopicOptions()
        {
            // Arrange
            var topics = new List<CommentTopicDto>
            {
                new() { Topic = "Benchmarking" },
                new() { Topic = "Contracts" },
                new() { Topic = "Finance" },
                new() { Topic = "Funding" },
                new() { Topic = "Performance" },
                new() { Topic = "Policy" }
            };
            _commentServiceMock.GetCommentTopicsAsync()
                .Returns(new ApiResponseDto<List<CommentTopicDto>> { Success = true, Data = topics });

            // Act
            var result = await _controller.GetAddEditCommentPartial("PP001", null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AddEditCommentViewModel>(partialViewResult.Model);
            Assert.NotEmpty(model.TopicOptions);
            Assert.Equal(7, model.TopicOptions.Count);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            // Arrange & Act
            var controller = new ProjectDetailsController(
                _mapperMock,
                _projectListServiceMock,
                _projectDetailsServiceMock,
                _commentServiceMock);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion
    }
}
