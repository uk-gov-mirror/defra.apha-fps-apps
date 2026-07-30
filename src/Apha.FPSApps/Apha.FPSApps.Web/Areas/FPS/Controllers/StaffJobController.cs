using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
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
    public class StaffJobController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IStaffJobService _staffJobService;

        public StaffJobController(IMapper mapper, IStaffJobService staffJobService)
        {
            _mapper = mapper;
            _staffJobService = staffJobService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffJobGrid(PaginationFilter<string> request, string? jobCode = null, string? title = null)
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter!);
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var staffJobPagedData = await _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode ?? string.Empty);
            List<StaffJobItemViewModel> staffJobItems = new List<StaffJobItemViewModel>();
            if (staffJobPagedData.Data != null)
            {
                staffJobItems = _mapper.Map<List<StaffJobItemViewModel>>(staffJobPagedData.Data.ToList());
            }
            var paginationModel = _mapper.Map<PaginationModel>(staffJobPagedData.Pagination)
                 ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridTitle = title ?? "Staff Booked";
            var staffJobGridConfig = new DataGridConfig<StaffJobItemViewModel>
            {
                GridId = "staffBookedGrid",
                Title = gridTitle,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "StaffID",
                AddFunction = "addStaffJob",
                EditFunction = "editStaffJob",
                DeleteFunction = "deleteStaffJob",
                ExtraFilterMethod = "getStaffJobExtraFilters",
                BindGridUrl = $"/FPS/StaffJob/LoadStaffJobGrid?title={Uri.EscapeDataString(gridTitle)}",
                Data = staffJobItems,
                Columns = GridDataProvider.GetColumnsDefination<StaffJobItemViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", staffJobGridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            StaffJobItemViewModel model = new StaffJobItemViewModel();
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditStaffJob", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StaffJobItemViewModel staffJobItem)
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

            var staffJob = new StaffJobDto
            {
                StaffId = staffJobItem.StaffID ?? string.Empty,
                JobCode = staffJobItem.JobCode,
                PlannedHours = staffJobItem.PlannedHours
            };
            var result = await _staffJobService.CreateStaffJobAsync(staffJob);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff job created successfully" });
            }

            var duplicateError = (result.Errors ?? new List<ApiErrorDto>())
                .FirstOrDefault(e => IsDuplicateError(e));

            if (duplicateError != null)
            {
                const string friendlyMessage = "This staff member has already been added to this project. Please update the existing entry instead.";
                return Json(new
                {
                    success = false,
                    message = friendlyMessage,
                    errors = new[] { new { field = string.Empty, message = friendlyMessage } }
                });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Staff cost planned hours.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private static bool IsDuplicateError(ApiErrorDto error)
        {
            var code = error.Code ?? string.Empty;
            if (code.Equals("CONFLICT", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("DUPLICATE", StringComparison.OrdinalIgnoreCase) ||
                code.Equals("BUSINESS_RULE_VIOLATION", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (error.Message ?? string.Empty).Contains("already exists", StringComparison.OrdinalIgnoreCase);
        }  

        [HttpGet]
        public async Task<IActionResult> Edit(string staffId, string? jobCode = null)
        {
            var result = await _staffJobService.GetViewByStaffIdAsync(staffId, jobCode ?? string.Empty);          

            if (result.Success && result.Data != null)
            {
                var staffJobItem = _mapper.Map<StaffJobItemViewModel>(result.Data);
                await PopulateDropdownsAsync(staffJobItem);
                return PartialView("_AddEditStaffJob", staffJobItem);
            }
            else
            {
                return Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to retrieve Staff job details.",
                    errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }
        }
       
        [HttpPost]
        public async Task<IActionResult> Edit(string staffId, [FromBody] StaffJobItemViewModel staffJobItem)
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

            var staffJobDto = new StaffJobDto
            {
                StaffId = staffJobItem.StaffID ?? staffId,
                JobCode = staffJobItem.JobCode,
                PlannedHours = staffJobItem.PlannedHours
            };
            var result = await _staffJobService.UpdateStaffJobAsync(staffId, staffJobDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Staff job updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Staff cost planned hours.",
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
            var result = await _staffJobService.DeleteStaffJobAsync(staffId, jobCode);
            if (result.Success)
            {
                return Json(new { success = true, message = "Staff job deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Staff cost planned hours.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetChargeRate(string staffId, string jobCode)
        {
            if (string.IsNullOrWhiteSpace(staffId))
            {
                return Json(new { success = false, message = "Staff ID is required", chargeRate = 0 });
            }

            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job Code is required", chargeRate = 0 });
            }

            var result = await _staffJobService.GetStaffChargeRate(staffId, jobCode);

            if (result.Success)
            {
                return Json(new { success = true, chargeRate = result.Data ?? 0 });
            }

            return Json(new
            {
                success = false,
                message = "Failed to retrieve charge rate.",
                chargeRate = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalStaffCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job Code is required", totalStaffCost = 0 });
            }

            var result = await _staffJobService.GetTotalStaffCostAsync(jobCode);

            if (result.Success)
            {
                return Json(new { success = true, totalStaffCost = result.Data });
            }

            return Json(new
            {
                success = false,
                message = "Failed to retrieve total staff cost.",
                totalStaffCost = 0,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task PopulateDropdownsAsync(StaffJobItemViewModel model)
        {
            // Manager dropdown
            var staffResponse = await _staffJobService.GetStaffWorkgroupLookupAsync();
            model.StaffList = staffResponse.Data == null ? new List<SelectListItem>() :
                staffResponse.Data
                .Select(m => new SelectListItem
                {
                    Value = m.StaffID,
                    Text  = $"{m.Name}|{m.WorkGroupGrade}|{m.HrsAvail}",
                    Selected = string.Equals(model.StaffID, m.StaffID, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }
    }
}
