using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkRatesValidator"/>.
/// Covers: duplicate test codes, missing mandatory fields on inserts,
/// negative rates, blank AgrupNew (unchanged), "Same as FEC" structural rule (US-XC-04).
/// </summary>
public class BulkRatesValidatorTests
{
    private const int FpsYear  = 2027;
    private static readonly Guid QueueId = Guid.NewGuid();

    private static BulkRatesValidator CreateValidator(
        IReadOnlySet<string>? existingTestCodes = null,
        IReadOnlySet<(string, string)>? existingAgrupKeys = null)
    {
        var repo = Substitute.For<IBulkRatesRepository>();
        repo.GetExistingTestCodesAsync(
                Arg.Any<IEnumerable<string>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(existingTestCodes ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        repo.GetExistingAgrupKeysAsync(
                Arg.Any<IEnumerable<(string, string)>>(), FpsYear, Arg.Any<CancellationToken>())
            .Returns(existingAgrupKeys ?? new HashSet<(string, string)>());
        return new BulkRatesValidator(repo);
    }

    private static BulkRatesParseResult FecParse(params FecStagingRow[] rows)
        => new()
        {
            JobName   = "BulkTestRatesUpdate",
            JobQueueId = QueueId,
            FecRows    = rows,
            AgrupRows  = []
        };

    private static BulkRatesParseResult AgrupParse(params AgrupStagingRow[] rows)
        => new()
        {
            JobName    = "BulkTestRatesUpdate",
            JobQueueId = QueueId,
            FecRows    = [],
            AgrupRows  = rows
        };

    // ── Duplicate FEC test codes ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenDuplicateTestCode_AddsErrorForEachOccurrence()
    {
        var parse = FecParse(
            new FecStagingRow { TestCode = "TC001", FecNewRate = 10m },
            new FecStagingRow { TestCode = "TC001", FecNewRate = 11m });

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Where(e => e.ValidationCode == "DUPLICATE_TEST_CODE")
            .Should().HaveCount(2);
    }

    // ── Missing FEC New rate ─────────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenFecNewRateNull_AddsError()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = null });

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().Contain(e => e.ValidationCode == "MISSING_FEC_NEW_RATE");
    }

    // ── Negative FEC rate ────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenNegativeRate_AddsError()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = -5m });

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().Contain(e => e.ValidationCode == "NEGATIVE_RATE");
    }

    // ── Zero rate is valid ───────────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_WhenZeroRate_NoNegativeRateError()
    {
        var parse = FecParse(new FecStagingRow { TestCode = "TC001", FecNewRate = 0m });

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().NotContain(e => e.ValidationCode == "NEGATIVE_RATE");
    }

    // ── New FEC row requires description/owner ───────────────────────────────

    [Fact]
    public async Task ValidateFec_NewRow_WhenDescriptionMissing_AddsError()
    {
        var parse = FecParse(new FecStagingRow
        {
            TestCode        = "TC999",  // does not exist → Insert path
            FecNewRate      = 10m,
            ItemDescription = null,
            ShortDescription = "short",
            Owner           = "owner"
        });
        // no existing test codes → it's an insert
        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().Contain(e =>
            e.ValidationCode == "MISSING_FOR_INSERT" && e.FieldName == "itemdescription");
    }

    [Fact]
    public async Task ValidateFec_NewRow_WhenOwnerMissing_AddsError()
    {
        var parse = FecParse(new FecStagingRow
        {
            TestCode         = "TC999",
            FecNewRate       = 10m,
            ItemDescription  = "desc",
            ShortDescription = "short",
            Owner            = null
        });

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().Contain(e =>
            e.ValidationCode == "MISSING_FOR_INSERT" && e.FieldName == "owner");
    }

    // ── Existing FEC row: description change is a warning, not an error ──────

    [Fact]
    public async Task ValidateFec_ExistingRow_WhenDescriptionProvided_AddsWarning()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TC001" };
        var parse = FecParse(new FecStagingRow
        {
            TestCode        = "TC001",
            FecNewRate      = 10m,
            ItemDescription = "some changed description"
        });

        var result = await CreateValidator(existingTestCodes: existing)
            .ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().Contain(e => e.Severity == "Warning" && e.ValidationCode == "IGNORED_ON_UPDATE");
    }

    // ── AGRUP: blank AgrupNew on existing row → unchanged (not an error) ─────

    [Fact]
    public async Task ValidateAgrup_WhenAgrupNewIsNull_CountedAsUnchanged()
    {
        var existingAgrup = new HashSet<(string, string)> { ("TC001", "BUYER1") };
        var parse = AgrupParse(new AgrupStagingRow
        {
            TestCode = "TC001",
            Buyer    = "BUYER1",
            AgrupNew = null   // blank = no update
        });

        var result = await CreateValidator(
                existingAgrupKeys: existingAgrup)
            .ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.RowCounts.Unchanged.Should().Be(1);
        result.Errors.Should().BeEmpty();
    }

    // ── File-level parse errors short-circuit all row validation ─────────────

    [Fact]
    public async Task Validate_WhenParseResultHasErrors_ReturnsFileErrors()
    {
        var parse = new BulkRatesParseResult
        {
            JobName    = "BulkTestRatesUpdate",
            JobQueueId = QueueId,
            ParseErrors = ["Worksheet 'FEC' not found."]
        };

        var result = await CreateValidator().ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.Errors.Should().ContainSingle(e => e.ValidationCode == "FILE_ERROR");
    }

    // ── Row counts are accurate ──────────────────────────────────────────────

    [Fact]
    public async Task ValidateFec_RowCountsReflectInsertUpdateUnchanged()
    {
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TC_EXISTING" };
        var parse = FecParse(
            new FecStagingRow { TestCode = "TC_NEW", FecNewRate = 10m, ItemDescription = "d", ShortDescription = "s", Owner = "o" },
            new FecStagingRow { TestCode = "TC_EXISTING", FecNewRate = 20m },
            new FecStagingRow { TestCode = "TC_EXISTING2", FecNewRate = null }   // error row
        );

        var result = await CreateValidator(existingTestCodes: existing)
            .ValidateAsync(parse, FpsYear, "BulkTestRatesUpdate");

        result.RowCounts.Insert.Should().Be(1);
        result.RowCounts.Update.Should().Be(1);
        result.RowCounts.Total.Should().Be(3);
    }
}
