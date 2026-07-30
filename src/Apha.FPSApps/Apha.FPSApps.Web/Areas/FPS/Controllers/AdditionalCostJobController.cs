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
    public class AdditionalCostJobController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAdditionalCostService _additionalCostService;

        public AdditionalCostJobController(IMapper mapper, IAdditionalCostService additionalCostService)
        {
            _mapper = mapper;
            _additionalCostService = additionalCostService;
        }

        [HttpPost]
        public async Task<IActionResult> LoadAdditionalCostGrid(PaginationFilter<string> request, string? jobCode = null, string? title = null)
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _additionalCostService.GetAdditionalCostsAsync(queryParameters, jobCode ?? string.Empty);

            var items = pagedData.Data != null
                ? _mapper.Map<List<AdditionalCostItemViewModel>>(pagedData.Data)
                : new List<AdditionalCostItemViewModel>();

            var paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            var gridTitle = title ?? "Additional Cost Plan";
            var gridConfig = new DataGridConfig<AdditionalCostItemViewModel>
            {
                GridId = "additionalCostGrid",
                Title = gridTitle,
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "Description",
                AddFunction = "addAdditionalCost",
                EditFunction = "editAdditionalCost",
                DeleteFunction = "deleteAdditionalCost",
                ExtraFilterMethod = "getAdditionalCostExtraFilters",
                BindGridUrl = $"/FPS/AdditionalCostJob/LoadAdditionalCostGrid?title={Uri.EscapeDataString(gridTitle)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AdditionalCostItemViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> Create(string jobCode)
        {
            var model = new AdditionalCostItemViewModel { JobCode = jobCode };
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditAdditionalCostJob", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdditionalCostItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var dto = _mapper.Map<AdditionalCostDto>(model);
            var result = await _additionalCostService.CreateAdditionalCostAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Additional cost created successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create additional cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string jobCode, string account, string description)
        {
            var result = await _additionalCostService.GetByIdAsync(jobCode, account, description);

            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to retrieve additional cost details.",
                    errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                    {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });
            }

            var model = _mapper.Map<AdditionalCostItemViewModel>(result.Data);
            model.OriginalDescription = model.Description;
            model.OriginalAccount = model.Account;
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditAdditionalCostJob", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] AdditionalCostItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the validation errors.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            var dto = _mapper.Map<AdditionalCostDto>(model);
            var originalAccount = string.IsNullOrWhiteSpace(model.OriginalAccount) ? model.Account : model.OriginalAccount;
            var result = await _additionalCostService.UpdateAdditionalCostAsync(model.JobCode!, originalAccount, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Additional cost updated successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update additional cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string jobCode, string account, string description)
        {
            var dto = new AdditionalCostDto { JobCode = jobCode, Account = account, Description = description };
            var result = await _additionalCostService.DeleteAdditionalCostAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = "Additional cost deleted successfully." });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete additional cost.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalItemCost(string jobCode)
        {
            if (string.IsNullOrWhiteSpace(jobCode))
            {
                return Json(new { success = false, message = "Job code is required.", totalItemCost = 0 });
            }

            var result = await _additionalCostService.GetTotalItemCostAsync(jobCode);

            if (result.Success)
            {
                return Json(new { success = true, totalItemCost = result.Data });
            }

            return Json(new { success = false, message = "Failed to retrieve total item cost.", totalItemCost = 0 });
        }

        private async Task PopulateDropdownsAsync(AdditionalCostItemViewModel model)
        {
            var accountResult = await _additionalCostService.GetAccountCategoriesAsync();
            model.AccountList = accountResult.Data == null ? new List<SelectListItem>() :
                accountResult.Data
                    .Select(a => new SelectListItem
                    {
                        Value = a.AccShortName,
                        Text = $"{a.AccShortName}|{a.AccountDescription ?? string.Empty}|{a.ConstituentAccountCodes ?? string.Empty}",
                        Selected = string.Equals(model.Account, a.AccShortName, StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
        }
    }
}
