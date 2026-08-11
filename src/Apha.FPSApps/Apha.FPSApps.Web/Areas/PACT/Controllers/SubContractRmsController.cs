using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using PactProjectSubContractService = Apha.FPSApps.Application.Interfaces.PACT.IProjectSubContractService;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.IO;
using Apha.Common.Utilities.ExcelExport;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class SubContractRmsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly PactProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;

        public SubContractRmsController(
            IMapper mapper,
            PactProjectSubContractService subContractService,
            IProjectService projectService,
            IMonthService monthService,
            IExcelExportService excelExportService)
        {
            _mapper = mapper;
            _subContractService = subContractService;
            _projectService = projectService;
            _monthService = monthService;
            _excelExportService = excelExportService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? month)
        {
            var defaultRequest = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Project",
                Descending = false
            };

            if (month.HasValue)
            {
                defaultRequest.Filter = $"{{\"Month\":\"{month.Value}\"}}";
            }

            var monthsList = await GetMonthsListAsync();
            var projectsList = await GetProjectsListAsync();

            ViewBag.Projects = projectsList;
            ViewBag.Months = monthsList;

            var failedRequest = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Id",
                Descending = false
            };

            return View(new SubContractRmsViewModel
            {
                Month = month,
                FilterMonths = monthsList,
                Projects = projectsList,
                SubContractsGrid = await BuildRmsSubContractGridAsync(defaultRequest, month),
                FailedSubContractsGrid = await BuildFailedSubContractRmsGridAsync(failedRequest)
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadRmsSubContractsGrid(PaginationFilter<string> request, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (month.HasValue)
            {
                var filterDict = string.IsNullOrEmpty(request.Filter)
                    ? new Dictionary<string, string>()
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter) ?? new Dictionary<string, string>();

                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var gridConfig = await BuildRmsSubContractGridAsync(request, month);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubContractRms(int id, int? month)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await PopulateProjectsViewBagAsync();

            if (id == 0)
            {
                await PopulateMonthsViewBagAsync(month);
                return PartialView("_AddEditSubContractRms", new SubContractRmsItem
                {
                    Month = month
                });
            }

            var result = await _subContractService.GetByIdAsync(id);
            if (!result.Success || result.Data == null) return NotFound();

            var item = _mapper.Map<SubContractRmsItem>(result.Data);
            await PopulateMonthsViewBagAsync(item.Month);
            return PartialView("_AddEditSubContractRms", item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubContractRms([FromBody] SubContractRmsItem model)
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

            var dto = _mapper.Map<ProjectSubContractDto>(model);
            ApiResponseDto<ProjectSubContractDto> result;
            string successMsg;

            if (model.SubContCounter == 0)
            {
                result = await _subContractService.CreateAsync(dto);
                successMsg = "Sub Contract saved successfully.";
            }
            else
            {
                result = await _subContractService.UpdateAsync(model.SubContCounter, dto);
                successMsg = "SubContract updated successfully.";
            }

            if (result.Success)
                return Json(new { success = true, message = successMsg });

            return Json(new
            {
                success = false,
                message = "Failed to save subcontract.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSubContractRms(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subContractService.DeleteAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete subcontract." });
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "PACT", "SubContractRMS-Template.xlsx");
            if (!System.IO.File.Exists(templatePath))
                return NotFound();

            var bytes = System.IO.File.ReadAllBytes(templatePath);
            var fileName = $"SubContractRMS_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> Import([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Please select an Excel file to import." });
            }

            var result = await _subContractService.ImportSubContractRmsAsync(file);
            if (!result.Success || result.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Import failed."
                });
            }

            return Json(new
            {
                success = true,
                passedCount = result.Data.PassedCount,
                failedCount = result.Data.FailedCount,
                message = result.Data.Message
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadFailedSubContractRmsGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var grid = await BuildFailedSubContractRmsGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> ExportFailedSubContractRms()
        {
            var exportQuery = new QueryParameters<string>
            {
                Page = 1,
                PageSize = int.MaxValue,
                SortBy = "Id",
                Descending = false
            };

            var response = await _subContractService.GetFailedSubContractRmsAsync(exportQuery);
            var items = response.Success && response.Data != null
                ? _mapper.Map<List<SubContractRmsFailedItem>>(response.Data)
                : new List<SubContractRmsFailedItem>();

            var bytes = _excelExportService.ExportToExcel(items, "SubContractRMS_Failed");
            var fileName = $"SubContractRMS_{DateTime.Now:yyyyMMdd}_failed.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllFailedSubContractRms()
        {
            var result = await _subContractService.DeleteFailedSubContractRmsByUserAsync();
            if (result.Success)
                return Json(new { success = result.Data });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete records." });
        }

        [HttpGet]
        public async Task<IActionResult> GetFailedSubContractRms(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subContractService.GetFailedSubContractRmsByIdAsync(id);
            if (!result.Success || result.Data == null) 
                return NotFound();

            var item = _mapper.Map<SubContractRmsFailedItem>(result.Data);
            return PartialView("_EditFailedSubContractRms", item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveFailedSubContractRms([FromBody] SubContractRmsFailedItem model)
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

            var dto = _mapper.Map<SubContractRmsImportRowDto>(model);
            var result = await _subContractService.SaveFailedSubContractRmsAsync(model.Id, dto);

            if (result.Success)
            {
                var movedToSubContract = result.Data;
                var message = movedToSubContract
                    ? "Record successfully validated and is now live."
                    : "Failed record updated successfully.";
                return Json(new { success = true, message, movedToSubContract });
            }

            return Json(new
            {
                success = false,
                message = "Validation failed. Please correct the errors below.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFailedSubContractRms(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subContractService.DeleteFailedSubContractRmsByIdAsync(id);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = "Failed to delete failed record." });
        }

        // ── PRIVATE GRID BUILDERS ─────────────────────────────────────────────

        private async Task<DataGridConfig<SubContractRmsItem>> BuildRmsSubContractGridAsync(
            PaginationFilter<string> request, int? month = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            if (month.HasValue && !filterDict.ContainsKey("Month"))
            {
                filterDict["Month"] = month.Value.ToString();
                request.Filter = JsonConvert.SerializeObject(filterDict);
            }

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _subContractService.GetPagedProjectSubContractsManualAsync(query, null);

            var items = response.Data != null
                ? _mapper.Map<List<SubContractRmsItem>>(response.Data)
                : new List<SubContractRmsItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var queryParams = new List<string>();
            if (month.HasValue)
                queryParams.Add($"month={month.Value}");

            string queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            return new DataGridConfig<SubContractRmsItem>
            {
                GridId = "rmsSubContractsGrid",
                Title = "Animal format",
                KeyProperty = "SubContCounter",
                AddFunction = "addSubContractRms",
                EditFunction = "editSubContractRms",
                DeleteFunction = "deleteSubContractRms",
                BindGridUrl = $"/PACT/SubContractRms/LoadRmsSubContractsGrid{queryString}",
                ExtraFilterMethod = "getRmsSubContractFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<SubContractRmsItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<SubContractRmsFailedItem>> BuildFailedSubContractRmsGridAsync(PaginationFilter<string> request)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _subContractService.GetFailedSubContractRmsAsync(query);

            var items = response.Data != null
                ? _mapper.Map<List<SubContractRmsFailedItem>>(response.Data)
                : new List<SubContractRmsFailedItem>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            return new DataGridConfig<SubContractRmsFailedItem>
            {
                GridId = "rmsFailedSubContractsGrid",
                Title = "Failed records",
                KeyProperty = "Id",
                EditFunction = "editFailedSubContractRms",
                DeleteFunction = "deleteFailedSubContractRms",
                BindGridUrl = "/PACT/SubContractRms/LoadFailedSubContractRmsGrid",
                AllowAdd = false,
                AllowEdit = true,
                AllowDelete = true, 
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<SubContractRmsFailedItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

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

        private async Task<List<SelectListItem>> GetMonthsListAsync(double? selectedMonth = null)
        {
            var result = await _monthService.GetAllMonthsAsync();

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                int? selectedMonthInt = selectedMonth.HasValue ? Convert.ToInt32(selectedMonth.Value) : null;

                var monthList = result.Data
                    .OrderBy(m => m.Monthnumber)
                    .Select(m => new SelectListItem
                    {
                        Value = m.Monthnumber.ToString(),
                        Text = $"{m.Monthnumber} - {m.Monthname}",
                        Selected = selectedMonthInt.HasValue && m.Monthnumber == selectedMonthInt.Value
                    })
                    .ToList();

                return monthList;
            }
            else
            {
                return new List<SelectListItem>();
            }
        }

        private async Task PopulateProjectsViewBagAsync()
        {
            ViewBag.Projects = await GetProjectsListAsync();
        }

        private async Task PopulateMonthsViewBagAsync(double? selectedMonth = null)
        {
            ViewBag.Months = await GetMonthsListAsync(selectedMonth);
        }
    }
}
