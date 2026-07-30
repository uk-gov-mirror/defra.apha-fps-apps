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
    public class PactTestPlanCrossTabApiClient : IPactTestPlanCrossTabApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactTestPlanCrossTabApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<TestPlanCostBreakdownDto>> GetPagedTestPlanCrossTabAsync(
            QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestPlanCrossTab, query);
            var response = await _http.GetAsync<TestPlanCostBreakdownRes>(url);

            if (response.Success && response.Data is not null)
            {
                return ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
                {
                    Columns = response.Data.Columns,
                    Rows = response.Data.Rows,
                    TotalCount = response.Data.TotalCount,
                    Page = response.Data.Page,
                    PageSize = response.Data.PageSize
                });
            }

            var dto = _mapper.Map<ApiResponseDto<TestPlanCostBreakdownDto>>(response);
            return ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}