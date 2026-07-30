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
    public class FpsWorkGroupEmployeeApiClient : IFpsWorkGroupEmployeeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsWorkGroupEmployeeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetWgStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployee data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetWgStaffForStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployeeForStaff data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade)
        {
            try
            {
                var baseUrl = string.Format(FpsApiEndpoints.GetActiveWgStaffForStaff, Uri.EscapeDataString(wgGrade));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<WorkGroupEmployeeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>>(response);
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkGroupEmployeeStaffDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve all active WorkGroupEmployee data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            try
            {
                var response = await _http.GetAsync<WorkGroupEmployeeRes>(
                    string.Format(FpsApiEndpoints.GetWgEmployeeById, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployee by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            try
            {
                var response = await _http.GetAsync<WorkGroupEmployeeRes>(
                    string.Format(FpsApiEndpoints.GetWgEmployeeById, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupEmployeeForStaff by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PostAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.CreateWgEmployeeForStaff, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkGroupEmployeeForStaff", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployee, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkGroupEmployee", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            try
            {
                var req = _mapper.Map<WorkGroupEmployeeReq>(dto);
                var response = await _http.PutAsync<WorkGroupEmployeeReq, WorkGroupEmployeeRes>(FpsApiEndpoints.UpdateWgEmployeeForStaff, req);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkGroupEmployeeStaffDto>>(response);
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkGroupEmployeeStaffDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkGroupEmployeeForStaff", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(
                    string.Format(FpsApiEndpoints.DeleteWgEmployee, Uri.EscapeDataString(pactId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkGroupEmployee", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
