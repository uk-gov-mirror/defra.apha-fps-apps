using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectDetailsApiClient : IPimsProjectDetailsApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProjectDetailsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<ProjectDetailDto>> GetPimsDetailAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProjectDetailRes>(string.Format(PimsApiEndpoints.GetPimsDetail, Uri.EscapeDataString(parentproject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);
            return ApiResponseDto<ProjectDetailDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDetailDto>> SavePimsDetailAsync(string parentproject, ProjectDetailDto dto)
        {
            ProjectDetailReq request = _mapper.Map<ProjectDetailReq>(dto);
            var response = await _http.PostAsync<ProjectDetailReq, ProjectDetailRes>(
                string.Format(PimsApiEndpoints.SavePimsDetail, Uri.EscapeDataString(parentproject)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectDetailDto>>(response);
            return ApiResponseDto<ProjectDetailDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> GetProposedProjectAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProposedProjectRes>(string.Format(PimsApiEndpoints.GetProposedProject, Uri.EscapeDataString(parentproject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
            return ApiResponseDto<ProposedProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProposedProjectDto>> UpdateProposedProjectAsync(string parentproject, ProposedProjectDto dto)
        {
            ProposedProjectReq request = _mapper.Map<ProposedProjectReq>(dto);
            var response = await _http.PutAsync<ProposedProjectReq, ProposedProjectRes>(
                string.Format(PimsApiEndpoints.UpdateProposedProject, Uri.EscapeDataString(parentproject)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProposedProjectDto>>(response);
            return ApiResponseDto<ProposedProjectDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<RiskDto>>> GetAllRiskAsync()
        {
            var response = await _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRisks);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);
            return ApiResponseDto<List<RiskDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<YearDto>>> GetAllYearAsync()
        {
            var response = await _http.GetAsync<List<YearRes>>(PimsApiEndpoints.GetAllYears);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<YearDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<YearDto>>>(response);
            return ApiResponseDto<List<YearDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProjectDto>> GetFpsProjectAsync(string parentproject)
        {
            var response = await _http.GetAsync<ProjectRes>(string.Format(PimsApiEndpoints.GetFpsProjectByProjectDetails, Uri.EscapeDataString(parentproject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectDto>>(response);
            return ApiResponseDto<ProjectDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
