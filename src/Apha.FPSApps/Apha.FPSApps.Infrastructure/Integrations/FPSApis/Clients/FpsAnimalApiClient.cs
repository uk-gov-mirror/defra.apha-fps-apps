using Apha.Common.Constants;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsAnimalApiClient : IFpsAnimalApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsAnimalApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<IEnumerable<AnimalDto>>> GetAllAnimalsAsync()
        {
            var response = await _http.GetAsync<IEnumerable<AnimalDto>>(FpsApiEndpoints.GetAllAnimalMasters);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<AnimalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<AnimalDto>>>(response);
            return ApiResponseDto<IEnumerable<AnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AnimalDto>>> GetAllAnimalsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedAnimalMasters, query);
            var response = await _http.GetAsync<List<AnimalDto>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);
            return ApiResponseDto<List<AnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AnimalSnapshotViewDto>>> GetAnimalSnapshotAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetAnimalSnapshot, query);
            var response = await _http.GetAsync<List<AnimalSnapshotViewDto>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalSnapshotViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AnimalSnapshotViewDto>>>(response);
            return ApiResponseDto<List<AnimalSnapshotViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AnimalDto?>> GetAnimalByIdAsync(string animalType)
        {
            var response = await _http.GetAsync<AnimalDto>(string.Format(FpsApiEndpoints.GetAnimalMasterById, animalType));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AnimalDto?>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AnimalDto?>>(response);
            return ApiResponseDto<AnimalDto?>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AnimalDto>> AddAnimalAsync(AnimalDto animalDto)
        {
            var response = await _http.PostAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.CreateAnimalMaster, animalDto);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AnimalDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AnimalDto>>(response);
            return ApiResponseDto<AnimalDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AnimalDto>> UpdateAnimalAsync(AnimalDto animalDto)
        {
            var response = await _http.PutAsync<AnimalDto, AnimalDto>(FpsApiEndpoints.UpdateAnimalMaster, animalDto);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AnimalDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AnimalDto>>(response);
            return ApiResponseDto<AnimalDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAnimalAsync(string animalType)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteAnimalMaster, animalType));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
