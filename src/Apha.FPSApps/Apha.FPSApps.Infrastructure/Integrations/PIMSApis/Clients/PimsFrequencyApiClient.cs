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
    public class PimsFrequencyApiClient : IPimsFrequencyApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        private const string BaseUrl = "api/v1/frequency";

        public PimsFrequencyApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<FrequencyRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);
                return ApiResponseDto<List<FrequencyDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<FrequencyDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Frequency data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<FrequencyRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<FrequencyDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<FrequencyDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<FrequencyDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<FrequencyDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<FrequencyDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged Frequency data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId)
        {
            try
            {
                var url = $"{BaseUrl}/{frequencyId}";
                var response = await _http.GetAsync<FrequencyRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Frequency by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto)
        {
            try
            {
                var request = _mapper.Map<FrequencyReq>(dto);
                var response = await _http.PostAsync<FrequencyReq, FrequencyRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto)
        {
            try
            {
                var request = _mapper.Map<FrequencyReq>(dto);
                var url = $"{BaseUrl}/{frequencyId}";
                var response = await _http.PutAsync<FrequencyReq, FrequencyRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
                return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<FrequencyDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId)
        {
            try
            {
                var url = $"{BaseUrl}/{frequencyId}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete Frequency", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
