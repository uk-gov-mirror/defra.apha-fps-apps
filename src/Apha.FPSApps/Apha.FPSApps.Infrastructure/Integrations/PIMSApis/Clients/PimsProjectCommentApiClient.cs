using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectCommentApiClient : IPimsProjectCommentApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsProjectCommentApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<CommentDto>>> GetCommentsByProjectAsync(string project, int? year, string? topic, QueryParameters<string> query)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
                url = QueryStringHelper.AddQueryString(url, new { project, year, topic });
                var response = await _http.GetAsync<List<CommentRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<CommentDto>>>(response);
                return ApiResponseDto<List<CommentDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CommentDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comments", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> GetByIdAsync(int commentno)
        {
            try
            {
                var response = await _http.GetAsync<CommentRes>(string.Format(PimsApiEndpoints.GetCommentById, commentno));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> CreateCommentAsync(CommentDto dto)
        {
            try
            {
                CommentReq request = _mapper.Map<CommentReq>(dto);
                var response = await _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<CommentDto>> UpdateCommentAsync(int commentno, CommentDto dto)
        {
            try
            {
                CommentReq request = _mapper.Map<CommentReq>(dto);
                var response = await _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, commentno), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<CommentDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<CommentDto>>(response);
                return ApiResponseDto<CommentDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<CommentDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteCommentAsync(int commentno)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(string.Format(PimsApiEndpoints.DeleteComment, commentno));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete comment", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<CommentTopicDto>>> GetCommentTopicsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(response);
                return ApiResponseDto<List<CommentTopicDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<CommentTopicDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve comment topics", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> GetForecastSpendByProjectAsync(string project)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
                var response = await _http.GetAsync<ProjectCommentForecastSpendRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);
                return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve forecast spend", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectCommentForecastSpendDto>> UpdateForecastSpendByProjectAsync(string project, double? forecastSpend)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
                ProjectCommentForecastSpendRes request = new() { ForecastSpend = forecastSpend };
                var response = await _http.PutAsync<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(response);
                return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update forecast spend", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
