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
    public class FpsWorkGroupGradeApiClient : IFpsWorkGroupGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsWorkGroupGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentre)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetWgGrades, Uri.EscapeDataString(profitCentre));
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupGrade data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.DeleteWgGrade, Uri.EscapeDataString(wgGrade));
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkGroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedWorkgroupGrades, query);
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve paged WorkgroupGrades", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade)
        {
            try
            {
                var response = await _http.GetAsync<WorkgroupGradeRes>(string.Format(FpsApiEndpoints.GetWorkgroupGradeByCode, wgGrade));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkgroupGrade by code", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupGradeReq>(dto);
                var response = await _http.PostAsync<WorkgroupGradeReq, WorkgroupGradeRes>(FpsApiEndpoints.CreateWorkgroupGrade, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkgroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupGradeReq>(dto);
                var response = await _http.PutAsync<WorkgroupGradeReq, WorkgroupGradeRes>(string.Format(FpsApiEndpoints.UpdateWorkgroupGrade, wgGrade), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkgroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(string.Format(FpsApiEndpoints.DeleteWorkgroupGrade, wgGrade));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkgroupGrade (maintain)", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllGradeCodes);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve all grade codes", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkgroupGradesByWorkGroupAsync(string workGroup)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetWgGradesByWorkGroup, Uri.EscapeDataString(workGroup));
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkgroupGrades by workgroup", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
