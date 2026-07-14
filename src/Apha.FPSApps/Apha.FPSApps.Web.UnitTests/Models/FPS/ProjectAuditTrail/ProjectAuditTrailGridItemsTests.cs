using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.FPS.ProjectAuditTrail
{
    public class ProjectAuditTrailGridItemsTests
    {
        private const string DateTimePropertyName = "DateTime";

        [Theory]
        [InlineData(typeof(ProjectLogItem))]
        [InlineData(typeof(StaffJobLogItem))]
        [InlineData(typeof(TestRequirementLogItem))]
        [InlineData(typeof(AnimalRequestLogItem))]
        [InlineData(typeof(AdditionalCostLogItem))]
        public void DateTimeProperty_GridColumnAttribute_IsConfiguredAsDateTimeType(Type itemType)
        {
            // Arrange
            var property = itemType.GetProperty(DateTimePropertyName);

            // Act
            var attribute = property?.GetCustomAttribute<GridColumnAttribute>();

            // Assert — regression guard: Date_Time must render date AND time (GridColumnType.DateTime),
            // not date-only (GridColumnType.Date), across all five Project Audit Trail grids.
            Assert.NotNull(property);
            Assert.NotNull(attribute);
            Assert.Equal(GridColumnType.DateTime, attribute!.Type);
        }

        [Theory]
        [InlineData(typeof(ProjectLogItem))]
        [InlineData(typeof(StaffJobLogItem))]
        [InlineData(typeof(TestRequirementLogItem))]
        [InlineData(typeof(AnimalRequestLogItem))]
        [InlineData(typeof(AdditionalCostLogItem))]
        public void GetColumnsDefination_DateTimeColumn_ProducesTimeInclusiveDateFormat(Type itemType)
        {
            // Arrange
            var method = typeof(GridDataProvider)
                .GetMethod(nameof(GridDataProvider.GetColumnsDefination))!
                .MakeGenericMethod(itemType);

            // Act
            var columns = (List<DataGridColumn>)method.Invoke(null, new object?[] { null })!;
            var dateTimeColumn = columns.Single(c => c.PropertyName == DateTimePropertyName);

            // Assert
            Assert.Equal(GridColumnType.DateTime, dateTimeColumn.ColumnType);
            Assert.Equal("dd/MM/yyyy HH:mm", dateTimeColumn.DateTimeFormatHhMm);
        }
    }
}
