using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectSpecificQueryControllerTest
{
    public class ProjectSpecificQueryControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ProjectSpecificQueryController _controller;

        public ProjectSpecificQueryControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectSpecificQueryController(_mapper, _projectService);
        }

        private void SetupPagedData(List<ProjectSpecificQueryDto>? data = null, PaginationDto? pagination = null)
        {
            data ??= new List<ProjectSpecificQueryDto>();
            pagination ??= new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = data.Count };

            var apiResponse = ApiResponseDto<List<ProjectSpecificQueryDto>>.SuccessResponse(data, pagination);
            _projectService.GetPagedProjectSpecificQueryAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<ProjectSpecificQueryItem>>(Arg.Any<List<ProjectSpecificQueryDto>>())
                   .Returns(data.Select(p => new ProjectSpecificQueryItem { ParentProject = p.ParentProject }).ToList());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        [Fact]
        public void Constructor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectSpecificQueryController(null!, _projectService));
        }

        [Fact]
        public void Constructor_NullProjectService_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectSpecificQueryController(_mapper, null!));
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithGrid()
        {
            var data = new List<ProjectSpecificQueryDto>
            {
                new() { ParentProject = "P001", Account = "A1" },
                new() { ParentProject = "P002", Account = "A2" }
            };
            SetupPagedData(data);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectSpecificQueryViewModel>(viewResult.Model);
            Assert.Equal("projectSpecificQueryGrid", model.ProjectSpecificQueryGrid.GridId);
            Assert.Equal("Project Specifics Query", model.ProjectSpecificQueryGrid.Title);
            Assert.False(model.ProjectSpecificQueryGrid.AllowAdd);
            Assert.False(model.ProjectSpecificQueryGrid.AllowEdit);
            Assert.False(model.ProjectSpecificQueryGrid.AllowDelete);
            Assert.True(model.ProjectSpecificQueryGrid.ShowPagination);
        }

        [Fact]
        public async Task Index_WhenApiFails_ReturnsEmptyGrid()
        {
            var apiResponse = ApiResponseDto<List<ProjectSpecificQueryDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());
            _projectService.GetPagedProjectSpecificQueryAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectSpecificQueryViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectSpecificQueryGrid.Data);
        }

        [Fact]
        public async Task LoadProjectSpecificQueryGrid_ValidRequest_ReturnsPartialView()
        {
            SetupPagedData();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            var request = new PaginationFilter<string>
            {
                Filter = JsonConvert.SerializeObject(new Dictionary<string, string> { { "Program", "PR1" } })
            };

            var result = await _controller.LoadProjectSpecificQueryGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var config = Assert.IsType<DataGridConfig<ProjectSpecificQueryItem>>(partial.Model);
            Assert.NotNull(config.CurrentFilters);
        }

        [Fact]
        public async Task LoadProjectSpecificQueryGrid_NoFilter_ReturnsPartialView()
        {
            SetupPagedData();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());

            var request = new PaginationFilter<string>();

            var result = await _controller.LoadProjectSpecificQueryGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<ProjectSpecificQueryItem>>(partial.Model);
            Assert.Null(config.CurrentFilters);
        }

        [Fact]
        public async Task LoadProjectSpecificQueryGrid_InvalidModelState_ReturnsJsonError()
        {
            _controller.ModelState.AddModelError("Filter", "Invalid");

            var result = await _controller.LoadProjectSpecificQueryGrid(new PaginationFilter<string>());

            Assert.IsType<JsonResult>(result);
        }
    }
}
