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
    public class FpsStaffJobApiClient : IFpsStaffJobApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string internalCodeError = "INTERNAL_ERROR";

        public FpsStaffJobApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetAllStaffJobAsync(QueryParameters<string> staffJob, string jobCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetAllStaffJobs, jobCode), staffJob);
            var response = await _http.GetAsync<List<StaffJobViewRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
                return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>> GetStaffWorkgroupLookupAsync()
        {
            var response = await _http.GetAsync<IEnumerable<StaffWorkgroupLookupRes>>(FpsApiEndpoints.GetStaffWorkgroupLookup);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(response);
            return ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<StaffWorkgroupLookupDto>> GetStaffSummaryByIdAsync(string staffId)
        {
            var response = await _http.GetAsync<StaffWorkgroupLookupRes>(string.Format(FpsApiEndpoints.GetStaffSummaryById, staffId));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StaffWorkgroupLookupDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<StaffWorkgroupLookupDto>>(response);
            return ApiResponseDto<StaffWorkgroupLookupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<double>> GetZtTotalHoursByStaffIdAsync(string staffId)
        {
            var response = await _http.GetAsync<double>(string.Format(FpsApiEndpoints.GetZtTotalHoursByStaffId, staffId));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<double>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<double>>(response);
            return ApiResponseDto<double>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<StaffJobZtViewDto>>> GetZtStaffJobsByStaffIdPagedAsync(QueryParameters<string> query, string staffId)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetZtStaffJobsByStaffIdPaged, query);
            url = QueryStringHelper.AddQueryString(url, new { staffId });
            var response = await _http.GetAsync<List<StaffJobZtViewRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StaffJobZtViewDto>>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobZtViewDto>>>(response);
            return ApiResponseDto<List<StaffJobZtViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<StaffJobViewDto>>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(QueryParameters<string> query, string jobcode, string wgGrade)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetStaffJobsAllocationByJobCodeWgGradePaged, query);
            url = QueryStringHelper.AddQueryString(url, new { jobcode, wgGrade });
            var response = await _http.GetAsync<List<StaffJobViewRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(response);
            return ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<StaffJobZtViewDto>> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode)
        {
            var response = await _http.GetAsync<StaffJobZtViewRes>(string.Format(FpsApiEndpoints.GetZtStaffJobDetailsById, staffId, jobCode));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<StaffJobZtViewDto>>(response);
            var responseDto = _mapper.Map<ApiResponseDto<StaffJobZtViewDto>>(response);
            return ApiResponseDto<StaffJobZtViewDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<decimal?>> GetStaffChargeRate(string staffId, string jobcode)
        {
            var response = await _http.GetAsync<decimal?>(string.Format(FpsApiEndpoints.GetStaffChargeRate, staffId, jobcode));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<decimal?>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<decimal?>>(response);
                return ApiResponseDto<decimal?>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }

        }

        public async Task<ApiResponseDto<decimal>> GetTotalStaffCostAsync(string jobCode)
        {
            var response = await _http.GetAsync<decimal>(string.Format(FpsApiEndpoints.GetTotalStaffCost, jobCode));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<decimal>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<decimal>>(response);
                return ApiResponseDto<decimal>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }

        }

        public async Task<ApiResponseDto<StaffJobDto>> GetStaffJobByIdAsync(string staffId, string jobCode)
        {
            var response = await _http.GetAsync<StaffJobRes>(string.Format(FpsApiEndpoints.GetStaffJobById, staffId, jobCode));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> CreateStaffJobAsync(StaffJobDto staffJob)
        {
            var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
            var response = await _http.PostAsync<StaffJobReq, StaffJobRes>(FpsApiEndpoints.CreateStaffJob, staffJobReq);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<StaffJobDto>> UpdateStaffJobAsync(StaffJobDto staffJob)
        {
            var staffJobReq = _mapper.Map<StaffJobReq>(staffJob);
            var response = await _http.PutAsync<StaffJobReq, StaffJobRes>(FpsApiEndpoints.UpdateStaffJob, staffJobReq);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<StaffJobDto>>(response);
                return ApiResponseDto<StaffJobDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteStaffJobAsync(string staffId, string jobCode)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteStaffJob, staffId, jobCode));

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

        public async Task<ApiResponseDto<StaffJobViewDto?>> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var response = await _http.GetAsync<StaffJobViewRes>(string.Format(FpsApiEndpoints.GetStaffJobViewById, staffId, jobCode));

            if (response.Success)
            {
                var mappedData = response.Data != null ? _mapper.Map<StaffJobViewDto>(response.Data) : null;
                return ApiResponseDto<StaffJobViewDto?>.SuccessResponse(mappedData);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<StaffJobViewDto?>>(response);
                return ApiResponseDto<StaffJobViewDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<List<StaffResourceUtilisationDto>>> GetStaffResourceUtilisationAsync(QueryParameters<string> query, string workgroup)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetStaffResourceUtilisation, query);
            url = QueryStringHelper.AddQueryString(url, new { workgroup });
            var response = await _http.GetAsync<List<StaffResourceUtilisationRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(response);
            return ApiResponseDto<List<StaffResourceUtilisationDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
