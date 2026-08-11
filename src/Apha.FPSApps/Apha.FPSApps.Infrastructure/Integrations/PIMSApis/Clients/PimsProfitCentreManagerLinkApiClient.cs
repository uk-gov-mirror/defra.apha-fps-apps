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
    public class PimsProfitCentreManagerLinkApiClient : IPimsProfitCentreManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/profitcentremanagerlink";

        public PimsProfitCentreManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProfitCentreLookupRes>>($"{BaseUrl}/profitcentres");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreLookupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreLookupDto>>>(response);
                return ApiResponseDto<List<ProfitCentreLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreLookupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentre lookup data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByProfitCentreAsync(string profitCentre)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitCentre)}";
                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink by profit centre", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByManagerAsync(string manager)
        {
            try
            {
                var url = $"{BaseUrl}/manager/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink by manager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedByManagerAsync(QueryParameters<string> query, string manager)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                url += $"&manager={Uri.EscapeDataString(manager)}";

                var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ProfitCentreManagerLinkDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ProfitCentreManagerLinkDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged ProfitCentreManagerLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitCentre)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.GetAsync<ProfitCentreManagerLinkRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProfitCentreManagerLink by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto)
        {
            try
            {
                var request = _mapper.Map<ProfitCentreManagerLinkReq>(dto);
                var response = await _http.PostAsync<ProfitCentreManagerLinkReq, ProfitCentreManagerLinkRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProfitCentreManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(profitCentre)}/{Uri.EscapeDataString(manager)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProfitCentreManagerLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
