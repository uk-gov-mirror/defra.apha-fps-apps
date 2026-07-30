using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsYearlyFinancialDataApiClient : IPimsYearlyFinancialDataApiClient
    {
        
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsYearlyFinancialDataApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<YearlyFinancialDataDto>>> GetAllAsync(
            string project, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(
                    string.Format(PimsApiEndpoints.GetAllYearlyFinancialData, Uri.EscapeDataString(project)),
                    query);
                var response = await _http.GetAsync<List<YearlyFinancialDataRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<YearlyFinancialDataDto>>>(response);
                return ApiResponseDto<List<YearlyFinancialDataDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<YearlyFinancialDataDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve yearly financial data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> GetByKeyAsync(short year, string project)
        {
            try
            {
                string url = string.Format(
                    PimsApiEndpoints.GetYearlyFinancialDataByKey,
                    year,
                    Uri.EscapeDataString(project));
                var response = await _http.GetAsync<YearlyFinancialDataRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve yearly financial data by key", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<YearlyFinancialDataDto>> CreateAsync(YearlyFinancialDataDto dto)
        {
            try
            {
                YearlyFinancialDataReq request = _mapper.Map<YearlyFinancialDataReq>(dto);
                var response = await _http.PostAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(
                    PimsApiEndpoints.CreateYearlyFinancialData,
                    request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<YearlyFinancialDataDto>> UpdateAsync(
            short year, string project, YearlyFinancialDataDto dto)
        {
            try
            {
                YearlyFinancialDataReq request = _mapper.Map<YearlyFinancialDataReq>(dto);
                string url = string.Format(
                    PimsApiEndpoints.UpdateYearlyFinancialData,
                    year,
                    Uri.EscapeDataString(project));
                var response = await _http.PutAsync<YearlyFinancialDataReq, YearlyFinancialDataRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<YearlyFinancialDataDto>>(response);
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<YearlyFinancialDataDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<object>> DeleteAsync(short year, string project)
        {
            try
            {
                string url = string.Format(
                    PimsApiEndpoints.DeleteYearlyFinancialData,
                    year,
                    Uri.EscapeDataString(project));
                var response = await _http.DeleteAsync<object>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<object>>(response);

                var dto = _mapper.Map<ApiResponseDto<object>>(response);
                return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<object>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete yearly financial data record", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
       
        public async Task<ApiResponseDto<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
        {
            try
            {
                string url = string.Format(
                    PimsApiEndpoints.GetYearlyFinancialDataPactCosts,
                    Uri.EscapeDataString(project),
                    year);
                var response = await _http.GetAsync<List<PactProjectYearCostsRes>>(url);
                if (response.Success)
                {
                    List<PactProjectYearCostsRes> rows = response.Data ?? [];
                    if (rows.Count == 0)
                    {
                        return ApiResponseDto<PactProjectYearCostsDto>.SuccessResponse(new PactProjectYearCostsDto
                        {
                            Project = project,
                            Year = year
                        });
                    }

                    PactProjectYearCostsRes firstRow = rows[0];
                    var aggregated = new PactProjectYearCostsDto
                    {
                        Project = firstRow.Project ?? project,
                        Year = firstRow.Year,
                        SubContracts = rows.Sum(x => x.SubContracts ?? 0m),
                        Animals = rows.Sum(x => x.Animals ?? 0m),
                        Tests = rows.Sum(x => x.Tests ?? 0m),
                        Pay = rows.Sum(x => x.Pay ?? 0m),
                        NonPayOH = rows.Sum(x => x.NonPayOH ?? 0m),
                        TotalCosts = rows.Sum(x => x.TotalCosts ?? 0m),
                        TimeCost = rows.Sum(x => x.TimeCost ?? 0m),
                        Hours = rows.Sum(x => x.Hours ?? 0d),
                        CustIncome = firstRow.CustIncome,
                        BudgetCvl = firstRow.BudgetCvl
                    };

                    return ApiResponseDto<PactProjectYearCostsDto>.SuccessResponse(aggregated);
                }

                var dto = _mapper.Map<ApiResponseDto<PactProjectYearCostsDto>>(response);
                return ApiResponseDto<PactProjectYearCostsDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<PactProjectYearCostsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve PACT project year costs", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string id)
        {
            try
            {
                string url = string.Format(PimsApiEndpoints.GetSettingValueById, Uri.EscapeDataString(id));
                var response = await _http.GetAsync<string>(url);
                if (response.Success && response.Data != null)
                {
                    return ApiResponseDto<string>.SuccessResponse(response.Data);
                }

                var dto = _mapper.Map<ApiResponseDto<string>>(response);
                return ApiResponseDto<string>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<string>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve PIMS setting value", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
