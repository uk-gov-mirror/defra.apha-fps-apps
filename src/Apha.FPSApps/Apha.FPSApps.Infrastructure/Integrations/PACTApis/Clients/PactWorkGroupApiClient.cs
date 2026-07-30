using Apha.Common.Constants;
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
    public class PactWorkGroupApiClient : IPactWorkGroupApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactWorkGroupApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync()
        {
            var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetAllWorkGroupNames);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
        {
            var url = QueryHelpers.AddQueryString(
                PactApiEndpoints.GetWorkGroupsByProfitCentreForBudget,
                "profitCentre", profitCentre);
            var response = await _http.GetAsync<List<WorkGroupViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
                return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetWorkGroupsByProfitCentreForBudgetPaged, query);
            url = QueryHelpers.AddQueryString(url, "profitCentre", profitCentre);

            var response = await _http.GetAsync<List<WorkGroupViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
                return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
        {
            var response = await _http.GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetAllWorkGroups);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
                return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string? workGroup, int? monthNumber)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (monthNumber.HasValue)
                url += $"&monthNumber={monthNumber.Value}";

            var response = await _http.GetAsync<List<WorkGroupTimeCodeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);
                return ApiResponseDto<List<WorkGroupTimeCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupValidTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";

            var response = await _http.GetAsync<List<WorkGroupValidTimeCodeRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);
                return ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WgSummarisedStaffTimeUsageDto>> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetWgSummarisedStaffTimeUsage, query);
            url += $"&staffName={Uri.EscapeDataString(staffName)}";

            var response = await _http.GetAsync<WgSummarisedStaffTimeUsageRes>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WgSummarisedStaffTimeUsageDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WgSummarisedStaffTimeUsageDto>>(response);
                return ApiResponseDto<WgSummarisedStaffTimeUsageDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupsByProfitCentre, query);
            url = QueryHelpers.AddQueryString(url, "profitCentre", profitCentre);

            var response = await _http.GetAsync<List<WorkGroupRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
                return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var request = new UpdateSendEmailFlagReq { ProfitCentre = profitCentre, SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForProfitCentreWorkGroups, request);

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

        public async Task<ApiResponseDto<bool>> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var request = new UpdateSendEmailFlagReq { SendEmail = flag };
            var response = await _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForAllWorkGroups, request);

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

        public async Task<ApiResponseDto<bool>> UpdateWorkGroupEmailAsync(
            string workGroupName, short sendEmail, string? emailRecipient)
        {
            var url = string.Format(PactApiEndpoints.UpdateWorkGroupEmail, Uri.EscapeDataString(workGroupName));
            var request = new UpdateWorkGroupEmailReq
            {
                WorkGroupName = workGroupName,
                SendEmail = sendEmail,
                EmailRecipient = emailRecipient
            };
            var response = await _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(url, request);

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

        // ── WorkGroup Maintenance (CRUD + lookups) ────────────────────────────────

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupMaintenance, query);
            var response = await _http.GetAsync<List<WorkGroupMaintenanceRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
                return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WorkGroupDto>> GetByWorkGroupNameAsync(string workGroupName)
        {
            var url = string.Format(PactApiEndpoints.GetWorkGroupMaintenanceByName, Uri.EscapeDataString(workGroupName));
            var response = await _http.GetAsync<WorkGroupMaintenanceRes>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
                return ApiResponseDto<WorkGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WorkGroupDto>> CreateAsync(WorkGroupDto dto)
        {
            var request = _mapper.Map<WorkGroupMaintenanceReq>(dto);
            var response = await _http.PostAsync<WorkGroupMaintenanceReq, WorkGroupMaintenanceRes>(
                PactApiEndpoints.CreateWorkGroupMaintenance, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
                return ApiResponseDto<WorkGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<WorkGroupDto>> UpdateAsync(string workGroupName, WorkGroupDto dto)
        {
            var url = string.Format(PactApiEndpoints.UpdateWorkGroupMaintenance, Uri.EscapeDataString(workGroupName));
            var request = _mapper.Map<WorkGroupMaintenanceReq>(dto);
            var response = await _http.PutAsync<WorkGroupMaintenanceReq, WorkGroupMaintenanceRes>(url, request);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupDto>>(response);
                return ApiResponseDto<WorkGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName)
        {
            var url = string.Format(PactApiEndpoints.DeleteWorkGroupMaintenance, Uri.EscapeDataString(workGroupName));
            var response = await _http.DeleteAsync<bool?>(url);

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

        public async Task<ApiResponseDto<List<string>>> GetProfitCentresAsync()
        {
            var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetWorkGroupProfitCentres);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<string>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<OwnerDto>>> GetOwnersAsync()
        {
            var response = await _http.GetAsync<List<OwnerRes>>(PactApiEndpoints.GetWorkGroupOwners);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<OwnerDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<OwnerDto>>>(response);
                return ApiResponseDto<List<OwnerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre)
        {
            var url = QueryHelpers.AddQueryString(PactApiEndpoints.GetWorkGroupCostCentres, "profitCentre", profitCentre);
            var response = await _http.GetAsync<List<double?>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<double?>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<double?>>>(response);
                return ApiResponseDto<List<double?>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
