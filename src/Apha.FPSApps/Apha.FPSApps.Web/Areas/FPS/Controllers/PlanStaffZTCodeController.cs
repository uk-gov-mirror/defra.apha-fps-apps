using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class PlanStaffZTCodeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPlanStaffZTCodeService _planStaffZTCodeService;

        public PlanStaffZTCodeController(IMapper mapper, IPlanStaffZTCodeService planStaffZTCodeService)
        {
            _mapper = mapper;
            _planStaffZTCodeService = planStaffZTCodeService;
        }

        public async Task<IActionResult> Index(string staffId, string? source = null)
        {
            var model = new PlanStaffZTCodePageViewModel
            {
                StaffId      = staffId ?? string.Empty,
                ReturnToSsr  = string.Equals(source, "ssr", StringComparison.OrdinalIgnoreCase)
            };

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                var staffResult = await _planStaffZTCodeService.GetStaffSummaryByIdAsync(staffId);
                if (staffResult.Success && staffResult.Data != null)
                {
                    model.Name = staffResult.Data.Name;
                    model.WorkGroupGrade = staffResult.Data.WorkGroupGrade;
                    model.HrsPaid = staffResult.Data.HrsPaid;
                    model.Leave = staffResult.Data.Leave;
                    model.SickSpecial = staffResult.Data.SickSpecial;
                    model.HrsAvail = staffResult.Data.HrsAvail;
                }
                var ztTotalResult = await _planStaffZTCodeService.GetZtTotalHoursByStaffIdAsync(staffId);
                if (ztTotalResult.Success)
                {
                    model.PlannedAdminZT = ztTotalResult.Data;
                }
            }

            model.GridConfig = await GetZtGridConfigAsync(staffId ?? string.Empty);
            return View(model);
        }

        /// <summary>
        /// Returns time-summary data for a given staffId as JSON (used for live refresh).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStaffSummary(string staffId)
        {
            if (string.IsNullOrWhiteSpace(staffId))
                return Json(new { success = false, message = "StaffId is required." });

            var staffResult = await _planStaffZTCodeService.GetStaffSummaryByIdAsync(staffId);
            var ztResult = await _planStaffZTCodeService.GetZtTotalHoursByStaffIdAsync(staffId);

            if (!staffResult.Success || staffResult.Data == null)
                return Json(new { success = false, message = "Staff not found." });

            var data = staffResult.Data;
            var ztTotal = ztResult.Success ? ztResult.Data : 0;

            return Json(new
            {
                success = true,
                name = data.Name,
                workGroupGrade = data.WorkGroupGrade,
                hrsPaid = data.HrsPaid,
                leave = data.Leave,
                sickSpecial = data.SickSpecial,
                hrsAvail = data.HrsAvail,
                plannedAdminZT = ztTotal,
                freeForChargeableWork = data.HrsAvail - ztTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadZtGrid(PaginationFilter<string> request, string? staffId = null)
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

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var gridConfig = await GetZtGridConfigAsync(staffId ?? string.Empty, queryParameters, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetZtCodes()
        {
            var result = await _planStaffZTCodeService.GetZtJobCodesAsync();
            if (result.Success && result.Data != null)
            {
                var items = result.Data.Select(j => new { value = j.JobCode, text = j.Description ?? j.JobCode });

                return Json(new { success = true, data = items });
            }
            return Json(new { success = false, message = "Failed to load ZT codes." });
        }

        [HttpGet]
        public async Task<IActionResult> Create(string? jobCode = null)
        {
            var model = new PlanStaffZTCodeItemViewModel
            {
                JobCode = jobCode ?? string.Empty
            };
            await PopulateZtDropdownAsync(model);
            return PartialView("_AddEditPlanStaffZTCode", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlanStaffZTCodeItemViewModel item)
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

            var errors = new List<object>();

            if (string.IsNullOrWhiteSpace(item.StaffID))
                errors.Add(new { field = nameof(item.StaffID), message = "StaffID is required." });

            if (string.IsNullOrWhiteSpace(item.JobCode))
                errors.Add(new { field = nameof(item.JobCode), message = "JobCode is required." });

            if (errors.Count > 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors
                });
            }

            var dto = new StaffJobDto
            {
                StaffId = item.StaffID,
                JobCode = item.JobCode,
                PlannedHours = item.PlannedHours
            };
            var result = await _planStaffZTCodeService.CreateStaffJobAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "ZT plan entry created successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create ZT plan entry.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string staffId, string? jobCode = null)
        {
            var result = await _planStaffZTCodeService.GetZtStaffJobDetailsByIdAsync(staffId, jobCode ?? string.Empty);

            if (result.Success && result.Data != null)
            {
                var model = new PlanStaffZTCodeItemViewModel
                {
                    StaffID = result.Data.StaffID!,
                    JobCode = result.Data.JobCode!,
                    OriginalJobCode = result.Data.JobCode,
                    ZtDescription = result.Data.ZtDescription ?? result.Data.Name,
                    PlannedHours = result.Data.PlannedHours
                };
                await PopulateZtDropdownAsync(model);
                return PartialView("_AddEditPlanStaffZTCode", model);
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to retrieve ZT plan entry."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] PlanStaffZTCodeItemViewModel item)
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

            var errors = new List<object>();

            if (string.IsNullOrWhiteSpace(item.StaffID))
                errors.Add(new { field = nameof(item.StaffID), message = "StaffID is required." });

            if (string.IsNullOrWhiteSpace(item.JobCode))
                errors.Add(new { field = nameof(item.JobCode), message = "JobCode is required." });

            if (errors.Count > 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors
                });
            }

            var dto = new StaffJobDto
            {
                StaffId = item.StaffID,
                JobCode = item.JobCode,
                OriginalJobCode = item.OriginalJobCode,
                PlannedHours = item.PlannedHours
            };
            var result = await _planStaffZTCodeService.UpdateStaffJobAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "ZT plan entry updated successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update ZT plan entry.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string staffId, string jobCode)
        {
            var result = await _planStaffZTCodeService.DeleteStaffJobAsync(staffId, jobCode);

            if (result.Success)
            {
                return Json(new { success = true, message = "ZT plan entry deleted successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete ZT plan entry.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateZtDropdownAsync(PlanStaffZTCodeItemViewModel model)
        {
            var ztResult = await _planStaffZTCodeService.GetZtJobCodesAsync();
            model.ZtCodeList = ztResult.Data == null ? new List<SelectListItem>() :
                ztResult.Data.Select(j => new SelectListItem
                {
                    Value = j.JobCode,
                    Text = j.Description,
                    Selected = string.Equals(model.JobCode, j.JobCode, StringComparison.OrdinalIgnoreCase)
                }).ToList();

            // Set description from the matched ZT code when not already populated (edit mode)
            if (string.IsNullOrEmpty(model.ZtDescription) && !string.IsNullOrEmpty(model.JobCode) && ztResult.Data != null)
            {
                var matched = ztResult.Data.FirstOrDefault(j => string.Equals(j.JobCode, model.JobCode, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    model.ZtDescription = matched.Description;
                }
            }
        }

        private async Task<DataGridConfig<PlanStaffZTCodeItemViewModel>> GetZtGridConfigAsync(
            string staffId,
            QueryParameters<string>? query = null,
            Dictionary<string, string>? filterDict = null)
        {
            var items = new List<PlanStaffZTCodeItemViewModel>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrWhiteSpace(staffId))
            {
                var rowsResult = await _planStaffZTCodeService.GetZtStaffJobsByStaffIdPagedAsync(
                    query ?? new QueryParameters<string>(), staffId);

                if (rowsResult.Data != null)
                {
                    items = rowsResult.Data.Select(d => new PlanStaffZTCodeItemViewModel
                    {
                        StaffID = staffId,
                        JobCode = d.JobCode ?? string.Empty,
                        ZtDescription = d.ZtDescription ?? d.Name,
                        PlannedHours = d.PlannedHours
                    }).ToList();
                }
                paginationModel = _mapper.Map<PaginationModel>(rowsResult.Pagination) ?? new PaginationModel();
                paginationModel.SortColumn = query?.SortBy;
                paginationModel.SortDirection = query?.Descending ?? false;
            }

            return new DataGridConfig<PlanStaffZTCodeItemViewModel>
            {
                GridId = "ztCodesGrid",
                Title = "ZT Code Plan",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "JobCode",
                AddFunction = "addZtPlan",
                EditFunction = "editZtPlan",
                DeleteFunction = "deleteZtPlan",
                BindGridUrl = $"/FPS/PlanStaffZTCode/LoadZtGrid?staffId={Uri.EscapeDataString(staffId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PlanStaffZTCodeItemViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
