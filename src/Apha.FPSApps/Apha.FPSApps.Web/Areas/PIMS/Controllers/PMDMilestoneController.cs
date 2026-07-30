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

            DataGridConfig<MilestoneItem> gridConfig =
                await BuildMilestonesGridAsync(parentproject ?? string.Empty, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<MilestoneItem>> BuildMilestonesGridAsync(
            string parentproject, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _milestoneService.GetPMDMilestonesAsync(queryParameters, parentproject);

            List<MilestoneItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
                items = _mapper.Map<List<MilestoneItem>>(pagedData.Data);

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<MilestoneItem>
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
                Columns = GridDataProvider.GetColumnsDefination<MilestoneItem>(),
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
    }
}
