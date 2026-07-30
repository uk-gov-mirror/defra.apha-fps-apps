using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    /// <summary>
    /// HTTP API client for project-specific component charges (TestRequirementRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE api/v1/testrequirementrccost
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// buyer is from the test requirement tab row; profitCentre is from the RC cost subform row.
    /// </summary>
    public class FpsTestRequirementRCCostApiClient : IFpsTestRequirementRCCostApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string BaseUrl = "api/v1/testrequirementrccost";
        public FpsTestRequirementRCCostApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestRequirementRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear)
        {
            var url = $"{BaseUrl}/{testCode}";
            var response = await _http.GetAsync<List<TestRequirementRCCostRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(response);
            return ApiResponseDto<List<TestRequirementRCCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<TestRequirementRCCostDto>> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}";
            var response = await _http.GetAsync<TestRequirementRCCostRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
            return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        //   TestRequirementRCCostDto mapped to TestRequirementRCCostReq for the request body
        public async Task<ApiResponseDto<TestRequirementRCCostDto>> CreateAsync(TestRequirementRCCostDto dto)
        {
            var request = _mapper.Map<TestRequirementRCCostReq>(dto);
            var response = await _http.PostAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(BaseUrl, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
            return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        //   All four PK segments placed in path; DTO body carries the full writable payload
        public async Task<ApiResponseDto<TestRequirementRCCostDto>> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto)
        {
            var request = _mapper.Map<TestRequirementRCCostReq>(dto);
            var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}/{fpsYear}";
            var response = await _http.PutAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(response);
            return ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear)
        {
            var url = $"{BaseUrl}/{testCode}/{buyer}/{profitCentre}/{fpsYear}";
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
