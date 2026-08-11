using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTestsRequiredByWgApiClient : IFpsTestsRequiredByWgApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTestsRequiredByWgApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<TestsRequiredByWgDto>>> GetTestsRequiredByWgAsync(string? profitCentre)
        {
            var response = await _http.GetAsync<List<TestsRequiredByWgRes>>(
                string.Format(FpsApiEndpoints.GetTestsRequiredByWg, Uri.EscapeDataString(profitCentre ?? string.Empty)));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<TestsRequiredByWgDto>>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<List<TestsRequiredByWgDto>>>(response);
            return ApiResponseDto<List<TestsRequiredByWgDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
