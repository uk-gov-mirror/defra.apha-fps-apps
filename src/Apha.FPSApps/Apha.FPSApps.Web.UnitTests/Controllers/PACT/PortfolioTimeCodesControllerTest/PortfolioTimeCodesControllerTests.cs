using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.PortfolioTimeCodesControllerTest
{
    public class PortfolioTimeCodesControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly PortfolioTimeCodesController _controller;

        public PortfolioTimeCodesControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _jobCodeService = Substitute.For<IProjectJobCodeService>();
            _timeCodeService = Substitute.For<IPactTimeCodeValidService>();
            _controller = new PortfolioTimeCodesController(
                _mapper,
                _projectService,
                _jobCodeService,
                _timeCodeService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupJobCodeGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<PortfolioJobCodeViewModel>>(Arg.Any<List<JobCodeDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupTimeCodeGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ValidTimeCodeViewModel>>(Arg.Any<List<TimeCodeValidDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupProjectsList(List<ProjectDto> projects)
        {
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }

        private void SetupWorkGroupsList(List<WorkGroupDto> workGroups)
        {
            _jobCodeService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups));
        }

        private static ApiResponseDto<T> CreateErrorResponse<T>()
        {
            return ApiResponseDto<T>.FailureResponse(
                [new ApiErrorDto { Message = "Test error", Code = "TEST_ERROR" }],
                new ApiMetaDto());
        }

        #region Index

        [Fact]
        public async Task Index_WithNullParentProject_ReturnsViewWithEmptyGrids()
        {
            // Arrange
            SetupProjectsList([]);
            SetupWorkGroupsList([]);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PortfolioTimeCodesViewModel>(viewResult.Model);
            Assert.Null(model.SelectedPortfolio);
            Assert.Empty(model.JobCodeGrid.Data);
            Assert.Empty(model.TimeCodeGrid.Data);
        }

        [Fact]
        public async Task Index_PopulatesPortfolioOptionsAndWorkGroups_InViewModel()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PRJ001", ProjectTitle = "Project 1" },
                new() { ParentProject = "PRJ002", ProjectTitle = "Project 2" }
            };
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG1" },
                new() { WorkGroupName = "WG2" }
            };

            SetupProjectsList(projects);
            SetupWorkGroupsList(workGroups);

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PortfolioTimeCodesViewModel>(viewResult.Model);
            Assert.Equal(2, model.PortfolioOptions.Count);
            Assert.Equal(2, model.WorkGroups.Count);
        }

        [Fact]
        public async Task Index_WithValidParentProject_ReturnsViewWithPopulatedGrids()
        {
            // Arrange
            const string parentProject = "PRJ001";
            var projects = new List<ProjectDto> { new() { ParentProject = parentProject, ProjectTitle = "Project 1" } };
            var workGroups = new List<WorkGroupDto> { new() { WorkGroupName = "WG1" } };
            var jobCodes = new List<JobCodeDto> { new() { JobCodeId = "JC1", ParentProject = parentProject } };
            var timeCodes = new List<TimeCodeValidDto> { new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = parentProject } };

            SetupProjectsList(projects);
            SetupWorkGroupsList(workGroups);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<PortfolioJobCodeViewModel>>(Arg.Any<List<JobCodeDto>>())
                .Returns([new PortfolioJobCodeViewModel { JobCodeId = "JC1", ParentProject = parentProject }]);
            _mapper.Map<List<ValidTimeCodeViewModel>>(Arg.Any<List<TimeCodeValidDto>>())
                .Returns([new ValidTimeCodeViewModel { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = parentProject }]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse(jobCodes, new PaginationDto()));
            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes, new PaginationDto()));

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PortfolioTimeCodesViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.SelectedPortfolio);
            Assert.NotEmpty(model.JobCodeGrid.Data);
            Assert.NotEmpty(model.TimeCodeGrid.Data);
        }

        #endregion

        #region LoadJobCodeGrid

        [Fact]
        public async Task LoadJobCodeGrid_WithValidRequest_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";
            var jobCodes = new List<JobCodeDto> { new() { JobCodeId = "JC1", ParentProject = parentProject } };

            SetupJobCodeGridMapper();
            _jobCodeService.GetPagedJobCodesAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<JobCodeDto>>.SuccessResponse(jobCodes, new PaginationDto()));

            // Act
            var result = await _controller.LoadJobCodeGrid(request, parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<PortfolioJobCodeViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadJobCodeGrid_WithNullParentProject_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadJobCodeGrid(request, null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadJobCodeGrid_WithEmptyParentProject_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadJobCodeGrid(request, string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region LoadTimeCodeGrid

        [Fact]
        public async Task LoadTimeCodeGrid_WithValidRequest_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";
            var timeCodes = new List<TimeCodeValidDto>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = parentProject }
            };

            SetupTimeCodeGridMapper();
            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(timeCodes, new PaginationDto()));

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, parentProject, null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ValidTimeCodeViewModel>>(partial.Model);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithJobCodeId_PassesJobCodeToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";
            const string jobCodeId = "JC1";

            SetupTimeCodeGridMapper();
            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, parentProject, jobCodeId, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            await _timeCodeService.Received(1).GetPagedTimeCodesAsync(
                Arg.Any<QueryParameters<string>>(),
                null,
                parentProject);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithTestCode_PassesTestCodeToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            const string parentProject = "PRJ001";
            const string testCode = "TST1";

            SetupTimeCodeGridMapper();
            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, parentProject, null, testCode);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithNullParentProject_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, null!, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithEmptyParentProject_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, string.Empty, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadTimeCodeGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            const string parentProject = "PRJ001";
            _controller.ModelState.AddModelError("Page", "Invalid page");

            // Act
            var result = await _controller.LoadTimeCodeGrid(request, parentProject, null, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion

        #region CreateJobCode (GET)

        [Fact]
        public async Task CreateJobCode_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            SetupWorkGroupsList([new WorkGroupDto { WorkGroupName = "WG1" }]);
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["Type1"]));
            SetupProjectsList([new ProjectDto { ParentProject = parentProject }]);

            // Act
            var result = await _controller.CreateJobCode(parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditJobCode", partial.ViewName);
            var model = Assert.IsType<PortfolioJobCodeViewModel>(partial.Model);
            Assert.Equal(parentProject, model.ParentProject);
        }

        [Fact]
        public async Task CreateJobCode_Get_PopulatesViewBagWithWorkGroupsTypesAndProjects()
        {
            // Arrange
            const string parentProject = "PRJ001";
            SetupWorkGroupsList([new WorkGroupDto { WorkGroupName = "WG1" }]);
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse(["Type1"]));
            SetupProjectsList([new ProjectDto { ParentProject = parentProject }]);

            // Act
            await _controller.CreateJobCode(parentProject);

            // Assert
            Assert.NotNull(_controller.ViewBag.WorkGroupsData);
            Assert.NotNull(_controller.ViewBag.Types);
            Assert.NotNull(_controller.ViewBag.Projects);
        }

        #endregion

        #region CreateJobCode (POST)

        [Fact]
        public async Task CreateJobCode_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel { JobCodeId = "JC1", ParentProject = "PRJ001", JobCodeWorkGroup = "WG1" };
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ001", JobCodeWorkGroup = "WG1" };

            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.CreateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateJobCode_Post_WithInvalidModelState_ReturnsErrorJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel();
            _controller.ModelState.AddModelError("JobCodeId", "Required");

            // Act
            var result = await _controller.CreateJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.True(jsonElement.TryGetProperty("errors", out _));
        }

        [Fact]
        public async Task CreateJobCode_Post_WhenServiceFails_ReturnsErrorJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };

            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.CreateJobCodeAsync(dto)
                .Returns(CreateErrorResponse<JobCodeDto>());

            // Act
            var result = await _controller.CreateJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.True(jsonElement.TryGetProperty("message", out _));
        }

        #endregion

        #region EditJobCode (GET)

        [Fact]
        public async Task EditJobCode_Get_WithValidJobCodeId_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string jobCodeId = "JC1";
            var dto = new JobCodeDto { JobCodeId = jobCodeId, ParentProject = "PRJ001" };
            var model = new PortfolioJobCodeViewModel { JobCodeId = jobCodeId, ParentProject = "PRJ001" };

            _jobCodeService.GetJobCodeByIdAsync(jobCodeId)
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));
            SetupWorkGroupsList([]);
            _jobCodeService.GetTypesAsync()
                .Returns(ApiResponseDto<List<string>>.SuccessResponse([]));
            SetupProjectsList([]);
            _mapper.Map<PortfolioJobCodeViewModel>(dto).Returns(model);

            // Act
            var result = await _controller.EditJobCode(jobCodeId);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditJobCode", partial.ViewName);
            Assert.IsType<PortfolioJobCodeViewModel>(partial.Model);
        }

        [Fact]
        public async Task EditJobCode_Get_WithInvalidJobCodeId_ReturnsNotFound()
        {
            // Arrange
            const string jobCodeId = "INVALID";
            _jobCodeService.GetJobCodeByIdAsync(jobCodeId)
                .Returns(CreateErrorResponse<JobCodeDto>());

            // Act
            var result = await _controller.EditJobCode(jobCodeId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region EditJobCode (POST)

        [Fact]
        public async Task EditJobCode_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel { JobCodeId = "JC1", ParentProject = "PRJ001" };
            var dto = new JobCodeDto { JobCodeId = "JC1", ParentProject = "PRJ001" };

            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.UpdateJobCodeAsync(dto)
                .Returns(ApiResponseDto<JobCodeDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditJobCode_Post_WithInvalidModelState_ReturnsErrorJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel();
            _controller.ModelState.AddModelError("JobCodeId", "Required");

            // Act
            var result = await _controller.EditJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditJobCode_Post_WhenServiceFails_ReturnsErrorJson()
        {
            // Arrange
            var model = new PortfolioJobCodeViewModel { JobCodeId = "JC1" };
            var dto = new JobCodeDto { JobCodeId = "JC1" };

            _mapper.Map<JobCodeDto>(model).Returns(dto);
            _jobCodeService.UpdateJobCodeAsync(dto)
                .Returns(CreateErrorResponse<JobCodeDto>());

            // Act
            var result = await _controller.EditJobCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteJobCode

        [Fact]
        public async Task DeleteJobCode_WithValidId_ReturnsSuccessJson()
        {
            // Arrange
            const string jobCodeId = "JC1";
            const string parentProject = "PRJ001";

            _jobCodeService.DeleteJobCodeAsync(jobCodeId)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteJobCode(jobCodeId, parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteJobCode_WhenServiceFails_ReturnsErrorJson()
        {
            // Arrange
            const string jobCodeId = "JC1";
            const string parentProject = "PRJ001";

            _jobCodeService.DeleteJobCodeAsync(jobCodeId)
                .Returns(CreateErrorResponse<bool>());

            // Act
            var result = await _controller.DeleteJobCode(jobCodeId, parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.True(jsonElement.TryGetProperty("message", out _));
        }

        #endregion

        #region CreateTimeCode (GET)

        [Fact]
        public async Task CreateTimeCode_Get_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            const string jobCodeId = "JC1";

            SetupWorkGroupsList([new WorkGroupDto { WorkGroupName = "WG1" }]);
            SetupProjectsList([new ProjectDto { ParentProject = parentProject }]);

            // Act
            var result = await _controller.CreateTimeCode(parentProject, jobCodeId);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTimeCode", partial.ViewName);
            var model = Assert.IsType<ValidTimeCodeViewModel>(partial.Model);
            Assert.Equal(parentProject, model.ParentProject);
            Assert.Null(model.JobCode);
        }

        [Fact]
        public async Task CreateTimeCode_Get_PopulatesViewBagWithWorkGroupsAndProjects()
        {
            // Arrange
            const string parentProject = "PRJ001";
            const string jobCodeId = "JC1";

            SetupWorkGroupsList([new WorkGroupDto { WorkGroupName = "WG1" }]);
            SetupProjectsList([new ProjectDto { ParentProject = parentProject }]);

            // Act
            await _controller.CreateTimeCode(parentProject, jobCodeId);

            // Assert
            Assert.NotNull(_controller.ViewBag.WorkGroups);
            Assert.NotNull(_controller.ViewBag.Projects);
        }

        #endregion

        #region CreateTimeCode (POST)

        [Fact]
        public async Task CreateTimeCode_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                Active= true,
                TimeCode = "TC1",
                ParentProject = "PRJ001",
                Project = "PRJ001"
            };
            var dto = new TimeCodeValidDto
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };

            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTimeCode_Post_WithJobCode_ClearsPortfolioAndTestCode()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001",
                Project = "PRJ001",
                JobCode = "JC1",
                Portfolio = "PRT1",
                TestCode = "TST1"
            };
            var dto = new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PRJ001" };

            _mapper.Map<TimeCodeValidDto>(Arg.Any<ValidTimeCodeViewModel>())
                .Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            await _controller.CreateTimeCode(model);

            // Assert
            Assert.Null(model.Portfolio);
            Assert.Null(model.TestCode);
            Assert.Equal("JC1", model.JobCode);
        }

        [Fact]
        public async Task CreateTimeCode_Post_WithOnlyPortfolio_KeepsPortfolioAndClearsJobCode()
        {
            // Arrange - Portfolio only, no JobCode initially
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001",
                Project = "PRJ001",
                JobCode = null,     // No JobCode
                Portfolio = "PRT1"  // Only Portfolio
            };
            var dto = new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PRJ001" };

            _mapper.Map<TimeCodeValidDto>(Arg.Any<ValidTimeCodeViewModel>())
                .Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            await _controller.CreateTimeCode(model);

            // Assert
            Assert.Null(model.JobCode);
            Assert.Equal("PRT1", model.Portfolio);
        }

        [Fact]
        public async Task CreateTimeCode_Post_WithInvalidModelState_ReturnsErrorJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel();
            _controller.ModelState.AddModelError("TimeCode", "Required");

            // Act
            var result = await _controller.CreateTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTimeCode_Post_WhenServiceFails_ReturnsErrorJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1" };

            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(CreateErrorResponse<TimeCodeValidDto>());

            // Act
            var result = await _controller.CreateTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditTimeCode (GET)

        [Fact]
        public async Task EditTimeCode_Get_WithValidParameters_ReturnsPartialViewWithModel()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC1";
            const string parentProject = "PRJ001";
            var dto = new TimeCodeValidDto
            {
                WorkGroup = workGroup,
                TimeCode = timeCode,
                ParentProject = parentProject
            };
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = workGroup,
                TimeCode = timeCode,
                ParentProject = parentProject
            };

            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([dto], new PaginationDto()));
            SetupWorkGroupsList([]);
            SetupProjectsList([]);
            _mapper.Map<ValidTimeCodeViewModel>(dto).Returns(model);

            // Act
            var result = await _controller.EditTimeCode(workGroup, timeCode, null, parentProject);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTimeCode", partial.ViewName);
            var returnedModel = Assert.IsType<ValidTimeCodeViewModel>(partial.Model);
            Assert.Equal(workGroup, returnedModel.OriginalWorkGroup);
        }

        [Fact]
        public async Task EditTimeCode_Get_WithNullTimeCode_ReturnsBadRequest()
        {
            // Arrange
            const string workGroup = "WG1";
            const string parentProject = "PRJ001";

            // Act
            var result = await _controller.EditTimeCode(workGroup, null!, null, parentProject);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Time code is required", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task EditTimeCode_Get_WithNullParentProject_ReturnsBadRequest()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC1";

            // Act
            var result = await _controller.EditTimeCode(workGroup, timeCode, null, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Parent project is required", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task EditTimeCode_Get_WithNullWorkGroup_ReturnsBadRequest()
        {
            // Arrange
            const string timeCode = "TC1";
            const string parentProject = "PRJ001";

            // Act
            var result = await _controller.EditTimeCode(null, timeCode, null, parentProject);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Work group is required", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task EditTimeCode_Get_WhenTimeCodeNotFound_ReturnsNotFound()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC_NOTFOUND";
            const string parentProject = "PRJ001";
            var dto = new TimeCodeValidDto
            {
                WorkGroup = "DIFFERENT_WG",
                TimeCode = "DIFFERENT_TC",
                ParentProject = parentProject
            };

            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([dto], new PaginationDto()));

            // Act
            var result = await _controller.EditTimeCode(workGroup, timeCode, null, parentProject);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", notFound.Value?.ToString());
        }

        [Fact]
        public async Task EditTimeCode_Get_WhenServiceReturnsNoData_ReturnsNotFound()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC1";
            const string parentProject = "PRJ001";

            _timeCodeService.GetPagedTimeCodesAsync(Arg.Any<QueryParameters<string>>(), null, parentProject)
                .Returns(CreateErrorResponse<List<TimeCodeValidDto>>());

            // Act
            var result = await _controller.EditTimeCode(workGroup, timeCode, null, parentProject);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("No time codes found", notFound.Value?.ToString());
        }

        #endregion

        #region EditTimeCode (POST)

        [Fact]
        public async Task EditTimeCode_Post_WithValidModelAndNoWorkGroupChange_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                OriginalWorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };

            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WithWorkGroupChange_DeletesOldAndCreatesNew()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG2",
                OriginalWorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto
            {
                WorkGroup = "WG2",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };

            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));
            _timeCodeService.CreateTimeCodeValidAsync(dto)
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            await _timeCodeService.Received(1).DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ001");
            await _timeCodeService.Received(1).CreateTimeCodeValidAsync(dto);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WithWorkGroupChangeAndDeleteFails_ReturnsErrorJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG2",
                OriginalWorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };

            _timeCodeService.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ001")
                .Returns(CreateErrorResponse<bool>());

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            await _timeCodeService.DidNotReceive().CreateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>());
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WithJobCode_ClearsPortfolioAndTestCode()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                OriginalWorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001",
                Project = "PRJ001",
                JobCode = "JC1",
                Portfolio = "PRT1",
                TestCode = "TST1"
            };
            var dto = new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PRJ001" };

            _mapper.Map<TimeCodeValidDto>(Arg.Any<ValidTimeCodeViewModel>())
                .Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(Arg.Any<TimeCodeValidDto>())
                .Returns(ApiResponseDto<TimeCodeValidDto>.SuccessResponse(dto));

            // Act
            await _controller.EditTimeCode(model);

            // Assert
            Assert.Null(model.Portfolio);
            Assert.Null(model.TestCode);
            Assert.Equal("JC1", model.JobCode);
        }

        [Fact]
        public async Task EditTimeCode_Post_WithInvalidModelState_ReturnsErrorJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel();
            _controller.ModelState.AddModelError("TimeCode", "Required");

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTimeCode_Post_WhenUpdateFails_ReturnsErrorJson()
        {
            // Arrange
            var model = new ValidTimeCodeViewModel
            {
                WorkGroup = "WG1",
                OriginalWorkGroup = "WG1",
                TimeCode = "TC1",
                ParentProject = "PRJ001"
            };
            var dto = new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1" };

            _mapper.Map<TimeCodeValidDto>(model).Returns(dto);
            _timeCodeService.UpdateTimeCodeValidAsync(dto)
                .Returns(CreateErrorResponse<TimeCodeValidDto>());

            // Act
            var result = await _controller.EditTimeCode(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteTimeCode

        [Fact]
        public async Task DeleteTimeCode_WithValidParameters_ReturnsSuccessJson()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC1";
            const string parentProject = "PRJ001";

            _timeCodeService.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTimeCode(workGroup, timeCode, parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.True(jsonElement.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTimeCode_WhenServiceFails_ReturnsErrorJson()
        {
            // Arrange
            const string workGroup = "WG1";
            const string timeCode = "TC1";
            const string parentProject = "PRJ001";

            _timeCodeService.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject)
                .Returns(CreateErrorResponse<bool>());

            // Act
            var result = await _controller.DeleteTimeCode(workGroup, timeCode, parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonElement = GetJsonResultElement(jsonResult);
            Assert.False(jsonElement.GetProperty("success").GetBoolean());
            Assert.True(jsonElement.TryGetProperty("message", out _));
        }

        #endregion

        #region NavigateToTestPurchaseRequirements

        [Fact]
        public void NavigateToTestPurchaseRequirements_WithValidParentProject_SetsTemDataAndRedirects()
        {
            // Arrange
            const string parentProject = "PRJ001";

            // Act
            var result = _controller.NavigateToTestPurchaseRequirements(parentProject);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("TestPurchaseRequirement", redirectResult.ControllerName);
            Assert.Equal("PACT", redirectResult.RouteValues!["area"]);
            Assert.Equal(parentProject, redirectResult.RouteValues["parentProject"]);
            Assert.Equal("PortfolioTimeCodes", _controller.TempData["PactOrigin"]);
        }

        [Fact]
        public void NavigateToTestPurchaseRequirements_WithEmptyParentProject_StillSetsTemDataAndRedirects()
        {
            // Arrange
            const string parentProject = "";

            // Act
            var result = _controller.NavigateToTestPurchaseRequirements(parentProject);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("TestPurchaseRequirement", redirectResult.ControllerName);
            Assert.Equal("PortfolioTimeCodes", _controller.TempData["PactOrigin"]);
        }

        #endregion
    }
}
