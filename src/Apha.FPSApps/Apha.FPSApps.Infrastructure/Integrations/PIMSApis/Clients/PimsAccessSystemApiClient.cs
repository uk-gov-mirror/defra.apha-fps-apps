using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessSystemApiClient : IPimsAccessSystemApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsAccessSystemApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessSystemRes>>(PimsApiEndpoints.GetAllAccessSystems);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(response);
                return ApiResponseDto<List<AccessSystemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessSystemDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessSystem data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessSystemDto>> GetByIdAsync(int systemid)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessSystemById, systemid);
                var response = await _http.GetAsync<AccessSystemRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessSystemDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessSystemDto>>(response);
                return ApiResponseDto<AccessSystemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessSystemDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessSystem by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
