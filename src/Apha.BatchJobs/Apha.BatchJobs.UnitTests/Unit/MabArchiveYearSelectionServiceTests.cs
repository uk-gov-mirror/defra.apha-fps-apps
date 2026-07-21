using Apha.BatchJobs.Domain.Exceptions;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Pure unit tests for MabArchiveYearSelectionService.Bucket/Validate
/// (docs/mabarchive-year-selection-processing-spec.md §21).
/// These require no database - "does not reference system date" is verified by
/// inspection (Bucket/Validate take pre-read rows/years as parameters; neither
/// touches DateTime.UtcNow or any clock).
/// </summary>
public sealed class MabArchiveYearSelectionServiceTests
{
    [Fact]
    public void Bucket_WhenOneOpenAndOnePlanned_ShouldSplitCorrectly()
    {
        var rows = new[]
        {
            (FpsYear: 2025, YearStatus: "Closed"),
            (FpsYear: 2026, YearStatus: "Open"),
            (FpsYear: 2027, YearStatus: "Planned"),
        };

        var (openYears, plannedYears) = MabArchiveYearSelectionService.Bucket(rows);

        Assert.Equal(new[] { 2026 }, openYears);
        Assert.Equal(new[] { 2027 }, plannedYears);
    }

    [Fact]
    public void Bucket_IgnoresClosedAndUnknownStatuses()
    {
        var rows = new[]
        {
            (FpsYear: 2024, YearStatus: "Closed"),
            (FpsYear: 2025, YearStatus: "Closed"),
            (FpsYear: 2026, YearStatus: "Open"),
            (FpsYear: 2099, YearStatus: "Bogus"),
        };

        var (openYears, plannedYears) = MabArchiveYearSelectionService.Bucket(rows);

        Assert.Equal(new[] { 2026 }, openYears);
        Assert.Empty(plannedYears);
    }

    [Fact]
    public void Bucket_StatusComparisonIsCaseInsensitive()
    {
        var rows = new[]
        {
            (FpsYear: 2026, YearStatus: "OPEN"),
            (FpsYear: 2027, YearStatus: "planned"),
        };

        var (openYears, plannedYears) = MabArchiveYearSelectionService.Bucket(rows);

        Assert.Equal(new[] { 2026 }, openYears);
        Assert.Equal(new[] { 2027 }, plannedYears);
    }

    [Fact]
    public void Validate_WithOneOpenAndOnePlanned_ShouldReturnBoth()
    {
        var result = MabArchiveYearSelectionService.Validate(
            openYears: new[] { 2026 },
            plannedYears: new[] { 2027 });

        Assert.Equal(2026, result.OpenYear);
        Assert.Equal(2027, result.PlannedYear);
    }

    [Fact]
    public void Validate_WithOneOpenAndNoPlanned_ShouldReturnNullPlannedYear()
    {
        var result = MabArchiveYearSelectionService.Validate(
            openYears: new[] { 2026 },
            plannedYears: Array.Empty<int>());

        Assert.Equal(2026, result.OpenYear);
        Assert.Null(result.PlannedYear);
    }

    [Fact]
    public void Validate_WithNoOpenYear_ShouldThrow()
    {
        var ex = Assert.Throws<MabArchiveYearConfigurationException>(
            () => MabArchiveYearSelectionService.Validate(Array.Empty<int>(), Array.Empty<int>()));

        Assert.Contains("exactly one Open", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithMultipleOpenYears_ShouldThrow()
    {
        var ex = Assert.Throws<MabArchiveYearConfigurationException>(
            () => MabArchiveYearSelectionService.Validate(new[] { 2025, 2026 }, Array.Empty<int>()));

        Assert.Contains("exactly one Open", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithMultiplePlannedYears_ShouldThrow()
    {
        var ex = Assert.Throws<MabArchiveYearConfigurationException>(
            () => MabArchiveYearSelectionService.Validate(new[] { 2026 }, new[] { 2027, 2028 }));

        Assert.Contains("zero or one Planned", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WithNonSequentialPlannedYear_ShouldThrow()
    {
        var ex = Assert.Throws<MabArchiveYearConfigurationException>(
            () => MabArchiveYearSelectionService.Validate(new[] { 2026 }, new[] { 2028 }));

        Assert.Contains("Expected 2027", ex.Message, StringComparison.Ordinal);
    }
}
