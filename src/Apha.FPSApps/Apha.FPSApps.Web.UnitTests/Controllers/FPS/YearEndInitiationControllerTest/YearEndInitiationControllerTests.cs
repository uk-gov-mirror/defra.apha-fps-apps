using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.YearEndInitiationControllerTest
{
    public class YearEndInitiationControllerTests
    {
        private const string JobName = "YearEnd-DataSetup";

        private readonly IMapper _mapper;
        private readonly IYearMasterService _yearMasterService;
        private readonly ISettingService _settingService;
        private readonly IMonthHourService _monthHourService;
        private readonly IYearEndService _yearEndService;
        private readonly ILogger<YearEndInitiationController> _logger;
        private readonly YearEndInitiationController _controller;

        public YearEndInitiationControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _yearMasterService = Substitute.For<IYearMasterService>();
            _settingService = Substitute.For<ISettingService>();
            _monthHourService = Substitute.For<IMonthHourService>();
            _yearEndService = Substitute.For<IYearEndService>();
            _logger = Substitute.For<ILogger<YearEndInitiationController>>();

            _controller = new YearEndInitiationController(
                _mapper,
                _yearMasterService,
                _settingService,
                _monthHourService,
                _yearEndService,
                _logger);

            // Default mapper wiring needed by BuildHistoryGridAsync
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel());
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupIndexDefaults(
            int plannedYear = 2025,
            bool canInitiate = true,
            bool canApprove = false)
        {
            _yearMasterService.GetFpsPlannedYearAsync()
                .Returns(ApiResponseDto<int>.SuccessResponse(plannedYear));

            _yearEndService.GetCanInitiateDataSetupRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(canInitiate));

            _yearEndService.GetCanApproveOrRejectDataSetupRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(canApprove));

            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(
                    new List<YearEndSettingDto>
                    {
                        new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024, ExistsForPlannedYear = "Yes" }
                    }));

            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(
                    new List<YearEndMonthHourDto>
                    {
                        new YearEndMonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024, ExistsForPlannedYear = "Yes" }
                    }));

            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(
                    new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>()));
        }

        // -----------------------------------------------------------------------
        // Index
        // -----------------------------------------------------------------------

        #region Index

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ViewModelHasCorrectPlannedYear()
        {
            // Arrange
            SetupIndexDefaults(plannedYear: 2025);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.Equal(2025, model.PlannedYear);
        }

        [Fact]
        public async Task Index_ViewModelCanInitiateIsTrue_WhenServiceReturnsTrue()
        {
            // Arrange
            SetupIndexDefaults(canInitiate: true, canApprove: false);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.True(model.CanInitiate);
            Assert.False(model.CanApprove);
        }

        [Fact]
        public async Task Index_ViewModelCanApproveIsTrue_WhenServiceReturnsTrue()
        {
            // Arrange
            SetupIndexDefaults(canInitiate: false, canApprove: true);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.False(model.CanInitiate);
            Assert.True(model.CanApprove);
        }

        [Fact]
        public async Task Index_ViewModelContainsAllRequiredGrids()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.NotNull(model.ConfigValuesGrid);
            Assert.NotNull(model.MonthHoursGrid);
            Assert.NotNull(model.HistoryGrid);
        }

        [Fact]
        public async Task Index_WhenPlannedYearServiceFails_PlannedYearIsZero()
        {
            // Arrange
            _yearMasterService.GetFpsPlannedYearAsync()
                .Returns(new ApiResponseDto<int> { Success = false });

            _yearEndService.GetCanInitiateDataSetupRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));
            _yearEndService.GetCanApproveOrRejectDataSetupRequestAsync(JobName)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse([]));
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse([]));
            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.Equal(0, model.PlannedYear);
        }

        [Fact]
        public async Task Index_ConfigValuesGridDataPopulatedFromService()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.Single(model.ConfigValuesGrid.Data);
            Assert.Equal("HoursInDay", model.ConfigValuesGrid.Data[0].Id);
        }

        [Fact]
        public async Task Index_MonthHoursGridDataPopulatedFromService()
        {
            // Arrange
            SetupIndexDefaults();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<YearEndInitiationViewModel>(viewResult.Model);
            Assert.Single(model.MonthHoursGrid.Data);
            Assert.Equal(1, model.MonthHoursGrid.Data[0].Month);
        }

        #endregion

        // -----------------------------------------------------------------------
        // LoadConfigValuesGrid
        // -----------------------------------------------------------------------

        #region LoadConfigValuesGrid

        [Fact]
        public async Task LoadConfigValuesGrid_ReturnsPartialViewResult()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.LoadConfigValuesGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadConfigValuesGrid_ReturnsDataGridConfigModel()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(
                    new List<YearEndSettingDto>
                    {
                        new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024, ExistsForPlannedYear = "Yes" }
                    }));

            // Act
            var result = await _controller.LoadConfigValuesGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndConfigValueItem>>(partial.Model);
            Assert.Single(model.Data);
            Assert.Equal("HoursInDay", model.Data[0].Id);
        }

        [Fact]
        public async Task LoadConfigValuesGrid_WhenServiceReturnsEmpty_ReturnsEmptyDataGrid()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.LoadConfigValuesGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndConfigValueItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadConfigValuesGrid_WhenServiceFails_ReturnsDataGridWithNoData()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(new ApiResponseDto<List<YearEndSettingDto>> { Success = false });

            // Act
            var result = await _controller.LoadConfigValuesGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndConfigValueItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        #endregion

        // -----------------------------------------------------------------------
        // LoadMonthHoursGrid
        // -----------------------------------------------------------------------

        #region LoadMonthHoursGrid

        [Fact]
        public async Task LoadMonthHoursGrid_ReturnsPartialViewResult()
        {
            // Arrange
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.LoadMonthHoursGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadMonthHoursGrid_ReturnsDataGridConfigWithMappedData()
        {
            // Arrange
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(
                    new List<YearEndMonthHourDto>
                    {
                        new YearEndMonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024, ExistsForPlannedYear = "Yes" }
                    }));

            // Act
            var result = await _controller.LoadMonthHoursGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndMonthWorkingItem>>(partial.Model);
            Assert.Single(model.Data);
            Assert.Equal(1, model.Data[0].Month);
        }

        [Fact]
        public async Task LoadMonthHoursGrid_WhenServiceFails_ReturnsDataGridWithNoData()
        {
            // Arrange
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(new ApiResponseDto<List<YearEndMonthHourDto>> { Success = false });

            // Act
            var result = await _controller.LoadMonthHoursGrid(new PaginationFilter<string>());

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndMonthWorkingItem>>(partial.Model);
            Assert.Empty(model.Data);
        }

        #endregion

        // -----------------------------------------------------------------------
        // LoadHistoryGrid
        // -----------------------------------------------------------------------

        #region LoadHistoryGrid

        [Fact]
        public async Task LoadHistoryGrid_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadHistoryGrid_ReturnsDataGridConfigWithHistoryData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "StartDateTime", Descending = true };
            var history = new List<BatchJobHistoryDto>
            {
                new BatchJobHistoryDto { JobName = JobName, RequestedBy = "user@test.com", Status = "Completed", StartDateTime = DateTime.UtcNow }
            };
            var paginated = new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>(history, 1);

            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(paginated));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.Single(model.Data);
            Assert.Equal(JobName, model.Data[0].JobName);
            Assert.Equal("Completed", model.Data[0].Status);
        }

        [Fact]
        public async Task LoadHistoryGrid_WhenNoPaginationReturned_UsesFallbackPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var response = new ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>
            {
                Success = true,
                Data = new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>(),
                Pagination = null
            };

            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(response);

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.NotNull(model.Pagination);
        }

        [Fact]
        public async Task LoadHistoryGrid_SortColumnAndDirectionSetFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "StartDateTime", Descending = true };
            _yearEndService.GetYearEndDataSetupBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(ApiResponseDto<Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>>
                    .SuccessResponse(new Apha.FPSApps.Application.Pagination.PaginatedResult<BatchJobHistoryDto>()));

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<YearEndHistoryItem>>(partial.Model);
            Assert.Equal("StartDateTime", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        #endregion

        // -----------------------------------------------------------------------
        // EditConfigValue
        // -----------------------------------------------------------------------

        #region EditConfigValue

        [Fact]
        public async Task EditConfigValue_WhenSettingExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var settings = new List<YearEndSettingDto>
            {
                new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 }
            };
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(settings));

            // Act
            var result = await _controller.EditConfigValue("HoursInDay");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditConfigValue", partial.ViewName);
            var model = Assert.IsType<YearEndEditConfigValueModel>(partial.Model);
            Assert.Equal("HoursInDay", model.Id);
            Assert.Equal("7.5", model.Value);
            Assert.Equal(2024, model.FpsYear);
        }

        [Fact]
        public async Task EditConfigValue_WhenSettingNotFound_ReturnsNotFound()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.EditConfigValue("NonExistentKey");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditConfigValue_WhenSettingDataIsNull_ReturnsNotFound()
        {
            // Arrange
            _settingService.GetYearEndSettingsAsync()
                .Returns(new ApiResponseDto<List<YearEndSettingDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.EditConfigValue("HoursInDay");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditConfigValue_IsYesNoTrue_WhenIdContainsApproval()
        {
            // Arrange
            var settings = new List<YearEndSettingDto>
            {
                new YearEndSettingDto { Id = "CapApprovalReset", Setting = "Yes", FpsYear = 2024 }
            };
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(settings));

            // Act
            var result = await _controller.EditConfigValue("CapApprovalReset");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<YearEndEditConfigValueModel>(partial.Model);
            Assert.True(model.IsYesNo);
        }

        [Fact]
        public async Task EditConfigValue_IsYesNoFalse_WhenSettingIsNumericValue()
        {
            // Arrange
            var settings = new List<YearEndSettingDto>
            {
                new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 }
            };
            _settingService.GetYearEndSettingsAsync()
                .Returns(ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(settings));

            // Act
            var result = await _controller.EditConfigValue("HoursInDay");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<YearEndEditConfigValueModel>(partial.Model);
            Assert.False(model.IsYesNo);
        }

        #endregion

        // -----------------------------------------------------------------------
        // EditMonthHour
        // -----------------------------------------------------------------------

        #region EditMonthHour

        [Fact]
        public async Task EditMonthHour_WhenRecordExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var records = new List<YearEndMonthHourDto>
            {
                new YearEndMonthHourDto { Year = 2024, Month = 3, FpsYear = 2024, Fmonth = 3, Days = 21, CvlHours = 5, VidHours = 3 }
            };
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(records));

            // Act
            var result = await _controller.EditMonthHour(2024, 3, 2024, 3);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditMonthHour", partial.ViewName);
            var model = Assert.IsType<YearEndEditMonthHourModel>(partial.Model);
            Assert.Equal(2024, model.Year);
            Assert.Equal(3, model.Month);
            Assert.Equal(2024, model.FpsYear);
            Assert.Equal(21m, model.Days);
        }

        [Fact]
        public async Task EditMonthHour_WhenRecordNotFound_ReturnsNotFound()
        {
            // Arrange
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.EditMonthHour(2099, 1, 2099, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditMonthHour_WhenDataIsNull_ReturnsNotFound()
        {
            // Arrange
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(new ApiResponseDto<List<YearEndMonthHourDto>> { Success = true, Data = null });

            // Act
            var result = await _controller.EditMonthHour(2024, 1, 2024, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditMonthHour_SetsMonthNameFromCultureInfo()
        {
            // Arrange
            var records = new List<YearEndMonthHourDto>
            {
                new YearEndMonthHourDto { Year = 2024, Month = 1, FpsYear = 2024, Fmonth = 1 }
            };
            _monthHourService.GetYearEndMonthHoursAsync()
                .Returns(ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(records));

            // Act
            var result = await _controller.EditMonthHour(2024, 1, 2024, 1);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<YearEndEditMonthHourModel>(partial.Model);
            Assert.Equal("January", model.MonthName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveSetting
        // -----------------------------------------------------------------------

        #region SaveSetting

        [Fact]
        public async Task SaveSetting_WhenServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new SettingDto { Id = "HoursInDay", Setting = "8", FpsYear = 2024 };
            _settingService.SaveSettingAsync(dto)
                .Returns(ApiResponseDto<SettingDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _settingService.Received(1).SaveSettingAsync(dto);
        }

        [Fact]
        public async Task SaveSetting_WhenServiceFails_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            var dto = new SettingDto { Id = "HoursInDay", Setting = "invalid" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Invalid value", Code = "VALIDATION_ERROR" }
            };
            _settingService.SaveSettingAsync(dto)
                .Returns(ApiResponseDto<SettingDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Invalid value", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveSetting_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            _settingService.SaveSettingAsync(dto)
                .Returns(new ApiResponseDto<SettingDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Failed to save setting.", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveSetting_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            _settingService.SaveSettingAsync(dto).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.SaveSetting(dto));
            Assert.Equal("Save failed", exception.Message);
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveMonthHour
        // -----------------------------------------------------------------------

        #region SaveMonthHour

        [Fact]
        public async Task SaveMonthHour_WhenServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, FpsYear = 2024 };
            _monthHourService.SaveMonthHourAsync(dto)
                .Returns(ApiResponseDto<MonthHourDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveMonthHour(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _monthHourService.Received(1).SaveMonthHourAsync(dto);
        }

        [Fact]
        public async Task SaveMonthHour_WhenServiceFails_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -1 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Invalid days value", Code = "VALIDATION_ERROR" }
            };
            _monthHourService.SaveMonthHourAsync(dto)
                .Returns(ApiResponseDto<MonthHourDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveMonthHour(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Invalid days value", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveMonthHour_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1 };
            _monthHourService.SaveMonthHourAsync(dto)
                .Returns(new ApiResponseDto<MonthHourDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.SaveMonthHour(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal("Failed to save month hour.", errorsArray[0].GetProperty("message").GetString());
        }

        #endregion

        // -----------------------------------------------------------------------
        // TriggerInitiate
        // -----------------------------------------------------------------------

        #region TriggerInitiate

        [Fact]
        public async Task TriggerInitiate_WhenServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto()));

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);
        }

        [Fact]
        public async Task TriggerInitiate_WhenServiceFails_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Job already running", Code = "CONFLICT" }
            };
            _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Job already running", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task TriggerInitiate_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear)
                .Returns(new ApiResponseDto<BatchJobQueueDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerInitiate(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal("Failed to trigger Year End Initiation job.", errorsArray[0].GetProperty("message").GetString());
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerInitiate_PassesCorrectPlannedYearToService(int plannedYear)
        {
            // Arrange
            _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto()));

            // Act
            await _controller.TriggerInitiate(plannedYear);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);
        }

        #endregion

        // -----------------------------------------------------------------------
        // TriggerApprove
        // -----------------------------------------------------------------------

        #region TriggerApprove

        [Fact]
        public async Task TriggerApprove_WhenServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.TriggerYearEndDataSetupApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                    new BatchJobEventTriggerDto { EventId = "evt-001", Jobqueue = new BatchJobQueueDto() }));

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _yearEndService.Received(1).TriggerYearEndDataSetupApprovalJobAsync(plannedYear);
        }

        [Fact]
        public async Task TriggerApprove_WhenServiceFails_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" }
            };
            _yearEndService.TriggerYearEndDataSetupApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Approval not allowed", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task TriggerApprove_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.TriggerYearEndDataSetupApprovalJobAsync(plannedYear)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerApprove(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal("Failed to trigger Year End Approval job.", errorsArray[0].GetProperty("message").GetString());
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerApprove_PassesCorrectPlannedYearToService(int plannedYear)
        {
            // Arrange
            _yearEndService.TriggerYearEndDataSetupApprovalJobAsync(plannedYear)
                .Returns(ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                    new BatchJobEventTriggerDto { Jobqueue = new BatchJobQueueDto() }));

            // Act
            await _controller.TriggerApprove(plannedYear);

            // Assert
            await _yearEndService.Received(1).TriggerYearEndDataSetupApprovalJobAsync(plannedYear);
        }

        #endregion

        // -----------------------------------------------------------------------
        // TriggerReject
        // -----------------------------------------------------------------------

        #region TriggerReject

        [Fact]
        public async Task TriggerReject_WhenServiceSucceeds_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(plannedYear);
        }

        [Fact]
        public async Task TriggerReject_WhenServiceFails_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            const int plannedYear = 2025;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" }
            };
            _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal("Rejection not allowed", errorsArray[0].GetProperty("message").GetString());
        }

        [Fact]
        public async Task TriggerReject_WhenServiceReturnsNullErrors_ReturnsDefaultErrorMessage()
        {
            // Arrange
            const int plannedYear = 2025;
            _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(plannedYear)
                .Returns(new ApiResponseDto<bool> { Success = false, Errors = null });

            // Act
            var result = await _controller.TriggerReject(plannedYear);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal("Failed to Reject Year End datasetup job.", errorsArray[0].GetProperty("message").GetString());
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerReject_PassesCorrectPlannedYearToService(int plannedYear)
        {
            // Arrange
            _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(plannedYear)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.TriggerReject(plannedYear);

            // Assert
            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(plannedYear);
        }

        #endregion
    }
}
