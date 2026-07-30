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
    public class FpsGradeApiClient : IFpsGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        private const string InternalCodeError = "INTERNAL_ERROR";

        private const string BaseUrl = "api/v1/Grade";

        public FpsGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        //   QueryParameters<string> appended as query string via QueryStringHelper.AddQueryString
        public async Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<GradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<GradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<GradeDto>>>(response);
                return ApiResponseDto<List<GradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<GradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Grade data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode)
        {
            try
            {
                var url = $"{BaseUrl}/{gradeCode}";
                var response = await _http.GetAsync<GradeRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Grade by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   GradeDto mapped to GradeReq for the request body
        public async Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto)
        {
            try
            {
                var request = _mapper.Map<GradeReq>(dto);
                var response = await _http.PostAsync<GradeReq, GradeRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   originalCode placed in path to support GradeCode rename (dto.GradeCode may differ from originalCode)
        public async Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto)
        {
            try
            {
                var request = _mapper.Map<GradeReq>(dto);
                var url = $"{BaseUrl}/{originalCode}";
                var response = await _http.PutAsync<GradeReq, GradeRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        //   bool? used as generic arg (nullable response body); return type is ApiResponseDto<bool>
        public async Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode)
        {
            try
            {
                var url = $"{BaseUrl}/{gradeCode}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
