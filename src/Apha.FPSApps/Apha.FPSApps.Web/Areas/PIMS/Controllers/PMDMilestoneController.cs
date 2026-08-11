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
using System.Web;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PMDAdmin,PIMSProjectManager")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class PMDMilestoneController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneService _milestoneService;

        public PMDMilestoneController(IMapper mapper, IMilestoneService milestoneService)
        {
            _mapper = mapper;
            _milestoneService = milestoneService;
        }

        public async Task<IActionResult> Index(string? parentproject = null)
        {
            PMDMilestoneViewModel viewModel = new();

            int year = GetFY();
            var managersResponse = await _milestoneService.GetProjectYearManagersAsync(year);

            viewModel.ProjectOptions = (managersResponse.Success && managersResponse.Data != null)
                ? managersResponse.Data
                    .Where(x => !string.IsNullOrWhiteSpace(x.ParentProject))
                    .Select(x => x.ParentProject!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new SelectListItem(x, x))
                    .ToList()
                : [];

            viewModel.Parentproject = parentproject ?? viewModel.ProjectOptions.FirstOrDefault()?.Value ?? string.Empty;

            (viewModel.ShowConfirmationSection, viewModel.ConfirmationLabelText) =
                await BuildConfirmationStateAsync(viewModel.Parentproject);

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.MilestonesGrid = await BuildMilestonesGridAsync(viewModel.Parentproject, defaultRequest);

            return View(viewModel);
        }

        private static int GetFY()
        {
            int currentYear = DateTime.Today.Year;
            int currentMonth = DateTime.Today.Month;

            return currentMonth < 6 ? currentYear - 1 : currentYear;
        }

        private static string GetMonthToUpdate()
        {
            int currentMonth = DateTime.Today.Month;

            if (currentMonth > 4 && currentMonth <= 6)
                return "Apr";
            if (currentMonth >= 7 && currentMonth <= 9)
                return "Jun";
            if (currentMonth >= 10 && currentMonth <= 12)
                return "Sep";
            if (currentMonth == 1)
                return "Dec";
            if (currentMonth == 2)
                return "Jan";
            if (currentMonth == 3)
                return "Feb";
            if (currentMonth == 4)
                return "Mar";

            return string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> LoadMilestoneGrid(PaginationFilter<string> request, string? parentproject = null)
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

            DataGridConfig<PMDMilestoneItem> gridConfig =
                await BuildMilestonesGridAsync(parentproject ?? string.Empty, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<PMDMilestoneItem>> BuildMilestonesGridAsync(
            string parentproject, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            List<PMDMilestoneItem> items = new();
            PaginationModel paginationModel = new();

            if (!string.IsNullOrWhiteSpace(parentproject))
            {
                QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
                var pagedData = await _milestoneService.GetPMDMilestonesAsync(queryParameters, parentproject);

                if (pagedData.Success && pagedData.Data != null)
                    items = _mapper.Map<List<PMDMilestoneItem>>(pagedData.Data);

                if (pagedData.Pagination is not null)
                    paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<PMDMilestoneItem>
            {
                GridId = "pmdMilestonesGrid",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Number",
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = false,
                EditFunction = "editMilestone",
                BindGridUrl = "/PIMS/PMDMilestone/LoadMilestoneGrid",
                ExtraFilterMethod = "getMilestoneExtraFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PMDMilestoneItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetConfirmationState(string? parentproject)
        {
            (bool showConfirmationSection, string confirmationLabelText) =
                await BuildConfirmationStateAsync(parentproject);

            return Json(new { showConfirmationSection, confirmationLabelText });
        }

        private static async Task<(bool ShowConfirmationSection, string ConfirmationLabelText)> BuildConfirmationStateAsync(string? parentproject)
        {
            await Task.CompletedTask;

            if (string.IsNullOrWhiteSpace(parentproject))
                return (false, string.Empty);

            string monthToUpdate = GetMonthToUpdate();
            if (string.IsNullOrWhiteSpace(monthToUpdate))
                return (false, string.Empty);

            string label = $"Please click here to confirm {monthToUpdate} information is correct.";
            return (true, label);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string parentproject, string number)
        {
            if (string.IsNullOrWhiteSpace(parentproject) || string.IsNullOrWhiteSpace(number))
                return RedirectToAction(nameof(Index));

            string decodedNumber = HttpUtility.UrlDecode(number);
            MilestoneItem model = new() { Project = parentproject, Number = decodedNumber };

            var result = await _milestoneService.GetMilestoneAsync(parentproject, decodedNumber);
            if (result is { Success: true, Data: not null })
                model = _mapper.Map<MilestoneItem>(result.Data);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditMilestoneDetails(
            string project, 
            string number, 
            string? datecompleted, 
            int underreview, 
            int ontarget, 
            string? projectleadercomment)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(number))
                    return Json(new { success = false, message = "Project and Milestone number are required.", errors = new[] { "Project and Milestone number are required." } });

                if (string.IsNullOrWhiteSpace(projectleadercomment))
                    return Json(new { success = false, message = "Project Leaders Comment is required.", errors = new[] { "Project Leaders Comment is required." } });

                // Parse date if provided
                DateTime? dateCompletedValue = null;
                if (!string.IsNullOrWhiteSpace(datecompleted))
                {
                    if (DateTime.TryParse(datecompleted, out DateTime parsedDate))
                        dateCompletedValue = parsedDate;
                }

                var milestoneDto = new Apha.FPSApps.Application.Dtos.PIMS.MilestoneDto
                {
                    Project = project,
                    Number = number,
                    DateCompleted = dateCompletedValue,
                    UnderSdReview = (short)underreview,
                    OnTarget = (short)ontarget,
                    ProjectLeaderComment = projectleadercomment
                };

                var result = await _milestoneService.UpdateMilestoneAsync_PMD(project, number, milestoneDto);

                if (result?.Success == true)
                    return Json(new { success = true, message = "Milestone updated successfully." });

                return Json(new 
                { 
                    success = false, 
                    message = "Failed to update milestone.", 
                    errors = result?.Errors?.Select(e => e.Message).ToList() ?? new List<string> { "An error occurred." }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred.", errors = new[] { ex.Message } });
            }
        }
    }
}
