using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers
{
    
    [Area("CostBook")]
    [Authorize(Roles = "CostbookAdmin,CostbookUser")]
    [AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
    public class MaintenanceController : Controller
    {
        private readonly IMapper _mapper;
       
        private readonly ICostBookMaintenanceService _maintenanceService;
        
        private readonly ICostBookAccountGroupService _accountGroupService;
        
        private readonly ICostBookCapsStaffService _capsStaffService;

        public MaintenanceController(
            IMapper mapper,
            ICostBookMaintenanceService maintenanceService,
            ICostBookAccountGroupService accountGroupService,
            ICostBookCapsStaffService capsStaffService)
        {
            _mapper = mapper;
            _maintenanceService = maintenanceService;
            _accountGroupService = accountGroupService;
            _capsStaffService = capsStaffService;
        }

        
        public async Task<IActionResult> Index()
        {
            var viewModel = new MaintenanceViewModel();
            
            var settingsResult = await _maintenanceService.GetSettingsAsync();
            if (settingsResult.Success && settingsResult.Data != null)
            {
                var dto = settingsResult.Data;
                viewModel.InflationAnimals          = dto.InflationAnimals;
                viewModel.InflationExceptionalCosts = dto.InflationExceptionalCosts;
                viewModel.InflationStaff            = dto.InflationStaff;
                viewModel.InflationTests            = dto.InflationTests;
                viewModel.CurrentFinancialYear       = dto.CurrentFinancialYear;
                viewModel.WorkingHoursInDay          = dto.WorkingHoursInDay;
                viewModel.WorkingDaysInYear          = dto.WorkingDaysInYear;
                viewModel.ProfitAnimals              = dto.ProfitAnimals;
                viewModel.ProfitExceptionalCosts     = dto.ProfitExceptionalCosts;
                viewModel.ProfitStaff                = dto.ProfitStaff;
                viewModel.ProfitTests                = dto.ProfitTests;
            }
            await PopulateDropdownsAsync(viewModel);
           
            viewModel.AccountCategoryGrid = new DataGridConfig<AccountCategoryItem>
            {
                GridId             = "accCatGrid",
                Title              = "Enter CSG7 Groups for Exceptional Cost Account Categories",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "AccShortName",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editAccountCategory",
                AllowDelete        = false,
                BindGridUrl        = "/CostBook/Maintenance/LoadAccountCategoryGrid",
                Data               = new List<AccountCategoryItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<AccountCategoryItem>(),
                Pagination         = new PaginationModel()
            };

            viewModel.Csg7GroupGrid = new DataGridConfig<Csg7GroupItem>
            {
                GridId             = "csg7Grid",
                Title              = "Set Inflation Option for CSG7 groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Csg7Group",
                AllowAdd           = true,
                AddFunction        = "addCsg7Group",
                AllowEdit          = true,
                EditFunction       = "editCsg7Group",
                AllowDelete        = true,
                DeleteFunction     = "deleteCsg7Group",
                BindGridUrl        = "/CostBook/Maintenance/LoadCsg7GroupGrid",
                Data               = new List<Csg7GroupItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<Csg7GroupItem>(),
                Pagination         = new PaginationModel()
            };

            viewModel.CapsStaffGrid = new DataGridConfig<CapsStaffItem>
            {
                GridId             = "capsStaffGrid",
                Title              = "CAPS Staff Members",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "MNumber",
                AllowAdd           = true,
                AddFunction        = "addCapsStaff",
                AllowEdit          = true,
                EditFunction       = "editCapsStaff",
                AllowDelete        = true,
                DeleteFunction     = "deleteCapsStaff",
                BindGridUrl        = "/CostBook/Maintenance/LoadCapsStaffGrid",
                Data               = new List<CapsStaffItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<CapsStaffItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        
        [HttpPost]
        public async Task<IActionResult> SaveInflationSettings([FromBody] InflationSettingsItem item)
        {
            if (item is null)
                return Json(new { success = false, message = "Invalid data." });

            var validationResult = ValidateModel();
            if (validationResult is not null) return validationResult;
           
            var currentResult = await _maintenanceService.GetSettingsAsync();
            var dto = currentResult.Success && currentResult.Data != null
                ? currentResult.Data
                : new MaintenanceSettingsDto();

            dto.InflationAnimals          = item.InflationAnimals;
            dto.InflationExceptionalCosts = item.InflationExceptionalCosts;
            dto.InflationStaff            = item.InflationStaff;
            dto.InflationTests            = item.InflationTests;
            dto.CurrentFinancialYear       = item.CurrentFinancialYear;
            dto.WorkingHoursInDay          = item.WorkingHoursInDay;
            dto.WorkingDaysInYear          = item.WorkingDaysInYear;

            var result = await _maintenanceService.UpdateSettingsAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Inflation values saved successfully." })
                : Json(new { success = false, errors = MapApiErrors(result.Errors) });
        }

        
        [HttpPost]
        public async Task<IActionResult> SaveProfitMargins([FromBody] ProfitMarginsItem item)
        {
            if (item is null)
                return Json(new { success = false, message = "Invalid data." });

            var validationResult = ValidateModel();
            if (validationResult is not null) return validationResult;

            
            var currentResult = await _maintenanceService.GetSettingsAsync();
            var dto = currentResult.Success && currentResult.Data != null
                ? currentResult.Data
                : new MaintenanceSettingsDto();

            dto.ProfitAnimals          = item.ProfitAnimals;
            dto.ProfitExceptionalCosts = item.ProfitExceptionalCosts;
            dto.ProfitStaff            = item.ProfitStaff;
            dto.ProfitTests            = item.ProfitTests;

            var result = await _maintenanceService.UpdateSettingsAsync(dto);
            return result.Success
                ? Json(new { success = true, message = "Profit margins saved successfully." })
                : Json(new { success = false, errors = MapApiErrors(result.Errors) });
        }

        
        [HttpPost]
        public async Task<IActionResult> LoadAccountCategoryGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetAccountCategoryGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AccountCategoryItem>> GetAccountCategoryGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _maintenanceService.GetPaginatedAccountCategoriesAsync(queryParameters);

            var items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<AccountCategoryItem>>(pagedData.Data)
                : new List<AccountCategoryItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<AccountCategoryItem>
            {
                GridId             = "accCatGrid",
                Title              = "Enter CSG7 Groups for Exceptional Cost Account Categories",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "AccShortName",
                AllowAdd           = false,
                AllowEdit          = true,
                EditFunction       = "editAccountCategory",
                AllowDelete        = false,
                BindGridUrl        = "/CostBook/Maintenance/LoadAccountCategoryGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AccountCategoryItem>(null),
                CurrentFilters     = filterDict,
                Pagination         = paginationModel
            };
        }

        
        [HttpGet]
        public async Task<IActionResult> EditAccountCategory(string accShortName)
        {
            if (string.IsNullOrWhiteSpace(accShortName))
                return NotFound("Account Short Name is required.");

            var result = await _maintenanceService.GetAccountCategoriesAsync();
            if (!result.Success || result.Data == null)
                return NotFound($"Account category '{accShortName}' not found.");

            var dto = result.Data.FirstOrDefault(x =>
                string.Equals(x.AccShortName, accShortName, StringComparison.OrdinalIgnoreCase));
            if (dto == null)
                return NotFound($"Account category '{accShortName}' not found.");

            await PopulateAccCatDropdownAsync();
            var item = _mapper.Map<AccountCategoryItem>(dto);
            return PartialView("_AddEditAccountCategory", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditAccountCategory(string accShortName, [FromBody] AccountCategoryItem model)
        {
            if (model is null || string.IsNullOrWhiteSpace(accShortName))
                return Json(new { success = false, message = "Invalid data." });

            var dto = new AccountCategoryMaintenanceDto { Csg7Group = model.Csg7Group };
            var result = await _maintenanceService.UpdateAccountCategoryAsync(accShortName, dto);
            return result.Success
                ? Json(new { success = true, message = "Account category updated successfully." })
                : Json(new { success = false, errors = MapApiErrors(result.Errors) });
        }

        
        [HttpPost]
        public async Task<IActionResult> LoadCsg7GroupGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCsg7GroupGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<Csg7GroupItem>> GetCsg7GroupGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _accountGroupService.GetPaginatedAccountGroupsAsync(queryParameters);

            var items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<Csg7GroupItem>>(pagedData.Data)
                : new List<Csg7GroupItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<Csg7GroupItem>
            {
                GridId             = "csg7Grid",
                Title              = "Set Inflation Option for CSG7 groups",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Csg7Group",
                AllowAdd           = true,
                AddFunction        = "addCsg7Group",
                AllowEdit          = true,
                EditFunction       = "editCsg7Group",
                AllowDelete        = true,
                DeleteFunction     = "deleteCsg7Group",
                BindGridUrl        = "/CostBook/Maintenance/LoadCsg7GroupGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<Csg7GroupItem>(null),
                CurrentFilters     = filterDict,
                Pagination         = paginationModel
            };
        }

        [HttpGet]
        public IActionResult CreateCsg7Group()
        {
            return PartialView("_AddEditCsg7Group", new Csg7GroupItem());
        }
                

        [HttpPost]
        public async Task<IActionResult> CreateCsg7Group([FromBody] AccountGroupDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data." });
         
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _accountGroupService.AddAccountGroupAsync(dto);
            if (result.Success)
                return Json(new { success = true, message = "CSG7 group saved successfully." });

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.";
            return Json(new { success = false, message = errorMessage, errors = result.Errors });
        }

       
        [HttpGet]
        public async Task<IActionResult> EditCsg7Group(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                return NotFound("CSG7 Group is required.");

            var result = await _accountGroupService.GetAccountGroupAsync(csg7Group);
            if (!result.Success || result.Data == null)
                return NotFound($"CSG7 group '{csg7Group}' not found.");

            var item = _mapper.Map<Csg7GroupItem>(result.Data);
            return PartialView("_AddEditCsg7Group", item);
        }

        
        [HttpPost]
        public async Task<IActionResult> EditCsg7Group(string csg7Group, [FromBody] AccountGroupDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(csg7Group))
                return Json(new { success = false, message = "Invalid data." });

           
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _accountGroupService.UpdateAccountGroupAsync(csg7Group, dto);
            return result.Success
                ? Json(new { success = true, message = "CSG7 group updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

       
        [HttpDelete]
        public async Task<IActionResult> DeleteCsg7Group(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                return Json(new { success = false, message = "CSG7 Group is required." });

            var result = await _accountGroupService.DeleteAccountGroupAsync(csg7Group);
            
            if (!result.Success)
            {
                var message = result.Errors?.FirstOrDefault()?.Message
                              ?? "Failed to delete CSG7 group.";
                return Json(new { success = false, message });
            }

            return Json(new { success = true });
        }

        
        [HttpPost]
        public async Task<IActionResult> LoadCapsStaffGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetCapsStaffGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<CapsStaffItem>> GetCapsStaffGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            
            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _capsStaffService.GetPaginatedCapsStaffAsync(queryParameters);

            var items = pagedData.Success && pagedData.Data != null
                ? _mapper.Map<List<CapsStaffItem>>(pagedData.Data)
                : new List<CapsStaffItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn    = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<CapsStaffItem>
            {
                GridId             = "capsStaffGrid",
                Title              = "CAPS Staff Members",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "MNumber",
                AllowAdd           = true,
                AddFunction        = "addCapsStaff",
                AllowEdit          = true,
                EditFunction       = "editCapsStaff",
                AllowDelete        = true,
                DeleteFunction     = "deleteCapsStaff",
                BindGridUrl        = "/CostBook/Maintenance/LoadCapsStaffGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<CapsStaffItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        
        [HttpGet]
        public IActionResult CreateCapsStaff()
        {
            return PartialView("_AddEditCapsStaff", new CapsStaffItem());
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateCapsStaff([FromBody] StaffDto dto)
        {
            if (dto is null)
                return Json(new { success = false, message = "Invalid data." });

           
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _capsStaffService.AddCapsStaffAsync(dto);
            if (result.Success)
                return Json(new { success = true, message = "Staff member saved successfully." });

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Save failed.";
            return Json(new { success = false, message = errorMessage, errors = result.Errors });
        }

        
        [HttpGet]
        public async Task<IActionResult> EditCapsStaff(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                return NotFound("mNumber is required.");

            var result = await _capsStaffService.GetCapsStaffByMNumberAsync(mNumber);
            if (!result.Success || result.Data == null)
                return NotFound($"Staff member '{mNumber}' not found.");

            var item = _mapper.Map<CapsStaffItem>(result.Data);
            return PartialView("_AddEditCapsStaff", item);
        }

        
        [HttpPost]
        public async Task<IActionResult> EditCapsStaff(string mNumber, [FromBody] StaffDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(mNumber))
                return Json(new { success = false, message = "Invalid data." });

            
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Validation failed.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _capsStaffService.UpdateCapsStaffAsync(mNumber, dto);
            return result.Success
                ? Json(new { success = true, message = "Staff member updated successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCapsStaff(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                return Json(new { success = false, message = "mNumber is required." });

            var result = await _capsStaffService.DeleteCapsStaffAsync(mNumber);
            return result.Success
                ? Json(new { success = true, message = "Staff member deleted successfully." })
                : Json(new { success = false, errors = result.Errors });
        }

        
        private JsonResult? ValidateModel()
        {
            if (!ModelState.IsValid)
            {                
                var fieldNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {                   
                    ["InflationAnimals"]          = "inflAnimals",
                    ["InflationExceptionalCosts"]  = "inflExceptionalCosts",
                    ["InflationStaff"]             = "inflStaff",
                    ["InflationTests"]             = "inflTests",
                    ["CurrentFinancialYear"]       = "inflCurrentFinancialYear",
                    ["WorkingHoursInDay"]          = "inflWorkingHoursInDay",
                    ["WorkingDaysInYear"]          = "inflWorkingDaysInYear",                 
                    ["ProfitAnimals"]              = "profitAnimals",
                    ["ProfitExceptionalCosts"]     = "profitExceptionalCosts",
                    ["ProfitStaff"]                = "profitStaff",
                    ["ProfitTests"]                = "profitTests",
                };

                var errors = ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new
                    {
                        field   = fieldNameMap.TryGetValue(e.Key, out var htmlName) ? htmlName : e.Key,
                        message = e.Value!.Errors.First().ErrorMessage
                    })
                    .ToList<object>();
                return Json(new { success = false, errors });
            }
            return null;
        }
        private static List<object> MapApiErrors(List<ApiErrorDto>? apiErrors)
        {
            if (apiErrors is null || apiErrors.Count == 0)
                return [new { field = string.Empty, message = "An unexpected error occurred." }];

            var errors = new List<object>();
            foreach (var error in apiErrors)
            {
                if (error.Details is Dictionary<string, string[]> validationDetails)
                {
                    foreach (var kvp in validationDetails)
                    {
                        foreach (var msg in kvp.Value)
                        {
                            errors.Add(new { field = kvp.Key, message = msg });
                        }
                    }
                }
                else
                {
                    errors.Add(new { field = string.Empty, message = error.Message });
                }
            }

            return errors.Count > 0
                ? errors
                : [new { field = string.Empty, message = "An unexpected error occurred." }];
        }
        
        private async Task PopulateAccCatDropdownAsync()
        {
            var groupResult = await _accountGroupService.GetAllAccountGroupsAsync();
            ViewBag.Csg7GroupList = groupResult.Success && groupResult.Data != null
                ? groupResult.Data
                    .Select(item => new SelectListItem { Value = item.Csg7Group, Text = item.Csg7Group })
                    .ToList()
                : new List<SelectListItem>();
        }
        private async Task PopulateDropdownsAsync(MaintenanceViewModel model)
        {
            var groupResult = await _accountGroupService.GetAllAccountGroupsAsync();
            if (groupResult.Success && groupResult.Data != null)
            {
                model.Csg7GroupList = groupResult.Data
                    .Select(item => new SelectListItem
                    {
                        Value = item.Csg7Group,
                        Text  = item.Csg7Group
                    })
                    .ToList();
            }
        }
    }
}
