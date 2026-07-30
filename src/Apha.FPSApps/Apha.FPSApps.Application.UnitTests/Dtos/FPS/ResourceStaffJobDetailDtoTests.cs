using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ResourceStaffJobDetailDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new ResourceStaffJobDetailDto
            {
                StaffId = "PACT001",
                PlannedHours = 10.5,
                JobCode = "J001",
                JobDescription = "Road Maintenance",
                Programme = "PROG-A",
                ProjectStatus = "Active"
            };

            Assert.Equal("PACT001", dto.StaffId);
            Assert.Equal(10.5, dto.PlannedHours);
            Assert.Equal("J001", dto.JobCode);
            Assert.Equal("Road Maintenance", dto.JobDescription);
            Assert.Equal("PROG-A", dto.Programme);
            Assert.Equal("Active", dto.ProjectStatus);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ResourceStaffJobDetailDto
            {
                StaffId = null,
                PlannedHours = null,
                JobCode = null,
                JobDescription = null,
                Programme = null,
                ProjectStatus = null
            };

            Assert.Null(dto.StaffId);
            Assert.Null(dto.PlannedHours);
            Assert.Null(dto.JobCode);
            Assert.Null(dto.JobDescription);
            Assert.Null(dto.Programme);
            Assert.Null(dto.ProjectStatus);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ResourceStaffJobDetailDto
            {
                JobCode = "OLD",
                PlannedHours = 5.0
            };

            dto.JobCode = "NEW";
            dto.PlannedHours = 15.0;
            dto.ProjectStatus = "Closed";

            Assert.Equal("NEW", dto.JobCode);
            Assert.Equal(15.0, dto.PlannedHours);
            Assert.Equal("Closed", dto.ProjectStatus);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new ResourceStaffJobDetailDto();

            Assert.Null(dto.StaffId);
            Assert.Null(dto.PlannedHours);
            Assert.Null(dto.JobCode);
            Assert.Null(dto.JobDescription);
            Assert.Null(dto.Programme);
            Assert.Null(dto.ProjectStatus);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void PlannedHours_AcceptsZeroValue()
        {
            var dto = new ResourceStaffJobDetailDto { PlannedHours = 0.0 };

            Assert.Equal(0.0, dto.PlannedHours);
        }

        [Fact]
        public void PlannedHours_AcceptsLargeValue()
        {
            var dto = new ResourceStaffJobDetailDto { PlannedHours = 9999.99 };

            Assert.Equal(9999.99, dto.PlannedHours);
        }

        #endregion
    }
}
