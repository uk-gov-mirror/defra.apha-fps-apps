using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Costbook.CostBookYearlyDetailsServiceTest;

public class CostBookYearlyDetailsServiceTests
{
    private readonly ICostBookApiClient _mockApiClient;
    private readonly ICostBookYearlyDetailsApiClient _yearlyDetailsClient;
    private readonly CostBookYearlyDetailsService _sut;

    public CostBookYearlyDetailsServiceTests()
    {
        _mockApiClient = Substitute.For<ICostBookApiClient>();
        _yearlyDetailsClient = Substitute.For<ICostBookYearlyDetailsApiClient>();
        _mockApiClient.YearlyDetails.Returns(_yearlyDetailsClient);
        _sut = new CostBookYearlyDetailsService(_mockApiClient);
    }

    #region GetProjectHeaderAsync

    [Fact]
    public async Task GetProjectHeaderAsync_WithSuccessResponse_ReturnsHeader()
    {
        var expected = ApiResponseDto<ProjectHeaderDto>.SuccessResponse(
            new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test" });
        _yearlyDetailsClient.GetProjectHeaderAsync("2024/001").Returns(expected);

        var result = await _sut.GetProjectHeaderAsync("2024/001");

        Assert.True(result.Success);
        Assert.Equal("2024/001", result.Data!.ProjectId);
        await _yearlyDetailsClient.Received(1).GetProjectHeaderAsync("2024/001");
    }

    [Fact]
    public async Task GetProjectHeaderAsync_WhenApiFails_ReturnsFailureResponse()
    {
        var expected = ApiResponseDto<ProjectHeaderDto>.FailureResponse(
            new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } }, new ApiMetaDto());
        _yearlyDetailsClient.GetProjectHeaderAsync("INVALID").Returns(expected);

        var result = await _sut.GetProjectHeaderAsync("INVALID");

