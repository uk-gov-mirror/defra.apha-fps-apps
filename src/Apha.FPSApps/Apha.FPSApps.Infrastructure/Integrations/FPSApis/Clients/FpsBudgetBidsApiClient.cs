using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsBudgetBidsApiClient : IFpsBudgetBidsApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsBudgetBidsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http   = http   ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            var response = await _http.GetAsync<List<BidViewRes>>(string.Format(FpsApiEndpoints.GetBids, workgroup));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
                return ApiResponseDto<List<BidViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewPagedAsync(
            QueryParameters<string> query, string workgroup)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetBidsPagedView, query);
            url = QueryHelpers.AddQueryString(url, "workgroup", workgroup);

            var response = await _http.GetAsync<IEnumerable<BidViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
                return ApiResponseDto<List<BidViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string WorkGroupName, string account)
        {
            var response = await _http.GetAsync<BidRes>(string.Format(FpsApiEndpoints.GetBidByWorkgroupAccount, WorkGroupName, account));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<BidDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
                return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PostAsync<BidReq, BidRes>(FpsApiEndpoints.CreateBudgetBid, req);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<BidDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
                return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PutAsync<BidReq, BidRes>(FpsApiEndpoints.UpdateBudgetBid, req);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<BidDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
                return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteBudgetBid, bid.WorkGroupName, bid.Account));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            var response = await _http.GetAsync<List<AccountCategoryRes>>(FpsApiEndpoints.GetBudgetBidsAccounts);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
                return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<GenericBidViewDto>>> GetGenericBidsPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetGenericBidsPaged, query);

            var response = await _http.GetAsync<IEnumerable<GenericBidViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<GenericBidViewDto>>>(response);
                return ApiResponseDto<List<GenericBidViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
