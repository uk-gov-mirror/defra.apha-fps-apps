using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class PactMonthlyOutputService : IPactMonthlyOutputService
    {
        private readonly IPactApiClient _pactApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PactMonthlyOutputService> _logger;
        private static readonly string[] RequiredHeaders =
            ["Work Group", "Test Code", "Buyer", "Month", "Volume"];

        public PactMonthlyOutputService(
            IPactApiClient pactApiClient,
            IExcelImportService excelImportService,
            IWorkGroupService workGroupService,
            IMonthService monthService,
            IS3StorageService s3StorageService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<PactMonthlyOutputService> logger)
        {
            _pactApiClient = pactApiClient;
            _excelImportService = excelImportService;
            _workGroupService = workGroupService;
            _monthService = monthService;
            _s3StorageService = s3StorageService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }        

        public async Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter)
            => await _pactApiClient.PactMonthlyOutput.SearchAsync(query, filter);
        
        public async Task<ApiResponseDto<List<PactMonthlyOutputDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
            => await _pactApiClient.PactMonthlyOutput.GetLiveAsync(query, workGroup, testCode, buyer, month);

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
            => await _pactApiClient.PactMonthlyOutput.GetLiveByKeyAsync(testCode, buyer, month, workGroup);

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> UpdateLiveAsync(PactMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.UpdateLiveAsync(dto);

        public async Task<ApiResponseDto<List<ValidationFieldErrorDto>>> ValidateLiveAsync(PactMonthlyOutputDto dto)
        {
            var errors = new List<ValidationFieldErrorDto>();

            var workGroup = dto.WorkGroup?.Trim();
            var testCode  = dto.TestCode?.Trim();
            var buyer     = dto.Buyer?.Trim();

            if (dto.Volume is null || dto.Volume <= 0)
                errors.Add(new ValidationFieldErrorDto { Field = "Volume", Message = "Volume must be greater than zero." });

            if (string.IsNullOrWhiteSpace(workGroup))
            {
                errors.Add(new ValidationFieldErrorDto { Field = "WorkGroup", Message = "The work group name is blank." });
            }
            else
            {
                var wgResponse = await _workGroupService.GetAllWorkGroupsAsync();
                var validWorkGroups = wgResponse.Success && wgResponse.Data != null
                    ? wgResponse.Data.Select(x => x.WorkGroupName)
                    : [];
                if (!validWorkGroups.Any(x => string.Equals(x, workGroup, StringComparison.OrdinalIgnoreCase)))
                    errors.Add(new ValidationFieldErrorDto { Field = "WorkGroup", Message = $"The work group name is invalid: {workGroup}" });
            }

            if (string.IsNullOrWhiteSpace(testCode))
                errors.Add(new ValidationFieldErrorDto { Field = "TestCode", Message = "The test code is blank." });

            if (string.IsNullOrWhiteSpace(buyer))
                errors.Add(new ValidationFieldErrorDto { Field = "Buyer", Message = "The buyer is blank." });

            var monthResponse = await _monthService.GetAllMonthsAsync();
            var validMonths = monthResponse.Success && monthResponse.Data != null
                ? monthResponse.Data.Select(x => x.Monthnumber.ToString())
                : [];
            var monthValue = dto.Month.ToString("0");
            if (!validMonths.Any(x => string.Equals(x, monthValue, StringComparison.OrdinalIgnoreCase)))
                errors.Add(new ValidationFieldErrorDto { Field = "Month", Message = $"The month number is invalid: {dto.Month}" });

            return ApiResponseDto<List<ValidationFieldErrorDto>>.SuccessResponse(errors);
        }

        public async Task<ApiResponseDto<List<StagingMonthlyOutputDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed)
            => await _pactApiClient.PactMonthlyOutput.GetStagingAsync(query, passed);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> GetStagingByIdAsync(int id)
            => await _pactApiClient.PactMonthlyOutput.GetStagingByIdAsync(id);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> CreateStagingAsync(StagingMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.CreateStagingAsync(dto);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> UpdateStagingAsync(int id, StagingMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.UpdateStagingAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteStagingAsync(int id)
            => await _pactApiClient.PactMonthlyOutput.DeleteStagingAsync(id);

        public async Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync()
            => await _pactApiClient.PactMonthlyOutput.DeleteAllStagingByUserAsync();

        public async Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync()
            => await _pactApiClient.PactMonthlyOutput.DeleteFailedStagingByUserAsync();        

        public async Task<ApiResponseDto<MonthlyOutputImportResultDto>> ImportMonthlyOutputAsync(IFormFile file, short importType)
        {
            using var originalFileStream = file.OpenReadStream();
            using var bufferStream = new MemoryStream();
            await originalFileStream.CopyToAsync(bufferStream);

            bufferStream.Position = 0;
            using var workbook = new XLWorkbook(bufferStream);

            ApiResponseDto<MonthlyOutputImportResultDto> importResponse;

            if (importType == 4)
            {
                importResponse = await ImportExportedDataAsync(file.FileName, workbook);
            }
            else
            {
                var importResult = _excelImportService.ReadExcel(
                    workbook,
                    MapOutputRow,
                    RequiredHeaders,
                    1,
                    "The uploaded Excel file format is not correct. Please use the correct PACT flat file template.");

                if (!importResult.IsSuccess)
                {
                    var errors = new List<ApiErrorDto>();
                    if (importResult.MissingHeaders?.Count > 0)
                        errors.Add(new ApiErrorDto
                        {
                            Code = "INVALID_TEMPLATE",
                            Message = $"Missing columns: {string.Join(", ", importResult.MissingHeaders)}. " +
                                      "Please use the correct PACT flat file template."
                        });
                    else
                        errors.Add(new ApiErrorDto
                        {
                            Code = "EMPTY_FILE",
                            Message = importResult.ErrorMessage ?? "No data rows found in the uploaded Excel file."
                        });

                    return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                        errors,
                        new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
                }

                var request = new MonthlyOutputImportReqDto
                {
                    FileName = file.FileName,
                    ImportType = 1,
                    Rows = importResult.Rows
                };

                importResponse = await _pactApiClient.PactMonthlyOutput.ImportStagingAsync(request);
            }

            if (!importResponse.Success || importResponse.Data == null)
            {
                return importResponse;
            }

            bufferStream.Position = 0;
            try
            {
                var uploadResult = await UploadAuditFileAsync(file, bufferStream);
                if (!uploadResult.Success)
                {
                    _logger.LogWarning(
                        "Monthly output import succeeded but S3 audit upload failed. FileName: {FileName}, ErrorCode: {ErrorCode}, Message: {Message}",
                        file.FileName,
                        uploadResult.ErrorCode,
                        uploadResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Monthly output import succeeded but S3 audit upload threw an exception. FileName: {FileName}",
                    file.FileName);
            }

            return importResponse;
        }

        private async Task<ApiResponseDto<MonthlyOutputImportResultDto>> ImportExportedDataAsync(string fileName, XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheet(1);
            var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? [];
            if (usedRows.Count <= 1)
            {
                return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "EMPTY_FILE", Message = "No data rows found in the uploaded Excel file." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var headerMap = _excelImportService.BuildHeaderMap(usedRows[0]);
            var missingStagingIdColumn = _excelImportService.GetMissingRequiredHeaders(headerMap, ["StagingId"]).Any();
            if (missingStagingIdColumn)
            {
                return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_TEMPLATE", Message = "This file is not a valid correction file. Please use the exported file without removing the hidden StagingId column." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var requiredHeaders = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapExportedOutputRow,
                requiredHeaders,
                1,
                "The uploaded Excel file format is not correct. Please use the correct exported file template.");

            if (!importResult.IsSuccess)
            {
                var errors = new List<ApiErrorDto>
                {
                    new ApiErrorDto
                    {
                        Code = importResult.MissingHeaders.Count > 0 ? "INVALID_TEMPLATE" : "EMPTY_FILE",
                        Message = importResult.ErrorMessage ?? "Import failed."
                    }
                };

                return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                    errors,
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var request = new MonthlyOutputImportReqDto
            {
                FileName = fileName,
                ImportType = 4,
                Rows = importResult.Rows
            };

            return await _pactApiClient.PactMonthlyOutput.ImportStagingAsync(request);
        }

        public async Task<ApiResponseDto<MonthlyOutputValidateResultDto>> ValidateStagingAsync()
            => await _pactApiClient.PactMonthlyOutput.ValidateStagingAsync();

        public async Task<ApiResponseDto<MonthlyOutputMakeLiveResultDto>> MakeLiveAsync()
            => await _pactApiClient.PactMonthlyOutput.MakeLiveAsync();

        private MonthlyOutputImportRowDto MapOutputRow(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            return new MonthlyOutputImportRowDto
            {
                WorkGroup       = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Work Group")])),
                TestCode        = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Test Code")])),
                //ItemDescription = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Item Description")])),
                Buyer           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Buyer")])),
                Month           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])),
                Volume          = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Volume")]))
            };
        }

        private MonthlyOutputImportRowDto MapExportedOutputRow(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            var stagingIdText = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("StagingId")])) ;
            return new MonthlyOutputImportRowDto
            {
                Id              = int.TryParse(stagingIdText, out var parsedStagingId) ? parsedStagingId : 0,
                WorkGroup       = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Work Group")])) ,
                TestCode        = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Test Code")])) ,
                Buyer           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Buyer")])) ,
                Month           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])) ,
                Volume          = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Volume")]))
            };
        }

        private async Task<S3UploadResult> UploadAuditFileAsync(IFormFile file, Stream fileStream)
        {
            var sourceFileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                sourceFileName = "monthly-output-import.xlsx";
            }

            var timestamp = DateTime.UtcNow;
            var selectedYear = timestamp.Year;
            var selectedYearItem = _httpContextAccessor.HttpContext?.Items["SelectedFPSYear"];
            if (selectedYearItem != null && int.TryParse(selectedYearItem.ToString(), out var parsedYear) && parsedYear > 0)
            {
                selectedYear = parsedYear;
            }

            var folderPath = $"FPS{selectedYear}/MonthlyOutput";

            var originalName = Path.GetFileNameWithoutExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(originalName))
            {
                originalName = "monthly-output-import";
            }

            var extension = Path.GetExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = ".xlsx";
            }

            var auditFileName = $"{originalName}_{timestamp:yyyyMMddHHmmss}{extension}";

            return await _s3StorageService.UploadFileAsync(
                fileStream,
                GetAuditBucketName(),
                folderPath,
                auditFileName,
                file.ContentType);
        }

        private string GetAuditBucketName()
            => _configuration["S3Storage:BucketName"]
               ?? throw new InvalidOperationException("S3Storage:BucketName is not configured.");
    }
}

