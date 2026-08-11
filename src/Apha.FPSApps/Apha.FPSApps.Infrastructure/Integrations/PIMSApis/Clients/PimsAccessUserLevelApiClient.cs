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
    public class PimsAccessUserLevelApiClient : IPimsAccessUserLevelApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsAccessUserLevelApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAsync(QueryParameters<string> request)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAccessUserLevelsPaged, request);
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                {
                    var items       = _mapper.Map<List<AccessUserLevelDto>>(response.Data ?? []);
                    var pageNumber  = response.Pagination?.PageNumber  ?? request.Page;
                    var pageSize    = response.Pagination?.PageSize    ?? request.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<AccessUserLevelDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged AccessUserLevel data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessUserLevelsBySystemId, systemid);
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetByUserAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessUserLevelsByUser, systemid, Uri.EscapeDataString(ntlogin));
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by user", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessUserLevelDto>> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessUserLevelById, systemid, Uri.EscapeDataString(ntlogin), accesslevelid);
                var response = await _http.GetAsync<AccessUserLevelRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessUserLevelDto>> CreateAsync(AccessUserLevelDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserLevelReq>(dto);
                var response = await _http.PostAsync<AccessUserLevelReq, AccessUserLevelRes>(PimsApiEndpoints.CreateAccessUserLevel, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.DeleteAccessUserLevel, systemid, Uri.EscapeDataString(ntlogin), accesslevelid);
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
