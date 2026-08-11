using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTestsRequiredByRcApiClient : IFpsTestsRequiredByRcApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTestsRequiredByRcApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestsRequiredByRcDto>>> GetTestsRequiredByRcAsync(string? profitCentre)
        {
            var response = await _http.GetAsync<List<TestsRequiredByRcRes>>(
                string.Format(FpsApiEndpoints.GetTestsRequiredByRc, Uri.EscapeDataString(profitCentre ?? string.Empty)));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<TestsRequiredByRcDto>>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<List<TestsRequiredByRcDto>>>(response);
            return ApiResponseDto<List<TestsRequiredByRcDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
