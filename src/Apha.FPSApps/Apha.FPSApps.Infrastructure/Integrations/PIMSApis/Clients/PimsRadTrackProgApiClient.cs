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
    public class PimsRadTrackProgApiClient : IPimsRadTrackProgApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
       
        private const string BaseUrl = "api/v1/radtrackprog";

        public PimsRadTrackProgApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<RadTrackProgRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);
                return ApiResponseDto<List<RadTrackProgDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<RadTrackProgDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrackProg data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<RadTrackProgRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<RadTrackProgDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<RadTrackProgDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<RadTrackProgDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<RadTrackProgDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<RadTrackProgDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged RadTrackProg data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByProgramAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.GetAsync<RadTrackProgRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve RadTrackProg by program", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto)
        {
            try
            {
                var request = _mapper.Map<RadTrackProgReq>(dto);
                var response = await _http.PostAsync<RadTrackProgReq, RadTrackProgRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto)
        {
            try
            {
                var request = _mapper.Map<RadTrackProgReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<RadTrackProgDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete RadTrackProg", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // GET /api/v1/radtrackprog/programs — distinct non-null Programme names for dropdown binding
        public async Task<ApiResponseDto<List<string>>> GetAllProgramNamesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>($"{BaseUrl}/programs");
                if (response.Success)
                    return ApiResponseDto<List<string>>.SuccessResponse(response.Data ?? []);

                return ApiResponseDto<List<string>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Programme names", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
