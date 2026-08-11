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
    public class MaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IMaintenanceService _service;

        public MaintenanceController(IMapper mapper, IMaintenanceService service)
        {
            _mapper = mapper;
            _service = service;
        }

        private JsonResult BadModelStateResult() =>
            Json(new
            {
                success = false,
                message = "Invalid request data",
                errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });

        public async Task<IActionResult> Index()
        {
            var viewModel = new MaintenanceViewModel
            {
                ReportsGrid = await BuildReportsGridAsync(new PaginationFilter<string> { Filter = "{}" })
            };

            // Load Time tab settings on page load
            var settingsResult = await _service.GetAllSettingsAsync();
            System.Diagnostics.Debug.WriteLine($"GetAllSettingsAsync Success: {settingsResult.Success}, Data Count: {settingsResult.Data?.Count ?? 0}");

            if (settingsResult.Success && settingsResult.Data != null)
            {
                var hoursInDay = settingsResult.Data.FirstOrDefault(s =>
                    s.Id != null && s.Id.Equals("HoursInDay", StringComparison.OrdinalIgnoreCase));
                var daysInYear = settingsResult.Data.FirstOrDefault(s =>
                    s.Id != null && s.Id.Equals("DaysInYear", StringComparison.OrdinalIgnoreCase));

                System.Diagnostics.Debug.WriteLine($"HoursInDay found: {hoursInDay != null}, Value: {hoursInDay?.SettingValue}");
                System.Diagnostics.Debug.WriteLine($"DaysInYear found: {daysInYear != null}, Value: {daysInYear?.SettingValue}");

                if (hoursInDay != null)
                {
                    viewModel.WorkingHoursSettingItem = _mapper.Map<SettingItem>(hoursInDay);
                    System.Diagnostics.Debug.WriteLine($"Mapped WorkingHoursSettingItem: {viewModel.WorkingHoursSettingItem?.SettingValue}");
                }
                if (daysInYear != null)
                {
                    viewModel.WorkingDaysSettingItem = _mapper.Map<SettingItem>(daysInYear);
                    System.Diagnostics.Debug.WriteLine($"Mapped WorkingDaysSettingItem: {viewModel.WorkingDaysSettingItem?.SettingValue}");
                }
            }

            return View(viewModel);
        }

        // ── Other Tab ─────────────────────────────────────────────────────────
        // Returns the ordered list of "Other" list descriptions.
        // Keeping this in the controller (rather than hardcoding in the view)
        // means the list can be extended or driven by config/DB in the future.
        [HttpGet]
        public IActionResult GetOtherListDescriptions()
        {
            var descriptions = new[]
            {
                new { key = "Frequency",        value = "Frequency" },
                new { key = "PublicationTypes",  value = "Publication Types" },
                new { key = "ReportGroups",      value = "Report Groups" },
                new { key = "ReviewItems",       value = "Review Items" },
                new { key = "Risk",              value = "Risk" }
            };
            return Json(descriptions);
        }

        [HttpGet]
        public async Task<IActionResult> LoadTimeTabSettings()
        {
            var result = await _service.GetAllSettingsAsync();
            if (!result.Success || result.Data == null)
                return Json(new { workingHours = (string?)null, workingDays = (string?)null });

            var workingHours = result.Data.FirstOrDefault(s =>
                s.Id != null && s.Id.Equals("HoursInDay", StringComparison.OrdinalIgnoreCase));
            var workingDays = result.Data.FirstOrDefault(s =>
                s.Id != null && s.Id.Equals("DaysInYear", StringComparison.OrdinalIgnoreCase));

            return Json(new
            {
                workingHours = workingHours?.SettingValue,
                workingDays = workingDays?.SettingValue
            });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB
        // ════════════════════════════════════════════════════════════════════════════

        // ── Reports Grid ─────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadReportsGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildReportsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReportItem>> BuildReportsGridAsync(PaginationFilter<string> request)
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedReportsAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<ReportDto>();

            var items = _mapper.Map<List<ReportItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ReportItem>
            {
                GridId = "reportsGrid",
                Title = "Reports",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Id",
                AllowAdd = true,
                AddFunction = "addReport",
                AllowEdit = true,
                EditFunction = "editReport",
                AllowDelete = true,
                DeleteFunction = "deleteReport",
                AllowRowSelection = true,
                RowSelectFunction = "onReportRowSelect",
                ExtraFilterMethod = "getReportsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadReportsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReportItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        // ── Reports CRUD ─────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditReportPartial(int? id = null)
        {
            var model = new ReportItem();
            model.Type = "R";
            if (id.HasValue)
            {
                var result = await _service.GetReportByIdAsync(id.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReportItem>(result.Data);
            }
            ViewBag.IsAddingNew = !id.HasValue;
            return PartialView("_AddEditReport", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReport(ReportItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ReportDto>(item);
            ApiResponseDto<ReportDto> result = item.Id == 0
                ? await _service.CreateReportAsync(dto)
                : await _service.UpdateReportAsync(item.Id, dto);

            return result.Success
                ? Json(new { success = true, message = item.Id == 0 ? "Report created successfully." : "Report updated successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.", errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var result = await _service.DeleteReportAsync(id);
            return result.Success
                ? Json(new { success = true, message = "Report deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ── Report Groups Grid ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetReportGroupsGrid(int? reportid = null, string gridId = "reportGroupsGrid")
        {
            if (gridId == "otherValuesTable")
            {
                var otherConfig = await BuildOtherReportGroupsGridAsync(new PaginationFilter<string>(), gridId);
                return PartialView("_DataGrid", otherConfig);
            }
            var gridConfig = await BuildReportGroupsGridAsync(new PaginationFilter<string>(), reportid, gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadReportGroupsGrid(PaginationFilter<string> request, int? reportid = null, string gridId = "reportGroupsGrid")
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            if (gridId == "otherValuesTable")
            {
                var otherConfig = await BuildOtherReportGroupsGridAsync(request, gridId);
                return PartialView("_DataGrid", otherConfig);
            }
            var gridConfig = await BuildReportGroupsGridAsync(request, reportid, gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReportGroupItem>> BuildReportGroupsGridAsync(PaginationFilter<string> request, int? reportid = null, string gridId = "reportGroupsGrid")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedReportGroupsAsync(query, reportid);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<ReportGroupDto>();

            var items = _mapper.Map<List<ReportGroupItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var bindUrl = $"/PIMS/Maintenance/LoadReportGroupsGrid?gridId={Uri.EscapeDataString(gridId)}";
            if (reportid.HasValue)
            {
                bindUrl += $"&reportid={reportid.Value}";
            }

            return new DataGridConfig<ReportGroupItem>
            {
                GridId = gridId,
                Title = "Report Groups",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "GroupId",
                AllowAdd = true,
                AddFunction = "addReportGroup",
                AllowEdit = false,
                EditFunction = "editReportGroup",
                AllowDelete = true,
                DeleteFunction = "deleteReportGroup",
                ExtraFilterMethod = "getReportGroupsExtraFilters",
                BindGridUrl = bindUrl,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReportGroupItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<OtherReportGroupItem>> BuildOtherReportGroupsGridAsync(PaginationFilter<string> request, string gridId = "otherValuesTable")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedReportGroupsAsync(query, null);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<ReportGroupDto>();

            var items = _mapper.Map<List<OtherReportGroupItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<OtherReportGroupItem>
            {
                GridId = gridId,
                Title = "Report Groups",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "GroupId",
                AllowAdd = true,
                AddFunction = "addOtherReportGroup",
                AllowEdit = true,
                EditFunction = "editOtherReportGroup",
                AllowDelete = true,
                DeleteFunction = "deleteOtherReportGroup",
                ExtraFilterMethod = "getOtherValuesExtraFilters",
                BindGridUrl = $"/PIMS/Maintenance/LoadReportGroupsGrid?gridId={Uri.EscapeDataString(gridId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<OtherReportGroupItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        // ── Report Groups CRUD ────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetAddEditReportGroupPartial(int? groupid = null, int? reportid = null)
        {
            var model = new ReportGroupViewModel();
            model.Reportid = reportid ?? 0;

            if (groupid.HasValue)
            {
                var result = await _service.GetReportGroupByIdAsync(groupid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReportGroupViewModel>(result.Data);
            }

            var reportGroupsResponse = await _service.GetAllReportGroupsAsync();

            model.ReportGroups = reportGroupsResponse.Data?
                .Select(x => new SelectListItem
                {
                    Value = x.GroupId.ToString(),
                    Text = x.Description
                })
                .ToList() ?? new List<SelectListItem>();

            ViewBag.IsAddingNew = !groupid.HasValue;
            return PartialView("_AddEditReportGroup", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReportGroup(ReportGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = new ReportGroupLinkDto
            {
                GroupId = model.GroupId,
                ReportId = model.Reportid
            };

            ApiResponseDto<ReportGroupLinkDto> result = await _service.CreateReportGroupLinkAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Report group created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReportGroup(int groupid, int? reportid = null)
        {
            ApiResponseDto<bool> result = reportid.HasValue
                ? await _service.DeleteReportGroupLinkAsync(reportid.Value, groupid)
                : await _service.DeleteReportGroupAsync(groupid);

            return result.Success
                ? Json(new { success = true, message = "Report group deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditOtherReportGroupPartial(int? groupid = null)
        {
            var model = new ReportGroupItem();
            if (groupid.HasValue)
            {
                var result = await _service.GetReportGroupByIdAsync(groupid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReportGroupItem>(result.Data);
            }

            ViewBag.IsAddingNew = !groupid.HasValue;
            return PartialView("_AddEditOtherReportGroup", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOtherReportGroup(ReportGroupItem model, bool isEdit)
        {
            if (string.IsNullOrWhiteSpace(model.Description))
            {
                return Json(new { success = false, message = "Description is required." });
            }

            var dto = _mapper.Map<ReportGroupDto>(model);
            ApiResponseDto<ReportGroupDto> result = isEdit
                ? await _service.UpdateReportGroupAsync(model.GroupId, dto)
                : await _service.CreateReportGroupAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEdit ? "Report group updated successfully." : "Report group created successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.", errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOtherReportGroup(int groupid)
        {
            var result = await _service.DeleteReportGroupAsync(groupid);
            return result.Success
                ? Json(new { success = true, message = "Report group deleted successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.", errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB
        // ════════════════════════════════════════════════════════════════════════════

        [HttpPost]
        public async Task<IActionResult> LoadRadTrackProgsGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildRadTrackProgsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<RadTrackProgItem>> BuildRadTrackProgsGridAsync(PaginationFilter<string> request)
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedRadTrackProgsAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<RadTrackProgDto>();

            var items = _mapper.Map<List<RadTrackProgItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<RadTrackProgItem>
            {
                GridId = "pimsRadTrackProgTable",
                Title = "PIMS Programmes",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Program",
                AllowAdd = true,
                AddFunction = "addRadTrackProg",
                AllowEdit = true,
                EditFunction = "editRadTrackProg",
                AllowDelete = true,
                DeleteFunction = "deleteRadTrackProg",
                ExtraFilterMethod = "getRadTrackProgsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadRadTrackProgsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<RadTrackProgItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditRadTrackProgPartial(string? program = null)
        {
            var model = new RadTrackProgItem();
            if (!string.IsNullOrWhiteSpace(program))
            {
                var result = await _service.GetRadTrackProgByIdAsync(program);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<RadTrackProgItem>(result.Data);
            }

            var programsResult = await _service.GetRadTrackProgProgramsAsync();
            ViewBag.ProgramOptions = programsResult.Success && programsResult.Data != null
                ? programsResult.Data.Select(p => new SelectListItem { Value = p, Text = p }).ToList()
                : new List<SelectListItem>();

            ViewBag.IsAddingNew = string.IsNullOrWhiteSpace(program);
            return PartialView("_AddEditRadTrackProg", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRadTrackProg(RadTrackProgItem item, bool isEditMode = false)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<RadTrackProgDto>(item);

            ApiResponseDto<RadTrackProgDto> result = isEditMode
                ? await _service.UpdateRadTrackProgAsync(item.Program!, dto)
                : await _service.CreateRadTrackProgAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEditMode ? "Programme updated successfully." : "Programme created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.",
                    errors = result.Errors
                });
        }

        [AcceptVerbs("DELETE", "POST")]
        public async Task<IActionResult> DeleteRadTrackProg(string program)
        {
            var result = await _service.DeleteRadTrackProgAsync(program);
            return result.Success
                ? Json(new { success = true, message = "Programme deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                    errors = result.Errors
                });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB
        // ════════════════════════════════════════════════════════════════════════════

        // ── Project Manager Grid ─────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProjectManagersGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildProjectManagersGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectManagerItem>> BuildProjectManagersGridAsync(PaginationFilter<string> request)
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedProjectManagersAsync(query);

            var items = result.Success && result.Data != null
                ? _mapper.Map<List<ProjectManagerItem>>(result.Data.data.ToList())
                : new List<ProjectManagerItem>();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ProjectManagerItem>
            {
                GridId = "mgrTable",
                Title = "Manager",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Projectmanager",
                AllowAdd = true,
                AddFunction = "addProjectManager",
                AllowEdit = true,
                EditFunction = "editProjectManager",
                AllowDelete = true,
                DeleteFunction = "deleteProjectManager",
                AllowRowSelection = true,
                RowSelectFunction = "onProjectManagerRowSelect",
                ExtraFilterMethod = "getProjectManagersExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProjectManagersGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectManagerItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = result.Data?.TotalCount ?? items.Count,
                    PageNumber = result.Data?.PageNumber ?? request.Page,
                    PageSize = result.Data?.PageSize ?? request.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditProjectManagerPartial(string? projectmanager = null)
        {
            var model = new ProjectManagerItem();
            if (!string.IsNullOrWhiteSpace(projectmanager))
            {
                var result = await _service.GetProjectManagerByIdAsync(projectmanager);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ProjectManagerItem>(result.Data);
            }

            var managerNamesResult = await _service.GetManagerNamesAsync();
            var managerNames = managerNamesResult.Success && managerNamesResult.Data != null
                ? managerNamesResult.Data
                : new List<string>();

            if (!string.IsNullOrWhiteSpace(model.Projectmanager) &&
                !managerNames.Any(x => string.Equals(x, model.Projectmanager, StringComparison.OrdinalIgnoreCase)))
            {
                managerNames.Insert(0, model.Projectmanager);
            }

            ViewBag.ManagerNames = managerNames
                .Select(name => new SelectListItem
                {
                    Text = name,
                    Value = name,
                    Selected = string.Equals(name, model.Projectmanager, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            ViewBag.IsAddingNew = string.IsNullOrWhiteSpace(projectmanager);
            return PartialView("_AddEditProjectManager", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProjectManager(ProjectManagerItem item, bool isEditMode = false)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ProjectManagerDto>(item);
            ApiResponseDto<ProjectManagerDto> result = isEditMode
                ? await _service.UpdateProjectManagerAsync(item.Projectmanager!, dto)
                : await _service.CreateProjectManagerAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEditMode ? "Manager updated successfully." : "Manager created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProjectManager(string projectmanager)
        {
            var result = await _service.DeleteProjectManagerAsync(projectmanager);
            return result.Success
                ? Json(new { success = true, message = "Manager deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                    errors = result.Errors
                });
        }

        // ── Program Manager Link Sub-Grid ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProgramManagerLinksGrid(
            PaginationFilter<string> request, string? manager = null)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildProgramManagerLinksGridAsync(request, manager);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProgramManagerLinkItem>> BuildProgramManagerLinksGridAsync(
            PaginationFilter<string> request, string? manager)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var data = new PaginatedResult<ProgramManagerLinkDto>();

            if (!string.IsNullOrWhiteSpace(manager))
            {
                var query = new QueryParameters<string>
                {
                    Search = request.Search,
                    SortBy = request.SortBy,
                    Descending = request.Descending,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Filter = request.Filter
                };

                var result = await _service.GetPagedProgramManagerLinksByManagerAsync(query, manager);
                if (result.Success && result.Data != null)
                {
                    data = result.Data;
                }
            }

            var items = _mapper.Map<List<ProgramManagerLinkItem>>(data.data.ToList());

            return new DataGridConfig<ProgramManagerLinkItem>
            {
                GridId = "mgrProgramTable",
                Title = "Program",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Program",
                AllowAdd = true,
                AddFunction = "addProgramManagerLink",
                AllowEdit = false,
                EditFunction = "editProgramManagerLink",
                AllowDelete = true,
                DeleteFunction = "deleteProgramManagerLink",
                ExtraFilterMethod = "getProgramManagerLinksExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProgramManagerLinksGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProgramManagerLinkItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditProgramManagerLinkPartial(string? manager = null, string? program = null)
        {
            var model = new ProgramManagerLinkItem
            {
                Manager = manager
            };

            if (!string.IsNullOrWhiteSpace(manager) && !string.IsNullOrWhiteSpace(program))
            {
                var result = await _service.GetProgramManagerLinkByIdAsync(program, manager);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ProgramManagerLinkItem>(result.Data);
            }

            var programOptionsResult = await _service.GetProgramsAsync();
            ViewBag.ProgramOptions = programOptionsResult.Success && programOptionsResult.Data != null
                ? programOptionsResult.Data
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProgramNo,
                        Text = $"{x.ProgramNo} ({x.LatestYear})",
                        Selected = string.Equals(x.ProgramNo, model.Program, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList()
                : new List<SelectListItem>();

            ViewBag.IsEditMode = !string.IsNullOrWhiteSpace(program);
            ViewBag.OriginalProgram = program;
            return PartialView("_AddEditProgramManagerLink", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProgramManagerLink([FromBody] ProgramManagerLinkDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.CreateProgramManagerLinkAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Programme assignment added successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.",
                    errors = result.Errors
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgramManagerLink([FromBody] ProgramManagerLinkUpdateReq req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Program) || string.IsNullOrWhiteSpace(req.Manager)
                || string.IsNullOrWhiteSpace(req.OriginalProgram) || string.IsNullOrWhiteSpace(req.OriginalManager))
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (string.Equals(req.Program, req.OriginalProgram, StringComparison.OrdinalIgnoreCase)
                && string.Equals(req.Manager, req.OriginalManager, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, message = "Programme assignment updated successfully." });
            }

            var createResult = await _service.CreateProgramManagerLinkAsync(new ProgramManagerLinkDto
            {
                Program = req.Program,
                Manager = req.Manager
            });

            if (!createResult.Success)
            {
                return Json(new
                {
                    success = false,
                    message = createResult.Errors?.FirstOrDefault()?.Message ?? "Update failed.",
                    errors = createResult.Errors
                });
            }

            var deleteOldResult = await _service.DeleteProgramManagerLinkAsync(req.OriginalProgram, req.OriginalManager);
            if (!deleteOldResult.Success)
            {
                await _service.DeleteProgramManagerLinkAsync(req.Program, req.Manager);
                return Json(new
                {
                    success = false,
                    message = deleteOldResult.Errors?.FirstOrDefault()?.Message ?? "Update failed.",
                    errors = deleteOldResult.Errors
                });
            }

            return Json(new { success = true, message = "Programme assignment updated successfully." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProgramManagerLink(string program, string manager)
        {
            var result = await _service.DeleteProgramManagerLinkAsync(program, manager);
            return result.Success
                ? Json(new { success = true, message = "Programme assignment deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                    errors = result.Errors
                });
        }

        // ── Profit Centre Manager Link Sub-Grid ──────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadProfitCentreManagerLinksGrid(
            PaginationFilter<string> request, string? manager = null)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildProfitCentreManagerLinksGridAsync(request, manager);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProfitCentreManagerLinkItem>> BuildProfitCentreManagerLinksGridAsync(
            PaginationFilter<string> request, string? manager)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var data = new PaginatedResult<ProfitCentreManagerLinkDto>();

            if (!string.IsNullOrWhiteSpace(manager))
            {
                var query = new QueryParameters<string>
                {
                    Search = request.Search,
                    SortBy = request.SortBy,
                    Descending = request.Descending,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Filter = request.Filter
                };

                var result = await _service.GetPagedProfitCentreManagerLinksByManagerAsync(query, manager);
                if (result.Success && result.Data != null)
                {
                    data = result.Data;
                }
            }

            var items = _mapper.Map<List<ProfitCentreManagerLinkItem>>(data.data.ToList());

            return new DataGridConfig<ProfitCentreManagerLinkItem>
            {
                GridId = "mgrResourceTable",
                Title = "Resource Centre",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "ProfitCentre",
                AllowAdd = true,
                AddFunction = "addProfitCentreManagerLink",
                AllowEdit = false,
                EditFunction = "editProfitCentreManagerLink",
                AllowDelete = true,
                DeleteFunction = "deleteProfitCentreManagerLink",
                ExtraFilterMethod = "getProfitCentreManagerLinksExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadProfitCentreManagerLinksGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProfitCentreManagerLinkItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditProfitCentreManagerLinkPartial(string? manager = null, string? profitcentre = null)
        {
            var model = new ProfitCentreManagerLinkItem
            {
                Manager = manager
            };

            if (!string.IsNullOrWhiteSpace(manager) && !string.IsNullOrWhiteSpace(profitcentre))
            {
                var result = await _service.GetProfitCentreManagerLinkByIdAsync(profitcentre, manager);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ProfitCentreManagerLinkItem>(result.Data);
            }

            var profitCentreOptionsResult = await _service.GetProfitCentresAsync();
            ViewBag.ProfitCentreOptions = profitCentreOptionsResult.Success && profitCentreOptionsResult.Data != null
                ? profitCentreOptionsResult.Data
                    .Select(x => new SelectListItem
                    {
                        Value = x.ProfitCentre,
                        Text = $"{x.ProfitCentre} ({x.LatestYear})",
                        Selected = string.Equals(x.ProfitCentre, model.ProfitCentre, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList()
                : new List<SelectListItem>();

            ViewBag.IsEditMode = !string.IsNullOrWhiteSpace(profitcentre);
            ViewBag.OriginalProfitCentre = profitcentre;
            return PartialView("_AddEditProfitCentreManagerLink", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfitCentreManagerLink([FromBody] ProfitCentreManagerLinkDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.CreateProfitCentreManagerLinkAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Resource Centre assignment added successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.",
                    errors = result.Errors
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfitCentreManagerLink([FromBody] ProfitCentreManagerLinkUpdateReq req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.ProfitCentre) || string.IsNullOrWhiteSpace(req.Manager)
                || string.IsNullOrWhiteSpace(req.OriginalProfitCentre) || string.IsNullOrWhiteSpace(req.OriginalManager))
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            if (string.Equals(req.ProfitCentre, req.OriginalProfitCentre, StringComparison.OrdinalIgnoreCase)
                && string.Equals(req.Manager, req.OriginalManager, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = true, message = "Resource Centre assignment updated successfully." });
            }

            var createResult = await _service.CreateProfitCentreManagerLinkAsync(new ProfitCentreManagerLinkDto
            {
                ProfitCentre = req.ProfitCentre,
                Manager = req.Manager
            });

            if (!createResult.Success)
            {
                return Json(new
                {
                    success = false,
                    message = createResult.Errors?.FirstOrDefault()?.Message ?? "Update failed.",
                    errors = createResult.Errors
                });
            }

            var deleteOldResult = await _service.DeleteProfitCentreManagerLinkAsync(req.OriginalProfitCentre, req.OriginalManager);
            if (!deleteOldResult.Success)
            {
                await _service.DeleteProfitCentreManagerLinkAsync(req.ProfitCentre, req.Manager);
                return Json(new
                {
                    success = false,
                    message = deleteOldResult.Errors?.FirstOrDefault()?.Message ?? "Update failed.",
                    errors = deleteOldResult.Errors
                });
            }

            return Json(new { success = true, message = "Resource Centre assignment updated successfully." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProfitCentreManagerLink(string profitcentre, string manager)
        {
            var result = await _service.DeleteProfitCentreManagerLinkAsync(profitcentre, manager);
            return result.Success
                ? Json(new { success = true, message = "Resource Centre assignment deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                    errors = result.Errors
                });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  TIME TAB (Settings)
        // ════════════════════════════════════════════════════════════════════════════

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSetting([FromBody] SettingDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Id))
                return Json(new { success = false, message = "Invalid data" });

            var result = await _service.UpdateSettingAsync(dto.Id, dto);
            return result.Success
                ? Json(new { success = true, message = "Setting updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB
        // ════════════════════════════════════════════════════════════════════════════

        private async Task<int> ResolveDefaultAccessSystemIdAsync()
        {
            var systemsResult = await _service.GetAllAccessSystemsAsync();
            if (systemsResult is { Success: true, Data: not null } && systemsResult.Data.Count > 0)
            {
                var pimsSystem = systemsResult.Data.FirstOrDefault(x =>
                    x.SystemName != null && x.SystemName.Equals("PIMS", StringComparison.OrdinalIgnoreCase));

                return pimsSystem?.SystemId ?? systemsResult.Data[0].SystemId;
            }

            return 1;
        }

        // ── Access Users Grid ────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadAccessUsersGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildAccessUsersGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccessUserItem>> BuildAccessUsersGridAsync(PaginationFilter<string> request)
        {
            var systemId = await ResolveDefaultAccessSystemIdAsync();

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            filterDict["SystemId"] = systemId.ToString();

            var query = new QueryParameters<string>
            {
                Page       = request.Page,
                PageSize   = request.PageSize,
                SortBy     = request.SortBy,
                Descending = request.Descending,
                Search     = request.Search,
                Filter     = JsonConvert.SerializeObject(filterDict)
            };

            var result = await _service.GetPagedAccessUsersAsync(query);

            var data = result is { Success: true, Data: not null }
                ? result.Data
                : new PaginatedResult<AccessUserDto>();

            var items = _mapper.Map<List<AccessUserItem>>(data.data.ToList());

            return new DataGridConfig<AccessUserItem>
            {
                GridId = "adminUsersTable",
                Title = "Users",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "CompositeKey",
                AllowAdd = true,
                AddFunction = "addAccessUser",
                AllowEdit = true,
                EditFunction = "editAccessUser",
                AllowDelete = true,
                DeleteFunction = "deleteAccessUser",
                ExtraFilterMethod = "getAccessUsersExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadAccessUsersGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AccessUserItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords  = data.TotalCount,
                    PageNumber    = data.PageNumber,
                    PageSize      = data.PageSize,
                    SortColumn    = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditAccessUserPartial(int? systemid = null, string? ntlogin = null)
        {
            var model = new AccessUserItem();
            if (systemid.HasValue && !string.IsNullOrWhiteSpace(ntlogin))
            {
                var result = await _service.GetAccessUserByIdAsync(systemid.Value, ntlogin);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<AccessUserItem>(result.Data);
            }

            ViewBag.IsAddingNew = !systemid.HasValue;
            return PartialView("_AddEditAccessUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAccessUser(
            AccessUserItem item,
            bool isEditMode = false,
            int? originalSystemid = null,
            string? originalNtlogin = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var shouldUpdate = isEditMode || item.SystemId > 0;
            if (!shouldUpdate && item.SystemId <= 0)
            {
                item.SystemId = await ResolveDefaultAccessSystemIdAsync();
            }

            var dto = _mapper.Map<AccessUserDto>(item);

            var routeSystemId = shouldUpdate ? (originalSystemid ?? item.SystemId) : item.SystemId;
            var routeNtLogin = shouldUpdate
                ? (string.IsNullOrWhiteSpace(originalNtlogin) ? item.NtLogin! : originalNtlogin)
                : item.NtLogin!;

            ApiResponseDto<AccessUserDto> result = shouldUpdate
                ? await _service.UpdateAccessUserAsync(routeSystemId, routeNtLogin, dto)
                : await _service.CreateAccessUserAsync(dto);

            return result.Success
                ? Json(new { success = true, message = shouldUpdate ? "User updated successfully." : "User added successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.",
                    errors = result.Errors
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccessUser(int systemid, string ntlogin)
        {
            var accessLevelsResult = await _service.GetAccessUserLevelsByUserAsync(systemid, ntlogin);
            if (accessLevelsResult is { Success: true, Data: not null } && accessLevelsResult.Data.Count > 0)
            {
                return Json(new
                {
                    success = false,
                    message = "This user has User Access references. Delete related records from the User Access grid first, then delete the user."
                });
            }

            var result = await _service.DeleteAccessUserAsync(systemid, ntlogin);
            return result.Success
                ? Json(new { success = true, message = "User deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed.",
                    errors = result.Errors
                });
        }

        // ── Access User Levels Grid ──────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadAccessUserLevelsGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildAccessUserLevelsGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccessUserLevelItem>> BuildAccessUserLevelsGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var systemId = await ResolveDefaultAccessSystemIdAsync();
            filterDict["SystemId"] = systemId.ToString();

            var query = new QueryParameters<string>
            {
                Page       = request.Page,
                PageSize   = request.PageSize,
                SortBy     = request.SortBy,
                Descending = request.Descending,
                Search     = request.Search,
                Filter     = JsonConvert.SerializeObject(filterDict)
            };

            var result = await _service.GetPagedAccessUserLevelsAsync(query);

            var data = result is { Success: true, Data: not null }
                ? result.Data
                : new PaginatedResult<AccessUserLevelDto>();

            var items = _mapper.Map<List<AccessUserLevelItem>>(data.data.ToList());

            var usersResult = await _service.GetAccessUsersBySystemIdAsync(systemId);
            var userNames = usersResult is { Success: true, Data: not null }
                ? usersResult.Data.ToDictionary(x => x.NtLogin, x => string.IsNullOrWhiteSpace(x.UserName) ? x.NtLogin : x.UserName)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var accessLevelsResult = await _service.GetAllAccessLevelsAsync();
            var accessLevelNames = accessLevelsResult is { Success: true, Data: not null }
                ? accessLevelsResult.Data
                    .GroupBy(x => (x.SystemId, x.AccessLevelId))
                    .ToDictionary(g => g.Key, g => g.First().AccessLevelName ?? string.Empty)
                : new Dictionary<(int SystemId, int AccessLevelId), string>();

            foreach (var item in items)
            {
                if (!string.IsNullOrWhiteSpace(item.NtLogin) && userNames.TryGetValue(item.NtLogin, out var userName))
                {
                    item.UserName = userName;
                }
                else
                {
                    item.UserName = item.NtLogin;
                }

                if (accessLevelNames.TryGetValue((item.SystemId, item.AccessLevelId), out var levelName))
                {
                    item.AccessLevelName = levelName;
                }
            }

            return new DataGridConfig<AccessUserLevelItem>
            {
                GridId = "adminAccessTable",
                Title = "User Access",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "CompositeKey",
                AllowAdd = true,
                AddFunction = "addAccessUserLevel",
                AllowEdit = true,
                EditFunction = "editAccessUserLevel",
                AllowDelete = true,
                DeleteFunction = "deleteAccessUserLevel",
                ExtraFilterMethod = "getAccessUserLevelsExtraFilters",
                BindGridUrl = "/PIMS/Maintenance/LoadAccessUserLevelsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AccessUserLevelItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber   = data.PageNumber,
                    PageSize     = data.PageSize,
                    SortColumn   = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditAccessUserLevelPartial(
            int? systemid = null, string? ntlogin = null, int? accesslevelid = null)
        {
            var model = new AccessUserLevelItem();
            if (systemid.HasValue && !string.IsNullOrWhiteSpace(ntlogin) && accesslevelid.HasValue)
            {
                var result = await _service.GetAccessUserLevelByIdAsync(systemid.Value, ntlogin, accesslevelid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<AccessUserLevelItem>(result.Data);
            }
            else
            {
                model.SystemId = await ResolveDefaultAccessSystemIdAsync();
            }

            var usersResult = await _service.GetAccessUsersBySystemIdAsync(model.SystemId);
            ViewBag.AccessUserOptions = usersResult is { Success: true, Data: not null }
                ? usersResult.Data
                    .Select(x => new SelectListItem
                    {
                        Value = x.NtLogin,
                        Text = string.IsNullOrWhiteSpace(x.UserName) ? x.NtLogin : x.UserName,
                        Selected = string.Equals(x.NtLogin, model.NtLogin, StringComparison.OrdinalIgnoreCase)
                    })
                    .OrderBy(x => x.Text)
                    .ToList()
                : new List<SelectListItem>();

            var accessLevelsResult = await _service.GetAllAccessLevelsAsync();
            ViewBag.AccessLevelOptions = accessLevelsResult is { Success: true, Data: not null }
                ? accessLevelsResult.Data
                    .Where(x => x.SystemId == model.SystemId)
                    .Select(x => new SelectListItem
                    {
                        Value = x.AccessLevelId.ToString(),
                        Text = string.IsNullOrWhiteSpace(x.AccessLevelName) ? x.AccessLevelId.ToString() : x.AccessLevelName,
                        Selected = x.AccessLevelId == model.AccessLevelId
                    })
                    .OrderBy(x => x.Text)
                    .ToList()
                : new List<SelectListItem>();

            ViewBag.IsAddingNew = !systemid.HasValue;
            return PartialView("_AddEditAccessUserLevel", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAccessUserLevel([FromBody] AccessUserLevelItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            if (item.IsEditMode)
            {
                var oldSystemId = item.OriginalSystemId ?? item.SystemId;
                var oldNtLogin = string.IsNullOrWhiteSpace(item.OriginalNtLogin) ? item.NtLogin : item.OriginalNtLogin;
                var oldAccessLevelId = item.OriginalAccessLevelId ?? item.AccessLevelId;

                if (!string.Equals(oldNtLogin, item.NtLogin, StringComparison.OrdinalIgnoreCase)
                    || oldSystemId != item.SystemId
                    || oldAccessLevelId != item.AccessLevelId)
                {
                    var deleteOldResult = await _service.DeleteAccessUserLevelAsync(oldSystemId, oldNtLogin!, oldAccessLevelId);
                    if (!deleteOldResult.Success)
                    {
                        return Json(new
                        {
                            success = false,
                            message = deleteOldResult.Errors?.FirstOrDefault()?.Message ?? "Failed to update user access.",
                            errors = deleteOldResult.Errors
                        });
                    }
                }
                else
                {
                    return Json(new { success = true, message = "User access updated successfully." });
                }
            }

            var dto = _mapper.Map<AccessUserLevelDto>(item);
            var result = await _service.CreateAccessUserLevelAsync(dto);

            return result.Success
                ? Json(new { success = true, message = item.IsEditMode ? "User access updated successfully." : "User access added successfully." })
                : Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.", errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccessUserLevel(int systemid, string ntlogin, int accesslevelid)
        {
            var result = await _service.DeleteAccessUserLevelAsync(systemid, ntlogin, accesslevelid);
            return result.Success
                ? Json(new { success = true, message = "User access deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Frequencies
        // ════════════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetFrequenciesGrid(string gridId = "frequenciesGrid")
        {
            var gridConfig = await BuildFrequenciesGridAsync(new PaginationFilter<string>(), gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadFrequenciesGrid(PaginationFilter<string> request, string gridId = "frequenciesGrid")
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildFrequenciesGridAsync(request, gridId);
            return PartialView("_DataGrid", gridConfig);
        }


        private async Task<DataGridConfig<FrequencyItem>> BuildFrequenciesGridAsync(PaginationFilter<string> request, string gridId = "frequenciesGrid")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedFrequenciesAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<FrequencyDto>();

            var items = _mapper.Map<List<FrequencyItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<FrequencyItem>
            {
                GridId = gridId,
                Title = "Frequency",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Frequencyid",
                AllowAdd = true,
                AddFunction = "addFrequency",
                AllowEdit = true,
                EditFunction = "editFrequency",
                AllowDelete = true,
                DeleteFunction = "deleteFrequency",
                ExtraFilterMethod = gridId == "otherValuesTable" ? "getOtherValuesExtraFilters" : "getFrequenciesExtraFilters",
                BindGridUrl = $"/PIMS/Maintenance/LoadFrequenciesGrid?gridId={Uri.EscapeDataString(gridId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<FrequencyItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditFrequencyPartial(int? frequencyid = null)
        {
            var model = new FrequencyItem();
            if (frequencyid.HasValue)
            {
                var result = await _service.GetFrequencyByIdAsync(frequencyid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<FrequencyItem>(result.Data);
            }
            ViewBag.IsAddingNew = !frequencyid.HasValue;
            return PartialView("_AddEditFrequency", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveFrequency(FrequencyItem item, bool isEdit)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<FrequencyDto>(item);
            ApiResponseDto<FrequencyDto> result = isEdit
                ? await _service.UpdateFrequencyAsync(item.Frequencyid, dto)
                : await _service.CreateFrequencyAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEdit ? "Frequency updated successfully." : "Frequency created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFrequency(int frequencyid)
        {
            var result = await _service.DeleteFrequencyAsync(frequencyid);
            return result.Success
                ? Json(new { success = true, message = "Frequency deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Review Items
        // ════════════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetReviewItemsGrid(string gridId = "reviewItemsGrid")
        {
            var gridConfig = await BuildReviewItemsGridAsync(new PaginationFilter<string>(), gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadReviewItemsGrid(PaginationFilter<string> request, string gridId = "reviewItemsGrid")
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildReviewItemsGridAsync(request, gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ReviewItemItem>> BuildReviewItemsGridAsync(PaginationFilter<string> request, string gridId = "reviewItemsGrid")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedReviewItemsAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<ReviewItemDto>();

            var items = _mapper.Map<List<ReviewItemItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<ReviewItemItem>
            {
                GridId = gridId,
                Title = "Review Items",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Itemid",
                AllowAdd = true,
                AddFunction = "addReviewItem",
                AllowEdit = true,
                EditFunction = "editReviewItem",
                AllowDelete = true,
                DeleteFunction = "deleteReviewItem",
                ExtraFilterMethod = gridId == "otherValuesTable" ? "getOtherValuesExtraFilters" : "getReviewItemsExtraFilters",
                BindGridUrl = $"/PIMS/Maintenance/LoadReviewItemsGrid?gridId={Uri.EscapeDataString(gridId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ReviewItemItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditReviewItemPartial(int? itemid = null)
        {
            var model = new ReviewItemItem();
            if (itemid.HasValue)
            {
                var result = await _service.GetReviewItemByIdAsync(itemid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<ReviewItemItem>(result.Data);
            }
            ViewBag.IsAddingNew = !itemid.HasValue;
            return PartialView("_AddEditReviewItem", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReviewItem(ReviewItemItem reviewItem, bool isEdit)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<ReviewItemDto>(reviewItem);
            ApiResponseDto<ReviewItemDto> result = isEdit
                ? await _service.UpdateReviewItemAsync(reviewItem.Itemid, dto)
                : await _service.CreateReviewItemAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEdit ? "Review item updated successfully." : "Review item created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteReviewItem(int itemid)
        {
            var result = await _service.DeleteReviewItemAsync(itemid);
            return result.Success
                ? Json(new { success = true, message = "Review item deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Risk Ratings
        // ════════════════════════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetRisksGrid(string gridId = "risksGrid")
        {
            var gridConfig = await BuildRisksGridAsync(new PaginationFilter<string>(), gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadRisksGrid(PaginationFilter<string> request, string gridId = "risksGrid")
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildRisksGridAsync(request, gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<RiskItem>> BuildRisksGridAsync(PaginationFilter<string> request, string gridId = "risksGrid")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedRiskRatingsAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<RiskDto>();

            var items = _mapper.Map<List<RiskItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<RiskItem>
            {
                GridId = gridId,
                Title = "Risk Ratings",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Riskid",
                AllowAdd = true,
                AddFunction = "addRisk",
                AllowEdit = true,
                EditFunction = "editRisk",
                AllowDelete = true,
                DeleteFunction = "deleteRisk",
                ExtraFilterMethod = gridId == "otherValuesTable" ? "getOtherValuesExtraFilters" : "getRisksExtraFilters",
                BindGridUrl = $"/PIMS/Maintenance/LoadRisksGrid?gridId={Uri.EscapeDataString(gridId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<RiskItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditRiskPartial(int? riskid = null)
        {
            var model = new RiskItem();
            if (riskid.HasValue)
            {
                var result = await _service.GetRiskRatingByIdAsync(riskid.Value);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<RiskItem>(result.Data);
            }
            ViewBag.IsAddingNew = !riskid.HasValue;
            return PartialView("_AddEditRisk", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRisk(RiskItem riskItem, bool isEdit)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<RiskDto>(riskItem);
            ApiResponseDto<RiskDto> result = isEdit
                ? await _service.UpdateRiskRatingAsync(riskItem.Riskid, dto)
                : await _service.CreateRiskRatingAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEdit ? "Risk rating updated successfully." : "Risk rating created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteRisk(int riskid)
        {
            var result = await _service.DeleteRiskRatingAsync(riskid);
            return result.Success
                ? Json(new { success = true, message = "Risk rating deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        // ════════════════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — Publication Types
        // ════════════════════════════════════════════════════════════════════════════════════════

        [HttpGet]
        public async Task<IActionResult> GetPublicationTypesGrid(string gridId = "publicationTypesGrid")
        {
            var gridConfig = await BuildPublicationTypesGridAsync(new PaginationFilter<string>(), gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadPublicationTypesGrid(PaginationFilter<string> request, string gridId = "publicationTypesGrid")
        {
            if (!ModelState.IsValid)
                return BadModelStateResult();

            var gridConfig = await BuildPublicationTypesGridAsync(request, gridId);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<PublicationTypeItem>> BuildPublicationTypesGridAsync(PaginationFilter<string> request, string gridId = "publicationTypesGrid")
        {
            var query = new QueryParameters<string>
            {
                Search = request.Search,
                SortBy = request.SortBy,
                Descending = request.Descending,
                Page = request.Page,
                PageSize = request.PageSize,
                Filter = request.Filter
            };

            var result = await _service.GetPagedPublicationTypesAsync(query);

            var data = result.Success && result.Data != null
                ? result.Data
                : new PaginatedResult<PublicationTypeDto>();

            var items = _mapper.Map<List<PublicationTypeItem>>(data.data.ToList());

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            return new DataGridConfig<PublicationTypeItem>
            {
                GridId = gridId,
                Title = "Publication Types",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Type",
                AllowAdd = true,
                AddFunction = "addPublicationType",
                AllowEdit = true,
                EditFunction = "editPublicationType",
                AllowDelete = true,
                DeleteFunction = "deletePublicationType",
                ExtraFilterMethod = gridId == "otherValuesTable" ? "getOtherValuesExtraFilters" : "getPublicationTypesExtraFilters",
                BindGridUrl = $"/PIMS/Maintenance/LoadPublicationTypesGrid?gridId={Uri.EscapeDataString(gridId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PublicationTypeItem>(),
                Pagination = new PaginationModel
                {
                    TotalRecords = data.TotalCount,
                    PageNumber = data.PageNumber,
                    PageSize = data.PageSize,
                    SortColumn = request.SortBy,
                    SortDirection = request.Descending
                },
                CurrentFilters = filterDict
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAddEditPublicationTypePartial(string? type = null)
        {
            var model = new PublicationTypeItem();
            if (!string.IsNullOrEmpty(type))
            {
                var result = await _service.GetPublicationTypeByCodeAsync(type);
                if (result is { Success: true, Data: not null })
                    model = _mapper.Map<PublicationTypeItem>(result.Data);
            }
            ViewBag.IsAddingNew = string.IsNullOrEmpty(type);
            return PartialView("_AddEditPublicationType", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePublicationType(PublicationTypeItem publicationTypeItem, bool isEdit)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            var dto = _mapper.Map<PublicationTypeDto>(publicationTypeItem);

            ApiResponseDto<PublicationTypeDto> result = isEdit
                ? await _service.UpdatePublicationTypeAsync(publicationTypeItem.Type, dto)
                : await _service.CreatePublicationTypeAsync(dto);

            return result.Success
                ? Json(new { success = true, message = isEdit ? "Publication type updated successfully." : "Publication type created successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePublicationType(string type)
        {
            var result = await _service.DeletePublicationTypeAsync(type);
            return result.Success
                ? Json(new { success = true, message = "Publication type deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }
    }
}
