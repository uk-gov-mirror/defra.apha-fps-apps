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
    public class PimsPublicationTypeApiClient : IPimsPublicationTypeApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        private const string BaseUrl = "api/v1/publication-types";

        public PimsPublicationTypeApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // GET /api/v1/publication-types
        public async Task<ApiResponseDto<List<PublicationTypeDto>>> GetAllPublicationTypesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<PublicationTypeRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<PublicationTypeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<PublicationTypeDto>>>(response);
                return ApiResponseDto<List<PublicationTypeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<PublicationTypeDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Publication Type data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // GET /api/v1/publication-types/paged
        public async Task<ApiResponseDto<PaginatedResult<PublicationTypeDto>>> GetPagedPublicationTypesAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<PublicationTypeRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<PublicationTypeDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<PublicationTypeDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<PublicationTypeDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<PublicationTypeDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<PublicationTypeDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged Publication Type data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // GET /api/v1/publication-types/{type}
        public async Task<ApiResponseDto<PublicationTypeDto>> GetPublicationTypeByCodeAsync(string type)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(type)}";
                var response = await _http.GetAsync<PublicationTypeRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve Publication Type by code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // POST /api/v1/publication-types
        public async Task<ApiResponseDto<PublicationTypeDto>> CreatePublicationTypeAsync(PublicationTypeDto dto)
        {
            try
            {
                var request = _mapper.Map<PublicationTypeReq>(dto);
                var response = await _http.PostAsync<PublicationTypeReq, PublicationTypeRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create Publication Type", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // PUT /api/v1/publication-types/{type}
        public async Task<ApiResponseDto<PublicationTypeDto>> UpdatePublicationTypeAsync(string type, PublicationTypeDto dto)
        {
            try
            {
                var request = _mapper.Map<PublicationTypeReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(type)}";
                var response = await _http.PutAsync<PublicationTypeReq, PublicationTypeRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<PublicationTypeDto>>(response);
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<PublicationTypeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update Publication Type", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // DELETE /api/v1/publication-types/{type}
        public async Task<ApiResponseDto<bool>> DeletePublicationTypeAsync(string type)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(type)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete Publication Type", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
