using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ResourceStaffJobDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new ResourceStaffJobDto
            {
                StaffId = 42,
                Project = "PRJ-001",
                Description = "Bridge Inspection",
                Hour = 8.0,
                Status = "Completed"
            };

            Assert.Equal(42, dto.StaffId);
            Assert.Equal("PRJ-001", dto.Project);
            Assert.Equal("Bridge Inspection", dto.Description);
            Assert.Equal(8.0, dto.Hour);
            Assert.Equal("Completed", dto.Status);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ResourceStaffJobDto
            {
                StaffId = null,
                Project = null,
                Description = null,
                Hour = null,
                Status = null
            };

            Assert.Null(dto.StaffId);
            Assert.Null(dto.Project);
            Assert.Null(dto.Description);
            Assert.Null(dto.Hour);
            Assert.Null(dto.Status);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ResourceStaffJobDto
            {
                StaffId = 1,
                Project = "OLD",
                Status = "Open"
            };

            dto.StaffId = 99;
            dto.Project = "NEW";
            dto.Status = "Closed";

            Assert.Equal(99, dto.StaffId);
            Assert.Equal("NEW", dto.Project);
            Assert.Equal("Closed", dto.Status);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new ResourceStaffJobDto();

            Assert.Null(dto.StaffId);
            Assert.Null(dto.Project);
            Assert.Null(dto.Description);
            Assert.Null(dto.Hour);
            Assert.Null(dto.Status);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void Hour_AcceptsZeroValue()
        {
            var dto = new ResourceStaffJobDto { Hour = 0.0 };

            Assert.Equal(0.0, dto.Hour);
        }

        [Fact]
        public void StaffId_AcceptsMaxInt()
        {
            var dto = new ResourceStaffJobDto { StaffId = int.MaxValue };

            Assert.Equal(int.MaxValue, dto.StaffId);
        }

        #endregion
    }
}