        Assert.False(result.Success);
        Assert.Single(result.Errors!);
    }

    #endregion

    #region GetProjectYearsAsync

    [Fact]
    public async Task GetProjectYearsAsync_WithSuccessResponse_ReturnsYears()
    {
        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _yearlyDetailsClient.GetProjectYearsAsync("2024/001").Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        var result = await _sut.GetProjectYearsAsync("2024/001");

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        await _yearlyDetailsClient.Received(1).GetProjectYearsAsync("2024/001");
    }

    [Fact]
    public async Task GetProjectYearsAsync_WithEmptyResult_ReturnsEmptyList()
    {
        _yearlyDetailsClient.GetProjectYearsAsync("2024/001")
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _sut.GetProjectYearsAsync("2024/001");

        Assert.True(result.Success);
        Assert.Empty(result.Data!);
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_DelegatesToClient()
    {
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };
        var expected = ApiResponseDto<ProjectYearDto>.SuccessResponse(dto);
        _yearlyDetailsClient.AddProjectYearAsync("2024/001", 2, dto).Returns(expected);

        var result = await _sut.AddProjectYearAsync("2024/001", 2, dto);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.YearValue);
        await _yearlyDetailsClient.Received(1).AddProjectYearAsync("2024/001", 2, dto);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_DelegatesToClient()
    {
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };
        _yearlyDetailsClient.UpdateProjectYearAsync("2024/001", 1, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(dto));

        var result = await _sut.UpdateProjectYearAsync("2024/001", 1, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).UpdateProjectYearAsync("2024/001", 1, dto);
    }

    #endregion

    #region Staff Requirements

    [Fact]
    public async Task GetStaffRequirementsAsync_DelegatesToClient()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var pagedResult = new PaginatedResult<StaffRequirementDto>(
            new List<StaffRequirementDto> { new() { SrIdentity = 1 } }, 1);
        _yearlyDetailsClient.GetStaffRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));

        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        await _yearlyDetailsClient.Received(1).GetStaffRequirementsAsync("2024/001", 2024, query);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_DelegatesToClient()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" };
        _yearlyDetailsClient.AddStaffRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddStaffRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).AddStaffRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_DelegatesToClient()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1 };
        _yearlyDetailsClient.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_DelegatesToClient()
    {
        _yearlyDetailsClient.DeleteStaffRequirementAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteStaffRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _yearlyDetailsClient.Received(1).DeleteStaffRequirementAsync("2024/001", 2024, 1);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.DeleteStaffRequirementAsync("2024/001", 2024, 999)
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.DeleteStaffRequirementAsync("2024/001", 2024, 999);

        Assert.False(result.Success);
    }

    #endregion

    #region Test Requirements

    [Fact]
    public async Task GetTestRequirementsAsync_DelegatesToClient()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var pagedResult = new PaginatedResult<TestRequirementDto>(
            new List<TestRequirementDto> { new() { TestCode = "TC001" } }, 1);
        _yearlyDetailsClient.GetTestRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.SuccessResponse(pagedResult));

        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        await _yearlyDetailsClient.Received(1).GetTestRequirementsAsync("2024/001", 2024, query);
    }

    [Fact]
    public async Task AddTestRequirementAsync_DelegatesToClient()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _yearlyDetailsClient.AddTestRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddTestRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).AddTestRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_DelegatesToClient()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _yearlyDetailsClient.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto)
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);
    }

    [Fact]
    public async Task DeleteTestRequirementAsync_DelegatesToClient()
    {
        _yearlyDetailsClient.DeleteTestRequirementAsync("2024/001", 2024, "TC001")
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _yearlyDetailsClient.Received(1).DeleteTestRequirementAsync("2024/001", 2024, "TC001");
    }

    #endregion

    #region Animal Requirements

    [Fact]
    public async Task GetAnimalRequirementsAsync_DelegatesToClient()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var animals = new List<AnimalRequirementDto> { new() { ArIdentity = 1, AnimalType = "CAT" } };
        var pagedResult = new PaginatedResult<AnimalRequirementDto>(animals, 1);
        _yearlyDetailsClient.GetAnimalRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.SuccessResponse(pagedResult));

        var result = await _sut.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        await _yearlyDetailsClient.Received(1).GetAnimalRequirementsAsync("2024/001", 2024, query);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_DelegatesToClient()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        _yearlyDetailsClient.AddAnimalRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(dto));

        var result = await _sut.AddAnimalRequirementAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).AddAnimalRequirementAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_DelegatesToClient()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1 };
        _yearlyDetailsClient.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(dto));

        var result = await _sut.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_DelegatesToClient()
    {
        _yearlyDetailsClient.DeleteAnimalRequirementAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteAnimalRequirementAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _yearlyDetailsClient.Received(1).DeleteAnimalRequirementAsync("2024/001", 2024, 1);
    }

    #endregion

    #region Additional Costs

    [Fact]
    public async Task GetAdditionalCostsAsync_DelegatesToClient()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var costs = new List<AdditionalCostDto> { new() { AcIdentity = 1, Description = "Travel" } };
        var pagedResult = new PaginatedResult<AdditionalCostDto>(costs, 1);
        _yearlyDetailsClient.GetAdditionalCostsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.SuccessResponse(pagedResult));

        var result = await _sut.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.True(result.Success);
        Assert.Single(result.Data!.data);
        await _yearlyDetailsClient.Received(1).GetAdditionalCostsAsync("2024/001", 2024, query);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_DelegatesToClient()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        _yearlyDetailsClient.AddAdditionalCostAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto));

        var result = await _sut.AddAdditionalCostAsync("2024/001", 2024, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).AddAdditionalCostAsync("2024/001", 2024, dto);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_DelegatesToClient()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1 };
        _yearlyDetailsClient.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(dto));

        var result = await _sut.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_DelegatesToClient()
    {
        _yearlyDetailsClient.DeleteAdditionalCostAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteAdditionalCostAsync("2024/001", 2024, 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _yearlyDetailsClient.Received(1).DeleteAdditionalCostAsync("2024/001", 2024, 1);
    }

    #endregion

    #region DeleteProjectYearAsync

    [Fact]
    public async Task DeleteProjectYearAsync_DelegatesToClient_WhenSuccess()
    {
        _yearlyDetailsClient.DeleteProjectYearAsync("2024/001", 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _sut.DeleteProjectYearAsync("2024/001", 1);

        Assert.True(result.Success);
        Assert.True(result.Data);
        await _yearlyDetailsClient.Received(1).DeleteProjectYearAsync("2024/001", 1);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.DeleteProjectYearAsync("2024/001", 1)
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "CHILD_RECORDS", Message = "Year has 2 staff requirements. Remove them first." } },
                new ApiMetaDto()));

        var result = await _sut.DeleteProjectYearAsync("2024/001", 1);

        Assert.False(result.Success);
        Assert.Single(result.Errors!);
        Assert.Contains("staff requirements", result.Errors![0].Message);
    }

    #endregion

    #region AddProjectYearAsync Failure

    [Fact]
    public async Task AddProjectYearAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new ProjectYearDto { YearValue = 2 };
        _yearlyDetailsClient.AddProjectYearAsync("2024/001", 2, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.AddProjectYearAsync("2024/001", 2, dto);

        Assert.False(result.Success);
    }

    #endregion

    #region UpdateProjectYearAsync Failure

    [Fact]
    public async Task UpdateProjectYearAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new ProjectYearDto { YearValue = 1 };
        _yearlyDetailsClient.UpdateProjectYearAsync("2024/001", 1, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.UpdateProjectYearAsync("2024/001", 1, dto);

        Assert.False(result.Success);
    }

    #endregion

    #region Staff Requirements Failures

    [Fact]
    public async Task GetStaffRequirementsAsync_WhenApiFails_ReturnsFailure()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        _yearlyDetailsClient.GetStaffRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" };
        _yearlyDetailsClient.AddStaffRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.AddStaffRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1 };
        _yearlyDetailsClient.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.UpdateStaffRequirementAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    #endregion

    #region Test Requirements Failures

    [Fact]
    public async Task GetTestRequirementsAsync_WhenApiFails_ReturnsFailure()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        _yearlyDetailsClient.GetTestRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddTestRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _yearlyDetailsClient.AddTestRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.AddTestRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" };
        _yearlyDetailsClient.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto)
            .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.UpdateTestRequirementAsync("2024/001", 2024, "TC001", dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteTestRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.DeleteTestRequirementAsync("2024/001", 2024, "TC001")
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.False(result.Success);
    }

    #endregion

    #region Animal Requirements Failures

    [Fact]
    public async Task GetAnimalRequirementsAsync_WhenApiFails_ReturnsFailure()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        _yearlyDetailsClient.GetAnimalRequirementsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" };
        _yearlyDetailsClient.AddAnimalRequirementAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.AddAnimalRequirementAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1 };
        _yearlyDetailsClient.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AnimalRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.UpdateAnimalRequirementAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.DeleteAnimalRequirementAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.DeleteAnimalRequirementAsync("2024/001", 2024, 1);

        Assert.False(result.Success);
    }

    #endregion

    #region Additional Costs Failures

    [Fact]
    public async Task GetAdditionalCostsAsync_WhenApiFails_ReturnsFailure()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        _yearlyDetailsClient.GetAdditionalCostsAsync("2024/001", 2024, query)
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new AdditionalCostDto { Description = "Travel" };
        _yearlyDetailsClient.AddAdditionalCostAsync("2024/001", 2024, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.AddAdditionalCostAsync("2024/001", 2024, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_WhenApiFails_ReturnsFailure()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1 };
        _yearlyDetailsClient.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto)
            .Returns(ApiResponseDto<AdditionalCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.UpdateAdditionalCostAsync("2024/001", 2024, 1, dto);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.DeleteAdditionalCostAsync("2024/001", 2024, 1)
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.DeleteAdditionalCostAsync("2024/001", 2024, 1);

        Assert.False(result.Success);
    }

    #endregion

    #region Lookup Failures and isDefra Variants

    [Fact]
    public async Task GetPayRatesAsync_WithIsDefraTrue_DelegatesToClient()
    {
        var rates = new List<PayRateDto> { new() { WgGrade = "SEO" } };
        _yearlyDetailsClient.GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), true).Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(rates));

        var result = await _sut.GetPayRatesAsync("2024/001", 2024, true);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), true);
    }

    [Fact]
    public async Task GetPayRatesAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), false)
            .Returns(ApiResponseDto<List<PayRateDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetPayRatesAsync("2024/001", 2024, false);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_WithIsDefraFalse_DelegatesToClient()
    {
        var rates = new List<AnimalRateDto> { new() { AnimalType = "DOG" } };
        _yearlyDetailsClient.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), false).Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(rates));

        var result = await _sut.GetAnimalRatesAsync("2024/001", 2024, false);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), false);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), true)
            .Returns(ApiResponseDto<List<AnimalRateDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAnimalRatesAsync("2024/001", 2024, true);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAccountCategoriesAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAccountCategoriesAsync();

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_WithIsDefraTrue_DelegatesToClient()
    {
        var lookups = new List<TestCodeLookupDto> { new() { ItemCode = "TC002" } };
        _yearlyDetailsClient.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), true).Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(lookups));

        var result = await _sut.GetTestCodeLookupsAsync("2024/001", 2024, true);

        Assert.True(result.Success);
        await _yearlyDetailsClient.Received(1).GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), true);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), false)
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetTestCodeLookupsAsync("2024/001", 2024, false);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAllAnimalsAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetAllAnimalsAsync()
            .Returns(ApiResponseDto<List<AnimalLookupDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAllAnimalsAsync();

        Assert.False(result.Success);
    }

    [Fact]
    public async Task GetAdditionalCostinflamationAsync_WithValidInputs_DelegatesToClient()
    {
        var expected = ApiResponseDto<string>.SuccessResponse("1.25");
        _yearlyDetailsClient.GetAdditionalCostinflamationAsync("2024/001", 2024).Returns(expected);

        var result = await _sut.GetAdditionalCostinflamationAsync("2024/001", 2024);

        Assert.True(result.Success);
        Assert.Equal("1.25", result.Data);
        await _yearlyDetailsClient.Received(1).GetAdditionalCostinflamationAsync("2024/001", 2024);
    }

    [Fact]
    public async Task GetAdditionalCostinflamationAsync_WhenApiFails_ReturnsFailure()
    {
        _yearlyDetailsClient.GetAdditionalCostinflamationAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<string>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _sut.GetAdditionalCostinflamationAsync("2024/001", 2024);

        Assert.False(result.Success);
    }

    #endregion
}

