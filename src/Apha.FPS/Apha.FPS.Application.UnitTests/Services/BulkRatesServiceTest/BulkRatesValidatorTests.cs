using Apha.Common.BulkRates.Validation;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkRatesValidator"/> — Phase D3 (fec-bulk-rates-plan-05-
/// differential-remediation.md §4). BulkRatesValidator's own job is orchestration only: build
/// DR-VAL-01's ValidationContext from repository bulk reads and call the real
/// BulkRatesValidationService (not a mock) — these tests exercise that wiring end to end,
/// covering DR-API-01/02/03/04/05/06/08/09. The underlying rule behaviour itself is DR-VAL-01's
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
        IReadOnlyList<AgrupStagingRow>? snapshotAgrup = null)
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
        return repo;
    }

    private static BulkRatesValidator CreateValidator(IBulkRatesRepository repo)
        => new(repo, new BulkRatesValidationService());

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

    // ── DR-API-01: FEC new-row blank rate is still an error; existing-row blank is not ──

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

    // ── DR-API-02: AGRUP existing-row blank rate is Zero-Rate Withdrawal, not Unchanged ──

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

    // ── DR-API-03: new AGRUP row with zero rate is blocked (BC-01 temporary rule) ────

    [Fact]
    public async Task ValidateAgrup_NewRow_WhenAgrupNewIsZero_AddsBlockedError()
    {
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "NEWBUYER", AgrupNew = 0m });

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec)), parse);

        result.Errors.Should().Contain(e => e.ValidationCode == "NEW_AGRUP_ZERO_RATE_BLOCKED");
    }

    // ── DR-API-04: routing-field validation for new AGRUP rows ───────────────

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

    // ── DR-API-05: existing AGRUP routing-field immutability (workflow-scoped) ───────

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
        // a workbook with no routing-field columns must not trip the routing-changed check
        // just because the live row happens to carry routing data this upload never asserted anything about.
        var liveFec = new[] { new FecStagingRow { TestCode = "TC001", UnitPriceVla = 5m, DefraUnitPrice = 5m } };
        var liveAgrup = new[] { new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", Agrup = 12m, ProjectBuyerCode = "PROJA", TestBuyerCode = "TBC1" } };
        var parse = AgrupParse(new AgrupStagingRow { TestCode = "TC001", Buyer = "BUYER1", AgrupNew = 20m }); // no routing fields supplied

        var result = await Validate(CreateValidator(CreateRepo(liveFec: liveFec, liveAgrup: liveAgrup)), parse);

        result.Errors.Should().NotContain(e => e.ValidationCode == "ROUTING_FIELD_CHANGED");
        result.RowCounts.Update.Should().Be(1);
    }

    // ── Downloaded-key preservation (request-level, snapshot-based) ────

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
        result.RowCounts.Total.Should().Be(1); // row-count check (DR-API-08) still runs alongside the key check
    }

    [Fact]
    public async Task ValidateFec_WhenNoDownloadHasHappenedYet_SkipsSnapshotCheck()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = 10m, ItemDescription = "d", ShortDescription = "s", Owner = "o" });

        var result = await Validate(CreateValidator(CreateRepo()), parse, downloadVersion: null);

        result.Errors.Should().NotContain(e => e.ValidationCode == "MISSING_DOWNLOADED_KEY");
    }

    // ── DR-API-09: interim BC-05 safety net (staged AGRUP vs. staged FEC withdrawal) ──

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

    // ── Row counts reflect DR-VAL-01's calculated action, not raw new/existing ───────

    [Fact]
    public async Task ValidateFec_RowCountsReflectCalculatedAction()
    {
        // Unlike the pre-D3 validator, a row only counts toward Insert/Update/Unchanged when
        // DR-VAL-01 actually classified it — an invalid new row with no rate (TC_EXISTING2)
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
}
