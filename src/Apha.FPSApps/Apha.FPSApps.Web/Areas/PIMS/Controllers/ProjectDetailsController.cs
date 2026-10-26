using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
   public class ProjectDetailsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectListService _projectListService;
        private readonly IProjectDetailsService _projectDetailsService;
        private readonly IProjectCommentService _commentService;

        public ProjectDetailsController(
            IMapper mapper,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService,
            IProjectCommentService commentService)
        {
            _mapper = mapper;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
            _commentService = commentService;
        }

        public async Task<IActionResult> Index(string parentproject, int? showprojects = null)
        {
            if (string.IsNullOrWhiteSpace(parentproject))
            {
                return RedirectToAction("Index", "ProjectList", new { area = "PIMS" });
            }

            int normalizedShowProjects = showprojects == 0 ? 0 : 1;
            ProjectDetailsViewModel viewModel = await BuildViewModelAsync(parentproject, normalizedShowProjects);
            return View(viewModel);
        }

        private async Task<ProjectDetailsViewModel> BuildViewModelAsync(string parentproject, int showProjects)
        {
            Task<ApiResponseDto<ProjectDto>> fpsTask = _projectDetailsService.GetFpsProjectAsync(parentproject);
            Task<ApiResponseDto<ProposedProjectDto>> proposedTask = _projectDetailsService.GetProposedProjectAsync(parentproject);
            Task<ApiResponseDto<List<ProjectsDto>>> yearlyTask = _projectListService.GetYearlyDetailsByProjectAsync(parentproject);
            Task<ApiResponseDto<ProjectDetailDto>> pimsTask = _projectDetailsService.GetPimsDetailAsync(parentproject);
            Task<ApiResponseDto<List<ProjectListViewDto>>> allProjectsTask = _projectListService.GetAllProjectsListAsync();
            Task<ApiResponseDto<List<RiskDto>>> risksTask = _projectDetailsService.GetAllRiskAsync();

            await Task.WhenAll(fpsTask, proposedTask, yearlyTask, pimsTask, allProjectsTask, risksTask);

            ProposedProjectDto? proposed = proposedTask.Result.Data;
            bool hasProposedProjectData = proposed is not null;
            proposed?.TransferTo = proposed.Parentproject;
            ProjectDetailDto? pimsDetail = pimsTask.Result.Data;

            PaginationFilter<string> defaultCommentRequest = new() { Filter = "{}" };
            DataGridConfig<ProjectCommentItem> commentsGrid = await BuildCommentsGridAsync(parentproject, null, defaultCommentRequest);

            return new ProjectDetailsViewModel
            {
                Parentproject = parentproject,
                ShowProjects = showProjects,
                FpsProjectDetails = fpsTask.Result.Data,
                YearlyDetails = (yearlyTask.Result.Data ?? [])
                    .OrderByDescending(y => y.Year)
                    .ToList(),
                ProposedProjectDetails = proposed ?? new ProposedProjectDto(),
                HasProposedProjectData = hasProposedProjectData,
                TransferToOptions = GetTransferToOptions(allProjectsTask),
                RiskRatingOptions = GetRiskRatingOptions(risksTask),
                ProjectDetails = pimsDetail,
                IsFPS = allProjectsTask.Result.Data?.Any(p => p.Parentproject == parentproject) ?? false,
                UseProjectYears = pimsDetail?.UseProjectYears ?? false,
                CommentsGrid = commentsGrid,
                YearOptions = await GetYearOptions(),
            };
        }

        private static List<SelectListItem> GetRiskRatingOptions(Task<ApiResponseDto<List<RiskDto>>> risksTask)
        {
            List<SelectListItem> options = risksTask.Result.Data?
                   .Select(p => new SelectListItem(p.Riskrating, p.Riskid.ToString()))
                   .ToList() ?? [];

            return PrependDefaultOption(options);
        }

        private async Task<List<SelectListItem>> GetYearOptions()
        {
            var years = await _projectDetailsService.GetAllYearAsync();

            List<SelectListItem> options = years?.Data?
                   .OrderByDescending(y => y.Value)
                   .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                   .ToList() ?? [];

            return options;
        }

        private static List<SelectListItem> GetTransferToOptions(Task<ApiResponseDto<List<ProjectListViewDto>>> allProjectsTask)
        {
            List<SelectListItem> options = allProjectsTask.Result.Data?
                            .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                            .ToList() ?? [];

            return PrependDefaultOption(options);
        }

        private static List<SelectListItem> PrependDefaultOption(List<SelectListItem> options)
        {
            options.Insert(0, new SelectListItem("-- Select --", "", selected: true));
            return options;
        }

        [HttpPost]
        public async Task<IActionResult> LoadCommentsGrid(string parentproject, int? year, PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            DataGridConfig<ProjectCommentItem> gridConfig = await BuildCommentsGridAsync(parentproject, year, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectCommentItem>> BuildCommentsGridAsync(
            string parentproject, int? year, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            
            //   topic filtering is used only on the standalone Comments page
            ApiResponseDto<List<CommentDto>> pagedData =
                await _commentService.GetCommentsByProjectAsync(parentproject, year, null, queryParameters);

            List<ProjectCommentItem> items = pagedData.Data is not null
                ? _mapper.Map<List<ProjectCommentItem>>(pagedData.Data)
                : new List<ProjectCommentItem>();

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            List<DataGridColumn> columns = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null);
            foreach (DataGridColumn column in columns)
            {
                if (column.PropertyName == nameof(ProjectCommentItem.MadeBy)
                    || column.PropertyName == nameof(ProjectCommentItem.DateEntered))
                {
                    column.IsVisible = false;
                }
            }

            return new DataGridConfig<ProjectCommentItem>
            {
                GridId = "projectCommentsGrid",
                Title = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "CommentNo",
                AllowAdd = false,
                EditFunction = "editComment",
                DeleteFunction = "deleteComment",
                ExtraFilterMethod = "getProjectDetailsExtraFilters",
                BindGridUrl = "/PIMS/ProjectDetails/LoadCommentsGrid",
                Data = items,
                Columns = columns,
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePimsDetail(string parentproject, [FromBody] ProjectDetailDto dto)
        {
            dto.Parentproject = parentproject;
            ApiResponseDto<ProjectDetailDto> result =
                await _projectDetailsService.SavePimsDetailAsync(parentproject, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "PIMS details saved successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProposedProject(string parentproject, ProjectDetailsViewModel projectDetailsViewModel)
        {
            if (projectDetailsViewModel.ProposedProjectDetails is null)
            {
                return RedirectToAction(nameof(Index), new { parentproject, showprojects = projectDetailsViewModel.ShowProjects });
            }

            if (!ModelState.IsValid)
            {
                ProjectDetailsViewModel invalidViewModel = await BuildViewModelAsync(parentproject, projectDetailsViewModel.ShowProjects);
                invalidViewModel.ProposedProjectDetails = projectDetailsViewModel.ProposedProjectDetails;
                invalidViewModel.IsFPS = false;
                invalidViewModel.IsProposedProjectUpdate = true;
                return View(nameof(Index), invalidViewModel);
            }

            projectDetailsViewModel.ProposedProjectDetails.Parentproject = parentproject;

            ApiResponseDto<ProposedProjectDto> result =
                await _projectDetailsService.UpdateProposedProjectAsync(parentproject, projectDetailsViewModel.ProposedProjectDetails);

            if (result.Success)
            {
                string resolvedParentProject = result.Data?.Parentproject ?? parentproject;
                return RedirectToAction(nameof(Index), new { parentproject = resolvedParentProject, showprojects = projectDetailsViewModel.ShowProjects });
            }

            ProjectDetailsViewModel viewModel = await BuildViewModelAsync(parentproject, projectDetailsViewModel.ShowProjects);
            viewModel.ProposedProjectDetails = projectDetailsViewModel.ProposedProjectDetails;
            viewModel.IsFPS = false;
            viewModel.IsProposedProjectUpdate = true;

            if (result.Errors?.Any() == true)
            {
                foreach (ApiErrorDto error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Message);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to update proposed project.");
            }

            return View(nameof(Index), viewModel);
        }



        [HttpGet]
        public async Task<IActionResult> GetAddEditCommentPartial(string parentproject, int? CommentNo, int? selectedYear)
        {
            AddEditCommentViewModel model = await LoadAddEditCommentViewModelAsync(parentproject, CommentNo, selectedYear);

            if (CommentNo is not null and not 0)
            {
                ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(CommentNo.Value);
                if (result is { Success: true, Data: not null })
                {
                    model.CommentNo = result.Data.CommentNo;
                    model.Year = result.Data.Year;
                    model.Topic = result.Data.Topic;
                    model.CommentText = result.Data.CommentText;
                }
            }

            return PartialView("_AddEditComment", model);
        }

        private async Task<AddEditCommentViewModel> LoadAddEditCommentViewModelAsync(string parentproject, int? CommentNo, int? selectedYear)
        {
            ApiResponseDto<List<CommentTopicDto>> topicsResult = await _commentService.GetCommentTopicsAsync();

            List<SelectListItem> topicOptions = [new SelectListItem("Select a topic", "")];
            if (topicsResult is { Success: true, Data: not null })
            {
                topicOptions.AddRange(topicsResult.Data
                    .Select(t => new SelectListItem(t.Topic, t.Topic)));
            }

            return new()
            {
                Project = parentproject,
                IsAddingNew = CommentNo is null or 0,
                Year = selectedYear,
                YearOptions =  await GetYearOptions(),
                TopicOptions = topicOptions
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetComment(int CommentNo)
        {
            ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(CommentNo);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment retrieved successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy = GetCurrentUser();
            dto.CommentText = dto.Comment?.Trim();
            ApiResponseDto<CommentDto> result = await _commentService.CreateCommentAsync(dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment added successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy = GetCurrentUser();
            dto.CommentText = !string.IsNullOrWhiteSpace(dto.Comment)
                ? dto.Comment.Trim()
                : dto.CommentText?.Trim();
            ApiResponseDto<CommentDto> result = await _commentService.UpdateCommentAsync(dto.CommentNo, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteComment(int CommentNo)
        {
            ApiResponseDto<bool> result = await _commentService.DeleteCommentAsync(CommentNo);
            return result.Success
                ? Json(new { success = true, message = "Comment deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        private string GetCurrentUser()
        {
            return User?.Identity?.Name ?? "";
        }
    }
}
