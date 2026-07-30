using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjectStaffPlanDetailsApiClient : IFpsProjectStaffPlanDetailsApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProjectStaffPlanDetailsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProjectStaffPlanDetails, query);

            // ApiResponseActionFilter on the FPS API already unwraps PaginationRes<T>:
            // $.data  -> List<ProjectStaffPlanDetailsViewRes>  (the items)
            // $.pagination -> Pagination                       (page metadata)
            var response = await _http.GetAsync<List<ProjectStaffPlanDetailsViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>>(response);
            return ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
