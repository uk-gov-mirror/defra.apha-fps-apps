using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class YearEndInitiationController : Controller
    {
        private const string YearEndInitiationJobName = "YearEnd-DataSetup"; 

        private readonly IMapper _mapper;
        private readonly IYearMasterService _yearMasterService;
        private readonly ISettingService _settingService;
        private readonly IMonthHourService _monthHourService;
        private readonly IYearEndService _yearEndService;
        private readonly ILogger<YearEndInitiationController> _logger;

        public YearEndInitiationController(
            IMapper mapper,
            IYearMasterService yearMasterService,
            ISettingService settingService,
            IMonthHourService monthHourService,
            IYearEndService yearEndService,
            ILogger<YearEndInitiationController> logger)
        {
            _mapper = mapper;
            _yearMasterService = yearMasterService;
            _settingService = settingService;
            _monthHourService = monthHourService;
            _yearEndService = yearEndService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var plannedYear = await GetPlannedYearAsync();
            var canInitiate = await GetCanInitiateDataSetupRequestAsync();
            var canApprove = await GetCanApproveOrRejectDataSetupRequestAsync();
            var configValuesGrid = await BuildConfigValuesGridAsync();
            var monthHoursGrid = await BuildMonthHoursGridAsync();
            var historyGrid = await BuildHistoryGridAsync(new PaginationFilter<string> { Filter = "{}" });

            return View(new YearEndInitiationViewModel
            {
                PlannedYear = plannedYear,
                CanInitiate = canInitiate,
                CanApprove = canApprove,
                ConfigValuesGrid = configValuesGrid,
                MonthHoursGrid = monthHoursGrid,
                HistoryGrid = historyGrid
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoadConfigValuesGrid(PaginationFilter<string> request)
        {
            var grid = await BuildConfigValuesGridAsync();
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadMonthHoursGrid(PaginationFilter<string> request)
        {
            var grid = await BuildMonthHoursGridAsync();
            return PartialView("_DataGrid", grid);
        }

        [HttpPost]
        public async Task<IActionResult> LoadHistoryGrid(PaginationFilter<string> request)
        {
            var grid = await BuildHistoryGridAsync(request);
            return PartialView("_DataGrid", grid);
        }

        [HttpGet]
        public async Task<IActionResult> EditConfigValue(string id)
        {
            var result = await _settingService.GetYearEndSettingsAsync();
            var setting = result.Data?.FirstOrDefault(s => s.Id == id);
            if (setting == null)
                return NotFound();

            var label = setting.Id;
            var value = setting.Setting ?? string.Empty;
            var isYesNo = label.Contains("approval", StringComparison.OrdinalIgnoreCase) ||
                          value == "Yes" || value == "No";

            var model = new YearEndEditConfigValueModel
            {
                Id = setting.Id,
                Value = value,
                IsYesNo = isYesNo,
                FpsYear = setting.FpsYear,
            };
            return PartialView("_EditConfigValue", model);
        }

        [HttpGet]
        public async Task<IActionResult> EditMonthHour(short year, short month, short fpsyear, short fmonth)
        {
            var result = await _monthHourService.GetYearEndMonthHoursAsync();
            var record = result.Data?.FirstOrDefault(m => m.Year == year && m.Month == month && m.FpsYear == fpsyear && m.Fmonth == fmonth);
            if (record == null)
                return NotFound();

            var model = new YearEndEditMonthHourModel
            {
                MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(record.Month),
                Year = record.Year,
                Month = record.Month,
                Fmonth = record.Fmonth,
                FpsYear = record.FpsYear,
                Days = record.Days,
                CvlHours = record.CvlHours,
                VidHours = record.VidHours
            };
            return PartialView("_EditMonthHour", model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSetting([FromBody] SettingDto dto)
        {
            var result = await _settingService.SaveSettingAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            var errors = result.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to save setting." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMonthHour([FromBody] MonthHourDto dto)
        {
            var result = await _monthHourService.SaveMonthHourAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            var errors = result.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to save month hour." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerInitiate(int plannedYear)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);
            if (result.Success)
            {
                _logger.LogInformation("Year End Initiation job triggered.");
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to trigger Year End Initiation job." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerApprove(int plannedYear)
        {
            var result = await _yearEndService.TriggerYearEndDataSetupApprovalJobAsync(plannedYear);
            if (result.Success)
            {
                _logger.LogInformation("Year End Approval job triggered. EventId: {EventId}", result?.Data?.EventId);
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to trigger Year End Approval job." }];
            return Json(new { success = false, errors });
        }

        [HttpPost]
        public async Task<IActionResult> TriggerReject(int plannedYear)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(plannedYear);
            if (result.Success)
            {
                _logger.LogInformation("Year End datasetup reject job triggered");
                return Json(new { success = true });
            }

            var errors = result?.Errors?.Select(e => new { field = string.Empty, message = e.Message }).ToArray()
                         ?? [new { field = string.Empty, message = "Failed to Reject Year End datasetup job." }];
            return Json(new { success = false, errors });
        }
        private async Task<int> GetPlannedYearAsync()
        {
            var result = await _yearMasterService.GetFpsPlannedYearAsync();
            return result.Success ? result.Data : 0;
        }

        private async Task<bool> GetCanInitiateDataSetupRequestAsync()
        {
            var result = await _yearEndService.GetCanInitiateDataSetupRequestAsync(YearEndInitiationJobName);
            return result.Success && result.Data;
        }

        private async Task<bool> GetCanApproveOrRejectDataSetupRequestAsync()
        {
            var result = await _yearEndService.GetCanApproveOrRejectDataSetupRequestAsync(YearEndInitiationJobName);
            return result.Success && result.Data;
        }
        private async Task<DataGridConfig<YearEndConfigValueItem>> BuildConfigValuesGridAsync()
        {
            var grid = ConfigValuesGridConfig();
            var result = await _settingService.GetYearEndSettingsAsync();
            if (result.Success && result.Data != null)
            {
                grid.Data = result.Data.Select(s => new YearEndConfigValueItem
                {
                    FpsYear= s.FpsYear,
                    Id = s.Id,
                    Setting = s.Setting,
                    //Notes = s.Notes,
                    ExistsForPlannedYear = s.ExistsForPlannedYear
                }).ToList();
            }
            return grid;
        }

        private async Task<DataGridConfig<YearEndMonthWorkingItem>> BuildMonthHoursGridAsync()
        {
            var grid = MonthHoursGridConfig();
            var result = await _monthHourService.GetYearEndMonthHoursAsync();
            if (result.Success && result.Data != null)
            {
                grid.Data = result.Data.Select(m => new YearEndMonthWorkingItem
                {
                    Year = m.Year,
                    Month = m.Month,
                    MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m.Month),
                    Days = m.Days,
                    CvlHours = m.CvlHours,
                    VidHours = m.VidHours,
                    Fmonth = m.Fmonth,
                    FpsYear = m.FpsYear,
                    ExistsForPlannedYear = m.ExistsForPlannedYear
                }).ToList();
            }
            return grid;
        }

        private async Task<DataGridConfig<YearEndHistoryItem>> BuildHistoryGridAsync(PaginationFilter<string> request)
        {
            var grid = HistoryGridConfig();
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(query, YearEndInitiationJobName);

            grid.Data = response.Data?.data != null
                ? response.Data.data.Select(d => new YearEndHistoryItem
                {
                    JobName = d.JobName,
                    RequestedBy = d.RequestedBy,
                    StartDateTime = d.StartDateTime,
                    EndDateTime = d.EndDateTime,
                    Status = d.Status,
                    ErrorMessage = d.ErrorMessage
                }).ToList()
                : [];

            grid.Pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();

            grid.Pagination.SortColumn = request.SortBy;
            grid.Pagination.SortDirection = request.Descending;

            return grid;
        }

        private static DataGridConfig<YearEndConfigValueItem> ConfigValuesGridConfig() => new()
        {
            GridId = "yearEndConfigValuesGrid",
            Title = "Config Value",
            BindGridUrl = "/FPS/YearEndInitiation/LoadConfigValuesGrid",
            ShowCheckboxColumn = false,
            ShowPagination = false,
            AllowAdd = false,
            AllowEdit = true,
            EditFunction = "openConfigEditModal",
            AllowDelete = false,
            AllowView = false,
            AllowCopy = false,
            AllowConfirm = true,
            ConfirmFunction = "confirmConfigValue",
            KeyProperty = "Id",
            Columns = GridDataProvider.GetColumnsDefination<YearEndConfigValueItem>()
        };

        private static DataGridConfig<YearEndMonthWorkingItem> MonthHoursGridConfig() => new()
        {
            GridId = "yearEndMonthHoursGrid",
            Title = "Month Working Hours",
            BindGridUrl = "/FPS/YearEndInitiation/LoadMonthHoursGrid",
            ShowCheckboxColumn = false,
            ShowPagination = false,
            AllowAdd = false,
            AllowEdit = true,
            EditFunction = "openMonthHourEditModal",
            AllowDelete = false,
            AllowView = false,
            AllowCopy = false,
            AllowConfirm = true,
            ConfirmFunction = "confirmMonthHour",
            KeyProperty = "Month",
            Columns = GridDataProvider.GetColumnsDefination<YearEndMonthWorkingItem>()
        };

        private static DataGridConfig<YearEndHistoryItem> HistoryGridConfig() => new()
        {
            GridId = "yearEndInitiationHistoryGrid",
            Title = "Request History",
            BindGridUrl = "/FPS/YearEndInitiation/LoadHistoryGrid",
            ShowCheckboxColumn = false,
            AllowAdd = false,
            AllowEdit = false,
            AllowDelete = false,
            Columns = GridDataProvider.GetColumnsDefination<YearEndHistoryItem>()
        };
    }
}

       