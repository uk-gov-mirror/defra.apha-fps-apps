using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Costbook.CostBookYearlyDetailsApiClientTest;

public class CostBookYearlyDetailsApiClientTests
{
    private readonly ICostBookHttpExecutor _http;
    private readonly IMapper _mapper;
    private readonly CostBookYearlyDetailsApiClient _client;

    public CostBookYearlyDetailsApiClientTests()
    {
        _http = Substitute.For<ICostBookHttpExecutor>();
        _mapper = Substitute.For<IMapper>();
        _client = new CostBookYearlyDetailsApiClient(_http, _mapper);
    }

    #region GetProjectHeaderAsync

    [Fact]
    public async Task GetProjectHeaderAsync_WithSuccessResponse_ReturnsMappedHeader()
    {
        // Arrange
        var res = new ProjectHeaderRes { ProjectId = "2024/001", ProjectTitle = "Test" };
        var apiResponse = new ApiResponse<ProjectHeaderRes> { Success = true, Data = res };
        var mappedDto = new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test" };

        _http.GetAsync<ProjectHeaderRes>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ProjectHeaderDto>(res).Returns(mappedDto);

        // Act
        var result = await _client.GetProjectHeaderAsync("2024/001");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("2024/001", result.Data!.ProjectId);
        await _http.Received(1).GetAsync<ProjectHeaderRes>(Arg.Any<string>());
    }

    [Fact]
    public async Task GetProjectHeaderAsync_WithNullData_ReturnsFailureResponse()
    {
        // Arrange
        var apiResponse = new ApiResponse<ProjectHeaderRes> { Success = true, Data = null };
        var mappedResponse = new ApiResponseDto<ProjectHeaderDto>
        {
            Success = false, Errors = new List<ApiErrorDto>(), Meta = new ApiMetaDto()
        };

        _http.GetAsync<ProjectHeaderRes>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<ProjectHeaderDto>>(apiResponse).Returns(mappedResponse);

        // Act
        var result = await _client.GetProjectHeaderAsync("2024/001");

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetProjectHeaderAsync_WhenApiFails_ReturnsFailureResponse()
    {
        // Arrange
        var apiResponse = new ApiResponse<ProjectHeaderRes>
        {
            Success = false, Data = null,
            Errors = new List<ApiError> { new() { Code = "ERR", Message = "Failed" } }
        };
        var mappedResponse = new ApiResponseDto<ProjectHeaderDto>
        {
            Success = false,
            Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } },
            Meta = new ApiMetaDto()
        };

        _http.GetAsync<ProjectHeaderRes>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<ProjectHeaderDto>>(apiResponse).Returns(mappedResponse);

