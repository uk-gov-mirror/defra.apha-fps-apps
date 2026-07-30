using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ResourceStaffAllocationDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var dto = new ResourceStaffAllocationDto
            {
                WorkGroupGrade = "WG01",
                StaffId = "PACT001",
                Name = "Smith, John",
                HrsAvail = 37.0,
                ZtHours = 2.5,
                AppPlannedHours = 30.0,
                PlannedHours = 35.0,
                ChargeHours = 28.0,
                AppChargeHours = 25.0,
                Allocation = 0.9459,
                Utilization = 0.7568,
                AppUtilization = 0.6757
            };

            Assert.Equal("WG01", dto.WorkGroupGrade);
            Assert.Equal("PACT001", dto.StaffId);
            Assert.Equal("Smith, John", dto.Name);
            Assert.Equal(37.0, dto.HrsAvail);
            Assert.Equal(2.5, dto.ZtHours);
            Assert.Equal(30.0, dto.AppPlannedHours);
            Assert.Equal(35.0, dto.PlannedHours);
            Assert.Equal(28.0, dto.ChargeHours);
            Assert.Equal(25.0, dto.AppChargeHours);
            Assert.Equal(0.9459, dto.Allocation);
            Assert.Equal(0.7568, dto.Utilization);
            Assert.Equal(0.6757, dto.AppUtilization);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ResourceStaffAllocationDto
            {
                WorkGroupGrade = null,
                StaffId = null,
                Name = null,
                HrsAvail = null,
                Allocation = null,
                Utilization = null,
                AppUtilization = null
            };

            Assert.Null(dto.WorkGroupGrade);
            Assert.Null(dto.StaffId);
            Assert.Null(dto.Name);
            Assert.Null(dto.HrsAvail);
            Assert.Null(dto.Allocation);
            Assert.Null(dto.Utilization);
            Assert.Null(dto.AppUtilization);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ResourceStaffAllocationDto
            {
                StaffId = "OLD001",
                PlannedHours = 10.0
            };

            dto.StaffId = "NEW001";
            dto.PlannedHours = 20.0;
            dto.HrsAvail = 37.0;

            Assert.Equal("NEW001", dto.StaffId);
            Assert.Equal(20.0, dto.PlannedHours);
            Assert.Equal(37.0, dto.HrsAvail);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new ResourceStaffAllocationDto();

            Assert.Equal(0.0, dto.ZtHours);
            Assert.Equal(0.0, dto.AppPlannedHours);
            Assert.Equal(0.0, dto.PlannedHours);
            Assert.Equal(0.0, dto.ChargeHours);
            Assert.Equal(0.0, dto.AppChargeHours);
        }

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new ResourceStaffAllocationDto();

            Assert.Null(dto.WorkGroupGrade);
            Assert.Null(dto.StaffId);
            Assert.Null(dto.Name);
            Assert.Null(dto.HrsAvail);
            Assert.Null(dto.Allocation);
            Assert.Null(dto.Utilization);
            Assert.Null(dto.AppUtilization);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void PlannedHours_AcceptsZeroValue()
        {
            var dto = new ResourceStaffAllocationDto { PlannedHours = 0.0 };

            Assert.Equal(0.0, dto.PlannedHours);
        }

        [Fact]
        public void Allocation_AcceptsOneHundredPercent()
        {
            var dto = new ResourceStaffAllocationDto { Allocation = 1.0 };

            Assert.Equal(1.0, dto.Allocation);
        }

        #endregion
    }
}
