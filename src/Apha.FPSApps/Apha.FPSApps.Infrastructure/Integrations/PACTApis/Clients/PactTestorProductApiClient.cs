using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Microsoft.AspNetCore.WebUtilities;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactTestorProductApiClient : IPactTestorProductApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactTestorProductApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
        {
            var response = await _http.GetAsync<List<TestorProductRes>>(PactApiEndpoints.GetAllTestorProducts);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);
            return ApiResponseDto<List<TestorProductDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
        {

            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestOrProducts, query);
            var response = await _http.GetAsync<List<TestorProductRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestorProductDto>>>(response);
            return ApiResponseDto<List<TestorProductDto>>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode)
        {

            var response = await _http.GetAsync<TestorProductRes>(string.Format(PactApiEndpoints.GetTestOrProductById, Uri.EscapeDataString(itemCode)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto)
        {

            var request = _mapper.Map<TestorProductReq>(dto);
            var response = await _http.PostAsync<TestorProductReq, TestorProductRes>(PactApiEndpoints.CreateTestOrProduct, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);

        }

        public async Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto)
        {

            var request = _mapper.Map<TestorProductReq>(dto);
            var response = await _http.PutAsync<TestorProductReq, TestorProductRes>(string.Format(PactApiEndpoints.UpdateTestOrProduct, Uri.EscapeDataString(itemCode)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestorProductDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestorProductDto>>(response);
            return ApiResponseDto<TestorProductDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode)
        {

            var response = await _http.DeleteAsync<bool?>(string.Format(PactApiEndpoints.DeleteTestOrProduct, Uri.EscapeDataString(itemCode)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<List<string>>> GetOwnersAsync()
        {

            var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetTestListOwners);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<List<TestPriceCheckDto>>> GetTestPriceCheckPagedAsync(
            QueryParameters<string> query, string priceFilter, string? owner)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetTestPriceCheckPaged, query);
            url = QueryHelpers.AddQueryString(url, "priceFilter", priceFilter);
            if (!string.IsNullOrWhiteSpace(owner))
                url = QueryHelpers.AddQueryString(url, "owner", owner);

            var response = await _http.GetAsync<List<TestPriceCheckRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestPriceCheckDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestPriceCheckDto>>>(response);
            return ApiResponseDto<List<TestPriceCheckDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestPriceCheckDto>> GetTestPriceCheckByKeyAsync(string testCode, string jobCode)
        {
            var url = string.Format(PactApiEndpoints.GetTestPriceCheckByKey,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(jobCode));

            var response = await _http.GetAsync<TestPriceCheckRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestPriceCheckDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TestPriceCheckDto>>(response);
            return ApiResponseDto<TestPriceCheckDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> UpdateTestPriceCheckByKeyAsync(string testCode, string jobCode, TestPriceCheckDto dto)
        {
            var url = string.Format(PactApiEndpoints.UpdateTestPriceCheckByKey,
                Uri.EscapeDataString(testCode), Uri.EscapeDataString(jobCode));

            var request = _mapper.Map<TestPriceCheckReq>(dto);
            var response = await _http.PutAsync<TestPriceCheckReq, bool>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var result = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(result.Errors, result.Meta);
        }
    }
}