        // Act
        var result = await _client.GetProjectHeaderAsync("INVALID");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors);
    }

    #endregion

    #region GetProjectYearsAsync

    [Fact]
    public async Task GetProjectYearsAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<ProjectYearRes> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        var apiResponse = new ApiResponse<List<ProjectYearRes>> { Success = true, Data = resList };
        var mappedDtos = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };

        _http.GetAsync<List<ProjectYearRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<ProjectYearDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetProjectYearsAsync("2024/001");

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetProjectYearsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<ProjectYearRes>> { Success = false, Data = null };
        _http.GetAsync<List<ProjectYearRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<ProjectYearDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<ProjectYearDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetProjectYearsAsync("2024/001");

        Assert.False(result.Success);
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new ProjectYearDto { MarkupTime = 10.0, ProfitTime = 5.0 };
        var res = new ProjectYearRes { YearValue = 2 };
        var apiResponse = new ApiResponse<ProjectYearRes> { Success = true, Data = res };
        var mappedDto = new ProjectYearDto { YearValue = 2 };

        _http.PostAsync<AddProjectYearReq, ProjectYearRes>(Arg.Any<string>(), Arg.Any<AddProjectYearReq>())
            .Returns(apiResponse);
        _mapper.Map<ProjectYearDto>(res).Returns(mappedDto);

        var result = await _client.AddProjectYearAsync("2024/001", 2, dto);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.YearValue);
        await _http.Received(1).PostAsync<AddProjectYearReq, ProjectYearRes>(Arg.Any<string>(), Arg.Any<AddProjectYearReq>());
    }

    [Fact]
    public async Task AddProjectYearAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<ProjectYearRes> { Success = false, Data = null };
        _http.PostAsync<AddProjectYearReq, ProjectYearRes>(Arg.Any<string>(), Arg.Any<AddProjectYearReq>())
            .Returns(apiResponse);
        _mapper.Map<ApiResponseDto<ProjectYearDto>>(apiResponse)
            .Returns(new ApiResponseDto<ProjectYearDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.AddProjectYearAsync("2024/001", 2, new ProjectYearDto());

        Assert.False(result.Success);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new ProjectYearDto { YearValue = 1 };
        var req = new ProjectYearReq();
        var res = new ProjectYearRes { YearValue = 1 };
        var mappedDto = new ProjectYearDto { YearValue = 1 };

        _mapper.Map<ProjectYearReq>(dto).Returns(req);
        _http.PutAsync<ProjectYearReq, ProjectYearRes>(Arg.Any<string>(), req).Returns(
            new ApiResponse<ProjectYearRes> { Success = true, Data = res });
        _mapper.Map<ProjectYearDto>(res).Returns(mappedDto);

        var result = await _client.UpdateProjectYearAsync("2024/001", 1, dto);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.YearValue);
    }

    #endregion

    #region Staff Requirements

    [Fact]
    public async Task GetStaffRequirementsAsync_WithSuccessResponse_ReturnsPaginatedResult()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var staffResList = new List<StaffRequirementRes> { new() { SrIdentity = 1 } };
        var paginationData = new Pagination { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        var paginationRes = new PaginationRes<StaffRequirementRes>
        {
            Data = staffResList,
            PaginationData = paginationData
        };
        var apiResponse = new ApiResponse<PaginationRes<StaffRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<StaffRequirementDto> { new() { SrIdentity = 1 } };

        _http.GetAsync<PaginationRes<StaffRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<StaffRequirementDto>>(staffResList).Returns(mappedDtos);

        var result = await _client.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task GetStaffRequirementsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var apiResponse = new ApiResponse<PaginationRes<StaffRequirementRes>> { Success = false, Data = null };

        _http.GetAsync<PaginationRes<StaffRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<PaginatedResult<StaffRequirementDto>>>(apiResponse)
            .Returns(new ApiResponseDto<PaginatedResult<StaffRequirementDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" };
        var req = new StaffRequirementReq();
        var res = new StaffRequirementRes { SrIdentity = 1 };
        var mappedDto = new StaffRequirementDto { SrIdentity = 1 };

        _mapper.Map<StaffRequirementReq>(dto).Returns(req);
        _http.PostAsync<StaffRequirementReq, StaffRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<StaffRequirementRes> { Success = true, Data = res });
        _mapper.Map<StaffRequirementDto>(res).Returns(mappedDto);

        var result = await _client.AddStaffRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.SrIdentity);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new StaffRequirementDto();
        var req = new StaffRequirementReq();
        var res = new StaffRequirementRes { SrIdentity = 1 };
        var mappedDto = new StaffRequirementDto { SrIdentity = 1 };

        _mapper.Map<StaffRequirementReq>(dto).Returns(req);
        _http.PutAsync<StaffRequirementReq, StaffRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<StaffRequirementRes> { Success = true, Data = res });
        _mapper.Map<StaffRequirementDto>(res).Returns(mappedDto);

        var result = await _client.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_WithSuccessResponse_ReturnsTrue()
    {
        _http.DeleteAsync<bool>(Arg.Any<string>())
            .Returns(new ApiResponse<bool> { Success = true, Data = true });

        var result = await _client.DeleteStaffRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var apiResponse = new ApiResponse<bool> { Success = false, Data = false };
        _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<bool>>(apiResponse)
            .Returns(new ApiResponseDto<bool> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.DeleteStaffRequirementAsync("2024/001", 2024, 999);

        Assert.False(result.Success);
    }

    #endregion

    #region Test Requirements

    [Fact]
    public async Task GetTestRequirementsAsync_WithSuccessResponse_ReturnsPaginatedResult()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<TestRequirementRes> { new() { TestCode = "TC001" } };
        var paginationData = new Pagination { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        var paginationRes = new PaginationRes<TestRequirementRes>
        {
            Data = resList,
            PaginationData = paginationData
        };
        var apiResponse = new ApiResponse<PaginationRes<TestRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<TestRequirementDto> { new() { TestCode = "TC001" } };

        _http.GetAsync<PaginationRes<TestRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<TestRequirementDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task AddTestRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        var req = new TestRequirementReq();
        var res = new TestRequirementRes { TestCode = "TC001" };
        var mappedDto = new TestRequirementDto { TestCode = "TC001" };

        _mapper.Map<TestRequirementReq>(dto).Returns(req);
        _http.PostAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<TestRequirementRes> { Success = true, Data = res });
        _mapper.Map<TestRequirementDto>(res).Returns(mappedDto);

        var result = await _client.AddTestRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        Assert.Equal("TC001", result.Data!.TestCode);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new TestRequirementDto();
        var req = new TestRequirementReq();
        var res = new TestRequirementRes { TestCode = "TC001" };
        var mappedDto = new TestRequirementDto { TestCode = "TC001" };

        _mapper.Map<TestRequirementReq>(dto).Returns(req);
        _http.PutAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<TestRequirementRes> { Success = true, Data = res });
        _mapper.Map<TestRequirementDto>(res).Returns(mappedDto);

        var result = await _client.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteTestRequirementAsync_WithSuccessResponse_ReturnsTrue()
    {
        _http.DeleteAsync<bool>(Arg.Any<string>())
            .Returns(new ApiResponse<bool> { Success = true, Data = true });

        var result = await _client.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    #endregion

    #region Animal Requirements

    [Fact]
    public async Task GetAnimalRequirementsAsync_WithSuccessResponse_ReturnsPaginatedResult()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<AnimalRequirementRes> { new() { ArIdentity = 1 } };
        var paginationData = new Pagination { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        var paginationRes = new PaginationRes<AnimalRequirementRes> { Data = resList, PaginationData = paginationData };
        var apiResponse = new ApiResponse<PaginationRes<AnimalRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<AnimalRequirementDto> { new() { ArIdentity = 1 } };

        _http.GetAsync<PaginationRes<AnimalRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AnimalRequirementDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        var req = new AnimalRequirementReq();
        var res = new AnimalRequirementRes { ArIdentity = 1 };
        var mappedDto = new AnimalRequirementDto { ArIdentity = 1 };

        _mapper.Map<AnimalRequirementReq>(dto).Returns(req);
        _http.PostAsync<AnimalRequirementReq, AnimalRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<AnimalRequirementRes> { Success = true, Data = res });
        _mapper.Map<AnimalRequirementDto>(res).Returns(mappedDto);

        var result = await _client.AddAnimalRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new AnimalRequirementDto();
        var req = new AnimalRequirementReq();
        var res = new AnimalRequirementRes { ArIdentity = 1 };
        var mappedDto = new AnimalRequirementDto { ArIdentity = 1 };

        _mapper.Map<AnimalRequirementReq>(dto).Returns(req);
        _http.PutAsync<AnimalRequirementReq, AnimalRequirementRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<AnimalRequirementRes> { Success = true, Data = res });
        _mapper.Map<AnimalRequirementDto>(res).Returns(mappedDto);

        var result = await _client.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_WithSuccessResponse_ReturnsTrue()
    {
        _http.DeleteAsync<bool>(Arg.Any<string>())
            .Returns(new ApiResponse<bool> { Success = true, Data = true });

        var result = await _client.DeleteAnimalRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    #endregion

    #region Additional Costs

    [Fact]
    public async Task GetAdditionalCostsAsync_WithSuccessResponse_ReturnsPaginatedResult()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<AdditionalCostRes> { new() { AcIdentity = 1 } };
        var paginationData = new Pagination { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };
        var paginationRes = new PaginationRes<AdditionalCostRes> { Data = resList, PaginationData = paginationData };
        var apiResponse = new ApiResponse<PaginationRes<AdditionalCostRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<AdditionalCostDto> { new() { AcIdentity = 1 } };

        _http.GetAsync<PaginationRes<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AdditionalCostDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        var req = new AdditionalCostReq();
        var res = new AdditionalCostRes { AcIdentity = 1 };
        var mappedDto = new AdditionalCostDto { AcIdentity = 1 };

        _mapper.Map<AdditionalCostReq>(dto).Returns(req);
        _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<AdditionalCostRes> { Success = true, Data = res });
        _mapper.Map<AdditionalCostDto>(res).Returns(mappedDto);

        var result = await _client.AddAdditionalCostAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_WithSuccessResponse_ReturnsMappedDto()
    {
        var dto = new AdditionalCostDto();
        var req = new AdditionalCostReq();
        var res = new AdditionalCostRes { AcIdentity = 1 };
        var mappedDto = new AdditionalCostDto { AcIdentity = 1 };

        _mapper.Map<AdditionalCostReq>(dto).Returns(req);
        _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req)
            .Returns(new ApiResponse<AdditionalCostRes> { Success = true, Data = res });
        _mapper.Map<AdditionalCostDto>(res).Returns(mappedDto);

        var result = await _client.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_WithSuccessResponse_ReturnsTrue()
    {
        _http.DeleteAsync<bool>(Arg.Any<string>())
            .Returns(new ApiResponse<bool> { Success = true, Data = true });

        var result = await _client.DeleteAdditionalCostAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    #endregion

    #region Lookups

    [Fact]
    public async Task GetPayRatesAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<PayRateRes> { new() { WgGrade = "HEO" } };
        var apiResponse = new ApiResponse<List<PayRateRes>> { Success = true, Data = resList };
        var mappedDtos = new List<PayRateDto> { new() { WgGrade = "HEO" } };

        _http.GetAsync<List<PayRateRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<PayRateDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetPayRatesAsync("2024/001", 2024, false);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<AnimalRateRes> { new() { AnimalType = "CAT" } };
        var apiResponse = new ApiResponse<List<AnimalRateRes>> { Success = true, Data = resList };
        var mappedDtos = new List<AnimalRateDto> { new() { AnimalType = "CAT" } };

        _http.GetAsync<List<AnimalRateRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AnimalRateDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAnimalRatesAsync("2024/001", 2024, true);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<AccountCategoryRes> { new() { AccShortName = "TRAVEL" } };
        var apiResponse = new ApiResponse<List<AccountCategoryRes>> { Success = true, Data = resList };
        var mappedDtos = new List<AccountCategoryDto> { new() { AccShortName = "TRAVEL" } };

        _http.GetAsync<List<AccountCategoryRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AccountCategoryDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAccountCategoriesAsync();

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<TestCodeLookupRes> { new() { ItemCode = "TC001" } };
        var apiResponse = new ApiResponse<List<TestCodeLookupRes>> { Success = true, Data = resList };
        var mappedDtos = new List<TestCodeLookupDto> { new() { ItemCode = "TC001" } };

        _http.GetAsync<List<TestCodeLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<TestCodeLookupDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetTestCodeLookupsAsync("2024/001", 2024, false);

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetAllAnimalsAsync_WithSuccessResponse_ReturnsMappedList()
    {
        var resList = new List<AnimalLookupRes> { new() { AnimalType = "CAT" } };
        var apiResponse = new ApiResponse<List<AnimalLookupRes>> { Success = true, Data = resList };
        var mappedDtos = new List<AnimalLookupDto> { new() { AnimalType = "CAT" } };

        _http.GetAsync<List<AnimalLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AnimalLookupDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAllAnimalsAsync();

        Assert.True(result.Success);
        Assert.Single(result.Data!);
    }

    [Fact]
    public async Task GetAllAnimalsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<AnimalLookupRes>> { Success = false, Data = null };
        _http.GetAsync<List<AnimalLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<AnimalLookupDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<AnimalLookupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } },
                Meta = new ApiMetaDto()
            });

        var result = await _client.GetAllAnimalsAsync();

        Assert.False(result.Success);
        Assert.Single(result.Errors!);
    }

    #endregion

    #region DeleteProjectYearAsync

    [Fact]
    public async Task DeleteProjectYearAsync_WithSuccessResponse_ReturnsTrue()
    {
        _http.DeleteAsync<bool>(Arg.Any<string>())
            .Returns(new ApiResponse<bool> { Success = true, Data = true });

        var result = await _client.DeleteProjectYearAsync("2024/001", 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _http.Received(1).DeleteAsync<bool>(Arg.Any<string>());
    }

    [Fact]
    public async Task DeleteProjectYearAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<bool> { Success = false, Data = false };
        _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<bool>>(apiResponse)
            .Returns(new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "CHILD_RECORDS", Message = "Year has 2 staff requirements. Remove them first." } },
                Meta = new ApiMetaDto()
            });

        var result = await _client.DeleteProjectYearAsync("2024/001", 1);

        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Contains("staff requirements", result.Errors![0].Message);
    }

    #endregion

    #region UpdateProjectYearAsync Failure

    [Fact]
    public async Task UpdateProjectYearAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new ProjectYearDto { YearValue = 1 };
        var req = new ProjectYearReq();
        _mapper.Map<ProjectYearReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<ProjectYearRes> { Success = false, Data = null };
        _http.PutAsync<ProjectYearReq, ProjectYearRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<ProjectYearDto>>(apiResponse)
            .Returns(new ApiResponseDto<ProjectYearDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.UpdateProjectYearAsync("2024/001", 1, dto);

        Assert.False(result.Success);
    }

    #endregion

    #region Staff Requirement Failures

    [Fact]
    public async Task GetStaffRequirementsAsync_WithNullPaginationData_UsesFallbackCounts()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var staffResList = new List<StaffRequirementRes> { new() { SrIdentity = 1 } };
        var paginationRes = new PaginationRes<StaffRequirementRes> { Data = staffResList, PaginationData = null! };
        var apiResponse = new ApiResponse<PaginationRes<StaffRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<StaffRequirementDto> { new() { SrIdentity = 1 } };

        _http.GetAsync<PaginationRes<StaffRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<StaffRequirementDto>>(staffResList).Returns(mappedDtos);

        var result = await _client.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" };
        var req = new StaffRequirementReq();
        _mapper.Map<StaffRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<StaffRequirementRes> { Success = false, Data = null };
        _http.PostAsync<StaffRequirementReq, StaffRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<StaffRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<StaffRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.AddStaffRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new StaffRequirementDto();
        var req = new StaffRequirementReq();
        _mapper.Map<StaffRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<StaffRequirementRes> { Success = false, Data = null };
        _http.PutAsync<StaffRequirementReq, StaffRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<StaffRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<StaffRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    #endregion

    #region Test Requirement Failures

    [Fact]
    public async Task GetTestRequirementsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var apiResponse = new ApiResponse<PaginationRes<TestRequirementRes>> { Success = false, Data = null };
        _http.GetAsync<PaginationRes<TestRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<PaginatedResult<TestRequirementDto>>>(apiResponse)
            .Returns(new ApiResponseDto<PaginatedResult<TestRequirementDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetTestRequirementsAsync_WithNullPaginationData_UsesFallbackCounts()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<TestRequirementRes> { new() { TestCode = "TC001" } };
        var paginationRes = new PaginationRes<TestRequirementRes> { Data = resList, PaginationData = null! };
        var apiResponse = new ApiResponse<PaginationRes<TestRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<TestRequirementDto> { new() { TestCode = "TC001" } };

        _http.GetAsync<PaginationRes<TestRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<TestRequirementDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task AddTestRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        var req = new TestRequirementReq();
        _mapper.Map<TestRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<TestRequirementRes> { Success = false, Data = null };
        _http.PostAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<TestRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.AddTestRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new TestRequirementDto();
        var req = new TestRequirementReq();
        _mapper.Map<TestRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<TestRequirementRes> { Success = false, Data = null };
        _http.PutAsync<TestRequirementReq, TestRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<TestRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<TestRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteTestRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<bool> { Success = false, Data = false };
        _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<bool>>(apiResponse)
            .Returns(new ApiResponseDto<bool> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.False(result.Success);
    }

    #endregion

    #region Animal Requirement Failures

    [Fact]
    public async Task GetAnimalRequirementsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var apiResponse = new ApiResponse<PaginationRes<AnimalRequirementRes>> { Success = false, Data = null };
        _http.GetAsync<PaginationRes<AnimalRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<PaginatedResult<AnimalRequirementDto>>>(apiResponse)
            .Returns(new ApiResponseDto<PaginatedResult<AnimalRequirementDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAnimalRequirementsAsync_WithNullPaginationData_UsesFallbackCounts()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<AnimalRequirementRes> { new() { ArIdentity = 1 } };
        var paginationRes = new PaginationRes<AnimalRequirementRes> { Data = resList, PaginationData = null! };
        var apiResponse = new ApiResponse<PaginationRes<AnimalRequirementRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<AnimalRequirementDto> { new() { ArIdentity = 1 } };

        _http.GetAsync<PaginationRes<AnimalRequirementRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AnimalRequirementDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        var req = new AnimalRequirementReq();
        _mapper.Map<AnimalRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<AnimalRequirementRes> { Success = false, Data = null };
        _http.PostAsync<AnimalRequirementReq, AnimalRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<AnimalRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.AddAnimalRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new AnimalRequirementDto();
        var req = new AnimalRequirementReq();
        _mapper.Map<AnimalRequirementReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<AnimalRequirementRes> { Success = false, Data = null };
        _http.PutAsync<AnimalRequirementReq, AnimalRequirementRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<AnimalRequirementDto>>(apiResponse)
            .Returns(new ApiResponseDto<AnimalRequirementDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<bool> { Success = false, Data = false };
        _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<bool>>(apiResponse)
            .Returns(new ApiResponseDto<bool> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.DeleteAnimalRequirementAsync("2024/001", 2024, 1);

        Assert.False(result.Success);
    }

    #endregion

    #region Additional Cost Failures

    [Fact]
    public async Task GetAdditionalCostsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var apiResponse = new ApiResponse<PaginationRes<AdditionalCostRes>> { Success = false, Data = null };
        _http.GetAsync<PaginationRes<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<PaginatedResult<AdditionalCostDto>>>(apiResponse)
            .Returns(new ApiResponseDto<PaginatedResult<AdditionalCostDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAdditionalCostsAsync_WithNullPaginationData_UsesFallbackCounts()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var resList = new List<AdditionalCostRes> { new() { AcIdentity = 1 } };
        var paginationRes = new PaginationRes<AdditionalCostRes> { Data = resList, PaginationData = null! };
        var apiResponse = new ApiResponse<PaginationRes<AdditionalCostRes>> { Success = true, Data = paginationRes };
        var mappedDtos = new List<AdditionalCostDto> { new() { AcIdentity = 1 } };

        _http.GetAsync<PaginationRes<AdditionalCostRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<List<AdditionalCostDto>>(resList).Returns(mappedDtos);

        var result = await _client.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.TotalCount);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        var req = new AdditionalCostReq();
        _mapper.Map<AdditionalCostReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<AdditionalCostRes> { Success = false, Data = null };
        _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse)
            .Returns(new ApiResponseDto<AdditionalCostDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.AddAdditionalCostAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var dto = new AdditionalCostDto();
        var req = new AdditionalCostReq();
        _mapper.Map<AdditionalCostReq>(dto).Returns(req);
        var apiResponse = new ApiResponse<AdditionalCostRes> { Success = false, Data = null };
        _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(Arg.Any<string>(), req).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<AdditionalCostDto>>(apiResponse)
            .Returns(new ApiResponseDto<AdditionalCostDto> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<bool> { Success = false, Data = false };
        _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<bool>>(apiResponse)
            .Returns(new ApiResponseDto<bool> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.DeleteAdditionalCostAsync("2024/001", 2024, 1);

        Assert.False(result.Success);
    }

    #endregion

    #region Lookup Failures and Flag Variants

    [Fact]
    public async Task GetPayRatesAsync_WithIsDefraTrue_PassesFlagInUrl()
    {
        var resList = new List<PayRateRes> { new() { WgGrade = "SEO" } };
        var apiResponse = new ApiResponse<List<PayRateRes>> { Success = true, Data = resList };
        _http.GetAsync<List<PayRateRes>>(Arg.Is<string>(s => s.Contains("isDefra=True"))).Returns(apiResponse);
        _mapper.Map<List<PayRateDto>>(resList).Returns(new List<PayRateDto> { new() { WgGrade = "SEO" } });

        var result = await _client.GetPayRatesAsync("2024/001", 2024, true);

        Assert.True(result.Success);
        await _http.Received(1).GetAsync<List<PayRateRes>>(Arg.Is<string>(s => s.Contains("isDefra=True")));
    }

    [Fact]
    public async Task GetPayRatesAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<PayRateRes>> { Success = false, Data = null };
        _http.GetAsync<List<PayRateRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<PayRateDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<PayRateDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetPayRatesAsync("2024/001", 2024, false);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_WithIsDefraFalse_PassesFlagInUrl()
    {
        var resList = new List<AnimalRateRes> { new() { AnimalType = "DOG" } };
        var apiResponse = new ApiResponse<List<AnimalRateRes>> { Success = true, Data = resList };
        _http.GetAsync<List<AnimalRateRes>>(Arg.Is<string>(s => s.Contains("isDefra=False"))).Returns(apiResponse);
        _mapper.Map<List<AnimalRateDto>>(resList).Returns(new List<AnimalRateDto> { new() { AnimalType = "DOG" } });

        var result = await _client.GetAnimalRatesAsync("2024/001", 2024, false);

        Assert.True(result.Success);
        await _http.Received(1).GetAsync<List<AnimalRateRes>>(Arg.Is<string>(s => s.Contains("isDefra=False")));
    }

    [Fact]
    public async Task GetAnimalRatesAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<AnimalRateRes>> { Success = false, Data = null };
        _http.GetAsync<List<AnimalRateRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<AnimalRateDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<AnimalRateDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAnimalRatesAsync("2024/001", 2024, true);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAccountCategoriesAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<AccountCategoryRes>> { Success = false, Data = null };
        _http.GetAsync<List<AccountCategoryRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<AccountCategoryDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAccountCategoriesAsync();

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_WithIsDefraTrue_PassesFlagInUrl()
    {
        var resList = new List<TestCodeLookupRes> { new() { ItemCode = "TC002" } };
        var apiResponse = new ApiResponse<List<TestCodeLookupRes>> { Success = true, Data = resList };
        _http.GetAsync<List<TestCodeLookupRes>>(Arg.Is<string>(s => s.Contains("isDefra=True"))).Returns(apiResponse);
        _mapper.Map<List<TestCodeLookupDto>>(resList).Returns(new List<TestCodeLookupDto> { new() { ItemCode = "TC002" } });

        var result = await _client.GetTestCodeLookupsAsync("2024/001", 2024, true);

        Assert.True(result.Success);
        await _http.Received(1).GetAsync<List<TestCodeLookupRes>>(Arg.Is<string>(s => s.Contains("isDefra=True")));
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<List<TestCodeLookupRes>> { Success = false, Data = null };
        _http.GetAsync<List<TestCodeLookupRes>>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<List<TestCodeLookupDto>>>(apiResponse)
            .Returns(new ApiResponseDto<List<TestCodeLookupDto>> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetTestCodeLookupsAsync("2024/001", 2024, false);

        Assert.False(result.Success);
    }

    #endregion

    #region GetAdditionalCostinflamationAsync

    [Fact]
    public async Task GetAdditionalCostinflamationAsync_WithSuccessResponse_ReturnsValue()
    {
        var apiResponse = new ApiResponse<string> { Success = true, Data = "1.25" };
        _http.GetAsync<string>(Arg.Is<string>(s => s.Contains("additionalcostinflamation") && s.Contains("projectId=2024%2f001") && s.Contains("year=2024")))
            .Returns(apiResponse);

        var result = await _client.GetAdditionalCostinflamationAsync("2024/001", 2024);

        Assert.True(result.Success);
        Assert.Equal("1.25", result.Data);
        await _http.Received(1).GetAsync<string>(Arg.Is<string>(s => s.Contains("additionalcostinflamation") && s.Contains("projectId=2024%2f001") && s.Contains("year=2024")));
    }

    [Fact]
    public async Task GetAdditionalCostinflamationAsync_WithNullData_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<string> { Success = true, Data = null };
        _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<string>>(apiResponse)
            .Returns(new ApiResponseDto<string> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAdditionalCostinflamationAsync("2024/001", 2024);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAdditionalCostinflamationAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var apiResponse = new ApiResponse<string> { Success = false, Data = null };
        _http.GetAsync<string>(Arg.Any<string>()).Returns(apiResponse);
        _mapper.Map<ApiResponseDto<string>>(apiResponse)
            .Returns(new ApiResponseDto<string> { Success = false, Errors = new(), Meta = new() });

        var result = await _client.GetAdditionalCostinflamationAsync("2024/001", 2024);

        Assert.False(result.Success);
    }

    #endregion
}

