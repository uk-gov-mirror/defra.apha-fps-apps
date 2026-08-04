using Apha.FPS.Application.Services;
using FluentAssertions;
using ClosedXML.Excel;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest;

/// <summary>
/// Unit tests for <see cref="BulkRatesExcelParser"/>.
/// Covers: correct decimal parsing (currency values), missing worksheets,
/// unrecognised job name, non-xlsx extension, and column header validation (US-XC-04).
/// </summary>
public class BulkRatesExcelParserTests
{
    private static readonly Guid QueueId = Guid.NewGuid();
    private readonly BulkRatesExcelParser _parser = new();

    // ── Helper to build .xlsx bytes ──────────────────────────────────────────

    private static byte[] BuildWorkbook(Action<XLWorkbook> configure)
    {
        using var wb = new XLWorkbook();
        configure(wb);
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static IXLWorksheet AddFecSheet(XLWorkbook wb)
    {
        var ws = wb.Worksheets.Add("FEC");
        ws.Cell(1, 1).Value = "TestCode";
        ws.Cell(1, 2).Value = "Unit Price VLA";
        ws.Cell(1, 3).Value = "Defra Unit Price";
        ws.Cell(1, 4).Value = "FEC New";
        ws.Cell(1, 5).Value = "Change";
        ws.Cell(1, 6).Value = "Item Description";
        ws.Cell(1, 7).Value = "Short Description";
        ws.Cell(1, 8).Value = "Owner";
        ws.Cell(1, 9).Value = "Comments";
        return ws;
    }

    private static IXLWorksheet AddAgrupSheet(XLWorkbook wb)
    {
        var ws = wb.Worksheets.Add("AGRUP");
        ws.Cell(1, 1).Value = "Test Code";
        ws.Cell(1, 2).Value = "Buyer";
        ws.Cell(1, 3).Value = "Agrup";
        ws.Cell(1, 4).Value = "Agrup New";
        ws.Cell(1, 5).Value = "Change";
        ws.Cell(1, 6).Value = "No Required";
        ws.Cell(1, 7).Value = "Date Created";
        ws.Cell(1, 8).Value = "Active";
        ws.Cell(1, 9).Value = "Comments";
        ws.Cell(1, 10).Value = "Project Buyer Code";
        ws.Cell(1, 11).Value = "Test Buyer Code";
        ws.Cell(1, 12).Value = "Test Buyer Work Group";
        return ws;
    }

    // ── Non-.xlsx extension ──────────────────────────────────────────────────

    [Fact]
    public void Parse_WhenFileIsNotXlsx_ReturnsParseError()
    {
        var result = _parser.Parse([0x50, 0x4B], "upload.csv", "BulkTestRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeTrue();
        result.ParseErrors.Should().ContainSingle(e => e.Contains(".xlsx"));
    }

    // ── Missing FEC worksheet ────────────────────────────────────────────────

    [Fact]
    public void Parse_WhenFecWorksheetMissing_ReturnsParseError()
    {
        var bytes = BuildWorkbook(wb => wb.Worksheets.Add("AGRUP"));

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeTrue();
        result.ParseErrors.Should().Contain(e => e.Contains("FEC"));
    }

    // ── Missing AGRUP worksheet ──────────────────────────────────────────────

    [Fact]
    public void Parse_WhenAgrupWorksheetMissing_ReturnsParseError()
    {
        var bytes = BuildWorkbook(wb => wb.Worksheets.Add("FEC"));

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeTrue();
        result.ParseErrors.Should().Contain(e => e.Contains("AGRUP"));
    }

    // ── Correct decimal parsing ──────────────────────────────────────────────

    [Fact]
    public void Parse_FecSheet_ParsesDecimalMoneyCorrectly()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var fec = AddFecSheet(wb);
            fec.Cell(2, 1).Value = "TC001";
            fec.Cell(2, 2).Value = 123.45;   // Unit Price VLA
            fec.Cell(2, 3).Value = 123.45;   // Defra Unit Price
            fec.Cell(2, 4).Value = 200.75;   // FEC New
            AddAgrupSheet(wb);
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeFalse();
        result.FecRows.Should().ContainSingle();
        var row = result.FecRows[0];
        row.TestCode.Should().Be("TC001");
        row.FecNewRate.Should().Be(200.75m);
        row.UnitPriceVla.Should().Be(123.45m);
        row.DefraUnitPrice.Should().Be(123.45m);
    }

    [Fact]
    public void Parse_FecSheet_AcceptsZeroRate()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var fec = AddFecSheet(wb);
            fec.Cell(2, 1).Value = "TC002";
            fec.Cell(2, 4).Value = 0.0;    // Zero is a valid rate (spec §2.2, A-11)
            AddAgrupSheet(wb);
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.FecRows.Should().ContainSingle(r => r.FecNewRate == 0m);
    }

