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
    public class PimsReviewItemApiClient : IPimsReviewItemApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/reviewitem";

        public PimsReviewItemApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReviewItemRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);
                return ApiResponseDto<List<ReviewItemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReviewItemDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReviewItem data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<ReviewItemRes>>(url);
                if (response.Success)
                {
                    var items = _mapper.Map<List<ReviewItemDto>>(response.Data ?? []);
                    var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                    var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<ReviewItemDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<ReviewItemDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<ReviewItemDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<ReviewItemDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged ReviewItem data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId)
        {
            try
            {
                var url = $"{BaseUrl}/{itemId}";
                var response = await _http.GetAsync<ReviewItemRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReviewItem by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto)
        {
            try
            {
                var request = _mapper.Map<ReviewItemReq>(dto);
                var response = await _http.PostAsync<ReviewItemReq, ReviewItemRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto)
        {
            try
            {
                var request = _mapper.Map<ReviewItemReq>(dto);
                var url = $"{BaseUrl}/{itemId}";
                var response = await _http.PutAsync<ReviewItemReq, ReviewItemRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
                return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ReviewItemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

       
        public async Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId)
        {
            try
            {
                var url = $"{BaseUrl}/{itemId}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ReviewItem", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
