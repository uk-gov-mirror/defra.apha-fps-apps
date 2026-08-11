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
    public class PimsReportApiClient : IPimsReportApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        private const string BaseUrl = "api/v1/report";

        public PimsReportApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReportRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportDto>>>(response);
                return ApiResponseDto<List<ReportDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Report data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // GET /api/v1/report/paged
        public async Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<ReportRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ReportDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ReportDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ReportDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ReportDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ReportDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged Report data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.GetAsync<ReportRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Report by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportReq>(dto);
                var response = await _http.PostAsync<ReportReq, ReportRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto)
        {
            try
            {
                var request = _mapper.Map<ReportReq>(dto);
                var url = $"{BaseUrl}/{id}";
                var response = await _http.PutAsync<ReportReq, ReportRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportDto>>(response);
                return ApiResponseDto<ReportDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReportDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteReportAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete Report", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
