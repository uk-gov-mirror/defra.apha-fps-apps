using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.Common.Utilities.ExcelImport;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IPactApiClient _pactClient;
        private readonly IExcelImportService _excelImportService;

        public ProjectSubContractService(IPactApiClient pactClient, IExcelImportService excelImportService)
        {
            _pactClient = pactClient;
            _excelImportService = excelImportService;
        }

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetPagedProjectSubContractsAsync(query, project);

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsManualAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetPagedProjectSubContractsManualAsync(query, project);

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project)
            => await _pactClient.PactProjectSubContract.GetTotalAmountAsync(project);

        public async Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.GetByIdAsync(subContCounter);

        public async Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.UpdateAsync(subContCounter, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.DeleteAsync(subContCounter);

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractsAsync(query, project, filterByAnimalAcctCodes);

        public async Task<ApiResponseDto<decimal>> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractTotalAmountAsync(project, filterByAnimalAcctCodes);

        public async Task<ApiResponseDto<MonthlySubContractsPivotDto>> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
           => await _pactClient.PactProjectSubContract.GetMonthlySubContractsSummaryAsync(query);

        public async Task<ApiResponseDto<List<SubContractRmsImportRowDto>>> GetFailedSubContractRmsAsync(QueryParameters<string> query)
            => await _pactClient.PactProjectSubContract.GetFailedSubContractRmsAsync(query);

        public async Task<ApiResponseDto<SubContractRmsImportRowDto>> GetFailedSubContractRmsByIdAsync(int id)
            => await _pactClient.PactProjectSubContract.GetFailedSubContractRmsByIdAsync(id);

        public async Task<ApiResponseDto<bool>> SaveFailedSubContractRmsAsync(int id, SubContractRmsImportRowDto dto)
            => await _pactClient.PactProjectSubContract.SaveFailedSubContractRmsAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteFailedSubContractRmsByIdAsync(int id)
            => await _pactClient.PactProjectSubContract.DeleteFailedSubContractRmsByIdAsync(id);

        public async Task<ApiResponseDto<SubContractRmsImportResultDto>> ImportSubContractRmsAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var requiredHeaders = new[]
            {
                "Project",
                "Test Job",
                "Month",
                "Amount",
                "Account Code",
                "Supplier",
                "Description",
                "Supplier Number",
                "Daily Rate",
                "Animal Days"
            };

            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapRowToDto,
                requiredHeaders,
                worksheetIndex: 1);

            if (!importResult.IsSuccess)
            {
                return ApiResponseDto<SubContractRmsImportResultDto>.FailureResponse(
                    new List<ApiErrorDto>
                    {
                        new ApiErrorDto
                        {
                            Code = importResult.MissingHeaders.Count > 0 ? "INVALID_TEMPLATE" : "EMPTY_FILE",
                            Message = importResult.ErrorMessage ?? "Import failed."
                        }
                    },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var request = new SubContractRmsImportReqDto
            {
                FileName = file.FileName,
                Rows = importResult.Rows
            };

            return await _pactClient.PactProjectSubContract.ImportSubContractRmsAsync(request);
        }

        private SubContractRmsImportRowDto MapRowToDto(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            return new SubContractRmsImportRowDto
            {
                Project = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Project")])),
                TestJob = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Test Job")])),
                Month = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])),
                Amount = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Amount")])),
                AcctCode = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Account Code")])),
                Supplier = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Supplier")])),
                Description = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Description")])),
                SupplierNumber = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Supplier Number")])),
                DailyRate = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Daily Rate")])),
                AnimalDays = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Animal Days")]))
            };
        }

        public async Task<ApiResponseDto<bool>> DeleteFailedSubContractRmsByUserAsync()
            => await _pactClient.PactProjectSubContract.DeleteFailedSubContractRmsByUserAsync();
    }
}
