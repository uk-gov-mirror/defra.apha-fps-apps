using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProfitCentreApiClient : IFpsProfitCentreApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProfitCentreApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProfitCentreRes>>(FpsApiEndpoints.GetProfitCentres);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ProfitCentres data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            try
            {
                var response = await _http.GetAsync<IEnumerable<ProfitCentreRes>>(FpsApiEndpoints.GetAllProfitCentres);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);
                return ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve all ProfitCentres", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetAllProfitCentresPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProfitCentres, query);
                var response = await _http.GetAsync<List<ProfitCentreRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve paged ProfitCentres", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentreId)
        {
            try
            {
                var response = await _http.GetAsync<ProfitCentreRes>(string.Format(FpsApiEndpoints.GetProfitCentreById, profitCentreId));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ProfitCentre by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> CreateProfitCentreAsync(ProfitCentreDto profitCentreDto)
        {
            try
            {
                var request = _mapper.Map<ProfitCentreReq>(profitCentreDto);
                var response = await _http.PostAsync<ProfitCentreReq, ProfitCentreRes>(FpsApiEndpoints.CreateProfitCentre, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create ProfitCentre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> UpdateProfitCentreAsync(string profitCentreId, ProfitCentreDto profitCentreDto)
        {
            try
            {
                var request = _mapper.Map<ProfitCentreReq>(profitCentreDto);
                var response = await _http.PutAsync<ProfitCentreReq, ProfitCentreRes>(string.Format(FpsApiEndpoints.UpdateProfitCentre, profitCentreId), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update ProfitCentre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreAsync(string profitCentreId)
        {
            try
            {
                var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteProfitCentre, profitCentreId));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete ProfitCentre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            try
            {
                var request = new UpdateProfitCentreSettingsReq
                {
                    ProfitCentre = profitCentre,
                    Timesheet = timesheet,
                    Outputsheet = outputsheet,
                    TimesheetLayout = timesheetLayout
                };
                var response = await _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                    FpsApiEndpoints.PatchProfitCentreSettings, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update ProfitCentre settings", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProfitCentreCostDto>>> GetPagedProfitCenterCostSummaryAsync(
            QueryParameters<string> query, double monthNumber)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProfitCenterCostSummary, query);
                url = $"{url}&monthNumber={monthNumber}";
                var response = await _http.GetAsync<List<ProfitCentreCostRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(response);

                var failDto = _mapper.Map<ApiResponseDto<List<ProfitCentreCostDto>>>(response);
                return ApiResponseDto<List<ProfitCentreCostDto>>.FailureResponse(failDto.Errors, failDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreCostDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ProfitCentre cost summary", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}