    [Fact]
    public void Parse_FecSheet_WhenFecNewIsBlank_ReturnsNull()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var fec = AddFecSheet(wb);
            fec.Cell(2, 1).Value = "TC003";
            // FEC New column left blank
            AddAgrupSheet(wb);
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.FecRows.Should().ContainSingle(r => r.FecNewRate == null);
    }

    // ── AGRUP decimal parsing ────────────────────────────────────────────────

    [Fact]
    public void Parse_AgrupSheet_ParsesDecimalRatesCorrectly()
    {
        var bytes = BuildWorkbook(wb =>
        {
            AddFecSheet(wb);
            var agrup = AddAgrupSheet(wb);
            agrup.Cell(2, 1).Value = "TC001";
            agrup.Cell(2, 2).Value = "VET";
            agrup.Cell(2, 3).Value = 50.00;   // Agrup (current)
            agrup.Cell(2, 4).Value = 55.50;   // Agrup New
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.AgrupRows.Should().ContainSingle();
        var row = result.AgrupRows[0];
        row.TestCode.Should().Be("TC001");
        row.Buyer.Should().Be("VET");
        row.Agrup.Should().Be(50.00m);
        row.AgrupNew.Should().Be(55.50m);
    }

    [Fact]
    public void Parse_AgrupSheet_WhenAgrupNewIsBlank_ReturnsNull()
    {
        var bytes = BuildWorkbook(wb =>
        {
            AddFecSheet(wb);
            var agrup = AddAgrupSheet(wb);
            agrup.Cell(2, 1).Value = "TC001";
            agrup.Cell(2, 2).Value = "VET";
            // Agrup New blank → null (unchanged row)
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.AgrupRows.Should().ContainSingle(r => r.AgrupNew == null);
    }

    // ── AGRUP routing columns ──────────────────────────────────────────────────

    [Fact]
    public void Parse_AgrupSheet_ParsesRoutingColumns()
    {
        var bytes = BuildWorkbook(wb =>
        {
            AddFecSheet(wb);
            var agrup = AddAgrupSheet(wb);
            agrup.Cell(2, 1).Value = "TC001";
            agrup.Cell(2, 2).Value = "NEWBUYER";
            agrup.Cell(2, 4).Value = 55.50;
            agrup.Cell(2, 10).Value = "PRJ001";  // Project Buyer Code
            agrup.Cell(2, 11).Value = "TBC001";  // Test Buyer Code
            agrup.Cell(2, 12).Value = "WG01";    // Test Buyer Work Group
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeFalse();
        var row = result.AgrupRows.Should().ContainSingle().Which;
        row.ProjectBuyerCode.Should().Be("PRJ001");
        row.TestBuyerCode.Should().Be("TBC001");
        row.TestBuyerWorkGroup.Should().Be("WG01");
    }

    [Fact]
    public void Parse_AgrupSheet_WhenRoutingColumnsBlank_ReturnsNull()
    {
        var bytes = BuildWorkbook(wb =>
        {
            AddFecSheet(wb);
            var agrup = AddAgrupSheet(wb);
            agrup.Cell(2, 1).Value = "TC001";
            agrup.Cell(2, 2).Value = "VET";
            agrup.Cell(2, 4).Value = 55.50;
            // Routing columns left blank — existing-row re-upload with no routing change.
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        var row = result.AgrupRows.Should().ContainSingle().Which;
        row.ProjectBuyerCode.Should().BeNull();
        row.TestBuyerCode.Should().BeNull();
        row.TestBuyerWorkGroup.Should().BeNull();
    }

    // ── Staff worksheet ──────────────────────────────────────────────────────

    [Fact]
    public void Parse_StaffSheet_ParsesRateColumnsCorrectly()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var ws = wb.Worksheets.Add("Staff");
            ws.Cell(1, 1).Value = "PcGrade";
            ws.Cell(1, 2).Value = "Pay Rate";
            ws.Cell(1, 3).Value = "NPR";
            ws.Cell(1, 4).Value = "OHR";
            ws.Cell(2, 1).Value = "G1";
            ws.Cell(2, 2).Value = 40000.00;
            ws.Cell(2, 3).Value = 1500.50;
            ws.Cell(2, 4).Value = 800.25;
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkStaffRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeFalse();
        result.StaffRows.Should().ContainSingle();
        var row = result.StaffRows[0];
        row.PcGrade.Should().Be("G1");
        row.PayRate.Should().Be(40000.00m);
        row.Npr.Should().Be(1500.50m);
        row.Ohr.Should().Be(800.25m);
    }

    // ── Animal worksheet ─────────────────────────────────────────────────────

    [Fact]
    public void Parse_AnimalSheet_ParsesDailyRateCorrectly()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var ws = wb.Worksheets.Add("Animals");
            ws.Cell(1, 1).Value = "AnimalType";
            ws.Cell(1, 2).Value = "Species";
            ws.Cell(1, 3).Value = "Security Level";
            ws.Cell(1, 4).Value = "Daily Rate";
            ws.Cell(1, 5).Value = "Defra Daily Rate";
            ws.Cell(1, 6).Value = "Plan By Week";
            ws.Cell(2, 1).Value = "BOVINE";
            ws.Cell(2, 2).Value = "Cattle";
            ws.Cell(2, 3).Value = "Low";
            ws.Cell(2, 4).Value = 25.75;
            ws.Cell(2, 5).Value = 25.75;
            ws.Cell(2, 6).Value = false;
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkAnimalRatesUpdate", QueueId);

        result.HasParseErrors.Should().BeFalse();
        result.AnimalRows.Should().ContainSingle();
        var row = result.AnimalRows[0];
        row.DailyRate.Should().Be(25.75m);
        row.DefraDailyRate.Should().Be(25.75m);
    }

    // ── Unrecognised job name ─────────────────────────────────────────────────

    [Fact]
    public void Parse_WhenJobNameUnknown_ReturnsParseError()
    {
        var bytes = BuildWorkbook(wb => wb.Worksheets.Add("Sheet1"));

        var result = _parser.Parse(bytes, "rates.xlsx", "UnknownJob", QueueId);

        result.HasParseErrors.Should().BeTrue();
        result.ParseErrors.Should().Contain(e => e.Contains("Unrecognised"));
    }

    // ── Rows with blank TestCode are skipped ─────────────────────────────────

    [Fact]
    public void Parse_FecSheet_SkipsRowsWithBlankTestCode()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var fec = AddFecSheet(wb);
            fec.Cell(2, 1).Value = "";   // blank → skip
            fec.Cell(2, 4).Value = 10m;
            fec.Cell(3, 1).Value = "TC001";
            fec.Cell(3, 4).Value = 15m;
            AddAgrupSheet(wb);
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "TC001");
    }

    // ── TestCode is trimmed ───────────────────────────────────────────────────

    [Fact]
    public void Parse_FecSheet_TrimsTestCodeWhitespace()
    {
        var bytes = BuildWorkbook(wb =>
        {
            var fec = AddFecSheet(wb);
            fec.Cell(2, 1).Value = "  TC001  ";
            fec.Cell(2, 4).Value = 10m;
            AddAgrupSheet(wb);
        });

        var result = _parser.Parse(bytes, "rates.xlsx", "BulkTestRatesUpdate", QueueId);

        result.FecRows.Should().ContainSingle(r => r.TestCode == "TC001");
    }
}
