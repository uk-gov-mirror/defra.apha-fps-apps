using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Application.Validation;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.YearlyDetailsServiceTest;

public class YearlyDetailsServiceTests
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectYearRepository _projectYearRepo;
    private readonly IStaffRequirementRepository _staffRepo;
    private readonly ITestRequirementRepository _testRepo;
    private readonly IAnimalRequirementRepository _animalRepo;
    private readonly IAdditionalCostRepository _additionalCostRepo;
    private readonly ISettingsService _settingsService;
    private readonly IMapper _mapper;
    private readonly YearlyDetailsService _sut;

    public YearlyDetailsServiceTests()
    {
        _projectRepo = Substitute.For<IProjectRepository>();
        _projectYearRepo = Substitute.For<IProjectYearRepository>();
        _staffRepo = Substitute.For<IStaffRequirementRepository>();
        _testRepo = Substitute.For<ITestRequirementRepository>();
        _animalRepo = Substitute.For<IAnimalRequirementRepository>();
        _additionalCostRepo = Substitute.For<IAdditionalCostRepository>();
        _settingsService = Substitute.For<ISettingsService>();
        _mapper = Substitute.For<IMapper>();

        _sut = new YearlyDetailsService(
            _projectRepo, _projectYearRepo, _staffRepo,
            _testRepo, _animalRepo, _additionalCostRepo, _settingsService, _mapper);
    }

    #region GetProjectHeaderAsync

    [Fact]
    public async Task GetProjectHeaderAsync_ReturnsNull_WhenProjectNotFound()
    {
        // Arrange
        _projectRepo.GetProjectByIdAsync("NOTFOUND").Returns((Project?)null);

        // Act
        var result = await _sut.GetProjectHeaderAsync("NOTFOUND");

        // Assert
        Assert.Null(result);
        await _projectRepo.Received(1).GetProjectByIdAsync("NOTFOUND");
    }

    [Fact]
    public async Task GetProjectHeaderAsync_ReturnsMappedDto_WhenProjectExists()
    {
        // Arrange
        var project = new Project { ProjectId = "2024/001", ProjectTitle = "Test" };
        var dto = new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test" };

        _projectRepo.GetProjectByIdAsync("2024/001").Returns(project);
        _mapper.Map<ProjectHeaderDto>(project).Returns(dto);

        // Act
        var result = await _sut.GetProjectHeaderAsync("2024/001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.ProjectId);
        _mapper.Received(1).Map<ProjectHeaderDto>(project);
    }

    #endregion

    #region GetProjectYearsAsync

    [Fact]
    public async Task GetProjectYearsAsync_ReturnsMappedDtos()
    {
        // Arrange
        var years = new List<ProjectYear> { new() { Project = "2024/001", YearValue = 1 } };
        var dtos = new List<ProjectYearDto> { new() { Project = "2024/001", YearValue = 1 } };

        _projectYearRepo.GetByProjectAsync("2024/001").Returns(years);
        _mapper.Map<IEnumerable<ProjectYearDto>>(years).Returns(dtos);

        // Act
        var result = await _sut.GetProjectYearsAsync("2024/001");

        // Assert
        Assert.Single(result);
        await _projectYearRepo.Received(1).GetByProjectAsync("2024/001");
    }

    #endregion

    #region AddProjectYearAsync

    [Fact]
    public async Task AddProjectYearAsync_MapsAndCallsRepo_ReturnsMappedDto()
    {
        // Arrange
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };
        var entity = new ProjectYear { Project = "2024/001", YearValue = 2 };
        var added = new ProjectYear { Project = "2024/001", YearValue = 2 };
        var resultDto = new ProjectYearDto { Project = "2024/001", YearValue = 2 };

        _mapper.Map<ProjectYear>(dto).Returns(entity);
        _projectYearRepo.AddProjectYearAsync("2024/001", 2, entity).Returns(added);
        _mapper.Map<ProjectYearDto>(added).Returns(resultDto);

        // Act
        var result = await _sut.AddProjectYearAsync("2024/001", 2, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.YearValue);
        await _projectYearRepo.Received(1).AddProjectYearAsync("2024/001", 2, entity);
    }

    #endregion

    #region UpdateProjectYearAsync

    [Fact]
    public async Task UpdateProjectYearAsync_MapsAndCallsRepo_ReturnsMappedDto()
    {
        // Arrange
        var dto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };
        var entity = new ProjectYear { Project = "2024/001", YearValue = 1 };
        var updated = new ProjectYear { Project = "2024/001", YearValue = 1 };
        var resultDto = new ProjectYearDto { Project = "2024/001", YearValue = 1 };

        _mapper.Map<ProjectYear>(dto).Returns(entity);
        _projectYearRepo.UpdateProjectYearAsync(entity).Returns(updated);
        _mapper.Map<ProjectYearDto>(updated).Returns(resultDto);

        // Act
        var result = await _sut.UpdateProjectYearAsync(dto);

        // Assert
        Assert.NotNull(result);
        await _projectYearRepo.Received(1).UpdateProjectYearAsync(entity);
    }

    #endregion

    #region GetStaffRequirementsAsync

    [Fact]
    public async Task GetStaffRequirementsAsync_ReturnsPaginatedResult_WithStaffCostCalculated()
    {
        // Arrange
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>
            {
                new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Chargerate = 50.0, Nohours = 100.0 }
            },
            PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        // Act
        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        // Assert
        Assert.NotNull(result);
        var staff = result.Data.First();
        Assert.Equal(1, staff.SrIdentity);
        Assert.Equal(5000.0, staff.StaffCost); // 50 * 100
        Assert.Equal(1, result.PaginationData.TotalRecords);
    }

    [Fact]
    public async Task GetStaffRequirementsAsync_StaffCostIsNull_WhenChargerateOrNohoursIsNull()
    {
        // Arrange
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>
            {
                new() { SrIdentity = 1, Chargerate = null, Nohours = 100.0 }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        // Act
        var result = await _sut.GetStaffRequirementsAsync("2024/001", 2024, query);

        // Assert
        Assert.Null(result.Data.First().StaffCost);
    }

    #endregion

    #region AddStaffRequirementAsync

    [Fact]
    public async Task AddStaffRequirementAsync_MapsAndCallsRepo_ReturnsDto()
    {
        // Arrange
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 100, Nodays = 10, Chargerate = 50, StaffCost = 5000 };
        var entity = new StaffRequirement { WgGrade = "HEO", Chargerate = 50, Nohours = 100 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.AddStaffRequirementAsync(entity).Returns(entity);

        // Act
        var result = await _sut.AddStaffRequirementAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HEO", result.WgGrade);
        Assert.Equal(5000.0, result.StaffCost);
        await _staffRepo.Received(1).AddStaffRequirementAsync(entity);
    }

    #endregion

    #region UpdateStaffRequirementAsync

    [Fact]
    public async Task UpdateStaffRequirementAsync_MapsAndCallsRepo_ReturnsDto()
    {
        // Arrange
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };
        var entity = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO" };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.UpdateStaffRequirementAsync(entity).Returns(entity);

        // Act
        var result = await _sut.UpdateStaffRequirementAsync(dto);

        // Assert
        Assert.NotNull(result);
        await _staffRepo.Received(1).UpdateStaffRequirementAsync(entity);
    }

    #endregion

    #region DeleteStaffRequirementAsync

    [Fact]
    public async Task DeleteStaffRequirementAsync_ReturnsTrue_WhenDeleted()
    {
        _staffRepo.DeleteStaffRequirementAsync(1).Returns(true);

        var result = await _sut.DeleteStaffRequirementAsync(1);

        Assert.True(result);
        await _staffRepo.Received(1).DeleteStaffRequirementAsync(1);
    }

    [Fact]
    public async Task DeleteStaffRequirementAsync_ReturnsFalse_WhenNotFound()
    {
        _staffRepo.DeleteStaffRequirementAsync(999).Returns(false);

        var result = await _sut.DeleteStaffRequirementAsync(999);

        Assert.False(result);
    }

    #endregion

    #region GetTestRequirementsAsync

    [Fact]
    public async Task GetTestRequirementsAsync_MapsFieldsCorrectly()
    {
        // Arrange
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var rows = new List<TestRequirementDetailView>
        {
            new() { Project = "2024/001", Year = 2024, TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5, TestCost = 500, TestDescription = "Blood Test" }
        };
        var repoResult = new PagedData<TestRequirementDetailView>
        {
            Data = rows,
            PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _testRepo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        // Act
        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024, query);

        // Assert
        Assert.Single(result.Data);
        Assert.Equal("TC001", result.Data.First().TestCode);
        Assert.Equal(500.0, result.Data.First().TestCost);
        Assert.Equal("Blood Test", result.Data.First().TestDescription);
        Assert.Equal(1, result.PaginationData.TotalRecords);
    }

    [Fact]
    public async Task GetTestRequirementsAsync_ReturnsEmptyList_WhenNoData()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<TestRequirementDetailView>
        {
            Data = new List<TestRequirementDetailView>(),
            PaginationData = new PaginationData { TotalRecords = 0 }
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _testRepo.GetTestRequirementsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(new PaginationDto());

        var result = await _sut.GetTestRequirementsAsync("2024/001", 2024, query);

        Assert.Empty(result.Data);
    }

    #endregion

    #region AddTestRequirementAsync

    [Fact]
    public async Task AddTestRequirementAsync_MapsAndCallsRepo_ReturnsDtoWithTestCost()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 100, NumberOfTests = 5, TestCost = 500 };
        var entity = new TestRequirement { TestCode = "TC001", UnitPrice = 100, NumberOfTests = 5 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.AddTestRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddTestRequirementAsync(dto);

        Assert.Equal("TC001", result.TestCode);
        Assert.Equal(500.0, result.TestCost); // 100 * 5
        await _testRepo.Received(1).AddTestRequirementAsync(entity);
    }

    #endregion

    #region UpdateTestRequirementAsync

    [Fact]
    public async Task UpdateTestRequirementAsync_MapsAndCallsRepo()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 100, NumberOfTests = 5, TestCost = 500 };
        var entity = new TestRequirement { TestCode = "TC001" };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.UpdateTestRequirementAsync(entity).Returns(entity);

        var result = await _sut.UpdateTestRequirementAsync(dto);

        Assert.Equal("TC001", result.TestCode);
        await _testRepo.Received(1).UpdateTestRequirementAsync(entity);
    }

    #endregion

    #region DeleteTestRequirementAsync

    [Fact]
    public async Task DeleteTestRequirementAsync_DelegatesToRepo()
    {
        _testRepo.DeleteTestRequirementAsync("2024/001", 2024, "TC001").Returns(true);

        var result = await _sut.DeleteTestRequirementAsync("2024/001", 2024, "TC001");

        Assert.True(result);
        await _testRepo.Received(1).DeleteTestRequirementAsync("2024/001", 2024, "TC001");
    }

    #endregion

    #region GetAnimalRequirementsAsync

    [Fact]
    public async Task GetAnimalRequirementsAsync_MapsFieldsCorrectly()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AnimalRequirementDetailView>
        {
            Data = new List<AnimalRequirementDetailView>
            {
                new() { ArIdentity = 1, Project = "2024/001", Year = 2024, AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 }
            },
            PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _animalRepo.GetAnimalRequirementsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        var result = await _sut.GetAnimalRequirementsAsync("2024/001", 2024, query);

        Assert.Single(result.Data);
        Assert.Equal("CAT", result.Data.First().AnimalType);
        Assert.Equal(150.0, result.Data.First().AnimalCost);
        Assert.Equal(1, result.PaginationData.TotalRecords);
    }

    #endregion

    #region AddAnimalRequirementAsync

    [Fact]
    public async Task AddAnimalRequirementAsync_MapsAndCallsRepo_ReturnsDtoWithAnimalCost()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 };
        var entity = new AnimalRequirement { AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Equal("CAT", result.AnimalType);
        Assert.Equal(150.0, result.AnimalCost); // 5 * 3 * 10
        await _animalRepo.Received(1).AddAnimalRequirementAsync(entity);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ReturnsNullAnimalCost_WhenNumberOfDaysIsNull()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = null, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = null };
        var entity = new AnimalRequirement { NumberOfDays = null, NumberOfAnimals = 3, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Null(result.AnimalCost);
    }

    #endregion

    #region UpdateAnimalRequirementAsync

    [Fact]
    public async Task UpdateAnimalRequirementAsync_MapsAndCallsRepo()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "DOG", Project = "P1", Year = 1, NumberOfDays = 5, NumberOfAnimals = 2, DailyRate = 10, AnimalCost = 100 };
        var entity = new AnimalRequirement { ArIdentity = 1, AnimalType = "DOG" };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.UpdateAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.UpdateAnimalRequirementAsync(dto);

        Assert.Equal("DOG", result.AnimalType);
        await _animalRepo.Received(1).UpdateAnimalRequirementAsync(entity);
    }

    #endregion

    #region DeleteAnimalRequirementAsync

    [Fact]
    public async Task DeleteAnimalRequirementAsync_DelegatesToRepo()
    {
        _animalRepo.DeleteAnimalRequirementAsync(1).Returns(true);

        var result = await _sut.DeleteAnimalRequirementAsync(1);

        Assert.True(result);
        await _animalRepo.Received(1).DeleteAnimalRequirementAsync(1);
    }

    #endregion

    #region GetAdditionalCostsAsync

    [Fact]
    public async Task GetAdditionalCostsAsync_MapsFieldsCorrectly()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AdditionalCostDetailView>
        {
            Data = new List<AdditionalCostDetailView>
            {
                new() { AcIdentity = 1, Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", Description = "Travel", ItemCost = 500, CostEntered = 500, Freq = "Annual" }
            },
            PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10, TotalPages = 1 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _additionalCostRepo.GetAdditionalCostsByProjectYearAsync("2024/001", 2024, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        var result = await _sut.GetAdditionalCostsAsync("2024/001", 2024, query);

        Assert.Single(result.Data);
        Assert.Equal("TRAVEL", result.Data.First().AccountCat);
        Assert.Equal(500.0, result.Data.First().CostEntered);
        Assert.Equal(1, result.PaginationData.TotalRecords);
    }

    #endregion

    #region AddAdditionalCostAsync

    [Fact]
    public async Task AddAdditionalCostAsync_MapsAndCallsRepo()
    {
        var dto = new AdditionalCostDto { Description = "Travel", AccountCat = "TRAVEL", Project = "P1", Year = 1, CostEntered = 500 };
        var entity = new AdditionalCost { Description = "Travel" };
        var resultDto = new AdditionalCostDto { Description = "Travel" };

        _mapper.Map<AdditionalCost>(dto).Returns(entity);
        _additionalCostRepo.AddAdditionalCostAsync(entity).Returns(entity);
        _mapper.Map<AdditionalCostDto>(entity).Returns(resultDto);

        var result = await _sut.AddAdditionalCostAsync(dto);

        Assert.Equal("Travel", result.Description);
        await _additionalCostRepo.Received(1).AddAdditionalCostAsync(entity);
    }

    #endregion

    #region UpdateAdditionalCostAsync

    [Fact]
    public async Task UpdateAdditionalCostAsync_MapsAndCallsRepo()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1, Description = "Travel", AccountCat = "TRAVEL", Project = "P1", Year = 1, CostEntered = 500 };
        var entity = new AdditionalCost { AcIdentity = 1 };
        var resultDto = new AdditionalCostDto { AcIdentity = 1 };

        _mapper.Map<AdditionalCost>(dto).Returns(entity);
        _additionalCostRepo.UpdateAdditionalCostAsync(entity).Returns(entity);
        _mapper.Map<AdditionalCostDto>(entity).Returns(resultDto);

        var result = await _sut.UpdateAdditionalCostAsync(dto);

        Assert.NotNull(result);
        await _additionalCostRepo.Received(1).UpdateAdditionalCostAsync(entity);
    }

    #endregion

    #region DeleteAdditionalCostAsync

    [Fact]
    public async Task DeleteAdditionalCostAsync_DelegatesToRepo()
    {
        _additionalCostRepo.DeleteAdditionalCostAsync(1).Returns(true);

        var result = await _sut.DeleteAdditionalCostAsync(1);

        Assert.True(result);
        await _additionalCostRepo.Received(1).DeleteAdditionalCostAsync(1);
    }

    #endregion

    #region GetPayRatesAsync

    [Fact]
    public async Task GetPayRatesAsync_MapsFieldsCorrectly()
    {
        var rates = new List<PayRateLookup>
        {
            new() { WgGrade = "HEO", ChargeRate = 45.50m, PayRate = 30.0m, Npr = 5.0m, Ohr = 10.0m }
        };
        _staffRepo.GetPayRatesAsync("2024/001", 2024, false).Returns(rates);

        var result = (await _sut.GetPayRatesAsync("2024/001", 2024, false)).ToList();

        Assert.Single(result);
        Assert.Equal("HEO", result[0].WgGrade);
        Assert.Equal(45.50m, result[0].ChargeRate);
        Assert.Equal(30.0m, result[0].PayRate);
    }

    #endregion

    #region GetAnimalRatesAsync

    [Fact]
    public async Task GetAnimalRatesAsync_MapsFieldsCorrectly()
    {
        var rates = new List<AnimalRateLookup>
        {
            new() { AnimalType = "CAT", DailyRate = 10.50m }
        };
        _animalRepo.GetAnimalRatesAsync("2024/001", 2024, true).Returns(rates);

        var result = (await _sut.GetAnimalRatesAsync("2024/001", 2024, true)).ToList();

        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal(10.50m, result[0].DailyRate);
    }

    #endregion

    #region GetAccountCategoriesAsync

    [Fact]
    public async Task GetAccountCategoriesAsync_MapsFieldsCorrectly()
    {
        var cats = new List<AccountCategoryLookup>
        {
            new("TRAVEL", true)
        };
        _additionalCostRepo.GetProjectSpecificAccountCategoriesAsync().Returns(cats);

        var result = (await _sut.GetAccountCategoriesAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("TRAVEL", result[0].AccShortName);
        Assert.True(result[0].UseInflation);
    }

    #endregion

    #region GetTestCodeLookupsAsync

    [Fact]
    public async Task GetTestCodeLookupsAsync_MapsFieldsCorrectly()
    {
        var lookups = new List<TestCodeLookup>
        {
            new() { ItemCode = "TC001", ItemDescription = "Blood Test", UnitPrice = 100m }
        };
        _testRepo.GetTestCodeLookupsAsync("2024/001", 2024, false).Returns(lookups);

        var result = (await _sut.GetTestCodeLookupsAsync("2024/001", 2024, false)).ToList();

        Assert.Single(result);
        Assert.Equal("TC001", result[0].ItemCode);
        Assert.Equal("Blood Test", result[0].ItemDescription);
        Assert.Equal(100m, result[0].UnitPrice);
    }

    #endregion

    #region GetAllAnimalsAsync

    [Fact]
    public async Task GetAllAnimalsAsync_MapsFieldsCorrectly()
    {
        var animals = new List<FpsAnimals>
        {
            new() { AnimalType = "CAT", Species = "Felis", SecurityLevel = "Low", DailyRate = 10m, PlanByWeek = true, DefraDailyRate = 15m }
        };
        _animalRepo.GetAllAnimalsAsync().Returns(animals);

        var result = (await _sut.GetAllAnimalsAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("CAT", result[0].AnimalType);
        Assert.Equal("Felis", result[0].Species);
        Assert.Equal("Low", result[0].SecurityLevel);
        Assert.Equal(10m, result[0].DailyRate);
        Assert.True(result[0].PlanByWeek);
        Assert.Equal(15m, result[0].DefraDailyRate);
    }

    #endregion

    #region ValidateStaffRequirement

    [Fact]
    public async Task AddStaffRequirementAsync_ThrowsValidation_WhenWgGradeIsEmpty()
    {
        var dto = new StaffRequirementDto { WgGrade = "", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_WGGRADE_REQUIRED");
    }

    [Fact]
    public async Task AddStaffRequirementAsync_ThrowsValidation_WhenNohoursIsNegative()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = -1, Nodays = 1, Chargerate = 50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_NOHOURS_INVALID");
    }

    [Fact]
    public async Task AddStaffRequirementAsync_ThrowsValidation_WhenChargerateIsNegative()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = -50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_CHARGERATE_INVALID");
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_ThrowsValidation_WhenWgGradeIsEmpty()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_WGGRADE_REQUIRED");
    }

    [Fact]
    public async Task AddStaffRequirementAsync_ThrowsValidation_WhenRequiredFieldsMissing()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO" }; // Missing Project, Name, Year, etc.

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddStaffRequirementAsync(dto));
        Assert.NotEmpty(ex.Errors);
    }

    #endregion

    #region ValidateTestRequirement

    [Fact]
    public async Task AddTestRequirementAsync_ThrowsValidation_WhenTestCodeIsEmpty()
    {
        var dto = new TestRequirementDto { TestCode = "", Project = "P1", Year = 1, NumberOfTests = 5, UnitPrice = 100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_TESTCODE_REQUIRED");
    }

    [Fact]
    public async Task AddTestRequirementAsync_ThrowsValidation_WhenNumberOfTestsIsNegative()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, NumberOfTests = -1, UnitPrice = 100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_NUMBEROFTESTS_INVALID");
    }

    [Fact]
    public async Task AddTestRequirementAsync_ThrowsValidation_WhenUnitPriceIsNegative()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, NumberOfTests = 5, UnitPrice = -100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_UNITPRICE_INVALID");
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_ThrowsValidation_WhenTestCodeIsEmpty()
    {
        var dto = new TestRequirementDto { TestCode = "", Project = "P1", Year = 1, NumberOfTests = 5, UnitPrice = 100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_TESTCODE_REQUIRED");
    }

    [Fact]
    public async Task AddTestRequirementAsync_ThrowsValidation_WhenRequiredFieldsMissing()
    {
        var dto = new TestRequirementDto { TestCode = "TC001" }; // Missing Project, Year, etc.

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddTestRequirementAsync(dto));
        Assert.NotEmpty(ex.Errors);
    }

    #endregion

    #region ValidateAnimalRequirement

    [Fact]
    public async Task AddAnimalRequirementAsync_ThrowsValidation_WhenAnimalTypeIsEmpty()
    {
        var dto = new AnimalRequirementDto { AnimalType = "", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = 5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_ANIMALTYPE_REQUIRED");
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ThrowsValidation_WhenNumberOfAnimalsIsNegative()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = -1, NumberOfDays = 5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_NUMBEROFANIMALS_INVALID");
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ThrowsValidation_WhenNumberOfDaysIsNegative()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = -5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_NUMBEROFDAYS_INVALID");
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ThrowsValidation_WhenDailyRateIsNegative()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = 5, DailyRate = -10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_DAILYRATE_INVALID");
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_ThrowsValidation_WhenAnimalTypeIsEmpty()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = 5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_ANIMALTYPE_REQUIRED");
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_ThrowsValidation_WhenRequiredFieldsMissing()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT" }; // Missing Project, Year, etc.

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAnimalRequirementAsync(dto));
        Assert.NotEmpty(ex.Errors);
    }

    #endregion

    #region ValidateAdditionalCost

    [Fact]
    public async Task AddAdditionalCostAsync_ThrowsValidation_WhenAccountCatIsEmpty()
    {
        var dto = new AdditionalCostDto { AccountCat = "", Description = "Test", Project = "P1", Year = 1, CostEntered = 100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_ACCOUNTCAT_REQUIRED");
    }

    [Fact]
    public async Task AddAdditionalCostAsync_ThrowsValidation_WhenDescriptionIsEmpty()
    {
        var dto = new AdditionalCostDto { AccountCat = "TRAVEL", Description = "", Project = "P1", Year = 1, CostEntered = 100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_DESCRIPTION_REQUIRED");
    }

    [Fact]
    public async Task AddAdditionalCostAsync_ThrowsValidation_WhenCostEnteredIsNegative()
    {
        var dto = new AdditionalCostDto { AccountCat = "TRAVEL", Description = "Test", Project = "P1", Year = 1, CostEntered = -100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_COSTENTERED_INVALID");
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_ThrowsValidation_WhenAccountCatIsEmpty()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1, AccountCat = "", Description = "Test", Project = "P1", Year = 1, CostEntered = 100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_ACCOUNTCAT_REQUIRED");
    }

    [Fact]
    public async Task AddAdditionalCostAsync_ThrowsValidation_WhenRequiredFieldsMissing()
    {
        var dto = new AdditionalCostDto { AccountCat = "TRAVEL", Description = "Test" }; // Missing Project, Year

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.AddAdditionalCostAsync(dto));
        Assert.NotEmpty(ex.Errors);
    }

    #endregion

    #region Additional Coverage - MapTestToDto null cost via GetTestRequirementsAsync

    [Fact]
    public async Task GetTestRequirementsAsync_TestCostIsNull_WhenUnitPriceIsNull()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<TestRequirementDetailView>
        {
            Data = new List<TestRequirementDetailView>
            {
                new() { Project = "P1", Year = 1, TestCode = "TC001", UnitPrice = null, NumberOfTests = 5, TestCost = null }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _testRepo.GetTestRequirementsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetTestRequirementsAsync("P1", 1, query);

        Assert.Null(result.Data.First().TestCost);
    }

    [Fact]
    public async Task GetTestRequirementsAsync_TestCostIsNull_WhenNumberOfTestsIsNull()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<TestRequirementDetailView>
        {
            Data = new List<TestRequirementDetailView>
            {
                new() { Project = "P1", Year = 1, TestCode = "TC001", UnitPrice = 100, NumberOfTests = null, TestCost = null }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _testRepo.GetTestRequirementsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetTestRequirementsAsync("P1", 1, query);

        Assert.Null(result.Data.First().TestCost);
    }

    #endregion

    #region Additional Coverage - Update validation negative values

    [Fact]
    public async Task UpdateStaffRequirementAsync_ThrowsValidation_WhenNohoursIsNegative()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = -5, Nodays = 1, Chargerate = 50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_NOHOURS_INVALID");
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_ThrowsValidation_WhenChargerateIsNegative()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = -50, StaffCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateStaffRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "STAFF_CHARGERATE_INVALID");
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_ThrowsValidation_WhenNumberOfTestsIsNegative()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, NumberOfTests = -1, UnitPrice = 100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_NUMBEROFTESTS_INVALID");
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_ThrowsValidation_WhenUnitPriceIsNegative()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, NumberOfTests = 5, UnitPrice = -100, TestCost = 500 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateTestRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "TEST_UNITPRICE_INVALID");
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_ThrowsValidation_WhenNumberOfAnimalsIsNegative()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = -1, NumberOfDays = 5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_NUMBEROFANIMALS_INVALID");
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_ThrowsValidation_WhenNumberOfDaysIsNegative()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = -5, DailyRate = 10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_NUMBEROFDAYS_INVALID");
    }

    [Fact]
    public async Task UpdateAnimalRequirementAsync_ThrowsValidation_WhenDailyRateIsNegative()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "CAT", Project = "P1", Year = 1, NumberOfAnimals = 3, NumberOfDays = 5, DailyRate = -10, AnimalCost = 150 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAnimalRequirementAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ANIMAL_DAILYRATE_INVALID");
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_ThrowsValidation_WhenDescriptionIsEmpty()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1, AccountCat = "TRAVEL", Description = "", Project = "P1", Year = 1, CostEntered = 100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_DESCRIPTION_REQUIRED");
    }

    [Fact]
    public async Task UpdateAdditionalCostAsync_ThrowsValidation_WhenCostEnteredIsNegative()
    {
        var dto = new AdditionalCostDto { AcIdentity = 1, AccountCat = "TRAVEL", Description = "Test", Project = "P1", Year = 1, CostEntered = -100 };

        var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAdditionalCostAsync(dto));
        Assert.Contains(ex.Errors, e => e.Code == "ADDITIONALCOST_COSTENTERED_INVALID");
    }

    #endregion

    #region Additional Coverage - Delete returns false

    [Fact]
    public async Task DeleteTestRequirementAsync_ReturnsFalse_WhenNotFound()
    {
        _testRepo.DeleteTestRequirementAsync("P1", 1, "NOTFOUND").Returns(false);

        var result = await _sut.DeleteTestRequirementAsync("P1", 1, "NOTFOUND");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAnimalRequirementAsync_ReturnsFalse_WhenNotFound()
    {
        _animalRepo.DeleteAnimalRequirementAsync(999).Returns(false);

        var result = await _sut.DeleteAnimalRequirementAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAdditionalCostAsync_ReturnsFalse_WhenNotFound()
    {
        _additionalCostRepo.DeleteAdditionalCostAsync(999).Returns(false);

        var result = await _sut.DeleteAdditionalCostAsync(999);

        Assert.False(result);
    }

    #endregion

    #region Additional Coverage - GetStaffRequirementsAsync enriched fields

    [Fact]
    public async Task GetStaffRequirementsAsync_MapsEnrichedFields()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>
            {
                new() { SrIdentity = 1, Project = "P1", Year = 1, WgGrade = "HEO", Name = "John",
                         Nohours = 10, Nodays = 2, Chargerate = 50, Payrate = 30, Npr = 5, Ohr = 10,
                         WorkGroup = "WG1", GradeCode = "G1", Programme = "Prog1", EuroConvRate = 1.2, EuGrade = "EU1" }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetStaffRequirementsAsync("P1", 1, query);
        var staff = result.Data.First();

        Assert.Equal("WG1", staff.WorkGroup);
        Assert.Equal("G1", staff.GradeCode);
        Assert.Equal("Prog1", staff.Programme);
        Assert.Equal(1.2, staff.EuroConvRate);
        Assert.Equal("EU1", staff.EuGrade);
        Assert.Equal("John", staff.Name);
        Assert.Equal(10.0, staff.Nohours);
        Assert.Equal(2.0, staff.Nodays);
        Assert.Equal(30.0, staff.Payrate);
        Assert.Equal(5.0, staff.Npr);
        Assert.Equal(10.0, staff.Ohr);
    }

    #endregion

    #region Additional Coverage - GetAdditionalCostsAsync all fields

    [Fact]
    public async Task GetAdditionalCostsAsync_MapsAllFields()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AdditionalCostDetailView>
        {
            Data = new List<AdditionalCostDetailView>
            {
                new() { AcIdentity = 5, Project = "P1", Year = 2, AccountCat = "EQUIP", Description = "Equipment", ItemCost = 200, CostEntered = 150, Freq = "Monthly" }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _additionalCostRepo.GetAdditionalCostsByProjectYearAsync("P1", 2, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetAdditionalCostsAsync("P1", 2, query);
        var item = result.Data.First();

        Assert.Equal(5, item.AcIdentity);
        Assert.Equal("P1", item.Project);
        Assert.Equal(2, item.Year);
        Assert.Equal("EQUIP", item.AccountCat);
        Assert.Equal("Equipment", item.Description);
        Assert.Equal(200.0, item.ItemCost);
        Assert.Equal(150.0, item.CostEntered);
        Assert.Equal("Monthly", item.Freq);
    }

    #endregion

    #region Additional Coverage - GetAnimalRequirementsAsync all fields

    [Fact]
    public async Task GetAnimalRequirementsAsync_MapsAllFields()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AnimalRequirementDetailView>
        {
            Data = new List<AnimalRequirementDetailView>
            {
                new() { ArIdentity = 3, Project = "P1", Year = 2, AnimalType = "DOG", NumberOfDays = 7, NumberOfAnimals = 2, DailyRate = 15, AnimalCost = 210 }
            },
            PaginationData = new PaginationData()
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _animalRepo.GetAnimalRequirementsByProjectYearAsync("P1", 2, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetAnimalRequirementsAsync("P1", 2, query);
        var item = result.Data.First();

        Assert.Equal(3, item.ArIdentity);
        Assert.Equal("P1", item.Project);
        Assert.Equal(2, item.Year);
        Assert.Equal("DOG", item.AnimalType);
        Assert.Equal(7.0, item.NumberOfDays);
        Assert.Equal(2.0, item.NumberOfAnimals);
        Assert.Equal(15.0, item.DailyRate);
        Assert.Equal(210.0, item.AnimalCost);
    }

    #endregion

    #region Additional Coverage - MapStaffToDto null StaffCost branch

    [Fact]
    public async Task AddStaffRequirementAsync_StaffCostIsNull_WhenRepoReturnsNullChargerate()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };
        var entity = new StaffRequirement { WgGrade = "HEO" };
        var returned = new StaffRequirement { WgGrade = "HEO", Chargerate = null, Nohours = 10 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.AddStaffRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddStaffRequirementAsync(dto);

        Assert.Null(result.StaffCost);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_StaffCostIsNull_WhenRepoReturnsNullNohours()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };
        var entity = new StaffRequirement { WgGrade = "HEO" };
        var returned = new StaffRequirement { WgGrade = "HEO", Chargerate = 50, Nohours = null };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.AddStaffRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddStaffRequirementAsync(dto);

        Assert.Null(result.StaffCost);
    }

    #endregion

    #region Additional Coverage - MapTestToDto null TestCost branch via Add

    [Fact]
    public async Task AddTestRequirementAsync_TestCostIsNull_WhenRepoReturnsNullUnitPrice()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 100, NumberOfTests = 5, TestCost = 500 };
        var entity = new TestRequirement { TestCode = "TC001" };
        var returned = new TestRequirement { TestCode = "TC001", UnitPrice = null, NumberOfTests = 5 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.AddTestRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddTestRequirementAsync(dto);

        Assert.Null(result.TestCost);
    }

    [Fact]
    public async Task AddTestRequirementAsync_TestCostIsNull_WhenRepoReturnsNullNumberOfTests()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 100, NumberOfTests = 5, TestCost = 500 };
        var entity = new TestRequirement { TestCode = "TC001" };
        var returned = new TestRequirement { TestCode = "TC001", UnitPrice = 100, NumberOfTests = null };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.AddTestRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddTestRequirementAsync(dto);

        Assert.Null(result.TestCost);
    }

    #endregion

    #region Additional Coverage - MapAnimalToDto null AnimalCost branch via Add

    [Fact]
    public async Task AddAnimalRequirementAsync_AnimalCostIsNull_WhenRepoReturnsNullNumberOfAnimals()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 };
        var entity = new AnimalRequirement { AnimalType = "CAT" };
        var returned = new AnimalRequirement { AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = null, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Null(result.AnimalCost);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_AnimalCostIsNull_WhenRepoReturnsNullDailyRate()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 };
        var entity = new AnimalRequirement { AnimalType = "CAT" };
        var returned = new AnimalRequirement { AnimalType = "CAT", NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = null };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(returned);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.Null(result.AnimalCost);
    }

    #endregion

    #region Additional Coverage - Validation pass-through (no errors)

    [Fact]
    public async Task AddStaffRequirementAsync_PassesValidation_WhenAllFieldsValid()
    {
        var dto = new StaffRequirementDto { WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 0, Nodays = 0, Chargerate = 0, StaffCost = 0 };
        var entity = new StaffRequirement { WgGrade = "HEO", Chargerate = 0, Nohours = 0 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.AddStaffRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddStaffRequirementAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(0.0, result.StaffCost);
    }

    [Fact]
    public async Task AddTestRequirementAsync_PassesValidation_WhenAllFieldsValid()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 0, NumberOfTests = 0, TestCost = 0 };
        var entity = new TestRequirement { TestCode = "TC001", UnitPrice = 0, NumberOfTests = 0 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.AddTestRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddTestRequirementAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(0.0, result.TestCost);
    }

    [Fact]
    public async Task AddAnimalRequirementAsync_PassesValidation_WhenAllFieldsValid()
    {
        var dto = new AnimalRequirementDto { AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = 0, NumberOfAnimals = 0, DailyRate = 0, AnimalCost = 0 };
        var entity = new AnimalRequirement { AnimalType = "CAT", NumberOfDays = 0, NumberOfAnimals = 0, DailyRate = 0 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.AddAnimalRequirementAsync(entity).Returns(entity);

        var result = await _sut.AddAnimalRequirementAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(0.0, result.AnimalCost);
    }

    [Fact]
    public async Task AddAdditionalCostAsync_PassesValidation_WhenAllFieldsValid()
    {
        var dto = new AdditionalCostDto { AccountCat = "TRAVEL", Description = "Valid", Project = "P1", Year = 1, CostEntered = 0 };
        var entity = new AdditionalCost { AccountCat = "TRAVEL", Description = "Valid" };
        var resultDto = new AdditionalCostDto { AccountCat = "TRAVEL", Description = "Valid" };

        _mapper.Map<AdditionalCost>(dto).Returns(entity);
        _additionalCostRepo.AddAdditionalCostAsync(entity).Returns(entity);
        _mapper.Map<AdditionalCostDto>(entity).Returns(resultDto);

        var result = await _sut.AddAdditionalCostAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("TRAVEL", result.AccountCat);
    }

    #endregion

    #region DeleteProjectYearAsync

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsSuccessResult_WhenDeletedSuccessfully()
    {
        var expected = (Deleted: true, Errors: (IReadOnlyList<string>)new List<string>());
        _projectYearRepo.DeleteProjectYearAsync("2024/001", 2024).Returns(expected);

        var result = await _sut.DeleteProjectYearAsync("2024/001", 2024);

        Assert.True(result.Deleted);
        Assert.Empty(result.Errors);
        await _projectYearRepo.Received(1).DeleteProjectYearAsync("2024/001", 2024);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFailureResult_WhenChildRecordsExist()
    {
        var errors = new List<string> { "Staff records exist.", "Animal records exist." };
        var expected = (Deleted: false, Errors: (IReadOnlyList<string>)errors);
        _projectYearRepo.DeleteProjectYearAsync("2024/001", 2024).Returns(expected);

        var result = await _sut.DeleteProjectYearAsync("2024/001", 2024);

        Assert.False(result.Deleted);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains("Staff records exist.", result.Errors);
        Assert.Contains("Animal records exist.", result.Errors);
    }

    [Fact]
    public async Task DeleteProjectYearAsync_ReturnsFailureResult_WhenYearNotFound()
    {
        var expected = (Deleted: false, Errors: (IReadOnlyList<string>)new List<string> { "Year not found." });
        _projectYearRepo.DeleteProjectYearAsync("2024/001", 9999).Returns(expected);

        var result = await _sut.DeleteProjectYearAsync("2024/001", 9999);

        Assert.False(result.Deleted);
        Assert.Single(result.Errors);
    }

    #endregion

    #region GetProjectYearsAsync - empty result

    [Fact]
    public async Task GetProjectYearsAsync_ReturnsEmpty_WhenNoYearsExist()
    {
        var empty = new List<ProjectYear>();
        var emptyDtos = new List<ProjectYearDto>();

        _projectYearRepo.GetByProjectAsync("2024/001").Returns(empty);
        _mapper.Map<IEnumerable<ProjectYearDto>>(empty).Returns(emptyDtos);

        var result = await _sut.GetProjectYearsAsync("2024/001");

        Assert.Empty(result);
    }

    #endregion

    #region GetAdditionalCostsAsync - empty result

    [Fact]
    public async Task GetAdditionalCostsAsync_ReturnsEmpty_WhenNoData()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AdditionalCostDetailView>
        {
            Data = new List<AdditionalCostDetailView>(),
            PaginationData = new PaginationData { TotalRecords = 0 }
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _additionalCostRepo.GetAdditionalCostsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetAdditionalCostsAsync("P1", 1, query);

        Assert.Empty(result.Data);
    }

    #endregion

    #region GetAnimalRequirementsAsync - empty result

    [Fact]
    public async Task GetAnimalRequirementsAsync_ReturnsEmpty_WhenNoData()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<AnimalRequirementDetailView>
        {
            Data = new List<AnimalRequirementDetailView>(),
            PaginationData = new PaginationData { TotalRecords = 0 }
        };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _animalRepo.GetAnimalRequirementsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(Arg.Any<PaginationData>()).Returns(new PaginationDto());

        var result = await _sut.GetAnimalRequirementsAsync("P1", 1, query);

        Assert.Empty(result.Data);
    }

    #endregion

    #region GetPayRatesAsync - isDefra variants

    [Fact]
    public async Task GetPayRatesAsync_ReturnsRates_WhenIsDefraTrue()
    {
        var rates = new List<PayRateLookup> { new() { WgGrade = "SEO", ChargeRate = 55.0m, PayRate = 40.0m, Npr = 6.0m, Ohr = 12.0m } };
        _staffRepo.GetPayRatesAsync("2024/001", 2024, true).Returns(rates);

        var result = (await _sut.GetPayRatesAsync("2024/001", 2024, true)).ToList();

        Assert.Single(result);
        Assert.Equal("SEO", result[0].WgGrade);
        await _staffRepo.Received(1).GetPayRatesAsync("2024/001", 2024, true);
    }

    [Fact]
    public async Task GetPayRatesAsync_ReturnsEmpty_WhenNoRatesExist()
    {
        _staffRepo.GetPayRatesAsync("2024/001", 2024, false).Returns(new List<PayRateLookup>());

        var result = await _sut.GetPayRatesAsync("2024/001", 2024, false);

        Assert.Empty(result);
    }

    #endregion

    #region GetAnimalRatesAsync - isDefra variants

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsRates_WhenIsDefraFalse()
    {
        var rates = new List<AnimalRateLookup> { new() { AnimalType = "DOG", DailyRate = 8.50m } };
        _animalRepo.GetAnimalRatesAsync("2024/001", 2024, false).Returns(rates);

        var result = (await _sut.GetAnimalRatesAsync("2024/001", 2024, false)).ToList();

        Assert.Single(result);
        Assert.Equal("DOG", result[0].AnimalType);
        Assert.Equal(8.50m, result[0].DailyRate);
        await _animalRepo.Received(1).GetAnimalRatesAsync("2024/001", 2024, false);
    }

    [Fact]
    public async Task GetAnimalRatesAsync_ReturnsEmpty_WhenNoRatesExist()
    {
        _animalRepo.GetAnimalRatesAsync("2024/001", 2024, true).Returns(new List<AnimalRateLookup>());

        var result = await _sut.GetAnimalRatesAsync("2024/001", 2024, true);

        Assert.Empty(result);
    }

    #endregion

    #region GetAccountCategoriesAsync - empty result

    [Fact]
    public async Task GetAccountCategoriesAsync_ReturnsEmpty_WhenNoCategories()
    {
        _additionalCostRepo.GetProjectSpecificAccountCategoriesAsync().Returns(new List<AccountCategoryLookup>());

        var result = await _sut.GetAccountCategoriesAsync();

        Assert.Empty(result);
    }

    #endregion

    #region GetTestCodeLookupsAsync - isDefra variants

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsLookups_WhenIsDefraTrue()
    {
        var lookups = new List<TestCodeLookup> { new() { ItemCode = "TC002", ItemDescription = "Virus Screen", UnitPrice = 200m } };
        _testRepo.GetTestCodeLookupsAsync("2024/001", 2024, true).Returns(lookups);

        var result = (await _sut.GetTestCodeLookupsAsync("2024/001", 2024, true)).ToList();

        Assert.Single(result);
        Assert.Equal("TC002", result[0].ItemCode);
        await _testRepo.Received(1).GetTestCodeLookupsAsync("2024/001", 2024, true);
    }

    [Fact]
    public async Task GetTestCodeLookupsAsync_ReturnsEmpty_WhenNoLookups()
    {
        _testRepo.GetTestCodeLookupsAsync("2024/001", 2024, false).Returns(new List<TestCodeLookup>());

        var result = await _sut.GetTestCodeLookupsAsync("2024/001", 2024, false);

        Assert.Empty(result);
    }

    #endregion

    #region GetAllAnimalsAsync - empty result

    [Fact]
    public async Task GetAllAnimalsAsync_ReturnsEmpty_WhenNoAnimals()
    {
        _animalRepo.GetAllAnimalsAsync().Returns(new List<FpsAnimals>());

        var result = await _sut.GetAllAnimalsAsync();

        Assert.Empty(result);
    }

    #endregion

    #region GetStaffRequirementsAsync - empty result

    [Fact]
    public async Task GetStaffRequirementsAsync_ReturnsEmptyData_WhenNoRecords()
    {
        var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
        var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
        var repoResult = new PagedData<StaffRequirementDetailView>
        {
            Data = new List<StaffRequirementDetailView>(),
            PaginationData = new PaginationData { TotalRecords = 0, PageNumber = 1, PageSize = 10, TotalPages = 0 }
        };
        var paginationDto = new PaginationDto { TotalRecords = 0 };

        _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
        _staffRepo.GetStaffRequirementsByProjectYearAsync("P1", 1, filter).Returns(repoResult);
        _mapper.Map<PaginationDto>(repoResult.PaginationData).Returns(paginationDto);

        var result = await _sut.GetStaffRequirementsAsync("P1", 1, query);

        Assert.Empty(result.Data);
        Assert.Equal(0, result.PaginationData.TotalRecords);
    }

    #endregion

    #region UpdateAnimalRequirementAsync - AnimalCost null branch

    [Fact]
    public async Task UpdateAnimalRequirementAsync_AnimalCostIsNull_WhenRepoReturnsNullNumberOfDays()
    {
        var dto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "CAT", Project = "P1", Year = 1, NumberOfDays = 5, NumberOfAnimals = 3, DailyRate = 10, AnimalCost = 150 };
        var entity = new AnimalRequirement { ArIdentity = 1, AnimalType = "CAT" };
        var returned = new AnimalRequirement { ArIdentity = 1, AnimalType = "CAT", NumberOfDays = null, NumberOfAnimals = 3, DailyRate = 10 };

        _mapper.Map<AnimalRequirement>(dto).Returns(entity);
        _animalRepo.UpdateAnimalRequirementAsync(entity).Returns(returned);

        var result = await _sut.UpdateAnimalRequirementAsync(dto);

        Assert.Null(result.AnimalCost);
    }

    #endregion

    #region UpdateTestRequirementAsync - TestCost null branch

    [Fact]
    public async Task UpdateTestRequirementAsync_TestCostIsNull_WhenRepoReturnsNullUnitPrice()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 100, NumberOfTests = 5, TestCost = 500 };
        var entity = new TestRequirement { TestCode = "TC001" };
        var returned = new TestRequirement { TestCode = "TC001", UnitPrice = null, NumberOfTests = 5 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.UpdateTestRequirementAsync(entity).Returns(returned);

        var result = await _sut.UpdateTestRequirementAsync(dto);

        Assert.Null(result.TestCost);
    }

    [Fact]
    public async Task UpdateTestRequirementAsync_TestCostIsCalculated_WhenBothValuesPresent()
    {
        var dto = new TestRequirementDto { TestCode = "TC001", Project = "P1", Year = 1, UnitPrice = 50, NumberOfTests = 4, TestCost = 200 };
        var entity = new TestRequirement { TestCode = "TC001" };
        var returned = new TestRequirement { TestCode = "TC001", UnitPrice = 50, NumberOfTests = 4 };

        _mapper.Map<TestRequirement>(dto).Returns(entity);
        _testRepo.UpdateTestRequirementAsync(entity).Returns(returned);

        var result = await _sut.UpdateTestRequirementAsync(dto);

        Assert.Equal(200.0, result.TestCost); // 50 * 4
    }

    #endregion

    #region UpdateStaffRequirementAsync - StaffCost null branch

    [Fact]
    public async Task UpdateStaffRequirementAsync_StaffCostIsNull_WhenRepoReturnsNullChargerate()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 10, Nodays = 1, Chargerate = 50, StaffCost = 500 };
        var entity = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO" };
        var returned = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO", Chargerate = null, Nohours = 10 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.UpdateStaffRequirementAsync(entity).Returns(returned);

        var result = await _sut.UpdateStaffRequirementAsync(dto);

        Assert.Null(result.StaffCost);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_StaffCostIsCalculated_WhenBothValuesPresent()
    {
        var dto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO", Project = "P1", Year = 1, Name = "Test", Nohours = 8, Nodays = 1, Chargerate = 25, StaffCost = 200 };
        var entity = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO" };
        var returned = new StaffRequirement { SrIdentity = 1, WgGrade = "HEO", Chargerate = 25, Nohours = 8 };

        _mapper.Map<StaffRequirement>(dto).Returns(entity);
        _staffRepo.UpdateStaffRequirementAsync(entity).Returns(returned);

        var result = await _sut.UpdateStaffRequirementAsync(dto);

        Assert.Equal(200.0, result.StaffCost); // 25 * 8
    }

    #endregion
}
