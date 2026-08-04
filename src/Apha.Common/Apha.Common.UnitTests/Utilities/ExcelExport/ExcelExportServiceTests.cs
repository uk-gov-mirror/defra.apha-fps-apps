using Apha.Common.Utilities.ExcelExport;
using ClosedXML.Excel;
using FluentAssertions;

namespace Apha.Common.UnitTests.Utilities.ExcelExport;

/// <summary>
/// Unit tests for <see cref="ExcelExportService"/>'s column-protection support
/// (<see cref="ExcelSheetDefinition.ProtectedColumnNames"/>) — existing-row routing cells must
/// render locked/protected while every other cell (and any row a user adds afterward) stays
/// editable, and sheets that never set this property must be completely unaffected.
/// </summary>
public class ExcelExportServiceTests
{
    private sealed class SampleRow
    {
        public string TestCode { get; set; } = string.Empty;
        public decimal? AgrupNew { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
    }

    private readonly ExcelExportService _service = new();

    [Fact]
    public void ExportToExcelMultiSheet_WhenProtectedColumnNamesSet_LocksOnlyThoseColumns_AndProtectsSheet()
    {
        var rows = new[]
        {
            new SampleRow { TestCode = "TC001", AgrupNew = 10m, ProjectBuyerCode = "PRJ1", TestBuyerCode = "TBC1" },
            new SampleRow { TestCode = "TC002", AgrupNew = 20m, ProjectBuyerCode = "PRJ2", TestBuyerCode = "TBC2" }
        };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "AGRUP",
            Data = rows.Cast<object>(),
            DataType = typeof(SampleRow),
            ProtectedColumnNames = [nameof(SampleRow.ProjectBuyerCode), nameof(SampleRow.TestBuyerCode)]
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("AGRUP");

        ws.IsProtected.Should().BeTrue();

        // Column order matches SampleRow's declared property order: TestCode(1), AgrupNew(2),
        // ProjectBuyerCode(3), TestBuyerCode(4).
        for (var row = 2; row <= 3; row++)
        {
            ws.Cell(row, 1).Style.Protection.Locked.Should().BeFalse("TestCode is not a protected column");
            ws.Cell(row, 2).Style.Protection.Locked.Should().BeFalse("AgrupNew is not a protected column");
            ws.Cell(row, 3).Style.Protection.Locked.Should().BeTrue("ProjectBuyerCode is protected");
            ws.Cell(row, 4).Style.Protection.Locked.Should().BeTrue("TestBuyerCode is protected");
        }
    }

    [Fact]
    public void ExportToExcelMultiSheet_WhenProtectedColumnNamesSet_StillAllowsColumnAndRowResize()
    {
        // Regression test: ClosedXML/Excel sheet protection denies every element not explicitly
        // allowed. FormatColumns/FormatRows must be in the allowed set, or a user opening a
        // protected Staff/Animal/FEC/AGRUP template cannot resize columns or rows at all —
        // Excel reports this as if it were a permissions error.
        var rows = new[] { new SampleRow { TestCode = "TC001", ProjectBuyerCode = "PRJ1" } };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "AGRUP",
            Data = rows.Cast<object>(),
            DataType = typeof(SampleRow),
            ProtectedColumnNames = [nameof(SampleRow.ProjectBuyerCode)]
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("AGRUP");

        ws.IsProtected.Should().BeTrue();
        ws.Protection.AllowedElements.Should().HaveFlag(XLSheetProtectionElements.FormatColumns);
        ws.Protection.AllowedElements.Should().HaveFlag(XLSheetProtectionElements.FormatRows);
    }

    [Fact]
    public void ExportToExcelMultiSheet_WhenProtectedColumnNamesNotSet_LeavesSheetUnprotected()
    {
        var rows = new[] { new SampleRow { TestCode = "TC001", AgrupNew = 10m } };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "AGRUP",
            Data = rows.Cast<object>(),
            DataType = typeof(SampleRow)
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("AGRUP");

        ws.IsProtected.Should().BeFalse();
    }

    [Fact]
    public void ExportToExcelMultiSheet_WhenProtectedColumnNamesSetButNoRows_DoesNotProtectSheet()
    {
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "AGRUP",
            Data = Enumerable.Empty<object>(),
            DataType = typeof(SampleRow),
            ProtectedColumnNames = [nameof(SampleRow.ProjectBuyerCode)]
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("AGRUP");

        ws.IsProtected.Should().BeFalse();
    }

    // ── FormulaColumns: live Excel formula instead of a static value ──────────────

    private sealed class ChangeRow
    {
        public string TestCode { get; set; } = string.Empty;
        public decimal? DefraUnitPrice { get; set; }
        public decimal? FecNew { get; set; }
        public decimal? Change { get; set; }
    }

    [Fact]
    public void ExportToExcelMultiSheet_WhenFormulaColumnsSet_WritesFormulaInsteadOfValue()
    {
        var rows = new[]
        {
            new ChangeRow { TestCode = "PT0001", DefraUnitPrice = 100m, FecNew = 120m, Change = 999m }
        };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "FEC",
            Data = rows.Cast<object>(),
            DataType = typeof(ChangeRow),
            FormulaColumns = new Dictionary<string, string>
            {
                [nameof(ChangeRow.Change)] = "IF({FecNew}=\"\",0-{DefraUnitPrice},{FecNew}-{DefraUnitPrice})"
            }
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("FEC");

        // Column order matches ChangeRow's declared property order:
        // TestCode(1), DefraUnitPrice(2), FecNew(3), Change(4).
        ws.Cell(2, 4).FormulaA1.Should().Be("IF(C2=\"\",0-B2,C2-B2)");
        // The stale stored value (999) must never appear as a literal — Excel evaluates the formula.
        ws.Cell(2, 4).CachedValue.ToString().Should().NotBe("999");
    }

    [Fact]
    public void ExportToExcelMultiSheet_WhenFormulaColumnsNotSet_WritesPlainValue()
    {
        var rows = new[] { new ChangeRow { TestCode = "PT0001", Change = 20m } };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "FEC",
            Data = rows.Cast<object>(),
            DataType = typeof(ChangeRow)
        };

        var bytes = _service.ExportToExcelMultiSheet([sheet]);

        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheet("FEC");

        ws.Cell(2, 4).HasFormula.Should().BeFalse();
        ws.Cell(2, 4).GetValue<decimal>().Should().Be(20m);
    }

    [Fact]
    public void ExportToExcelMultiSheet_FormulaColumnTemplateReferencingUnknownProperty_Throws()
    {
        var rows = new[] { new ChangeRow { TestCode = "PT0001" } };
        var sheet = new ExcelSheetDefinition
        {
            SheetName = "FEC",
            Data = rows.Cast<object>(),
            DataType = typeof(ChangeRow),
            FormulaColumns = new Dictionary<string, string> { [nameof(ChangeRow.Change)] = "{NotAProperty}-1" }
        };

        _service.Invoking(s => s.ExportToExcelMultiSheet([sheet]))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*NotAProperty*");
    }
}
