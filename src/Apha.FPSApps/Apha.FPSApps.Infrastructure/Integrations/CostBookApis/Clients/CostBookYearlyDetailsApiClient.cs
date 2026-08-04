using Apha.Common.Constants;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;
using Apha.FPSApps.Application.Pagination;
using Apha.Common.Contracts;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;

public class CostBookYearlyDetailsApiClient : ICostBookYearlyDetailsApiClient
{
    private readonly ICostBookHttpExecutor _http;
    private readonly IMapper _mapper;

    public CostBookYearlyDetailsApiClient(ICostBookHttpExecutor http, IMapper mapper)
    {
        _http = http;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId)
    {
        var response = await _http.GetAsync<ProjectHeaderRes>(
            string.Format(CostBookApiEndpoints.GetProjectHeader, HttpUtility.UrlEncode(projectId)));
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectHeaderDto>.SuccessResponse(_mapper.Map<ProjectHeaderDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectHeaderDto>>(response);
        return ApiResponseDto<ProjectHeaderDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId)
    {
        var response = await _http.GetAsync<List<ProjectYearRes>>(
            string.Format(CostBookApiEndpoints.GetProjectYears, HttpUtility.UrlEncode(projectId)));
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(_mapper.Map<List<ProjectYearDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<ProjectYearDto>>>(response);
        return ApiResponseDto<List<ProjectYearDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto)
    {
        var req = new AddProjectYearReq
        {
            Project = projectId,
            Year = year,
            YearValue = year,
            MarkupTime = dto.MarkupTime,
            MarkupTests = dto.MarkupTests,
            MarkupAnimals = dto.MarkupAnimals,
            MarkupAdditional = dto.MarkupAdditional,
            ProfitTime = dto.ProfitTime,
            ProfitTests = dto.ProfitTests,
            ProfitAnimals = dto.ProfitAnimals,
            ProfitAdditional = dto.ProfitAdditional
        };
        var response = await _http.PostAsync<AddProjectYearReq, ProjectYearRes>(
            string.Format(CostBookApiEndpoints.AddProjectYear, HttpUtility.UrlEncode(projectId)), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectYearDto>.SuccessResponse(_mapper.Map<ProjectYearDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectYearDto>>(response);
        return ApiResponseDto<ProjectYearDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto)
    {
        var req = _mapper.Map<ProjectYearReq>(dto);
        var response = await _http.PutAsync<ProjectYearReq, ProjectYearRes>(
            string.Format(CostBookApiEndpoints.UpdateProjectYear, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<ProjectYearDto>.SuccessResponse(_mapper.Map<ProjectYearDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<ProjectYearDto>>(response);
        return ApiResponseDto<ProjectYearDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteProjectYearAsync(string projectId, int year)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteProjectYear, HttpUtility.UrlEncode(projectId), year));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Staff ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedResult<StaffRequirementDto>>> GetStaffRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        var endpoint = string.Format(CostBookApiEndpoints.GetStaffRequirements,
                                     HttpUtility.UrlEncode(projectId), year);
        var url = QueryStringHelper.AddQueryString(endpoint, query);

        // BuildOk() in the API wraps PaginationRes inside ApiResponse<PaginationRes<...>>,
        // so the filter leaves it intact — $.data is the PaginationRes object, not a flat list.
        var response = await _http.GetAsync<PaginationRes<StaffRequirementRes>>(url);


        if (response.Success && response.Data != null)
        {
            var items      = _mapper.Map<List<StaffRequirementDto>>(response.Data.Data);
            var pagination = response.Data.PaginationData;

            var result = new PaginatedResult<StaffRequirementDto>(
                items,
                pagination?.TotalRecords ?? items.Count,
                pagination?.PageNumber   ?? query.Page,
                pagination?.PageSize     ?? query.PageSize);

            return ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(result);
        }

        var err = _mapper.Map<ApiResponseDto<PaginatedResult<StaffRequirementDto>>>(response);
        return ApiResponseDto<PaginatedResult<StaffRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto)
    {
        var req = _mapper.Map<StaffRequirementReq>(dto);
        var response = await _http.PostAsync<StaffRequirementReq, StaffRequirementRes>(
            string.Format(CostBookApiEndpoints.AddStaffRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<StaffRequirementDto>.SuccessResponse(_mapper.Map<StaffRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<StaffRequirementDto>>(response);
        return ApiResponseDto<StaffRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto)
    {
        var req = _mapper.Map<StaffRequirementReq>(dto);
        var response = await _http.PutAsync<StaffRequirementReq, StaffRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateStaffRequirement, HttpUtility.UrlEncode(projectId), year, srIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<StaffRequirementDto>.SuccessResponse(_mapper.Map<StaffRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<StaffRequirementDto>>(response);
        return ApiResponseDto<StaffRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteStaffRequirement, HttpUtility.UrlEncode(projectId), year, srIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedResult<TestRequirementDto>>> GetTestRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        var endpoint = string.Format(CostBookApiEndpoints.GetTestRequirements,
                                     HttpUtility.UrlEncode(projectId), year);
        var url = QueryStringHelper.AddQueryString(endpoint, query);

        var response = await _http.GetAsync<PaginationRes<TestRequirementRes>>(url);

        if (response.Success && response.Data != null)
        {
            var items      = _mapper.Map<List<TestRequirementDto>>(response.Data.Data);
            var pagination = response.Data.PaginationData;

            var result = new PaginatedResult<TestRequirementDto>(
                items,
                pagination?.TotalRecords ?? items.Count,
                pagination?.PageNumber   ?? query.Page,
                pagination?.PageSize     ?? query.PageSize);

            return ApiResponseDto<PaginatedResult<TestRequirementDto>>.SuccessResponse(result);
        }

        var err = _mapper.Map<ApiResponseDto<PaginatedResult<TestRequirementDto>>>(response);
        return ApiResponseDto<PaginatedResult<TestRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto)
    {
        var req = _mapper.Map<TestRequirementReq>(dto);
        var response = await _http.PostAsync<TestRequirementReq, TestRequirementRes>(
            string.Format(CostBookApiEndpoints.AddTestRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<TestRequirementDto>.SuccessResponse(_mapper.Map<TestRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
        return ApiResponseDto<TestRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto)
    {
        var req = _mapper.Map<TestRequirementReq>(dto);
        var response = await _http.PutAsync<TestRequirementReq, TestRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateTestRequirement, HttpUtility.UrlEncode(projectId), year, HttpUtility.UrlEncode(testCode)), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<TestRequirementDto>.SuccessResponse(_mapper.Map<TestRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<TestRequirementDto>>(response);
        return ApiResponseDto<TestRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteTestRequirement, HttpUtility.UrlEncode(projectId), year, HttpUtility.UrlEncode(testCode)));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Animals ───────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedResult<AnimalRequirementDto>>> GetAnimalRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        var endpoint = string.Format(CostBookApiEndpoints.GetAnimalRequirements,
                                     HttpUtility.UrlEncode(projectId), year);
        var url = QueryStringHelper.AddQueryString(endpoint, query);

        var response = await _http.GetAsync<PaginationRes<AnimalRequirementRes>>(url);

        if (response.Success && response.Data != null)
        {
            var items      = _mapper.Map<List<AnimalRequirementDto>>(response.Data.Data);
            var pagination = response.Data.PaginationData;

            var result = new PaginatedResult<AnimalRequirementDto>(
                items,
                pagination?.TotalRecords ?? items.Count,
                pagination?.PageNumber   ?? query.Page,
                pagination?.PageSize     ?? query.PageSize);

            return ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.SuccessResponse(result);
        }

        var err = _mapper.Map<ApiResponseDto<PaginatedResult<AnimalRequirementDto>>>(response);
        return ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto)
    {
        var req = _mapper.Map<AnimalRequirementReq>(dto);
        var response = await _http.PostAsync<AnimalRequirementReq, AnimalRequirementRes>(
            string.Format(CostBookApiEndpoints.AddAnimalRequirement, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AnimalRequirementDto>.SuccessResponse(_mapper.Map<AnimalRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(response);
        return ApiResponseDto<AnimalRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto)
    {
        var req = _mapper.Map<AnimalRequirementReq>(dto);
        var response = await _http.PutAsync<AnimalRequirementReq, AnimalRequirementRes>(
            string.Format(CostBookApiEndpoints.UpdateAnimalRequirement, HttpUtility.UrlEncode(projectId), year, arIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AnimalRequirementDto>.SuccessResponse(_mapper.Map<AnimalRequirementDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(response);
        return ApiResponseDto<AnimalRequirementDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteAnimalRequirement, HttpUtility.UrlEncode(projectId), year, arIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Additional Costs ──────────────────────────────────────────────────────

    public async Task<ApiResponseDto<PaginatedResult<AdditionalCostDto>>> GetAdditionalCostsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        var endpoint = string.Format(CostBookApiEndpoints.GetAdditionalCosts,
                                     HttpUtility.UrlEncode(projectId), year);
        var url = QueryStringHelper.AddQueryString(endpoint, query);

        var response = await _http.GetAsync<PaginationRes<AdditionalCostRes>>(url);

        if (response.Success && response.Data != null)
        {
            var items      = _mapper.Map<List<AdditionalCostDto>>(response.Data.Data);
            var pagination = response.Data.PaginationData;

            var result = new PaginatedResult<AdditionalCostDto>(
                items,
                pagination?.TotalRecords ?? items.Count,
                pagination?.PageNumber   ?? query.Page,
                pagination?.PageSize     ?? query.PageSize);

            return ApiResponseDto<PaginatedResult<AdditionalCostDto>>.SuccessResponse(result);
        }

        var err = _mapper.Map<ApiResponseDto<PaginatedResult<AdditionalCostDto>>>(response);
        return ApiResponseDto<PaginatedResult<AdditionalCostDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto)
    {
        var req = _mapper.Map<AdditionalCostReq>(dto);
        var response = await _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(
            string.Format(CostBookApiEndpoints.AddAdditionalCost, HttpUtility.UrlEncode(projectId), year), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AdditionalCostDto>.SuccessResponse(_mapper.Map<AdditionalCostDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
        return ApiResponseDto<AdditionalCostDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto)
    {
        var req = _mapper.Map<AdditionalCostReq>(dto);
        var response = await _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(
            string.Format(CostBookApiEndpoints.UpdateAdditionalCost, HttpUtility.UrlEncode(projectId), year, acIdentity), req);
        if (response.Success && response.Data != null)
            return ApiResponseDto<AdditionalCostDto>.SuccessResponse(_mapper.Map<AdditionalCostDto>(response.Data));
        var err = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
        return ApiResponseDto<AdditionalCostDto>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity)
    {
        var response = await _http.DeleteAsync<bool>(
            string.Format(CostBookApiEndpoints.DeleteAdditionalCost, HttpUtility.UrlEncode(projectId), year, acIdentity));
        if (response.Success)
            return ApiResponseDto<bool>.SuccessResponse(response.Data);
        var err = _mapper.Map<ApiResponseDto<bool>>(response);
        return ApiResponseDto<bool>.FailureResponse(err.Errors, err.Meta);
    }

    // ── Lookups ───────────────────────────────────────────────────────────────

    public async Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(string projectId, int year, bool isDefra)
    {
        var response = await _http.GetAsync<List<PayRateRes>>($"{CostBookApiEndpoints.GetPayRates}?projectId={HttpUtility.UrlEncode(projectId)}&year={year}&isDefra={isDefra}");
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<PayRateDto>>.SuccessResponse(_mapper.Map<List<PayRateDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<PayRateDto>>>(response);
        return ApiResponseDto<List<PayRateDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(string projectId, int year, bool isDefra)
    {
        var response = await _http.GetAsync<List<AnimalRateRes>>($"{CostBookApiEndpoints.GetAnimalRates}?projectId={HttpUtility.UrlEncode(projectId)}&year={year}&isDefra={isDefra}");
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(_mapper.Map<List<AnimalRateDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AnimalRateDto>>>(response);
        return ApiResponseDto<List<AnimalRateDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
    {
        var response = await _http.GetAsync<List<AccountCategoryRes>>(CostBookApiEndpoints.GetAccountCategories);
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(_mapper.Map<List<AccountCategoryDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
        return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<TestCodeLookupDto>>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra)
    {
        var response = await _http.GetAsync<List<TestCodeLookupRes>>($"{CostBookApiEndpoints.GetTestCodeLookups}?projectId={HttpUtility.UrlEncode(projectId)}&year={year}&isDefra={isDefra}");
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(_mapper.Map<List<TestCodeLookupDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<TestCodeLookupDto>>>(response);
        return ApiResponseDto<List<TestCodeLookupDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<List<AnimalLookupDto>>> GetAllAnimalsAsync()
    {
        var response = await _http.GetAsync<List<AnimalLookupRes>>(CostBookApiEndpoints.GetAllAnimals);
        if (response.Success && response.Data != null)
            return ApiResponseDto<List<AnimalLookupDto>>.SuccessResponse(_mapper.Map<List<AnimalLookupDto>>(response.Data));
        var err = _mapper.Map<ApiResponseDto<List<AnimalLookupDto>>>(response);
        return ApiResponseDto<List<AnimalLookupDto>>.FailureResponse(err.Errors, err.Meta);
    }

    public async Task<ApiResponseDto<string>> GetAdditionalCostinflamationAsync(string projectId, int year)
    {
        var query = $"{CostBookApiEndpoints.GetAdditionalCostinflamation}?projectId={HttpUtility.UrlEncode(projectId)}&year={year}";
        var response = await _http.GetAsync<string>(query);

        if (response.Success && response.Data != null)
            return ApiResponseDto<string>.SuccessResponse(response.Data);

        var err = _mapper.Map<ApiResponseDto<string>>(response);
        return ApiResponseDto<string>.FailureResponse(err.Errors, err.Meta);
    }
}
