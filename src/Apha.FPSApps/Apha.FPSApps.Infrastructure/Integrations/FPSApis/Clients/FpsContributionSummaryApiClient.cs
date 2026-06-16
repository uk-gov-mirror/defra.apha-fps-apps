/*
 * TRANSFORMENGINE MIGRATION — FpsContributionSummaryApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: real HTTP API client replacing FpsContributionSummaryApiClientStub (Phase 7).
 *   - Implements IFpsContributionSummaryApiClient with six methods bound to
 *     backend ContributionSummaryController (route: api/v1/contributionsummary):
 *       GET  api/v1/contributionsummary              -> GetByProfitCentreAsync (paged + profitCentre query param)
 *       GET  api/v1/contributionsummary/summary      -> GetSummaryAsync (aggregate summary-box totals)
 *       GET  api/v1/contributionsummary/{id}         -> GetByIdAsync
 *       POST api/v1/contributionsummary              -> CreateAsync
 *       PUT  api/v1/contributionsummary/{id}         -> UpdateAsync
 *       DELETE api/v1/contributionsummary/{id}       -> DeleteAsync
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError.
 *   - Success path uses _mapper.Map<ApiResponseDto<T>>(response) — no manual DTO construction.
 *   - BaseUrl and InternalCodeError extracted to private const strings (Sonar S1192).
 *   - _http and _mapper are private readonly fields (Sonar S2933).
 *   - profitCentre appended as query string via QueryStringHelper + string interpolation
 *     (matches codebase pattern: FpsProfitCentreApiClient.GetPagedProfitCenterCostSummaryAsync).
 *   - fpsYear appended conditionally when supplied (nullable int, optional server-side default).
 *   - ContributionSummaryReq mapped from ContributionSummaryDto for POST/PUT bodies.
 *   - ContributionSummarySummaryRes mapped to ContributionSummarySummaryDto for summary endpoint.
 *   - bool? used as DeleteAsync generic arg (nullable body); return type is ApiResponseDto<bool>.
 *
 * PRESERVED:
 *   - All six method signatures from IFpsContributionSummaryApiClient.
 *   - Return type envelope: ApiResponseDto<T> for all methods.
 *   - profitCentre and fpsYear semantics match the interface contract.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear should be carried as a query param or resolved
 *     server-side from active year context once IFpsRequestContext wiring (Phase 5 TODO) is complete.
 *   - TRANSFORMENGINE TODO: FpsContributionSummaryApiClientStub.cs can be deleted once this client
 *     is confirmed working end-to-end in integration/smoke tests.
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
    /// <summary>
    /// HTTP API client for the ContributionSummary resource (frmTimeSellerPC).
    /// Calls backend ContributionSummaryController via IFpsHttpExecutor.
    /// Route base: api/v1/contributionsummary.
    /// </summary>
    public class FpsContributionSummaryApiClient : IFpsContributionSummaryApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend ContributionSummaryController
        //   [Route("api/v{version:apiVersion}/contributionsummary")]
        private const string BaseUrl = "api/v1/contributionsummary";

        public FpsContributionSummaryApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/contributionsummary — paginated list filtered by profitCentre;
        //   maps to ContributionSummaryController.GetByProfitCentreAsync.
        //   profitCentre is a required business context parameter appended to the query string
        //   after pagination params, following the codebase pattern (e.g. FpsProfitCentreApiClient).
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<ContributionSummaryDto>>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre)
        {
            try
            {
                // TRANSFORMENGINE: pagination query built first, then profitCentre appended
                var url = QueryStringHelper.AddQueryString(BaseUrl, query);
                url = $"{url}&profitCentre={profitCentre}";

                var response = await _http.GetAsync<List<ContributionSummaryRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ContributionSummaryDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ContributionSummaryDto>>>(response);
                return ApiResponseDto<List<ContributionSummaryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ContributionSummaryDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ContributionSummary data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/contributionsummary/summary — aggregate summary-box totals;
        //   maps to ContributionSummaryController.GetSummaryAsync.
        //   profitCentre is required; fpsYear is optional (null => active year resolved server-side).
        /// <inheritdoc />
        public async Task<ApiResponseDto<ContributionSummarySummaryDto>> GetSummaryAsync(
            string profitCentre,
            int? fpsYear = null)
        {
            try
            {
                // TRANSFORMENGINE: summary sub-route; profitCentre required, fpsYear conditionally appended
                var url = $"{BaseUrl}/summary?profitCentre={profitCentre}";
                if (fpsYear.HasValue)
                    url = $"{url}&fpsYear={fpsYear.Value}";

                var response = await _http.GetAsync<ContributionSummarySummaryRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ContributionSummarySummaryDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ContributionSummarySummaryDto>>(response);
                return ApiResponseDto<ContributionSummarySummaryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ContributionSummarySummaryDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ContributionSummary aggregate totals", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/contributionsummary/{id} — single row by PK int;
        //   maps to ContributionSummaryController.GetByIdAsync
        /// <inheritdoc />
        public async Task<ApiResponseDto<ContributionSummaryDto>> GetByIdAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.GetAsync<ContributionSummaryRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve ContributionSummary by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/contributionsummary — create new row;
        //   maps to ContributionSummaryController.CreateAsync.
        //   ContributionSummaryDto mapped to ContributionSummaryReq for the request body.
        /// <inheritdoc />
        public async Task<ApiResponseDto<ContributionSummaryDto>> CreateAsync(ContributionSummaryDto dto)
        {
            try
            {
                var request = _mapper.Map<ContributionSummaryReq>(dto);
                var response = await _http.PostAsync<ContributionSummaryReq, ContributionSummaryRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create ContributionSummary row", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/contributionsummary/{id} — update existing row by PK;
        //   maps to ContributionSummaryController.UpdateAsync.
        //   id carried separately in the route to ensure row identity even if dto.Id were to differ.
        /// <inheritdoc />
        public async Task<ApiResponseDto<ContributionSummaryDto>> UpdateAsync(int id, ContributionSummaryDto dto)
        {
            try
            {
                var request = _mapper.Map<ContributionSummaryReq>(dto);
                var url = $"{BaseUrl}/{id}";
                var response = await _http.PutAsync<ContributionSummaryReq, ContributionSummaryRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(response);
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update ContributionSummary row", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/contributionsummary/{id} — delete row by PK int;
        //   maps to ContributionSummaryController.DeleteAsync.
        //   bool? used as generic response arg (nullable body); return type is ApiResponseDto<bool>.
        /// <inheritdoc />
        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            try
            {
                var url = $"{BaseUrl}/{id}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete ContributionSummary row", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
