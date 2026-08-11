using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactBosworthInterfaceApiClient : IPactBosworthInterfaceApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactBosworthInterfaceApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TimePurchaseProjectDto>>> GetTimePurchaseProjectAsync(string project)
        {
            var url = $"{PactApiEndpoints.GetTimePurchaseProject}?project={Uri.EscapeDataString(project)}";
            var response = await _http.GetAsync<List<TimePurchaseProjectRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimePurchaseProjectDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<TimePurchaseProjectDto>>>(response);
            return ApiResponseDto<List<TimePurchaseProjectDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta);
        }

        public async Task<ApiResponseDto<List<TimeSaleProfitCentreDto>>> GetTimeSaleProfitCentreAsync(string profitCentre)
        {
            var url = $"{PactApiEndpoints.GetTimeSaleProfitCentre}?profitCentre={Uri.EscapeDataString(profitCentre)}";
            var response = await _http.GetAsync<List<TimeSaleProfitCentreRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeSaleProfitCentreDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<TimeSaleProfitCentreDto>>>(response);
            return ApiResponseDto<List<TimeSaleProfitCentreDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta);
        }

        public async Task<ApiResponseDto<List<TimeSaleWorkGroupDto>>> GetTimeSaleWorkGroupAsync(string workGroup)
        {
            var url = $"{PactApiEndpoints.GetTimeSaleWorkGroup}?workGroup={Uri.EscapeDataString(workGroup)}";
            var response = await _http.GetAsync<List<TimeSaleWorkGroupRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeSaleWorkGroupDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<TimeSaleWorkGroupDto>>>(response);
            return ApiResponseDto<List<TimeSaleWorkGroupDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta);
        }

        public async Task<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>> GetTestSaleSellingWorkgroupAsync(string workGroup)
        {
            var url = $"{PactApiEndpoints.GetTestSaleSellingWorkgroup}?workGroup={Uri.EscapeDataString(workGroup)}";
            var response = await _http.GetAsync<List<TestSaleSellingWorkgroupRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>>(response);
            return ApiResponseDto<List<TestSaleSellingWorkgroupDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta);
        }

        public async Task<ApiResponseDto<List<TestSaleBuyingProjectDto>>> GetTestSaleBuyingProjectAsync(string parentProject)
        {
            var url = $"{PactApiEndpoints.GetTestSaleBuyingProject}?parentProject={Uri.EscapeDataString(parentProject)}";
            var response = await _http.GetAsync<List<TestSaleBuyingProjectRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestSaleBuyingProjectDto>>>(response);

            var failureResponse = _mapper.Map<ApiResponseDto<List<TestSaleBuyingProjectDto>>>(response);
            return ApiResponseDto<List<TestSaleBuyingProjectDto>>.FailureResponse(failureResponse.Errors, failureResponse.Meta); 
        }
    }
}