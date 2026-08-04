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
    public class FpsAnimalPlanApiClient : IFpsAnimalPlanApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsAnimalPlanApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AnimalCostViewDto>>> GetAllAnimalCostAsync(QueryParameters<string> query, string jobCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetAnimalCosts, jobCode), query);
            var response = await _http.GetAsync<List<AnimalCostViewRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(response);
            return ApiResponseDto<List<AnimalCostViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        // Animal Costs ASU View (AnimalCosts — frmAnimalCosts)
        public async Task<ApiResponseDto<List<AnimalCostViewDto>>> GetAnimalCostByAnimalTypeAsync(
            QueryParameters<string> query, string animalType)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetAnimalCostsByAnimalType, query);
            if (!string.IsNullOrWhiteSpace(animalType))
                url = $"{url}&animalType={Uri.EscapeDataString(animalType)}";

            var response = await _http.GetAsync<List<AnimalCostViewRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AnimalCostViewDto>>>(response);
            return ApiResponseDto<List<AnimalCostViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<AnimalDto>>> GetAnimalLookupAsync()
        {
            var response = await _http.GetAsync<List<AnimalRes>>(FpsApiEndpoints.GetAnimalLookup);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);
            return ApiResponseDto<List<AnimalDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<decimal?>> GetAnimalRateAsync(string animalType, string jobCode)
        {
            var response = await _http.GetAsync<decimal?>(string.Format(FpsApiEndpoints.GetAnimalRate, Uri.EscapeDataString(animalType), Uri.EscapeDataString(jobCode)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<decimal?>>(response);

            var dto = _mapper.Map<ApiResponseDto<decimal?>>(response);
            return ApiResponseDto<decimal?>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<AnimalRequestDto>> CreateAnimalCostAsync(AnimalRequestDto animalRequest)
        {
            AnimalRequestReq req = _mapper.Map<AnimalRequestReq>(animalRequest);
            var response = await _http.PostAsync<AnimalRequestReq, AnimalRequestRes>(FpsApiEndpoints.CreateAnimalCost, req);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AnimalRequestDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<AnimalRequestDto>>(response);
            return ApiResponseDto<AnimalRequestDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<AnimalRequestDto>> UpdateAnimalCostAsync(AnimalRequestDto animalRequest)
        {
            AnimalRequestReq req = _mapper.Map<AnimalRequestReq>(animalRequest);
            var response = await _http.PutAsync<AnimalRequestReq, AnimalRequestRes>(FpsApiEndpoints.UpdateAnimalCost, req);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<AnimalRequestDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<AnimalRequestDto>>(response);
            return ApiResponseDto<AnimalRequestDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAnimalCostAsync(int indCounter)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteAnimalCost, indCounter));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<decimal>> GetTotalAnimalCostAsync(string jobCode)
        {
            var response = await _http.GetAsync<decimal>(string.Format(FpsApiEndpoints.GetTotalAnimalCost, jobCode));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<decimal>>(response);

            var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
            return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<AnimalCostViewDto?>> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
        {
            var response = await _http.GetAsync<AnimalCostViewRes>(string.Format(FpsApiEndpoints.GetAnimalCostViewById, indCounter, jobCode));
            if (response.Success)
            {
                var mappedData = response.Data != null ? _mapper.Map<AnimalCostViewDto>(response.Data) : null;
                return ApiResponseDto<AnimalCostViewDto?>.SuccessResponse(mappedData);
            }

            var dto = _mapper.Map<ApiResponseDto<AnimalCostViewDto?>>(response);
            return ApiResponseDto<AnimalCostViewDto?>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
