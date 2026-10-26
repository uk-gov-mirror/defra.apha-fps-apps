using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectListApiClient : IPimsProjectListApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProjectListApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsAsync(QueryParameters<string> query, int filterOption = 2)
        {
            
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAllProjects, query);
                url += $"&showWhichProjects={filterOption}";
                var response = await _http.GetAsync<List<ProjectListRes>>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);
                return ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(dto.Errors, dto.Meta);
           
        }

        public async Task<ApiResponseDto<List<ProjectListViewDto>>> GetAllProjectsListAsync(int filterOption = 2)
        {

                string url = $"{PimsApiEndpoints.GetAllProjectsList}?showWhichProjects={filterOption}";
                var response = await _http.GetAsync<List<ProjectListRes>>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectListViewDto>>>(response);
                return ApiResponseDto<List<ProjectListViewDto>>.FailureResponse(dto.Errors, dto.Meta);

        }

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectByIdAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProjectRes>(string.Format(PimsApiEndpoints.GetFpsProjectById, parentproject));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectByIdAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProposedProjectRes>(string.Format(PimsApiEndpoints.GetProposedProjectById, parentproject));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
            return ApiResponseDto<ProposedProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectsDto>>> GetYearlyDetailsByProjectAsync(string parentproject)
        {
            var response = await _http.GetAsync<List<ProjectsRes>>(string.Format(PimsApiEndpoints.GetYearlyDetailsByProject, Uri.EscapeDataString(parentproject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectsDto>>>(response);
            return ApiResponseDto<List<ProjectsDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
        public async Task<ApiResponseDto<List<ProjectListMilestoneDto>>> GetAllProjectsForMilestoneAsync()
        {
            var response = await _http.GetAsync<List<ProjectListMilestoneRes>>(PimsApiEndpoints.GetAllProjectsMilestone);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<ProjectListMilestoneDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectListMilestoneDto>>>(response);
            return ApiResponseDto<List<ProjectListMilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
        public async Task<ApiResponseDto<ProjectDetailsMilestoneDto>> GetProjectsDetailsForMilestoneAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProjectDetailsMilestoneRes>(string.Format(PimsApiEndpoints.GetProjectsDetailsForMilestone, Uri.EscapeDataString(parentproject)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDetailsMilestoneDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDetailsMilestoneDto>>(response);
            return ApiResponseDto<ProjectDetailsMilestoneDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
 }
