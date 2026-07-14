using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.Components.DataGrid
{
    public class GridDataProviderTests
    {
        private sealed class SampleGridModel
        {
            [Display(Name = "Event Date Time")]
            [GridColumn(Type = GridColumnType.DateTime)]
            public DateTime? EventDateTime { get; set; }

            [Display(Name = "Event Date")]
            [GridColumn(Type = GridColumnType.Date)]
            public DateTime? EventDateOnly { get; set; }

            [GridColumn(Type = GridColumnType.ReadOnly)]
            public string? PlainText { get; set; }

            public string? NoAttributeText { get; set; }
        }

        [Fact]
        public void GetColumnsDefination_DateTimeColumnType_SetsTimeInclusiveDateFormat()
        {
            // Act
            var columns = GridDataProvider.GetColumnsDefination<SampleGridModel>();

            // Assert
            var column = columns.Single(c => c.PropertyName == nameof(SampleGridModel.EventDateTime));
            Assert.Equal(GridColumnType.DateTime, column.ColumnType);
            Assert.Equal("dd/MM/yyyy HH:mm", column.DateTimeFormatHhMm);
        }

        [Fact]
        public void GetColumnsDefination_DateColumnType_SetsDateOnlyFormat()
        {
            // Act
            var columns = GridDataProvider.GetColumnsDefination<SampleGridModel>();

            // Assert
            var column = columns.Single(c => c.PropertyName == nameof(SampleGridModel.EventDateOnly));
            Assert.Equal(GridColumnType.Date, column.ColumnType);
            Assert.Equal("dd/MM/yyyy", column.DateFormat);
        }

        [Fact]
        public void GetColumnsDefination_NonDateColumnType_DefaultsToDateOnlyFormat()
        {
            // Act
            var columns = GridDataProvider.GetColumnsDefination<SampleGridModel>();

            // Assert
            var column = columns.Single(c => c.PropertyName == nameof(SampleGridModel.PlainText));
            Assert.Equal(GridColumnType.ReadOnly, column.ColumnType);
            Assert.Equal("dd/MM/yyyy", column.DateFormat);
        }

        [Fact]
        public void GetColumnsDefination_PropertyWithoutGridColumnAttribute_DefaultsToTextTypeWithDateOnlyFormat()
        {
            // Act
            var columns = GridDataProvider.GetColumnsDefination<SampleGridModel>();

            // Assert
            var column = columns.Single(c => c.PropertyName == nameof(SampleGridModel.NoAttributeText));
            Assert.Equal(GridColumnType.Text, column.ColumnType);
            Assert.Equal("dd/MM/yyyy", column.DateFormat);
            Assert.Equal(nameof(SampleGridModel.NoAttributeText), column.DisplayName);
        }
    }
}
