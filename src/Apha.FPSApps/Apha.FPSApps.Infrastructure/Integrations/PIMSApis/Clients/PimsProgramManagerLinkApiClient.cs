using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProgramManagerLinkApiClient : IPimsProgramManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/programmanagerlink";

        public PimsProgramManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedByManagerAsync(
            QueryParameters<string> query,
            string manager)
        {
            try
            {
                var url = $"{BaseUrl}/paged?" +
                    $"search={Uri.EscapeDataString(query.Search ?? string.Empty)}" +
                    $"&sortBy={Uri.EscapeDataString(query.SortBy ?? string.Empty)}" +
                    $"&descending={query.Descending}" +
                    $"&page={query.Page}" +
                    $"&pageSize={query.PageSize}" +
                    $"&manager={Uri.EscapeDataString(manager)}";

                if (!string.IsNullOrWhiteSpace(query.Filter))
                {
                    url += $"&filter={Uri.EscapeDataString(query.Filter)}";
                }

                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ProgramManagerLinkDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ProgramManagerLinkDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged ProgramManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByProgramAsync(string program)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}";
                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink by program", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByManagerAsync(string manager)
        {
            try
            {
                var url = $"{BaseUrl}/manager/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<List<ProgramManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink by manager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<ProgramManagerLinkRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProgramManagerLink by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto)
        {
            try
            {
                var request = _mapper.Map<ProgramManagerLinkReq>(dto);
                var response = await _http.PostAsync<ProgramManagerLinkReq, ProgramManagerLinkRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProgramManagerLinkDto>>(response);
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProgramManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProgramManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProgramLookupRes>>($"{BaseUrl}/programs");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramLookupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProgramLookupDto>>>(response);
                return ApiResponseDto<List<ProgramLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramLookupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Program lookup data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(program)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProgramManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
