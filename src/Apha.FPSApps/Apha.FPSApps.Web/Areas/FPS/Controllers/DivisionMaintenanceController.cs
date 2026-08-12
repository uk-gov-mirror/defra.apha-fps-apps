using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// MVC controller for Division maintenance operations.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class DivisionMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IDivisionService _divisionService;

        public DivisionMaintenanceController(IMapper mapper, IDivisionService divisionService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _divisionService = divisionService ?? throw new ArgumentNullException(nameof(divisionService));
        }

        /// <summary>
        /// Displays the Division maintenance page with DataGrid.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}"
            };

            var divisionGridConfig = await GetDivisionGridConfigAsync(defaultRequest);

            var viewModel = new DivisionMaintenanceViewModel
            {
                DivisionGrid = divisionGridConfig
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the Division grid via AJAX for pagination and filtering.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadDivisionGrid(PaginationFilter<string> request)
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

            var divisionGridConfig = await GetDivisionGridConfigAsync(request);
            return PartialView("_DataGrid", divisionGridConfig);
        }

        private async Task<DataGridConfig<DivisionViewModel>> GetDivisionGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var divisionPagedData = await _divisionService.GetAllDivisionsPagedAsync(queryParameters);

            var divisionItems = new List<DivisionViewModel>();
            if (divisionPagedData.Data != null)
            {
                divisionItems = _mapper.Map<List<DivisionViewModel>>(divisionPagedData.Data);
            }

            var paginationModel = divisionPagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(divisionPagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<DivisionViewModel>
            {
                GridId = "divisionGrid",
                Title = "Division Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "DivName",
                AddFunction = "addDivision",
                EditFunction = "editDivision",
                DeleteFunction = "deleteDivision",
                BindGridUrl = "/FPS/DivisionMaintenance/LoadDivisionGrid",
                Data = divisionItems,
                Columns = GridDataProvider.GetColumnsDefination<DivisionViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Displays the create division modal.
        /// </summary>
        [HttpGet]

        public IActionResult Create()
        {
            var model = new DivisionViewModel
            {
                DivName = string.Empty,
                AgencyId = 0,
                CentOverhead = null
            };
            return PartialView("_AddEditDivision", model);
        }

        /// <summary>
        /// Creates a new division.
        /// </summary>
        [HttpPost]
     
        public async Task<IActionResult> Create([FromBody] DivisionViewModel divisionViewModel)
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

            var divisionDto = _mapper.Map<DivisionDto>(divisionViewModel);
            var result = await _divisionService.CreateDivisionAsync(divisionDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Division created successfully" });
            }

            // Use the actual error message from the API response
            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create division.";

            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Checks whether a division name already exists, ignoring letter casing.
        /// Used for inline client-side validation before submitting the add/edit modal.
        /// </summary>
        /// <param name="divName">The division name to check.</param>
        /// <param name="originalDivName">The current name when editing, excluded from the duplicate check.</param>
        [HttpGet]
        public async Task<IActionResult> CheckDivisionNameExists(string divName, string? originalDivName = null)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return Json(new { exists = false });
            }

            // When editing, re-casing the division's own name is allowed and must not count as a conflict.
            if (!string.IsNullOrWhiteSpace(originalDivName)
                && divName.Equals(originalDivName, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { exists = false });
            }

            var result = await _divisionService.GetAllDivisionsAsync();
            var exists = result.Success
                && result.Data != null
                && result.Data.Any(d =>
                    !string.IsNullOrWhiteSpace(d.DivName)
                    && d.DivName.Equals(divName, StringComparison.OrdinalIgnoreCase));

            return Json(new { exists });
        }

        /// <summary>
        /// Displays the edit division modal.
        /// </summary>
        [HttpGet]

        public async Task<IActionResult> Edit(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return Json(new { success = false, message = "Division name is required" });
            }

            var result = await _divisionService.GetDivisionByNameAsync(divName);

            if (result.Success && result.Data != null)
            {
                var divisionViewModel = _mapper.Map<DivisionViewModel>(result.Data);
                return PartialView("_AddEditDivision", divisionViewModel);
            }

            return Json(new { success = false, message = $"Division '{divName}' not found." });
        }

        /// <summary>
        /// Updates an existing division.
        /// </summary>
        /// <param name="divisionViewModel">The updated division data.</param>
        /// <param name="originalDivName">The original division name (used when division name is changed).</param>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] DivisionViewModel divisionViewModel, [FromQuery] string? originalDivName = null)
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

            // Use originalDivName if provided (when division name is being changed), otherwise use current name
            var identifyingDivName = !string.IsNullOrWhiteSpace(originalDivName) ? originalDivName : divisionViewModel.DivName;

            var divisionDto = _mapper.Map<DivisionDto>(divisionViewModel);
            var result = await _divisionService.UpdateDivisionAsync(identifyingDivName, divisionDto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Division updated successfully" });
            }

            // Use the actual error message from the API response
            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update division.";

            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes a division.
        /// </summary>
        [HttpDelete]
       public async Task<IActionResult> Delete(string divName)
        {
            if (string.IsNullOrWhiteSpace(divName))
            {
                return Json(new { success = false, message = "Division name is required" });
            }

            var result = await _divisionService.DeleteDivisionAsync(divName);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Division deleted successfully" });
            }

            return Json(new { success = false, message = "Unable to delete the division name as it is already in use." });
        }

        /// <summary>
        /// Gets all agencies from the Agency lookup table for dropdown population.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDistinctAgencies()
        {
            try
            {
                var result = await _divisionService.GetAllAgenciesAsync();

                if (result.Success && result.Data != null)
                {
                    var agencies = result.Data
                        .Select(a => new { agencyId = a.AgencyId, agencyName = a.AgencyName ?? a.AgencyId.ToString() })
                        .OrderBy(a => a.agencyId)
                        .ToList();

                    return Json(new { success = true, data = agencies });
                }

                return Json(new { success = false, message = "Failed to load agencies" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading agencies: {ex.Message}" });
            }
        }
    }
}
