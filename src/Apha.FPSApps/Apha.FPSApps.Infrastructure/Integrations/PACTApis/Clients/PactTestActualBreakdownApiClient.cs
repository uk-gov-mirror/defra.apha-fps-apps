using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactTestActualBreakdownApiClient : IPactTestActualBreakdownApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactTestActualBreakdownApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestActualBreakdownDto>>> GetActualsTestsWithPlannedDataByWorkgroupAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestActualBreakdown, query);
            var response = await _http.GetAsync<List<TestActualBreakdownRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(response);
            return ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
