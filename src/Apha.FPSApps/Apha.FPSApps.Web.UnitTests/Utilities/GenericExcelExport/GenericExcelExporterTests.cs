using System.ComponentModel.DataAnnotations;
using Apha.Common.Utilities.GenericExcelExport;
using Apha.Common.Utilities.GenericExcelExport.Attributes;
using ClosedXML.Excel;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Utilities.GenericExcelExport
{
    public class GenericExcelExporterTests
    {
        private readonly GenericExcelExporter _exporter = new();

        private sealed class SampleRow
        {
            [Display(Name = "Project")]
            public string? JobCode { get; set; }

            public int Quantity { get; set; }

            [ExcelColumn(Name = "Total Cost", Order = 1)]
            [DisplayFormat(DataFormatString = "{0:C}")]
            public decimal Cost { get; set; }

            [ExcelIgnore]
            public int InternalId { get; set; }
        }

        private static IXLWorksheet LoadFirstSheet(byte[] content)
        {
            using var stream = new MemoryStream(content);
            var workbook = new XLWorkbook(stream);
            return workbook.Worksheet(1);
        }

        [Fact]
        public void Export_UsesDisplayName_WhenPresent()
        {
            var data = new[] { new SampleRow { JobCode = "P1" } };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            // Column order: Cost (Order = 1) first, then remaining declaration order.
            Assert.Contains("Project", GetHeaders(worksheet));
        }

        [Fact]
        public void Export_UsesExcelColumnName_OverDisplayAndPropertyName()
        {
            var data = new[] { new SampleRow { Cost = 10m } };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            Assert.Contains("Total Cost", GetHeaders(worksheet));
        }

        [Fact]
        public void Export_FallsBackToPropertyName_WhenNoAttribute()
        {
            var data = new[] { new SampleRow { Quantity = 5 } };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            Assert.Contains("Quantity", GetHeaders(worksheet));
        }

        [Fact]
        public void Export_ExcludesPropertiesMarkedWithExcelIgnore()
        {
            var data = new[] { new SampleRow { InternalId = 99 } };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            Assert.DoesNotContain("InternalId", GetHeaders(worksheet));
        }

        [Fact]
        public void Export_OrdersColumns_ByExcelColumnOrder()
        {
            var data = new[] { new SampleRow { Cost = 10m } };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            Assert.Equal("Total Cost", worksheet.Cell(1, 1).GetString());
        }

        [Fact]
        public void Export_WritesDataRows()
        {
            var data = new[]
            {
                new SampleRow { JobCode = "P1", Quantity = 2, Cost = 15m },
                new SampleRow { JobCode = "P2", Quantity = 3, Cost = 20m }
            };

            var worksheet = LoadFirstSheet(_exporter.Export(data));

            Assert.Equal(3, worksheet.LastRowUsed()!.RowNumber());
        }

        [Fact]
        public void Export_WithEmptyCollection_StillWritesHeaders()
        {
            var worksheet = LoadFirstSheet(_exporter.Export(Array.Empty<SampleRow>()));

            var headers = GetHeaders(worksheet);
            Assert.Contains("Project", headers);
            Assert.Equal(1, worksheet.LastRowUsed()!.RowNumber());
        }

        [Fact]
        public void Export_WithNullData_DoesNotThrow_AndWritesHeaders()
        {
            var worksheet = LoadFirstSheet(_exporter.Export<SampleRow>(null!));

            Assert.Contains("Quantity", GetHeaders(worksheet));
        }

        [Fact]
        public void Export_AppliesSheetName()
        {
            using var stream = new MemoryStream(_exporter.Export(Array.Empty<SampleRow>(), "My Data"));
            var workbook = new XLWorkbook(stream);

            Assert.Equal("My Data", workbook.Worksheet(1).Name);
        }

        [Fact]
        public void Export_SanitisesInvalidSheetName()
        {
            using var stream = new MemoryStream(_exporter.Export(Array.Empty<SampleRow>(), "Bad/Name:[1]"));
            var workbook = new XLWorkbook(stream);

            Assert.Equal("BadName1", workbook.Worksheet(1).Name);
        }

        private static IReadOnlyList<string> GetHeaders(IXLWorksheet worksheet)
        {
            var headers = new List<string>();
            int col = 1;
            while (!worksheet.Cell(1, col).IsEmpty())
            {
                headers.Add(worksheet.Cell(1, col).GetString());
                col++;
            }
            return headers;
        }
    }
}
