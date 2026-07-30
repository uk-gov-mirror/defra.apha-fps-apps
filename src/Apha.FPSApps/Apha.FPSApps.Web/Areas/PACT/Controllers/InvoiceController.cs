using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class InvoiceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;

        public InvoiceController(
            IMapper mapper,
            IProjectInvoiceService invoiceService,
            IProjectService projectService,
            IMonthService monthService)
        {
            _mapper = mapper;
            _invoiceService = invoiceService;
            _projectService = projectService;
            _monthService = monthService;
        }

        /// <summary>
        /// Displays the invoice management page with a paginated data grid, project dropdown, and month filter.
        /// </summary>
        /// <param name="parentProject">Optional parent project code to pre-filter the invoice grid.</param>
        /// <param name="month">Optional month number to pre-filter the invoice grid.</param>
        /// <returns>The Invoice Index view populated with grid configuration and filter options.</returns>
        public async Task<IActionResult> Index(string? parentProject, int? month)
        {
            var defaultRequest = new PaginationFilter<string>{};

            // Apply month filter via the Filter property
            if (month.HasValue)
            {
                defaultRequest.Filter = $"{{\"Month\":\"{month.Value}\"}}";
            }

            var gridConfig = await BuildInvoiceManualGridAsync(defaultRequest, parentProject, month);

            // Populate project dropdown for filter panel
            var projectsList = await GetProjectsListAsync();

            // Populate months dropdown
            var monthsList = await GetMonthsListAsync();

            // Also set ViewBag for modal form compatibility
            ViewBag.Projects = projectsList;
            ViewBag.FilterProjects = projectsList;

            return View(new InvoiceViewModel
            {
                ParentProject = parentProject ?? string.Empty,
                Month = month,
                InvoicesGrid = gridConfig,
                FilterProjects = projectsList,
                FilterMonths = monthsList
            });
        }

        /// <summary>
        /// Reloads the invoice data grid partial view based on the supplied pagination, sort, and filter parameters.
        /// </summary>
        /// <param name="request">Pagination and filter parameters submitted from the data grid.</param>
        /// <param name="parentProject">Optional parent project code to filter invoices.</param>
        /// <param name="month">Optional month number to filter invoices.</param>
        /// <returns>A partial view containing the refreshed data grid, or <see cref="BadRequestResult"/> if the model state is invalid.</returns>
        [HttpPost]
        public async Task<IActionResult> LoadInvoicesGrid(PaginationFilter<string> request, string? parentProject, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Merge month filter into request filter
            if (month.HasValue)
            {
                var filterDict = string.IsNullOrEmpty(request.Filter)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var gridConfig = await BuildInvoiceManualGridAsync(request, parentProject, month);
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Returns the add/edit invoice modal partial view for a new or existing invoice.
        /// </summary>
        /// <param name="id">The invoice counter of the invoice to edit, or <c>0</c> to create a new invoice.</param>
        /// <param name="parentProject">Optional parent project code pre-populated on the new invoice form.</param>
        /// <returns>
        /// A partial view pre-populated with the invoice data, or <see cref="NotFoundResult"/> if the invoice does not exist.
        /// Returns <see cref="BadRequestResult"/> if the model state is invalid.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id, string? parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                return PartialView("_AddEditInvoice", new InvoiceItem
                {
                    ProjectParent = parentProject ?? string.Empty
                });
            }

            var result = await _invoiceService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<InvoiceItem>(result.Data);
            return PartialView("_AddEditInvoice", item);
        }

        /// <summary>
        /// Creates or updates an invoice record based on the submitted model.
        /// A new invoice is created when <see cref="InvoiceItem.InvoiceCounter"/> is <c>0</c>; otherwise the existing record is updated.
        /// </summary>
        /// <param name="model">The invoice data submitted from the add/edit modal form.</param>
        /// <returns>
        /// A JSON response indicating success or failure. On validation failure, field-level error details are included.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] InvoiceItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<ProjectInvoiceDto>(model);
            ApiResponseDto<ProjectInvoiceDto> result;
            string successMsg;

            if (model.InvoiceCounter == 0)
            {
                result = await _invoiceService.CreateAsync(dto);
                successMsg = "Invoice saved successfully.";
            }
            else
            {
                result = await _invoiceService.UpdateAsync(model.InvoiceCounter, dto);
                successMsg = "Invoice updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to save invoice.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes the invoice with the specified identifier.
        /// </summary>
        /// <param name="id">The invoice counter of the invoice to delete.</param>
        /// <returns>
        /// A JSON response indicating success or failure.
        /// Returns <see cref="BadRequestResult"/> if the model state is invalid.
        /// </returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _invoiceService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete invoice." });
        }        

        /// <summary>
        /// Builds the invoice data grid configuration by fetching paged invoice data from the service
        /// and applying any active pagination, sort, and filter state.
        /// </summary>
        /// <param name="request">Pagination and filter parameters for the grid query.</param>
        /// <param name="parentProject">Optional parent project code used to scope the invoice query.</param>
        /// <param name="month">Optional month number injected into the filter when not already present.</param>
        /// <returns>A fully configured <see cref="DataGridConfig{T}"/> ready for rendering.</returns>
        private async Task<DataGridConfig<InvoiceItem>> BuildInvoiceManualGridAsync(
            PaginationFilter<string> request, string? parentProject, int? month = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            // Add month filter if specified
            if (month.HasValue && !filterDict.ContainsKey("Month"))
            {
                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _invoiceService.GetPagedProjectInvoiceManualAsync(query, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<InvoiceItem>>(response.Data)
                : new List<InvoiceItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(parentProject))
                queryParams.Add($"parentProject={Uri.EscapeDataString(parentProject)}");
            if (month.HasValue)
                queryParams.Add($"month={month.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<InvoiceItem>
            {
                GridId = "invoicesGrid",
                Title = "Invoice Record",
                KeyProperty = "InvoiceCounter",
                AddFunction = "addInvoice",
                EditFunction = "editInvoice",
                DeleteFunction = "deleteInvoice",
                BindGridUrl = $"/PACT/Invoice/LoadInvoicesGrid{queryString}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<InvoiceItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Retrieves an ordered list of all PACT projects formatted as <see cref="SelectListItem"/> entries
        /// for use in project filter dropdowns.
        /// </summary>
        /// <returns>An ordered list of project select items, or an empty list if none are available.</returns>
        private async Task<List<SelectListItem>> GetProjectsListAsync()
        {

            var result = await _projectService.GetAllPactProjectsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var projectList = result.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject
                    })
                    .ToList();

                return projectList;
            }
            else
            {
                return new List<SelectListItem>();
            }

        }

        /// <summary>
        /// Retrieves an ordered list of all months formatted as <see cref="SelectListItem"/> entries
        /// for use in month filter dropdowns.
        /// </summary>
        /// <returns>An ordered list of month select items in the format "number - name", or an empty list if none are available.</returns>
        private async Task<List<SelectListItem>> GetMonthsListAsync()
        {
            var result = await _monthService.GetAllMonthsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                var monthList = result.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Monthnumber.ToString(),
                        Text = $"{m.Monthnumber} - {m.Monthname}"
                    })
                    .ToList();

                return monthList;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }

        /// <summary>
        /// Populates <c>ViewBag.Projects</c> with the list of PACT projects for use in modal form dropdowns.
        /// </summary>
        private async Task PopulateProjectsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
            ViewBag.Months = await GetMonthsListAsync();
        }
    }
}
