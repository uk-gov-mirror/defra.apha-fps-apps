using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsMilestoneApiClient : IPimsMilestoneApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsMilestoneApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MilestoneDto>>> GetAllMilestonesAsync(QueryParameters<string> query, string project)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAllMilestones, query);
            url += $"&project={project}";
            var response = await _http.GetAsync<List<MilestoneRes>>(url);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(response);
            return ApiResponseDto<List<MilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }       

        public async Task<ApiResponseDto<MilestoneDto>> GetMilestoneAsync(string project, string number)
        {
            var response = await _http.GetAsync<MilestoneRes>(
                string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number)));
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            if (response.Success && response.Data == null)
                return ApiResponseDto<MilestoneDto>.SuccessResponse(null!);
            var dto = _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            return ApiResponseDto<MilestoneDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MilestoneDto>> SaveMilestoneAsync(string project, MilestoneDto dto)
        {
            MilestoneReq request = _mapper.Map<MilestoneReq>(dto);
            var response = await _http.PostAsync<MilestoneReq, MilestoneRes>(
                string.Format(PimsApiEndpoints.SaveMilestone, Uri.EscapeDataString(project)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            return ApiResponseDto<MilestoneDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<MilestoneDto>> UpdateMilestoneAsync(string project, string number, MilestoneDto dto)
        {
            MilestoneReq request = _mapper.Map<MilestoneReq>(dto);
            var response = await _http.PutAsync<MilestoneReq, MilestoneRes>(
                string.Format(PimsApiEndpoints.UpdateMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number)), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<MilestoneDto>>(response);
            return ApiResponseDto<MilestoneDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<object>> DeleteMilestoneAsync(string project, string number)
        {
            var response = await _http.DeleteAsync<object>(
                string.Format(PimsApiEndpoints.DeleteMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<object>> UpdateFormRequiredAsync(string parentProject, bool formRequired)
        {
            var response = await _http.PatchAsync<bool, object>(
                string.Format(PimsApiEndpoints.UpdateFormRequired, Uri.EscapeDataString(parentProject)),
                formRequired);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }
        public async Task<ApiResponseDto<List<MilestoneTypeDto>>> GetMilestoneTypesAsync(string? milestoneDeliverable = null)
        {
           
                string url = PimsApiEndpoints.GetMilestoneTypes;
                if (!string.IsNullOrWhiteSpace(milestoneDeliverable))
                    url += $"?milestoneDeliverable={Uri.EscapeDataString(milestoneDeliverable)}";
                var response = await _http.GetAsync<List<MilestoneTypeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(response);
                var dto = _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(response);
                return ApiResponseDto<List<MilestoneTypeDto>>.FailureResponse(dto.Errors, dto.Meta);
           
        }
        public async Task<ApiResponseDto<List<MilestoneFormDatesDto>>> GetAllMilestoneFormDatesAsync(
            string parentProject, QueryParameters<string> parameters)
        {
            string url = QueryStringHelper.AddQueryString(
                string.Format(PimsApiEndpoints.GetAllMilestoneFormDates, Uri.EscapeDataString(parentProject)),
                parameters);
            var response = await _http.GetAsync<List<MilestoneFormDatesRes>>(url);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<MilestoneFormDatesDto>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<MilestoneFormDatesDto>>>(response);
            return ApiResponseDto<List<MilestoneFormDatesDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MilestoneFormDatesDto>> GetMilestoneFormDatesAsync(string parentProject, short year)
        {
           
                var response = await _http.GetAsync<MilestoneFormDatesRes>(
                    string.Format(PimsApiEndpoints.GetMilestoneFormDates, Uri.EscapeDataString(parentProject), year));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(response);
                var dto = _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(response);
                return ApiResponseDto<MilestoneFormDatesDto>.FailureResponse(dto.Errors, dto.Meta);
            
        }

        public async Task<ApiResponseDto<MilestoneFormDatesDto>> SaveMilestoneFormDatesAsync(string parentProject, MilestoneFormDatesDto dto)
        {
            
                MilestoneFormDatesReq request = _mapper.Map<MilestoneFormDatesReq>(dto);
                var response = await _http.PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(
                    string.Format(PimsApiEndpoints.SaveMilestoneFormDates, Uri.EscapeDataString(parentProject)), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(response);
                var responseDto = _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(response);
                return ApiResponseDto<MilestoneFormDatesDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            
        }

        public async Task<ApiResponseDto<object>> DeleteMilestoneFormDatesAsync(string parentProject, short year)
        {
            try
            {
                var response = await _http.DeleteAsync<object>(
                    string.Format(PimsApiEndpoints.DeleteMilestoneFormDates, Uri.EscapeDataString(parentProject), year));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<object>>(response);
                var dto = _mapper.Map<ApiResponseDto<object>>(response);
                return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<object>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete milestone form dates", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<LogMilestoneDto>>> GetLogMilestonesAsync(QueryParameters<string> parameters,string? project,string? numberPart1,string? numberPart2)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetLogMilestones, parameters);
            if (!string.IsNullOrWhiteSpace(project))
                url += $"&project={Uri.EscapeDataString(project)}";
            if (!string.IsNullOrWhiteSpace(numberPart1))
                url += $"&numberPart1={Uri.EscapeDataString(numberPart1)}";
            if (!string.IsNullOrWhiteSpace(numberPart2))
                url += $"&numberPart2={Uri.EscapeDataString(numberPart2)}";

            var response = await _http.GetAsync<List<LogMilestoneRes>>(url);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(response);
            return ApiResponseDto<List<LogMilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
        // ── Staging / Import ─────────────────────────────────────────────────
        public async Task<ApiResponseDto<List<StagingMilestoneDto>>> GetAllStagingRowsAsync(QueryParameters<string> parameters)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAllStagingMilestones, parameters);
            var response = await _http.GetAsync<List<StagingMilestoneRes>>(url);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
            return ApiResponseDto<List<StagingMilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<StagingMilestoneDto>>> GetStagingRowsAsync(int id)
        {
            string url = PimsApiEndpoints.GetStagingMilestones + "?id=" + id;
            var response = await _http.GetAsync<List<StagingMilestoneRes>>(url);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
            var dto = _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
            return ApiResponseDto<List<StagingMilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        //public async Task<ApiResponseDto<List<StagingMilestoneDto>>> GetStagingRowsAsync(string? project)
        //{
        //    string url = PimsApiEndpoints.GetStagingMilestones;
        //    if (!string.IsNullOrWhiteSpace(project))
        //        url += "?project=" + Uri.EscapeDataString(project);

        //    var response = await _http.GetAsync<List<StagingMilestoneRes>>(url);
        //    if (response.Success && response.Data != null)
        //        return _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
        //    var dto = _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(response);
        //    return ApiResponseDto<List<StagingMilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        //}

        public async Task<ApiResponseDto<StagingMilestoneDto>> AddStagingRowAsync(StagingMilestoneDto dto, int year)
        {
            StagingMilestoneReq request = _mapper.Map<StagingMilestoneReq>(dto);
            var response = await _http.PostAsync<StagingMilestoneReq, StagingMilestoneRes>(
                string.Format(PimsApiEndpoints.AddStagingMilestone, year), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(response);
            return ApiResponseDto<StagingMilestoneDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<StagingMilestoneDto>> UpdateStagingRowAsync(int id, StagingMilestoneDto dto)
        {
            StagingMilestoneReq request = _mapper.Map<StagingMilestoneReq>(dto);
            var response = await _http.PutAsync<StagingMilestoneReq, StagingMilestoneRes>(
                string.Format(PimsApiEndpoints.UpdateStagingMilestone, id), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(response);
            return ApiResponseDto<StagingMilestoneDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<object>> DeleteStagingRowAsync(int id)
        {
            var response = await _http.DeleteAsync<object>(
                string.Format(PimsApiEndpoints.DeleteStagingMilestone, id));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<object>> ClearStagingAsync(string project)
        {
            var response = await _http.DeleteAsync<object>(
                string.Format(PimsApiEndpoints.ClearStagingMilestones, Uri.EscapeDataString(project)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<object>> ValidateStagingAsync(
            string project, string? typeId, bool isDeliverableMode)
        {
            string url = string.Format(PimsApiEndpoints.ValidateStagingMilestones, Uri.EscapeDataString(project));
            if (!string.IsNullOrWhiteSpace(typeId))
                url += $"?typeId={Uri.EscapeDataString(typeId)}";
            url += $"{(url.Contains('?') ? "&" : "?")}isDeliverableMode={isDeliverableMode}";

            var response = await _http.PostAsync<object, object>(url, new object());
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<object>> ImportStagingAsync(string project)
        {
            var response = await _http.PostAsync<object, object>(
                string.Format(PimsApiEndpoints.ImportStagingMilestones, Uri.EscapeDataString(project)),
                new object());
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<object>> ImportWithOverwriteAsync(string project)
        {
            var response = await _http.PostAsync<object, object>(
                string.Format(PimsApiEndpoints.ImportOverwriteStagingMilestones, Uri.EscapeDataString(project)),
                new object());
            if (response.Success)
                return _mapper.Map<ApiResponseDto<object>>(response);
            var dto = _mapper.Map<ApiResponseDto<object>>(response);
            return ApiResponseDto<object>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectYearManagerDto>>> GetProjectYearManagersAsync(int year)
        {
            var response = await _http.GetAsync<List<ProjectYearManagerRes>>(
                string.Format(PimsApiEndpoints.GetProjectYearManagers, year));
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<ProjectYearManagerDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectYearManagerDto>>>(response);
            return ApiResponseDto<List<ProjectYearManagerDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
        public async Task<ApiResponseDto<List<MilestoneDto>>> GetPMDMilestonesAsync(QueryParameters<string> query, string project)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPMDMilestones, query);
            url += $"{(url.Contains('?') ? "&" : "?")}project={Uri.EscapeDataString(project)}";
            var response = await _http.GetAsync<List<MilestoneRes>>(url);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(response);
            return ApiResponseDto<List<MilestoneDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

    }
}
