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
    public class PimsReportGroupApiClient : IPimsReportGroupApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/reportgroup";

        public PimsReportGroupApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReportGroupRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroup data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId)
        {
            try
            {
                var url = $"{BaseUrl}/byreport/{reportId}";
                var response = await _http.GetAsync<List<ReportGroupRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroups by report ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                if (reportId.HasValue)
                    url += $"&reportid={reportId.Value}";

                var response = await _http.GetAsync<List<ReportGroupRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ReportGroupDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ReportGroupDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ReportGroupDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ReportGroupDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ReportGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged ReportGroup data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId)
        {
            try
            {
                var url = $"{BaseUrl}/{groupId}";
                var response = await _http.GetAsync<ReportGroupRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroup by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportGroupReq>(dto);
                var response = await _http.PostAsync<ReportGroupReq, ReportGroupRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ReportGroup", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportGroupReq>(dto);
                var url = $"{BaseUrl}/{groupId}";
                var response = await _http.PutAsync<ReportGroupReq, ReportGroupRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
                return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ReportGroup", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId)
        {
            var url = $"{BaseUrl}/{groupId}";
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
