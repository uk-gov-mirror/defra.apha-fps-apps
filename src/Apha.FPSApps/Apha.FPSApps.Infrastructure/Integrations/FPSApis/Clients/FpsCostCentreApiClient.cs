using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Globalization;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsCostCentreApiClient : IFpsCostCentreApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsCostCentreApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<CostCentreWorkgroupDto>>> GetAllCostCentresAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<CostCentreWorkgroupRes>>(FpsApiEndpoints.GetAllCostCentres);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(response);
                return ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve cost centre workgroup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<CostCentreDto>>> GetAllCostCentresPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedCostCentres, query);
                var response = await _http.GetAsync<List<CostCentreRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CostCentreDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<CostCentreDto>>>(response);
                return ApiResponseDto<List<CostCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CostCentreDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve paged cost centre data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   double formatted culture-invariant to avoid locale-dependent decimal separators in URL path
        public async Task<ApiResponseDto<CostCentreDto>> GetCostCentreByIdAsync(double costCentreNo)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetCostCentreById, costCentreNo.ToString("G", CultureInfo.InvariantCulture));
                var response = await _http.GetAsync<CostCentreRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CostCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CostCentreDto>>(response);
                return ApiResponseDto<CostCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CostCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve cost centre by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   CostCentreDto mapped to CostCentreReq (FpsYear excluded from Req — set server-side by request context)
        public async Task<ApiResponseDto<CostCentreDto>> CreateCostCentreAsync(CostCentreDto costCentreDto)
        {
            try
            {
                var request = _mapper.Map<CostCentreReq>(costCentreDto);
                var response = await _http.PostAsync<CostCentreReq, CostCentreRes>(FpsApiEndpoints.CreateCostCentre, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CostCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CostCentreDto>>(response);
                return ApiResponseDto<CostCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CostCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create cost centre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   costCentreNo (original) placed in path to identify existing record; CostCentreDto.CostCentreNo may differ (rename scenario)
        public async Task<ApiResponseDto<CostCentreDto>> UpdateCostCentreAsync(double costCentreNo, CostCentreDto costCentreDto)
        {
            try
            {
                var request = _mapper.Map<CostCentreReq>(costCentreDto);
                var url = string.Format(FpsApiEndpoints.UpdateCostCentre, costCentreNo.ToString("G", CultureInfo.InvariantCulture));
                var response = await _http.PutAsync<CostCentreReq, CostCentreRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CostCentreDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CostCentreDto>>(response);
                return ApiResponseDto<CostCentreDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CostCentreDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update cost centre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   bool? used as generic arg (nullable response body); return type is ApiResponseDto<bool>
        public async Task<ApiResponseDto<bool>> DeleteCostCentreAsync(double costCentreNo)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.DeleteCostCentre, costCentreNo.ToString("G", CultureInfo.InvariantCulture));
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete cost centre", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
