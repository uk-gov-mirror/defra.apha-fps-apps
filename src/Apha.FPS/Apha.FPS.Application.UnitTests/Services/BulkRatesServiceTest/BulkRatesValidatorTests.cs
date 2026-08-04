using Apha.Common.BulkRates.Validation;
using Apha.Common.BulkRates.Validation.StaffAnimal;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkRatesValidator"/>. BulkRatesValidator's own job is orchestration only: build
/// ValidationContext from repository bulk reads and call the real
/// BulkRatesValidationService (not a mock) — these tests exercise that wiring end to end.
/// The underlying rule behaviour itself is its
/// own test responsibility (Apha.Common.UnitTests.BulkRates.Validation.BulkRatesValidationServiceTests).
/// </summary>
public class BulkRatesValidatorTests
{
    private const int FpsYear = 2027;
    private const string JobName = "BulkTestRatesUpdate";
    private static readonly Guid QueueId = Guid.NewGuid();

    // ── Fixtures ──────────────────────────────────────────────────────────────

    private static IBulkRatesRepository CreateRepo(
        IReadOnlyList<FecStagingRow>? liveFec = null,
        IReadOnlyList<AgrupStagingRow>? liveAgrup = null,
        IReadOnlySet<string>? projectCodes = null,
        IReadOnlySet<(string, string)>? capabilityPairs = null,
        IReadOnlyList<FecStagingRow>? snapshotFec = null,
        IReadOnlyList<AgrupStagingRow>? snapshotAgrup = null,
        IReadOnlyList<StaffStagingRow>? liveStaff = null,
        IReadOnlyList<AnimalStagingRow>? liveAnimal = null,
        IReadOnlyList<StaffStagingRow>? stagedStaff = null,
        IReadOnlyList<AnimalStagingRow>? stagedAnimal = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetFecRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveFec ?? Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveAgrup ?? Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetExistingProjectCodesAsync(Arg.Any<IEnumerable<string>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(projectCodes ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        repo.GetExistingCapabilityPairsAsync(Arg.Any<IEnumerable<(string, string)>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(capabilityPairs ?? new HashSet<(string, string)>());
        repo.GetFecSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(snapshotFec ?? Array.Empty<FecStagingRow>() as IReadOnlyList<FecStagingRow>);
        repo.GetAgrupSnapshotRowsAsync(QueueId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(snapshotAgrup ?? Array.Empty<AgrupStagingRow>() as IReadOnlyList<AgrupStagingRow>);
        repo.GetStaffRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveStaff ?? Array.Empty<StaffStagingRow>() as IReadOnlyList<StaffStagingRow>);
        repo.GetAnimalRowsForExportAsync(FpsYear, Arg.Any<CancellationToken>())
            .Returns(liveAnimal ?? Array.Empty<AnimalStagingRow>() as IReadOnlyList<AnimalStagingRow>);
        repo.GetStaffStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(stagedStaff ?? Array.Empty<StaffStagingRow>() as IReadOnlyList<StaffStagingRow>);
        repo.GetAnimalStagingRowsAsync(QueueId, Arg.Any<CancellationToken>())
            .Returns(stagedAnimal ?? Array.Empty<AnimalStagingRow>() as IReadOnlyList<AnimalStagingRow>);
        return repo;
    }

    private static BulkRatesValidator CreateValidator(IBulkRatesRepository repo)
        => new(repo, new BulkRatesValidationService(), new StaffAnimalValidationService());

    private static BulkRatesParseResult FecParse(params FecStagingRow[] rows)
        => new() { JobName = JobName, JobQueueId = QueueId, FecRows = rows, AgrupRows = [] };

    private static BulkRatesParseResult AgrupParse(params AgrupStagingRow[] rows)
        => new() { JobName = JobName, JobQueueId = QueueId, FecRows = [], AgrupRows = rows };

    private static BulkRatesParseResult MixedParse(FecStagingRow[] fec, AgrupStagingRow[] agrup)
        => new() { JobName = JobName, JobQueueId = QueueId, FecRows = fec, AgrupRows = agrup };

    private static Task<BulkRatesValidationResult> Validate(
        BulkRatesValidator validator, BulkRatesParseResult parse, int? downloadVersion = null)
        => validator.ValidateAsync(parse, FpsYear, JobName, uploadVersion: 1, downloadVersion, CancellationToken.None);

    // ── Duplicate FEC test codes ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenDuplicateTestCode_AddsErrorForEachOccurrence()
    {
        var parse = FecParse(
            new FecStagingRow { TestCode = "TC001", FecNewRate = 10m, ItemDescription = "d", ShortDescription = "s", Owner = "o" },
            new FecStagingRow { TestCode = "TC001", FecNewRate = 11m, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Where(e => e.ValidationCode == "DUPLICATE_TEST_CODE").Should().HaveCount(2);
    }

    // ── FEC new-row blank rate is still an error; existing-row blank is not ──────────

    [Fact]
    public async Task ValidateFec_NewRow_WhenFecNewRateNull_AddsError()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC999", FecNewRate = null, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "MISSING_FEC_NEW_RATE");
    }

    [Fact]
    public async Task ValidateFec_ExistingRow_WhenRateBlank_ClassifiesAsZeroRateWithdrawal_NotAnError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 10m, DefraUnitPrice = 10m } };
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = null });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_FEC_NEW_RATE");
        result.RowCounts.Update.Should().Be(1); // ZeroRateWithdrawal counts as a real write, not Unchanged
    }

    // ── Negative / zero rate ─────────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenNegativeRate_AddsError()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = -5m, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "NEGATIVE_RATE");
    }

    [Fact]
    public async Task ValidateFec_NewRow_WhenZeroRate_NoNegativeRateError_ClassifiesAsInsert()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = 0m, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().NotContain(e => e.ValidationCode == "NEGATIVE_RATE");
        result.RowCounts.Insert.Should().Be(1);
    }

    // ── New FEC row requires description/owner ───────────────────────────────

    [Fact]
    public async Task ValidateFec_NewRow_WhenDescriptionMissing_AddsError()
    {
        var parse = FecParse(new FecStagingRow
        {
            TestCode = "TC999", FecNewRate = 10m,
            ItemDescription = null, ShortDescription = "short", Owner = "owner"
        });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "MISSING_FOR_INSERT" && e.FieldName == "itemdescription");
    }

    [Fact]
    public async Task ValidateFec_NewRow_WhenOwnerMissing_AddsError()
    {
        var parse = FecParse(new FecStagingRow
        {
            TestCode = "TC999", FecNewRate = 10m,
            ItemDescription = "desc", ShortDescription = "short", Owner = null
        });

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "MISSING_FOR_INSERT" && e.FieldName == "owner");
    }

    // ── Existing FEC row: description change is a warning, not an error ──────

    [Fact]
    public async Task ValidateFec_ExistingRow_WhenDescriptionProvided_AddsWarning()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = 10m, ItemDescription = "some changed description" });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.Errors.Should().Contain(e => e.Severity == "Warning" && e.ValidationCode == "IGNORED_ON_UPDATE");
    }

    // ── AGRUP existing-row blank rate is Zero-Rate Withdrawal, not Unchanged ──────────

    [Fact]
    public async Task ValidateAgrup_ExistingRow_WhenAgrupNewIsNull_ClassifiesAsZeroRateWithdrawal()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", Agrup = 12m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", AgrupNew = null });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec, liveAgrup: liveAgrup)), parse);

        result.RowCounts.Update.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    // ── New AGRUP row with zero rate is blocked (BC-01 temporary rule) ────────────────

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenAgrupNewIsZero_AddsBlockedError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 0m });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "NEW_AGRUP_ZERO_RATE_BLOCKED");
    }

    // ── Routing-field validation for new AGRUP rows ───────────────────────────

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenNoRoutingFieldSupplied_AddsMissingRoutingFieldError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 15m });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "MISSING_ROUTING_FIELD");
    }

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenProjectBuyerCodeInvalid_AddsError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 15m, ProjectBuyerCode = "BADPROJ" });
        var repo = CreateRepo(liveFec: liveFec, projectCodes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GOODPROJ" });

        var result = await Validate(CreateValidator(repo), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
    }

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenProjectBuyerCodeValid_NoRoutingError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 15m, ProjectBuyerCode = "GOODPROJ" });
        var repo = CreateRepo(liveFec: liveFec, projectCodes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GOODPROJ" });

        var result = await Validate(CreateValidator(repo), parse);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_ROUTING_FIELD" || e.ValidationCode == "INVALID_PROJECT_BUYER_CODE");
        result.RowCounts.Insert.Should().Be(1);
    }

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenTestBuyerWorkGroupInvalid_AddsError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 15m, TestBuyerWorkGroup = "WG1" });
        var repo = CreateRepo(liveFec: liveFec); // capability pairs empty -> WG1 not recognised

        var result = await Validate(CreateValidator(repo), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "INVALID_TEST_BUYER_WORKGROUP");
    }

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenBothRoutingFieldsPopulatedAndValid_NoError_PendingBC03()
    {
        // BC-03 (can both routing fields be populated) is still open — the shared validator
        // does not block this today, only "at least one" is enforced.
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow
        {
            TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 15m,
            ProjectBuyerCode = "GOODPROJ", TestBuyerWorkGroup = "WG1"
        });
        var repo = CreateRepo(
            liveFec: liveFec,
            projectCodes: new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "GOODPROJ" },
            capabilityPairs: new HashSet<(string, string)> { ("TC001", "WG1") });

        var result = await Validate(CreateValidator(repo), parse);

        result.Errors.Should().BeEmpty();
    }

    // ── Existing AGRUP routing-field immutability (workflow-scoped) ───────────────────

    [Fact]
    public async Task ValidateAgrup_ExistingRow_WhenAssertedProjectBuyerCodeDiffersFromLive_AddsRoutingFieldChangedError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", Agrup = 12m, ProjectBuyerCode = "PROJA" } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", AgrupNew = 12m, ProjectBuyerCode = "PROJB" });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec, liveAgrup: liveAgrup)), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "ROUTING_FIELD_CHANGED" && e.FieldName == "projectbuyercode");
    }

    [Fact]
    public async Task ValidateAgrup_ExistingRow_WhenWorkbookDoesNotAssertRoutingField_EchoesLiveValue_NoFalsePositive()
    {
        // The workbook has no column yet to assert a routing-field value (Phase
        // D5) — an ordinary rate-only re-upload must not trip the routing-field check just because the live
        // row happens to carry routing data this upload never asserted anything about.
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", Agrup = 12m, ProjectBuyerCode = "PROJA", TestBuyerCode = "TBC1" } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", AgrupNew = 20m }); // no routing fields supplied

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec, liveAgrup: liveAgrup)), parse);

        result.Errors.Should().NotContain(e => e.ValidationCode == "ROUTING_FIELD_CHANGED");
        result.RowCounts.Update.Should().Be(1);
    }

    // ── Downloaded-key preservation (request-level, snapshot-based) ──────────────────

    [Fact]
    public async Task ValidateFec_WhenDownloadedKeyMissingFromUpload_AddsRequestLevelError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var snapshotFec = new[]
        {
            new FecStagingRow { TestCode = "TC001", DefraUnitPrice = 5m },
            new FecStagingRow { TestCode = "TC002", DefraUnitPrice = 8m } // downloaded, but not re-uploaded
        };
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = null });
        var repo = CreateRepo(liveFec: liveFec, snapshotFec: snapshotFec);

        var result = await Validate(CreateValidator(repo), parse, downloadVersion: 3);

        var missing = result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY").Which;
        missing.TestCode.Should().Be("TC002");
        missing.SourceRowNumber.Should().Be(0); // request-level: no uploaded row number to attach to
        missing.IsRequestLevel.Should().BeTrue(); // must render as a distinct request-level message, not a per-row error
        result.RowCounts.Total.Should().Be(1); // row-count check still runs alongside the key check
    }

    [Fact]
    public async Task ValidateFec_WhenNoDownloadHasHappenedYet_SkipsSnapshotCheck()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = 10m, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse, downloadVersion: null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY");
    }

    // ── Interim BC-05 safety net (staged AGRUP vs. staged FEC withdrawal) ─────────────

    [Fact]
    public async Task ValidateAgrup_WhenFecWithdrawnInSameUpload_AndAgrupStillPositive_AddsConflictError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", Agrup = 12m } };
        var parse = MixedParse(
            fec: [new FecStagingRow { TestCode = "TC001", FecNewRate = 0m }], // withdrawal
            agrup: [new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", AgrupNew = 12m }]); // still positive

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec, liveAgrup: liveAgrup)), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "AGRUP_POSITIVE_FOR_WITHDRAWN_FEC");
    }

    // ── File-level parse errors short-circuit all row validation ─────────────

    [Fact]
    public async Task Validate_WhenParseResultHasErrors_ReturnsFileErrors()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = JobName,
            JobQueueId = QueueId,
            ParseErrors = ["Worksheet 'FEC' not found."]
        };

        var result = await Validate(CreateValidator(CreateRepo()), parse);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "FILE_ERROR");
    }

    // ── Row counts reflect the calculated action, not raw new/existing ────────────────

    [Fact]
    public async Task ValidateFec_RowCountsReflectCalculatedAction()
    {
        // Unlike the pre-D3 validator, a row only counts toward Insert/Update/Unchanged when
        // the validator actually classified it — an invalid new row with no rate (TC_EXISTING2)
        // gets an error but no calculated action, so it counts toward Total/Invalid only.
        var liveFec = new[] { new FecStagingRow { TestCode = "TC_EXISTING", UnitPriceVla = 20m, DefraUnitPrice = 20m } };
        var parse = FecParse(
            new FecStagingRow { TestCode = "TC_NEW", FecNewRate = 10m, ItemDescription = "d", ShortDescription = "s", Owner = "o" },
            new FecStagingRow { TestCode = "TC_EXISTING", FecNewRate = 20m },  // unchanged: matches live rate
            // Unknown TestCode (so classified new) but otherwise insert-ready, isolating the
            // one deliberate defect (no rate) so Invalid reflects exactly that one error.
            new FecStagingRow { TestCode = "TC_EXISTING2", FecNewRate = null, ItemDescription = "d", ShortDescription = "s", Owner = "o" }
        );

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.RowCounts.Insert.Should().Be(1);
        result.RowCounts.Unchanged.Should().Be(1);
        result.RowCounts.Total.Should().Be(3);
        result.RowCounts.Invalid.Should().Be(1);
    }

    // ── Staff/Animal findings carry a business key for inline grid display ──────────
    //
    // StagingValidationError.TestCode is the generic business-key column FEC/AGRUP use for a
    // literal test code — Staff/Animal reuse the same column to carry PcGrade/AnimalType so the
    // Web UI can attach the finding inline to the matching staging grid row (parity with
    // FEC/AGRUP's own inline validation display). A finding with no usable key (missing
    // grade/animal type) is expected to leave TestCode unset — it has nowhere to attach inline
    // and is correctly left for the page's top-level unmatched list.

    private static Task<BulkRatesValidationResult> ValidateJob(
        BulkRatesValidator validator, BulkRatesParseResult parse, string jobName)
        => validator.ValidateAsync(parse, FpsYear, jobName, uploadVersion: 1, downloadVersion: null, CancellationToken.None);

    [Fact]
    public async Task ValidateStaff_DuplicateGrade_FindingCarriesPcGradeAsTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows =
            [
                new StaffStagingRow { PcGrade = "G1", PayRate = 100m },
                new StaffStagingRow { PcGrade = "G1", PayRate = 110m }
            ]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Where(e => e.ValidationCode == "DUPLICATE_GRADE")
            .Should().OnlyContain(e => e.TestCode == "G1");
    }

    [Fact]
    public async Task ValidateStaff_NegativeRate_FindingCarriesPcGradeAsTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows = [new StaffStagingRow { PcGrade = "G2", PayRate = -5m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.TestCode == "G2");
    }

    [Fact]
    public async Task ValidateStaff_MissingGrade_FindingHasNoTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows = [new StaffStagingRow { PcGrade = "", PayRate = 100m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_GRADE" && string.IsNullOrEmpty(e.TestCode));
    }

    [Fact]
    public async Task ValidateAnimal_DuplicateAnimalType_FindingCarriesAnimalTypeAsTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Animal,
            JobQueueId = QueueId,
            AnimalRows =
            [
                new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 10m },
                new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 11m }
            ]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Animal);

        result.Errors.Where(e => e.ValidationCode == "DUPLICATE_ANIMAL_TYPE")
            .Should().OnlyContain(e => e.TestCode == "Cattle");
    }

    [Fact]
    public async Task ValidateAnimal_NegativeRate_FindingCarriesAnimalTypeAsTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Animal,
            JobQueueId = QueueId,
            AnimalRows = [new AnimalStagingRow { AnimalType = "Sheep", DailyRate = -1m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Animal);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "NEGATIVE_RATE" && e.TestCode == "Sheep");
    }

    [Fact]
    public async Task ValidateAnimal_MissingAnimalType_FindingHasNoTestCode()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Animal,
            JobQueueId = QueueId,
            AnimalRows = [new AnimalStagingRow { AnimalType = "", DailyRate = 10m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Animal);

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "MISSING_ANIMAL_TYPE" && string.IsNullOrEmpty(e.TestCode));
    }

    // ── Live-data-aware Staff/Animal validation ────────────────────────────────────────
    //
    // The old ad hoc validator never checked live data at all — every staged row was
    // blindly counted as Update regardless of whether a live counterpart existed. These tests
    // cover what's new: NotFound detection (hard failure) and row counts
    // that actually reflect NoChange/Update classification.

    [Fact]
    public async Task ValidateStaff_WhenGradeNotFoundLive_AddsBlockingErrorAndCountsAsInvalid()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows = [new StaffStagingRow { PcGrade = "GHOST", PayRate = 50m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Should().ContainSingle(e =>
            e.ValidationCode == "GRADE_NOT_FOUND" && e.Severity == "Error" && e.TestCode == "GHOST");
        result.RowCounts.Invalid.Should().Be(1);
        result.RowCounts.Update.Should().Be(0);
    }

    [Fact]
    public async Task ValidateStaff_WhenLiveMatchesExactly_ClassifiesAsUnchanged_NotUpdate()
    {
        var liveStaff = new[] { new StaffStagingRow { PcGrade = "G1", PayRate = 100m, Npr = 10m, Ohr = 5m } };
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows = [new StaffStagingRow { PcGrade = "G1", PayRate = 100m, Npr = 10m, Ohr = 5m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo(liveStaff: liveStaff)), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Should().BeEmpty();
        result.RowCounts.Unchanged.Should().Be(1);
        result.RowCounts.Update.Should().Be(0);
    }

    [Fact]
    public async Task ValidateStaff_WhenRateDiffersFromLive_ClassifiesAsUpdate()
    {
        var liveStaff = new[] { new StaffStagingRow { PcGrade = "G1", PayRate = 100m, Npr = 10m, Ohr = 5m } };
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Staff,
            JobQueueId = QueueId,
            StaffRows = [new StaffStagingRow { PcGrade = "G1", PayRate = 150m, Npr = 10m, Ohr = 5m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo(liveStaff: liveStaff)), parse, Apha.Common.Constants.BulkRatesJobNames.Staff);

        result.Errors.Should().BeEmpty();
        result.RowCounts.Update.Should().Be(1);
        result.RowCounts.Unchanged.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAnimal_WhenAnimalTypeNotFoundLive_AddsBlockingErrorAndCountsAsInvalid()
    {
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Animal,
            JobQueueId = QueueId,
            AnimalRows = [new AnimalStagingRow { AnimalType = "Dragon", DailyRate = 5m }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo()), parse, Apha.Common.Constants.BulkRatesJobNames.Animal);

        result.Errors.Should().ContainSingle(e =>
            e.ValidationCode == "ANIMAL_TYPE_NOT_FOUND" && e.Severity == "Error" && e.TestCode == "Dragon");
        result.RowCounts.Invalid.Should().Be(1);
        result.RowCounts.Update.Should().Be(0);
    }

    [Fact]
    public async Task ValidateAnimal_WhenLiveMatchesExactly_ClassifiesAsUnchanged()
    {
        var liveAnimal = new[] { new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 8m, Species = "Bovine" } };
        var parse = new BulkRatesParseResult
        {
            JobName = Apha.Common.Constants.BulkRatesJobNames.Animal,
            JobQueueId = QueueId,
            AnimalRows = [new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 8m, Species = "Bovine" }]
        };

        var result = await ValidateJob(CreateValidator(CreateRepo(liveAnimal: liveAnimal)), parse, Apha.Common.Constants.BulkRatesJobNames.Animal);

        result.Errors.Should().BeEmpty();
        result.RowCounts.Unchanged.Should().Be(1);
    }

    // ── BuildStaffFreezeAsync / BuildAnimalFreezeAsync ────────────────────────────────

    [Fact]
    public async Task BuildStaffFreezeAsync_WhenClean_ReturnsFreezeEntryWithSourceAndEffectiveState()
    {
        var liveStaff = new[] { new StaffStagingRow { PcGrade = "G1", PayRate = 100m, Npr = 10m, Ohr = 5m } };
        var stagedStaff = new[] { new StaffStagingRow { PcGrade = "G1", PayRate = 150m, Npr = 10m, Ohr = 5m } };
        var validator = CreateValidator(CreateRepo(liveStaff: liveStaff, stagedStaff: stagedStaff));

        var freeze = await validator.BuildStaffFreezeAsync(QueueId, FpsYear);

        freeze.BlockingErrors.Should().BeEmpty();
        var entry = freeze.Freezes.Should().ContainSingle().Which;
        entry.PcGrade.Should().Be("G1");
        entry.CalculatedAction.Should().Be(StaffAnimalCalculatedAction.Update);
        entry.SourcePayRate.Should().Be(100m);
        entry.EffectivePayRate.Should().Be(150m);
    }

    [Fact]
    public async Task BuildStaffFreezeAsync_WhenGradeDeletedSinceUpload_ReturnsBlockingError()
    {
        // Live drift: the grade existed at upload time but was deleted before release —
        // BuildStaffFreezeAsync re-validates against *current* live data, so this must surface
        // as NotFound here even though it wasn't the case when the row was originally staged.
        var stagedStaff = new[] { new StaffStagingRow { PcGrade = "GONE", PayRate = 100m } };
        var validator = CreateValidator(CreateRepo(stagedStaff: stagedStaff)); // no live rows

        var freeze = await validator.BuildStaffFreezeAsync(QueueId, FpsYear);

        freeze.BlockingErrors.Should().ContainSingle(e => e.ValidationCode == "GRADE_NOT_FOUND");
    }

    [Fact]
    public async Task BuildAnimalFreezeAsync_WhenClean_ReturnsFreezeEntryWithSourceAndEffectiveState()
    {
        var liveAnimal = new[] { new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 8m, Species = "Bovine" } };
        var stagedAnimal = new[] { new AnimalStagingRow { AnimalType = "Cattle", DailyRate = 9m, Species = "Bovine" } };
        var validator = CreateValidator(CreateRepo(liveAnimal: liveAnimal, stagedAnimal: stagedAnimal));

        var freeze = await validator.BuildAnimalFreezeAsync(QueueId, FpsYear);

        freeze.BlockingErrors.Should().BeEmpty();
        var entry = freeze.Freezes.Should().ContainSingle().Which;
        entry.AnimalType.Should().Be("Cattle");
        entry.CalculatedAction.Should().Be(StaffAnimalCalculatedAction.Update);
        entry.SourceDailyRate.Should().Be(8m);
        entry.EffectiveDailyRate.Should().Be(9m);
    }

    [Fact]
    public async Task BuildAnimalFreezeAsync_WhenAnimalTypeDeletedSinceUpload_ReturnsBlockingError()
    {
        var stagedAnimal = new[] { new AnimalStagingRow { AnimalType = "GONE", DailyRate = 5m } };
        var validator = CreateValidator(CreateRepo(stagedAnimal: stagedAnimal)); // no live rows

        var freeze = await validator.BuildAnimalFreezeAsync(QueueId, FpsYear);

        freeze.BlockingErrors.Should().ContainSingle(e => e.ValidationCode == "ANIMAL_TYPE_NOT_FOUND");
    }
}
