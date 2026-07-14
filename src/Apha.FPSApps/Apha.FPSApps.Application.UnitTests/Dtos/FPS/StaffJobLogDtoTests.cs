using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class StaffJobLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var now = new DateTime(2024, 3, 10, 8, 0, 0);

            var dto = new StaffJobLogDto
            {
                SequenceNo   = 5,
                StaffId      = "STAFF01",
                JobCode      = "JC005",
                PlannedHours = 37.5,
                DateTime     = now,
                UserId       = "user05",
                InsertDelete = "I",
                FpsYear      = 2024
            };

            Assert.Equal(5,          dto.SequenceNo);
            Assert.Equal("STAFF01",  dto.StaffId);
            Assert.Equal("JC005",    dto.JobCode);
            Assert.Equal(37.5,       dto.PlannedHours);
            Assert.Equal(now,        dto.DateTime);
            Assert.Equal("user05",   dto.UserId);
            Assert.Equal("I",        dto.InsertDelete);
            Assert.Equal(2024,       dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new StaffJobLogDto
            {
                StaffId      = "STAFF02",
                JobCode      = "JC006",
                DateTime     = null,
                UserId       = null,
                InsertDelete = null
            };

            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new StaffJobLogDto { StaffId = "OLD", JobCode = "OLD" };

            dto.StaffId      = "STAFF99";
            dto.JobCode      = "JC099";
            dto.PlannedHours = 40.0;
            dto.FpsYear      = 2025;

            Assert.Equal("STAFF99", dto.StaffId);
            Assert.Equal("JC099",   dto.JobCode);
            Assert.Equal(40.0,      dto.PlannedHours);
            Assert.Equal(2025,      dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new StaffJobLogDto();

            Assert.Equal(0,   dto.SequenceNo);
            Assert.Equal(0.0, dto.PlannedHours);
            Assert.Equal(0,   dto.FpsYear);
        }

        [Fact]
        public void DefaultConstructor_NullableProperties_AreNull()
        {
            var dto = new StaffJobLogDto();

            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void PlannedHours_AcceptsZero()
        {
            var dto = new StaffJobLogDto { StaffId = "S", JobCode = "J", PlannedHours = 0.0 };

            Assert.Equal(0.0, dto.PlannedHours);
        }

        [Fact]
        public void PlannedHours_AcceptsLargeValue()
        {
            var dto = new StaffJobLogDto { StaffId = "S", JobCode = "J", PlannedHours = 2080.0 };

            Assert.Equal(2080.0, dto.PlannedHours);
        }

        #endregion
    }
}
