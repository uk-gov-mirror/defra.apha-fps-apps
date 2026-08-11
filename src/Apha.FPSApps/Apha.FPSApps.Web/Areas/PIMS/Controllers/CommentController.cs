using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
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
    public class CommentController : Controller
    {
        private readonly IMapper _mapper;

        
        private readonly IProjectCommentService _commentService;

        private readonly IProjectListService _projectListService;

        
        private readonly IProjectDetailsService _projectDetailsService;

        public CommentController(
            IMapper mapper,
            IProjectCommentService commentService,
            IProjectListService projectListService,
            IProjectDetailsService projectDetailsService)
        {
            _mapper = mapper;
            _commentService = commentService;
            _projectListService = projectListService;
            _projectDetailsService = projectDetailsService;
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? parentproject)
        {
            CommentViewModel viewModel = new()
            {
                Parentproject = parentproject ?? string.Empty,
                SelectedProject = parentproject
            };
            await PopulateDropdownsAsync(viewModel);

           
            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.CommentsGrid = await BuildCommentsGridAsync(
                defaultRequest,
                viewModel.SelectedProject,
                topic: null,
                year: null);

            return View(viewModel);
        }

        // ── Dropdown Population ───────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(CommentViewModel model)
        {
            
            QueryParameters<string> projectDropdownQuery = new()
            {
                Page = -1,
                PageSize = 10,
                Filter = "{}"
            };
            Task<ApiResponseDto<List<ProjectListViewDto>>> projectsTask =
                _projectListService.GetAllProjectsAsync(projectDropdownQuery, 1);

            Task<ApiResponseDto<List<CommentTopicDto>>> topicsTask =
                _commentService.GetCommentTopicsAsync();

            
            Task<ApiResponseDto<List<YearDto>>> yearsTask =
                _projectDetailsService.GetAllYearAsync();

            await Task.WhenAll(projectsTask, topicsTask, yearsTask);

            // Project selector
            List<SelectListItem> projectOptions = [new SelectListItem("-- Select --", "", string.IsNullOrWhiteSpace(model.SelectedProject))];
            if (projectsTask.Result is { Success: true, Data: not null })
            {
                projectOptions.AddRange(projectsTask.Result.Data
                    .Where(p => !string.IsNullOrWhiteSpace(p.Parentproject))
                    .Select(p => new SelectListItem(p.Parentproject, p.Parentproject)
                    {
                        Selected = !string.IsNullOrWhiteSpace(model.SelectedProject)
                                   && string.Equals(p.Parentproject, model.SelectedProject, StringComparison.OrdinalIgnoreCase)
                    }));
            }

           
            if (!string.IsNullOrWhiteSpace(model.SelectedProject))
            {
                SelectListItem? selectedItem = projectOptions
                    .FirstOrDefault(x => string.Equals(x.Value, model.SelectedProject, StringComparison.OrdinalIgnoreCase));

                if (selectedItem is null)
                {
                    projectOptions.Add(new SelectListItem(model.SelectedProject, model.SelectedProject, selected: true));
                }
                else
                {
                    foreach (SelectListItem option in projectOptions)
                        option.Selected = false;

                    selectedItem.Selected = true;
                }
            }

            model.ProjectOptions = projectOptions;

            
            List<SelectListItem> topicOptions = [new SelectListItem("-- All topics --", "", true)];
            if (topicsTask.Result is { Success: true, Data: not null })
            {
                topicOptions.AddRange(topicsTask.Result.Data
                    .Select(t => new SelectListItem(t.Topic, t.Topic)));
            }
            model.TopicOptions = topicOptions;

            
            model.YearOptions = yearsTask.Result?.Data?
                .OrderByDescending(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                .ToList() ?? [];
        }

        // ── DataGrid AJAX Reload ───────────────────────────────────────────────

        
        [HttpPost]
        public async Task<IActionResult> LoadCommentsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? topic = null,
            string? year = null)
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

            DataGridConfig<ProjectCommentItem> gridConfig =
                await BuildCommentsGridAsync(request, project, topic, year);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectCommentItem>> BuildCommentsGridAsync(
            PaginationFilter<string> request,
            string? project,
            string? topic,
            string? year)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

          
            if (string.IsNullOrWhiteSpace(project))
            {
                return new DataGridConfig<ProjectCommentItem>
                {
                    GridId             = "comments",
                    Title              = string.Empty,
                    ShowCheckboxColumn = false,
                    ShowPagination     = true,
                    KeyProperty        = "CommentNo",
                    AllowAdd           = true,
                    AddFunction        = "addComment",
                    AllowEdit          = true,
                    EditFunction       = "editComment",
                    AllowDelete        = true,
                    DeleteFunction     = "deleteComment",
                    ExtraFilterMethod  = "getCommentsExtraFilters",
                    BindGridUrl        = "/PIMS/Comment/LoadCommentsGrid",
                    Data               = new List<ProjectCommentItem>(),
                    Columns            = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                    Pagination         = new PaginationModel(),
                    CurrentFilters     = filterDict
                };
            }

            
            int? parsedYear = int.TryParse(year, out int yr) ? yr : null;

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            string? topicFilter = string.IsNullOrWhiteSpace(topic) ? null : topic;

            ApiResponseDto<List<CommentDto>> pagedData =
                await _commentService.GetCommentsByProjectAsync(project, parsedYear, topicFilter, queryParameters);

            List<ProjectCommentItem> items = pagedData.Data is not null
                ? _mapper.Map<List<ProjectCommentItem>>(pagedData.Data)
                : new List<ProjectCommentItem>();

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectCommentItem>
            {
                GridId             = "comments",
                Title              = string.Empty,
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "CommentNo",
                AllowAdd           = true,
                AddFunction        = "addComment",
                AllowEdit          = true,
                EditFunction       = "editComment",
                AllowDelete        = true,
                DeleteFunction     = "deleteComment",
                ExtraFilterMethod  = "getCommentsExtraFilters",
                BindGridUrl        = "/PIMS/Comment/LoadCommentsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectCommentItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditCommentPartial(
            string? parentproject, int? commentNo, int? selectedYear)
        {
            AddEditCommentViewModel model = await LoadAddEditCommentViewModelAsync(
                parentproject, commentNo, selectedYear);

            if (commentNo is not null and not 0)
            {
                ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(commentNo.Value);
                if (result is { Success: true, Data: not null })
                {
                    model.CommentNo   = result.Data.CommentNo;
                    model.Year        = result.Data.Year;
                    model.Topic       = result.Data.Topic;
                    model.CommentText = result.Data.CommentText;
                }
            }

            return PartialView("_AddEditComment", model);
        }

        private async Task<AddEditCommentViewModel> LoadAddEditCommentViewModelAsync(
            string? parentproject, int? commentNo, int? selectedYear)
        {
            
            ApiResponseDto<List<CommentTopicDto>> topicsResult = await _commentService.GetCommentTopicsAsync();

            List<SelectListItem> topicOptions = [new SelectListItem("Select a topic", "")];
            if (topicsResult is { Success: true, Data: not null })
            {
                topicOptions.AddRange(topicsResult.Data
                    .Select(t => new SelectListItem(t.Topic, t.Topic)));
            }

            List<SelectListItem> yearOptions = await GetYearOptionsAsync();

            return new AddEditCommentViewModel
            {
                Project      = parentproject ?? string.Empty,
                IsAddingNew  = commentNo is null or 0,
                Year         = selectedYear,
                YearOptions  = yearOptions,
                TopicOptions = topicOptions
            };
        }

        private async Task<List<SelectListItem>> GetYearOptionsAsync()
        {
            ApiResponseDto<List<YearDto>> years = await _projectDetailsService.GetAllYearAsync();
            return years?.Data?
                .OrderByDescending(y => y.Value)
                .Select(y => new SelectListItem(y.Value.ToString(), y.Value.ToString()))
                .ToList() ?? [];
        }

        
        [HttpGet]
        public async Task<IActionResult> GetComment(int commentNo)
        {
            ApiResponseDto<CommentDto> result = await _commentService.GetByIdAsync(commentNo);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment retrieved successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy      = GetCurrentUser();
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
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(ms => ms.Value?.Errors.Count > 0)
                    .Select(ms => new { field = ms.Key, message = ms.Value!.Errors.First().ErrorMessage })
                    .ToList();
                return Json(new { success = false, errors });
            }

            dto.MadeBy = GetCurrentUser();
            ApiResponseDto<CommentDto> result = await _commentService.UpdateCommentAsync(dto.CommentNo, dto);
            return result.Success
                ? Json(new { success = true, data = result.Data, message = "Comment updated successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

       
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentNo)
        {
            ApiResponseDto<bool> result = await _commentService.DeleteCommentAsync(commentNo);
            return result.Success
                ? Json(new { success = true, message = "Comment deleted successfully" })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetForecastSpend(string project)
        {
            if (string.IsNullOrWhiteSpace(project))
                return Json(new { success = true, forecastSpend = (double?)null });

            ApiResponseDto<ProjectCommentForecastSpendDto> result =
                await _commentService.GetForecastSpendByProjectAsync(project);

            if (!result.Success)
                return Json(new { success = false, forecastSpend = (double?)null, errors = result.Errors });

            return Json(new
            {
                success = true,
                forecastSpend = result.Data?.ForecastSpend
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveForecastSpend([FromQuery] string project, [FromBody] ProjectCommentForecastSpendDto dto)
        {
            if (string.IsNullOrWhiteSpace(project))
                return Json(new { success = false, message = "Project is required." });

            ApiResponseDto<ProjectCommentForecastSpendDto> result =
                await _commentService.UpdateForecastSpendByProjectAsync(project, dto?.ForecastSpend);

            if (!result.Success)
                return Json(new { success = false, errors = result.Errors, message = "Failed to save forecast spend." });

            return Json(new { success = true, forecastSpend = result.Data?.ForecastSpend, message = "Forecast spend saved." });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string GetCurrentUser()
        {
            return User?.Identity?.Name ?? string.Empty;
        }
    }
}
