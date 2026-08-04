using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Validation;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Application.Services;

public class YearlyDetailsService : IYearlyDetailsService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectYearRepository _projectYearRepo;
    private readonly IStaffRequirementRepository _staffRepo;
    private readonly ITestRequirementRepository _testRepo;
    private readonly IAnimalRequirementRepository _animalRepo;
    private readonly IAdditionalCostRepository _additionalCostRepo;
    private readonly ISettingsService _settingsService;
    private readonly IMapper _mapper;

    public YearlyDetailsService(
        IProjectRepository projectRepo,
        IProjectYearRepository projectYearRepo,
        IStaffRequirementRepository staffRepo,
        ITestRequirementRepository testRepo,
        IAnimalRequirementRepository animalRepo,
        IAdditionalCostRepository additionalCostRepo,
        ISettingsService settingsService,
        IMapper mapper)
    {
        _projectRepo = projectRepo;
        _projectYearRepo = projectYearRepo;
        _staffRepo = staffRepo;
        _testRepo = testRepo;
        _animalRepo = animalRepo;
        _additionalCostRepo = additionalCostRepo;
        _settingsService = settingsService;
        _mapper = mapper;
    }

    public async Task<ProjectHeaderDto?> GetProjectHeaderAsync(string projectId)
    {
        var project = await _projectRepo.GetProjectByIdAsync(projectId);
        return project is null ? null : _mapper.Map<ProjectHeaderDto>(project);
    }

    public async Task<IEnumerable<ProjectYearDto>> GetProjectYearsAsync(string projectId)
    {
        var years = await _projectYearRepo.GetByProjectAsync(projectId);
        return _mapper.Map<IEnumerable<ProjectYearDto>>(years);
    }

    public async Task<ProjectYearDto> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto)
    {
        var entity = _mapper.Map<ProjectYear>(dto);
        var added = await _projectYearRepo.AddProjectYearAsync(projectId, year, entity);
        return _mapper.Map<ProjectYearDto>(added);
    }

    public async Task<ProjectYearDto> UpdateProjectYearAsync(ProjectYearDto dto)
    {
        var entity = _mapper.Map<ProjectYear>(dto);
        var updated = await _projectYearRepo.UpdateProjectYearAsync(entity);
        return _mapper.Map<ProjectYearDto>(updated);
    }

    public async Task<(bool Deleted, IReadOnlyList<string> Errors)> DeleteProjectYearAsync(string projectId, int year)
        => await _projectYearRepo.DeleteProjectYearAsync(projectId, year);

    // ── Staff ─────────────────────────────────────────────────────────────────────

    public async Task<PaginatedResult<StaffRequirementDto>> GetStaffRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
        PagedData<StaffRequirementDetailView> result = await _staffRepo.GetStaffRequirementsByProjectYearAsync(projectId, year, filter);

        var dtos = result.Data.Select(r => new StaffRequirementDto
        {
            SrIdentity   = r.SrIdentity,
            Project      = r.Project,
            Year         = r.Year,
            WgGrade      = r.WgGrade,
            Name         = r.Name,
            Nohours      = r.Nohours,
            Nodays       = r.Nodays,
            Chargerate   = r.Chargerate,
            StaffCost    = r.Chargerate.HasValue && r.Nohours.HasValue
                           ? r.Chargerate.Value * r.Nohours.Value
                           : null,
            Payrate      = r.Payrate,
            Npr          = r.Npr,
            Ohr          = r.Ohr,
            WorkGroup    = r.WorkGroup,
            GradeCode    = r.GradeCode,
            Programme    = r.Programme,
            EuroConvRate = r.EuroConvRate,
            EuGrade      = r.EuGrade
        });

        return new PaginatedResult<StaffRequirementDto>(dtos, _mapper.Map<PaginationDto>(result.PaginationData));
    }


    public async Task<StaffRequirementDto> AddStaffRequirementAsync(StaffRequirementDto dto)
    {
        ValidateStaffRequirement(dto);
        var entity = _mapper.Map<StaffRequirement>(dto);
        var result = await _staffRepo.AddStaffRequirementAsync(entity);
        return MapStaffToDto(result);
    }

    public async Task<StaffRequirementDto> UpdateStaffRequirementAsync(StaffRequirementDto dto)
    {
        ValidateStaffRequirement(dto);
        var entity = _mapper.Map<StaffRequirement>(dto);
        var result = await _staffRepo.UpdateStaffRequirementAsync(entity);
        return MapStaffToDto(result);
    }

    public async Task<bool> DeleteStaffRequirementAsync(int srIdentity)
        => await _staffRepo.DeleteStaffRequirementAsync(srIdentity);

    // ── Tests ────────────────────────────────────────────────────────────────

    public async Task<PaginatedResult<TestRequirementDto>> GetTestRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
        PagedData<TestRequirementDetailView> result = await _testRepo.GetTestRequirementsByProjectYearAsync(projectId, year, filter);

        var dtos = result.Data.Select(r => new TestRequirementDto
        {
            Project         = r.Project,
            Year            = r.Year,
            TestCode        = r.TestCode,
            NumberOfTests   = r.NumberOfTests,
            UnitPrice       = r.UnitPrice,
            TestCost        = r.TestCost,
            TestDescription = r.TestDescription
        });

        return new PaginatedResult<TestRequirementDto>(dtos, _mapper.Map<PaginationDto>(result.PaginationData));
    }

    public async Task<TestRequirementDto> AddTestRequirementAsync(TestRequirementDto dto)
    {
        ValidateTestRequirement(dto);
        var entity = _mapper.Map<TestRequirement>(dto);
        var result = await _testRepo.AddTestRequirementAsync(entity);
        return MapTestToDto(result);
    }

    public async Task<TestRequirementDto> UpdateTestRequirementAsync(TestRequirementDto dto)
    {
        ValidateTestRequirement(dto);
        var entity = _mapper.Map<TestRequirement>(dto);
        var result = await _testRepo.UpdateTestRequirementAsync(entity);
        return MapTestToDto(result);
    }

    public async Task<bool> DeleteTestRequirementAsync(string projectId, int year, string testCode)
        => await _testRepo.DeleteTestRequirementAsync(projectId, year, testCode);

    // ── Animals ──────────────────────────────────────────────────────────────

    public async Task<PaginatedResult<AnimalRequirementDto>> GetAnimalRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
        PagedData<AnimalRequirementDetailView> result = await _animalRepo.GetAnimalRequirementsByProjectYearAsync(projectId, year, filter);

        var dtos = result.Data.Select(r => new AnimalRequirementDto
        {
            ArIdentity      = r.ArIdentity,
            Project         = r.Project,
            Year            = r.Year,
            AnimalType      = r.AnimalType,
            NumberOfDays    = r.NumberOfDays,
            NumberOfAnimals = r.NumberOfAnimals,
            DailyRate       = r.DailyRate,
            AnimalCost      = r.AnimalCost
        });

        return new PaginatedResult<AnimalRequirementDto>(dtos, _mapper.Map<PaginationDto>(result.PaginationData));
    }

    public async Task<AnimalRequirementDto> AddAnimalRequirementAsync(AnimalRequirementDto dto)
    {
        ValidateAnimalRequirement(dto);
        var entity = _mapper.Map<AnimalRequirement>(dto);
        var result = await _animalRepo.AddAnimalRequirementAsync(entity);
        return MapAnimalToDto(result);
    }

    public async Task<AnimalRequirementDto> UpdateAnimalRequirementAsync(AnimalRequirementDto dto)
    {
        ValidateAnimalRequirement(dto);
        var entity = _mapper.Map<AnimalRequirement>(dto);
        var result = await _animalRepo.UpdateAnimalRequirementAsync(entity);
        return MapAnimalToDto(result);
    }

    public async Task<bool> DeleteAnimalRequirementAsync(int arIdentity)
        => await _animalRepo.DeleteAnimalRequirementAsync(arIdentity);

    // ── Additional Costs ─────────────────────────────────────────────────────

    public async Task<PaginatedResult<AdditionalCostDto>> GetAdditionalCostsAsync(
        string projectId, int year, QueryParameters<string> query)
    {
        PaginationParameters<string> filter = _mapper.Map<PaginationParameters<string>>(query);
        PagedData<AdditionalCostDetailView> result = await _additionalCostRepo.GetAdditionalCostsByProjectYearAsync(projectId, year, filter);

        var dtos = result.Data.Select(r => new AdditionalCostDto
        {
            AcIdentity  = r.AcIdentity,
            Project     = r.Project,
            Year        = r.Year,
            AccountCat  = r.AccountCat,
            Description = r.Description,
            ItemCost    = r.ItemCost,
            CostEntered = r.CostEntered,
            Freq        = r.Freq
        });

        return new PaginatedResult<AdditionalCostDto>(dtos, _mapper.Map<PaginationDto>(result.PaginationData));
    }

    public async Task<AdditionalCostDto> AddAdditionalCostAsync(AdditionalCostDto dto)
    {
        ValidateAdditionalCost(dto);
        var entity = _mapper.Map<AdditionalCost>(dto);
        var result = await _additionalCostRepo.AddAdditionalCostAsync(entity);
        return _mapper.Map<AdditionalCostDto>(result);
    }

    public async Task<AdditionalCostDto> UpdateAdditionalCostAsync(AdditionalCostDto dto)
    {
        ValidateAdditionalCost(dto);
        var entity = _mapper.Map<AdditionalCost>(dto);
        var result = await _additionalCostRepo.UpdateAdditionalCostAsync(entity);
        return _mapper.Map<AdditionalCostDto>(result);
    }

    public async Task<bool> DeleteAdditionalCostAsync(int acIdentity)
        => await _additionalCostRepo.DeleteAdditionalCostAsync(acIdentity);

    // ── Lookups ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PayRateDto>> GetPayRatesAsync(string projectId, int year, bool isDefra)
    {
        var rates = await _staffRepo.GetPayRatesAsync(projectId, year, isDefra);
        return rates.Select(r => new PayRateDto
        {
            WgGrade = r.WgGrade,
            ChargeRate = r.ChargeRate,
            PayRate = r.PayRate,
            Npr = r.Npr,
            Ohr = r.Ohr,
            ChargeRateWithInflamation = r.ChargeRateWithInflamation
        });
    }

    public async Task<IEnumerable<AnimalRateDto>> GetAnimalRatesAsync(string projectId, int year, bool isDefra)
    {
        var rates = await _animalRepo.GetAnimalRatesAsync(projectId, year, isDefra);
        return rates.Select(r => new AnimalRateDto { AnimalType = r.AnimalType, DailyRate = r.DailyRate, DailyRateWithInflamation = r.DailyRateWithInflamation });
    }

    public async Task<IEnumerable<AccountCategoryDto>> GetAccountCategoriesAsync()
    {
        var cats = await _additionalCostRepo.GetProjectSpecificAccountCategoriesAsync();
        return cats.Select(c => new AccountCategoryDto { AccShortName = c.AccShortName, UseInflation = c.UseInflation });
    }

    public async Task<IEnumerable<TestCodeLookupDto>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra)
    {
        var lookups = await _testRepo.GetTestCodeLookupsAsync(projectId, year, isDefra);
        return lookups.Select(t => new TestCodeLookupDto
        {
            ItemCode = t.ItemCode,
            ItemDescription = t.ItemDescription,
            UnitPrice = t.UnitPrice,
            UnitPriceWithInflamation = t.UnitPriceWithInflamation
        });
    }

    public async Task<IEnumerable<AnimalLookupDto>> GetAllAnimalsAsync()
    {
        var animals = await _animalRepo.GetAllAnimalsAsync();
        return animals.Select(a => new AnimalLookupDto
        {
            AnimalType = a.AnimalType,
            Species = a.Species,
            SecurityLevel = a.SecurityLevel,
            DailyRate = a.DailyRate,
            PlanByWeek = a.PlanByWeek,
            DefraDailyRate = a.DefraDailyRate
        });
    }

    public async Task<string> GetAdditionalCostinflamationAsync(string projectId, int year)
    {
        var currentYearValue = await _settingsService.GetSettingValueByIdAsync("CurrentYear");
        var currentYear = int.TryParse(currentYearValue, out var parsedCurrentYear) ? parsedCurrentYear : 2025;

        var inflationFactor = await _projectRepo.GetInflationFactorAsync("InflationExceptional", projectId, year, currentYear);
        return inflationFactor.ToString(CultureInfo.InvariantCulture);
    }

    // ── Private helpers

    private static void ValidateStaffRequirement(StaffRequirementDto dto)
    {
        var errors = new List<BusinessValidationError>();
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var field = result.MemberNames.FirstOrDefault() ?? "Unknown";
                errors.Add(new BusinessValidationError(
                    result.ErrorMessage ?? $"{field} is invalid.",
                    $"STAFF_{field.ToUpperInvariant()}_REQUIRED"));
            }
        }

        if (string.IsNullOrWhiteSpace(dto.WgGrade))
        {
            errors.Add(new BusinessValidationError(
                "Work Group Grade is required.",
                "STAFF_WGGRADE_REQUIRED"));
        }

        if (dto.Nohours.HasValue && dto.Nohours.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Number of hours cannot be negative.",
                "STAFF_NOHOURS_INVALID"));
        }

        if (dto.Chargerate.HasValue && dto.Chargerate.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Charge rate cannot be negative.",
                "STAFF_CHARGERATE_INVALID"));
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationErrorException(errors);
        }
    }

    private static void ValidateTestRequirement(TestRequirementDto dto)
    {
        var errors = new List<BusinessValidationError>();
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var field = result.MemberNames.FirstOrDefault() ?? "Unknown";
                errors.Add(new BusinessValidationError(
                    result.ErrorMessage ?? $"{field} is invalid.",
                    $"TEST_{field.ToUpperInvariant()}_REQUIRED"));
            }
        }

        if (string.IsNullOrWhiteSpace(dto.TestCode))
        {
            errors.Add(new BusinessValidationError(
                "Test Code is required.",
                "TEST_TESTCODE_REQUIRED"));
        }

        if (dto.NumberOfTests.HasValue && dto.NumberOfTests.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Number of Tests cannot be negative.",
                "TEST_NUMBEROFTESTS_INVALID"));
        }

        if (dto.UnitPrice.HasValue && dto.UnitPrice.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Unit Price cannot be negative.",
                "TEST_UNITPRICE_INVALID"));
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationErrorException(errors);
        }
    }

    private static void ValidateAnimalRequirement(AnimalRequirementDto dto)
    {
        var errors = new List<BusinessValidationError>();
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var field = result.MemberNames.FirstOrDefault() ?? "Unknown";
                errors.Add(new BusinessValidationError(
                    result.ErrorMessage ?? $"{field} is invalid.",
                    $"ANIMAL_{field.ToUpperInvariant()}_REQUIRED"));
            }
        }

        if (string.IsNullOrWhiteSpace(dto.AnimalType))
        {
            errors.Add(new BusinessValidationError(
                "Animal Type is required.",
                "ANIMAL_ANIMALTYPE_REQUIRED"));
        }

        if (dto.NumberOfAnimals.HasValue && dto.NumberOfAnimals.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Number of Animals cannot be negative.",
                "ANIMAL_NUMBEROFANIMALS_INVALID"));
        }

        if (dto.NumberOfDays.HasValue && dto.NumberOfDays.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Number of Days cannot be negative.",
                "ANIMAL_NUMBEROFDAYS_INVALID"));
        }

        if (dto.DailyRate.HasValue && dto.DailyRate.Value < 0)
        {
            errors.Add(new BusinessValidationError(
                "Daily Rate cannot be negative.",
                "ANIMAL_DAILYRATE_INVALID"));
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationErrorException(errors);
        }
    }

    private static void ValidateAdditionalCost(AdditionalCostDto dto)
    {
        var errors = new List<BusinessValidationError>();
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(dto, context, results, validateAllProperties: true))
        {
            foreach (var result in results)
            {
                var field = result.MemberNames.FirstOrDefault() ?? "Unknown";
                errors.Add(new BusinessValidationError(
                    result.ErrorMessage ?? $"{field} is invalid.",
                    $"ADDITIONALCOST_{field.ToUpperInvariant()}_REQUIRED"));
            }
        }

        if (string.IsNullOrWhiteSpace(dto.AccountCat))
        {
            errors.Add(new BusinessValidationError(
                "Account Category is required.",
                "ADDITIONALCOST_ACCOUNTCAT_REQUIRED"));
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            errors.Add(new BusinessValidationError(
                "Description is required.",
                "ADDITIONALCOST_DESCRIPTION_REQUIRED"));
        }

        if (dto.CostEntered < 0)
        {
            errors.Add(new BusinessValidationError(
                "Cost cannot be negative.",
                "ADDITIONALCOST_COSTENTERED_INVALID"));
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationErrorException(errors);
        }
    }

    private static StaffRequirementDto MapStaffToDto(StaffRequirement r) => new()
    {
        SrIdentity = r.SrIdentity,
        Project = r.Project,
        Year = r.Year,
        WgGrade = r.WgGrade,
        Name = r.Name,
        Nohours = r.Nohours,
        Nodays = r.Nodays,
        Chargerate = r.Chargerate,
        StaffCost = r.Chargerate.HasValue && r.Nohours.HasValue
                      ? r.Chargerate.Value * r.Nohours.Value
                      : null,
        Payrate = r.Payrate,
        Npr = r.Npr,
        Ohr = r.Ohr
    };
    private static TestRequirementDto MapTestToDto(TestRequirement r) => new()
    {
        Project = r.Project,
        Year = r.Year,
        TestCode = r.TestCode,
        NumberOfTests = r.NumberOfTests,
        UnitPrice = r.UnitPrice,
        TestCost = r.UnitPrice.HasValue && r.NumberOfTests.HasValue ? r.UnitPrice.Value * r.NumberOfTests.Value : null
    };

    private static AnimalRequirementDto MapAnimalToDto(AnimalRequirement r) => new()
    {
        ArIdentity = r.ArIdentity,
        Project = r.Project,
        Year = r.Year,
        AnimalType = r.AnimalType,
        NumberOfDays = r.NumberOfDays,
        NumberOfAnimals = r.NumberOfAnimals,
        DailyRate = r.DailyRate,
        AnimalCost = r.NumberOfDays.HasValue && r.NumberOfAnimals.HasValue && r.DailyRate.HasValue
            ? r.NumberOfDays.Value * r.NumberOfAnimals.Value * r.DailyRate.Value
            : null
    };
}
