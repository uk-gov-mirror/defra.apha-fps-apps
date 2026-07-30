using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;

        public ProjectController(
            IMapper mapper,
            IProjectService projectService,
            IProgramService programService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _programService = programService;
        }

        // GET: ProgrammeNewProject/Index (no model — plain entry page)
        public IActionResult Index()
        {
            return View();
        }

        // GET: ProgrammeNewProject/Add for new project (with model for dropdowns)
        public async Task<IActionResult> Add()
        {
            var model = new ProgrammeNewProjectViewModel { Disease = "Not Specified" };
            await PopulateDropdownsAsync(model);
            ViewBag.IsEditMode = false;
            return View("ProjectAddEdit", model);
        }

        // POST: ProgrammeNewProject/Add
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] ProgrammeNewProjectViewModel model)
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

            var dto = _mapper.Map<ProjectDto>(model);
            var response = await _projectService.CreateProjectAsync(dto);
            if (response.Success)
                return Json(new { success = true, data = response.Data, message = "Project created successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create project.",
                errors = (response.Errors ?? new()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: ProgrammeNewProject/Edit/{parentProject}
        public async Task<IActionResult> Edit(string parentProject)
        {
            var response = await _projectService.GetProgrammeNewProjectByIdAsync(parentProject);
            if (!response.Success || response.Data == null)
                return NotFound();

            var model = _mapper.Map<ProgrammeNewProjectViewModel>(response.Data);
            await PopulateDropdownsAsync(model);
            ViewBag.IsEditMode = true;
            return View("ProjectAddEdit", model);
        }

        // POST: ProgrammeNewProject/Edit/{parentProject}
        [HttpPost]
        public async Task<IActionResult> Edit(string parentProject, [FromBody] ProgrammeNewProjectViewModel model)
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

            var dto = _mapper.Map<ProjectDto>(model);
            var response = await _projectService.UpdateProjectAsync(parentProject, dto);
            if (response.Success)
                return Json(new { success = true, data = response.Data, message = "Project updated successfully." });

            return Json(new
            {
                success = false,
                message = "Failed to update project.",
                errors = (response.Errors ?? new()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // POST: ProgrammeNewProject/Delete/{parentProject}
        [HttpPost]
        public async Task<IActionResult> Delete(string parentProject)
        {
            var response = await _projectService.DeleteProjectAndChildrenAsync(parentProject);
            if (response.Success)
                return Json(new
                {
                    success = true,
                    message = "Project deleted successfully.",
                    redirectUrl = Url.Action("Add", "Project", new { area = "FPS" })
                });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete project.",
                errors = (response.Errors ?? new()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // POST: ProgrammeNewProject/ChangeCode — FPSAdmin only
        [HttpPost]
        [Authorize(Roles = "FPSAdmin")]
        public async Task<IActionResult> ChangeCode(string oldCode, string newCode)
        {
            if (string.IsNullOrWhiteSpace(oldCode) || string.IsNullOrWhiteSpace(newCode))
                return Json(new { success = false, message = "Both old and new project codes are required." });

            var response = await _projectService.ChangeProjectCodeAsync(oldCode, newCode);
            if (response.Success)
                return Json(new
                {
                    success = true,
                    message = "Project code changed successfully.",
                    redirectUrl = Url.Action("Edit", "Project", new { area = "FPS", parentProject = newCode })
                });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to change project code.",
                errors = (response.Errors ?? new()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateDropdownsAsync(ProgrammeNewProjectViewModel model)
        {
            var managerTask = _projectService.GetManagersAsync();
            var costCentreTask = _projectService.GetCostCentresAsync();
            var projectGroupTask = _projectService.GetProjectGroupsByUserAsync();
            var accountCodeTask = _projectService.GetAccountCodesAsync();
            var subAccountTask = _projectService.GetSubAccountsAsync();
            var statusTask = _projectService.GetAllStatusesAsync();
            var diseaseTask = _projectService.GetAllDiseasesAsync();
            var customerTask = _projectService.GetAllCustomersAsync();
            var contractTask = _projectService.GetContractsByUserAsync();
            var programTask = _programService.GetAllProgramsAsync();

            await Task.WhenAll(managerTask, costCentreTask, projectGroupTask,
                               accountCodeTask, subAccountTask, statusTask,
                               diseaseTask, customerTask, contractTask, programTask);

            var managers = (await managerTask).Data ?? new();
            model.ManagerList = managers
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .Select(m => new SelectListItem($"{m.Name} | {m.WorkGroup ?? string.Empty}", m.Name, m.Name == model.Manager))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var costCentres = (await costCentreTask).Data ?? new();
            model.CostCentreList = costCentres
                .Where(cc => cc.CostCentre.HasValue)
                .OrderBy(cc => cc.ProfitCentre)
                .Select(cc => new SelectListItem(
                    $"{cc.CostCentre} | {cc.ProfitCentre ?? string.Empty} | {cc.WGs ?? string.Empty}",
                    cc.CostCentre?.ToString() ?? "",
                    cc.CostCentre == model.CostCentre))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var projectGroups = (await projectGroupTask).Data ?? new();
            model.ProjectGroupList = projectGroups
                .Where(pg => !string.IsNullOrEmpty(pg.ProjectGroupName))
                .OrderBy(pg => pg.ProjectGroupName)
                .Select(pg => new SelectListItem(pg.ProjectGroupName, pg.ProjectGroupName, pg.ProjectGroupName == model.ProjectGroup))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var accountCodes = (await accountCodeTask).Data ?? new();
            model.IncomeAccountCodeList = accountCodes
                .Where(ac => !string.IsNullOrEmpty(ac.Code))
                .Select(ac => new SelectListItem($"{ac.Code} - {ac.Description ?? string.Empty}", ac.Code, ac.Code == model.IncomeAccountCode))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var subAccounts = (await subAccountTask).Data ?? new();
            model.SubAccountCodeList = subAccounts
                .Where(sa => !string.IsNullOrEmpty(sa.SubAccountCode))
                .Select(sa => new SelectListItem($"{sa.SubAccountCode} - {sa.SubAccount ?? string.Empty}", sa.SubAccountCode, sa.SubAccountCode == model.SubAccountCode))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var statuses = (await statusTask).Data ?? new();
            model.ProjectStatusList = statuses
                .Where(s => !string.IsNullOrEmpty(s.Status))
                .Select(s => new SelectListItem(s.Status, s.Status, s.Status == model.ProjectStatus))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var diseases = (await diseaseTask).Data ?? new();
            model.DiseaseList = diseases
                .Where(d => !string.IsNullOrEmpty(d.Disease))
                .Select(d => new SelectListItem(d.Disease, d.Disease, d.Disease == model.Disease))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var customers = (await customerTask).Data ?? new();
            model.CustomerList = customers
                .Where(c => !string.IsNullOrEmpty(c.Customer))
                .OrderBy(c => c.Customer.Trim())
                .Select(c => new SelectListItem(c.Customer.Trim(), c.Customer.Trim(), c.Customer.Trim() == model.Customer?.Trim()))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var contracts = (await contractTask).Data ?? new();
            model.ContractList = contracts
                .Where(c => !string.IsNullOrEmpty(c.ContractNo))
                .Select(c => new SelectListItem(c.ContractNo, c.ContractNo, c.ContractNo == model.Contract))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            var programs = (await programTask).Data ?? Enumerable.Empty<ProgramDto>();
            model.ProgramList = programs
                .Where(p => !string.IsNullOrEmpty(p.ProgramNo))
                .Select(p => new SelectListItem($"{p.ProgramNo} | {p.ProgramName ?? string.Empty}", p.ProgramNo, p.ProgramNo == model.Program))
                .Prepend(new SelectListItem("", ""))
                .ToList();

            model.IsDefraProjectList = new List<SelectListItem>
            {
                new("", ""),
                new("Yes", "-1", model.IsDefraProject == -1),
                new("No", "0", model.IsDefraProject == 0)
            };
        }
    }
}

