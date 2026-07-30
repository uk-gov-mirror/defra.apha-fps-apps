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
    public class MilestoneImportController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMilestoneService _milestoneService;
        private readonly IProjectListService _projectListService;

        public MilestoneImportController(
            IMapper mapper,
            IMilestoneService milestoneService,
            IProjectListService projectListService)
        {
            _mapper = mapper;
            _milestoneService = milestoneService;
            _projectListService = projectListService;
        }

        public async Task<IActionResult> Index(string? project = null)
        {
            MilestoneImportViewModel viewModel = new();

            var allProjects = await _projectListService.GetAllProjectsForMilestoneAsync();
            viewModel.ProjectOptions = allProjects.Data?
                .Select(p => new SelectListItem(p.Parentproject, p.Parentproject))
                .ToList() ?? [];

            string selectedProject = project ?? viewModel.ProjectOptions.FirstOrDefault()?.Value ?? string.Empty;
            viewModel.Parentproject = selectedProject;         
                      
            var matchedProject = allProjects.Data?.FirstOrDefault(p => p.Parentproject == project);

            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            viewModel.TypeLookUp = matchedProject?.Program?.EndsWith("surv", StringComparison.OrdinalIgnoreCase) == true ? 'D' : 'M';
            await PopulateDropdownsAsync(viewModel);
            viewModel.ImportGrid = await BuildImportGridAsync(selectedProject, defaultRequest);
            viewModel.Year=  DateTime.Now.Year.ToString();
            return View(viewModel);
        }

        // ── Import DataGrid ──────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadMilestoneImportGrid(PaginationFilter<string> request,string? project = null)
        {
            DataGridConfig<StagingMilestoneItem> gridConfig = await BuildImportGridAsync(project ?? string.Empty, request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<StagingMilestoneItem>> BuildImportGridAsync(string project, PaginationFilter<string> request)
        {
            Dictionary<string, string> filterDict =
               JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            queryParameters.Page = -1;
            queryParameters.PageSize = 50;
            var apiResult = await _milestoneService.GetAllStagingRowsAsync(queryParameters);

            List<StagingMilestoneItem> items = [];
            if (apiResult.Success && apiResult.Data != null)
                items = _mapper.Map<List<StagingMilestoneItem>>(apiResult.Data);

            return new DataGridConfig<StagingMilestoneItem>
            {
                GridId = "importGrid",
                ShowCheckboxColumn = false,
                ShowPagination = false,
                KeyProperty = "Id",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addImportRow",
                EditFunction = "editImportRow",
                DeleteFunction = "deleteImportRow",
                BindGridUrl = "/PIMS/MilestoneImport/LoadMilestoneImportGrid",
                ExtraFilterMethod = "getMilestoneImportExtraFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<StagingMilestoneItem>(),
                CurrentFilters = filterDict
            };
        }

        // ── Add / Edit partial ───────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditMilestoneImportPartial(string? project = null, int? id = null, char? typeLookUp = null, string? typeId = null)
        {
            StagingMilestoneItem model = new() { Project = project, TypeId = typeId };

            if (id.HasValue)
            {
                var apiResult = await _milestoneService.GetStagingRowsAsync(id.Value);
                if (apiResult.Success && apiResult.Data != null)
                {
                    var dto = apiResult.Data.FirstOrDefault();
                    if (dto != null)
                        model = _mapper.Map<StagingMilestoneItem>(dto);
                }
            }

            ViewBag.IsAddingNew = !id.HasValue;
            ViewBag.TypeLookUp = typeLookUp ?? 'M';
            ViewBag.TypeId = typeId;
           
            return PartialView("_AddEditMilestoneImport", model);
        }

        // ── CRUD endpoints (called by DataGrid JS) ───────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImportRow(StagingMilestoneItem item,int year)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            StagingMilestoneDto dto = _mapper.Map<StagingMilestoneDto>(item);

            var result = item.IsAddingNew
                ? await _milestoneService.AddStagingRowAsync(dto, year)
                : await _milestoneService.UpdateStagingRowAsync(item.Id, dto);

            return result.Success
                ? Json(new { success = true, data = result.Data, message = item.IsAddingNew ? "Import record saved successfully." : "Import record updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }     
            
        [HttpDelete]
        public async Task<IActionResult> DeleteImportRow(int id)
        {
            var result = await _milestoneService.DeleteStagingRowAsync(id);
            return result.Success
                ? Json(new { success = true, message = "Import record deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }
        [HttpGet]
        public async Task<IActionResult> GetFormRequired(string parentproject)
        {
            ApiResponseDto<ProjectDetailsMilestoneDto> allProjects =
                await _projectListService.GetProjectsDetailsForMilestoneAsync(parentproject);
            var matchedProject = allProjects.Data;
            char typeLookUp = matchedProject?.Program?.EndsWith("surv", StringComparison.OrdinalIgnoreCase) == true ? 'D' : 'M';

            var milestoneTypes = await _milestoneService.GetMilestoneTypesAsync(typeLookUp.ToString());
            var milestoneTypeOptions = milestoneTypes.Data?
                .Where(t => t.MilestoneDeliverable == typeLookUp)
                .OrderBy(t => t.IdType)
                .Select(t => new { value = t.IdType.ToString(), text = t.Type })
                .ToList() ?? [];

            return Json(new { typeLookUp, milestoneTypeOptions });
        }

        // ── Workflow actions ─────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> ValidateImport(
            [FromForm] string project,
            [FromForm] string? typeId = null,
            [FromForm] bool isDeliverableMode = false)
        {
            var result = await _milestoneService.ValidateStagingAsync(project, typeId, isDeliverableMode);
            if (!result.Success)
                return Json(new { success = false, errors = result.Errors });

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ImportRecords(
            [FromForm] string project,
            [FromForm] bool overwrite = false)
        {
            if (string.IsNullOrWhiteSpace(project))
                return Json(new { success = false, errors = new[] { new { message = "Project is required." } } });

            static int ToCount(object? value)
            {
                if (value is null) return 0;
                return int.TryParse(value.ToString(), out int count) ? count : 0;
            }

            int overwritten = 0;

            if (overwrite)
            {
                var owResult = await _milestoneService.ImportWithOverwriteAsync(project);
                if (!owResult.Success)
                    return Json(new { success = false, errors = owResult.Errors });

                overwritten = ToCount(owResult.Data);
            }

            var result = await _milestoneService.ImportStagingAsync(project);
            if (!result.Success)
                return Json(new { success = false, errors = result.Errors });

            int imported = ToCount(result.Data);

            return Json(new
            {
                success = true,
                imported,
                overwritten,
                message = "Import completed successfully."
            });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearImport([FromQuery] string project)
        {
            var result = await _milestoneService.ClearStagingAsync(project);
            if (!result.Success)
                return Json(new { success = false, errors = result.Errors });

            int deleted = 0;           

            return Json(new
            {
                success = true,
                deleted,
                message = "Data cleared successfully."
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync(MilestoneImportViewModel viewModel)
        {
            
            var MilestoneTypeOptions = await _milestoneService.GetMilestoneTypesAsync();
            viewModel.MilestoneTypeOptions = MilestoneTypeOptions.Data?
                 .Where(t => t.MilestoneDeliverable == viewModel.TypeLookUp )
                .OrderBy(t => t.IdType)
                .Select(t => new SelectListItem(t.Type, t.IdType.ToString()))
                .ToList() ?? [];
        }
    }
}