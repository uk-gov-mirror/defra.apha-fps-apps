using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using Apha.Common.Contracts.FPS;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsSettingApiClient : IFpsSettingApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsSettingApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<decimal>> GetHoursPerDayAsync()
        {
            var response = await _http.GetAsync<decimal>(FpsApiEndpoints.GetHoursPerDay);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<decimal>>(response);

            var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
            return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
        {
            var response = await _http.GetAsync<List<FpsSettingRes>>(FpsApiEndpoints.GetAllSettings);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<SettingDto>>>(response);
            return ApiResponseDto<List<SettingDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<YearEndSettingDto>>> GetYearEndSettingsAsync()
        {
            var response = await _http.GetAsync<List<FpsYearEndSettingRes>>(FpsApiEndpoints.GetYearEndSettings);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<YearEndSettingDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<YearEndSettingDto>>>(response);
            return ApiResponseDto<List<YearEndSettingDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<SettingDto>> AddSettingAsync(SettingDto settingDto)
        {
            var request = _mapper.Map<FpsSettingReq>(settingDto);
            var response = await _http.PostAsync<FpsSettingReq, FpsSettingRes>(FpsApiEndpoints.CreateSetting, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<SettingDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
            return ApiResponseDto<SettingDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto settingDto)
        {
            var request = _mapper.Map<FpsSettingReq>(settingDto);
            var url = string.Format(FpsApiEndpoints.UpdateSetting, id);
            var response = await _http.PutAsync<FpsSettingReq, FpsSettingRes>(url, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<SettingDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
            return ApiResponseDto<SettingDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<SettingDto>> SaveSettingAsync(SettingDto settingDto)
        {
            var request = _mapper.Map<FpsSettingReq>(settingDto);
            var response = await _http.PostAsync<FpsSettingReq, FpsSettingRes>(FpsApiEndpoints.SaveSetting, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<SettingDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<SettingDto>>(response);
            return ApiResponseDto<SettingDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
