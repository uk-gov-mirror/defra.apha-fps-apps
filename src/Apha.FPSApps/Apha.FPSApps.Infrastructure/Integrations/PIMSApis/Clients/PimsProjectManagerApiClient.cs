using Apha.Common.Contracts;
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
    public class PimsProjectManagerApiClient : IPimsProjectManagerApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/projectmanager";

        public PimsProjectManagerApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync(QueryParameters<string>? query = null)
        {
            try
            {
                query ??= new QueryParameters<string>();
                string url = QueryStringHelper.AddQueryString(BaseUrl, query);
                var response = await _http.GetAsync<List<ProjectManagerRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectManagerDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProjectManagerDto>>>(response);
                return ApiResponseDto<List<ProjectManagerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectManagerDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProjectManager data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<string>>> GetManagerNamesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>($"{BaseUrl}/names");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProjectManager names", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<PaginatedResult<ProjectManagerDto>>> GetPagedProjectManagersAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(BaseUrl, query);
                var response = await _http.GetAsync<List<ProjectManagerRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ProjectManagerDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ProjectManagerDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ProjectManagerDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ProjectManagerDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ProjectManagerDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged ProjectManager data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByNameAsync(string projectManagerName)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectManagerName)}";
                var response = await _http.GetAsync<ProjectManagerRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProjectManager by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto)
        {
            try
            {
                var request = _mapper.Map<ProjectManagerReq>(dto);
                var response = await _http.PostAsync<ProjectManagerReq, ProjectManagerRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectManagerName, ProjectManagerDto dto)
        {
            try
            {
                var request = _mapper.Map<ProjectManagerReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectManagerName)}";
                var response = await _http.PutAsync<ProjectManagerReq, ProjectManagerRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectManagerName)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectManagerName)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
