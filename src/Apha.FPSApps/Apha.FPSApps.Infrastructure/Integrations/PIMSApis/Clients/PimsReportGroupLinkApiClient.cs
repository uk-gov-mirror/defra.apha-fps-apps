using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsReportGroupLinkApiClient : IPimsReportGroupLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        
        private const string InternalCodeError = "INTERNAL_ERROR";
        
        private const string BaseUrl = "api/v1/reportgrouplink";

        public PimsReportGroupLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ReportGroupLinkRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(response);
                return ApiResponseDto<List<ReportGroupLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportGroupLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroupLink data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportId)
        {
            try
            {
                var url = $"{BaseUrl}/{reportId}";
                var response = await _http.GetAsync<List<ReportGroupLinkRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupLinkDto>>>(response);
                return ApiResponseDto<List<ReportGroupLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ReportGroupLinkDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ReportGroupLink by report ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportId, int groupId)
        {
            var url = $"{BaseUrl}/{reportId}/{groupId}";
            var response = await _http.GetAsync<ReportGroupLinkRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(response);
            return ApiResponseDto<ReportGroupLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto)
        {
                var request = _mapper.Map<ReportGroupLinkReq>(dto);
                var response = await _http.PostAsync<ReportGroupLinkReq, ReportGroupLinkRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ReportGroupLinkDto>>(response);
                return ApiResponseDto<ReportGroupLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportId, int groupId)
        {
            try
            {
                var url = $"{BaseUrl}/{reportId}/{groupId}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ReportGroupLink", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
