/*
 * TRANSFORMENGINE MIGRATION — FpsAsuViewApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Phase 7 stub bodies (throw NotImplementedException) replaced with real async HTTP calls
 *   - GetAsuViewAsync: GET api/v1/animal/asu-view?animalType=X + pagination query string
 *   - GetAnimalTypeLookupAsync: GET api/v1/animal (all animal master records for Animal Type dropdown)
 *   - Both methods now async and wrapped in try/catch(Exception) returning FailureResponse per phase rules
 *   - Added using Apha.Common.Contracts.FPS and Apha.Common.Utilities.Query
 *
 * PRESERVED:
 *   - Class name, namespace, constructor signature (IFpsHttpExecutor + IMapper)
 *   - BaseUrl const "api/v1/animal" — matches backend AnimalController [Route]
 *   - IFpsAsuViewApiClient interface contract unchanged (method names and signatures)
 *   - private const InternalCodeError "INTERNAL_ERROR" — Sonar S1192 compliance
 *   - private readonly fields _http, _mapper — Sonar S2933 compliance
 *   - null guard ArgumentNullException checks in constructor
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm animalType nullability — controller rejects null/empty with 400;
 *     client should pass a non-null value but the interface leaves enforcement to the caller
 *   - TRANSFORMENGINE TODO: verify Cost type (decimal vs double) in AsuViewDto/AsuViewRes matches DB column
 */

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
    public class FpsAsuViewApiClient : IFpsAsuViewApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend AnimalController [Route("api/v{version:apiVersion}/animal")]
        private const string BaseUrl = "api/v1/animal";

        public FpsAsuViewApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/animal/asu-view?animalType=X + pagination → AnimalController.GetAsuViewAsync
        //   animalType is a required business filter; QueryStringHelper appends pagination params from QueryParameters<string>
        //   Uri.EscapeDataString protects against special characters in the animalType value
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<AsuViewDto>>> GetAsuViewAsync(QueryParameters<string> query, string animalType)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseUrl}/asu-view?animalType={Uri.EscapeDataString(animalType)}", query);
                var response = await _http.GetAsync<List<AsuViewRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(response);
                return ApiResponseDto<List<AsuViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AsuViewDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ASU View data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/animal → AnimalController GET all animal master records
        //   Used to populate the Animal Type dropdown on the ASU View form (fps_asuview.html)
        //   Returns full AnimalRes list mapped to AnimalDto via FpsApiDtoMapper (Phase 10)
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<AnimalDto>>> GetAnimalTypeLookupAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AnimalRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AnimalDto>>>(response);
                return ApiResponseDto<List<AnimalDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Animal Type lookup data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
