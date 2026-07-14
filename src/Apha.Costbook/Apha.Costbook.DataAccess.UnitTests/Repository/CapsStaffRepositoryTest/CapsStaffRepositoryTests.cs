using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.CapsStaffRepositoryTest
{
    public class CapsStaffRepositoryTests
    {
        // ── Factory helper ────────────────────────────────────────────────────

        private static CapsStaffRepository CreateRepository(IEnumerable<Staff> staffs)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var staffsMockSet = RepositoryTestHelper.CreateMockDbSet(staffs);
            mockContext.Setup(x => x.Set<Staff>()).Returns(staffsMockSet.Object);
            mockContext.Setup(x => x.Staffs).Returns(staffsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new CapsStaffRepository(mockContext.Object);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllCapsStaff()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice", Dt2number = "DT001" },
                new Staff { Mnumber = "M002", Name = "Bob",   Dt2number = null },
                new Staff { Mnumber = "M003", Name = "Charlie", Dt2number = "DT003" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoCapsStaff()
        {
            // Arrange
            var repo = CreateRepository(new List<Staff>());

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectProperties()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice Smith", Dt2number = "DT001" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal("M001", item.Mnumber);
            Assert.Equal("Alice Smith", item.Name);
            Assert.Equal("DT001", item.Dt2number);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsStaffWithNullDt2Number()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice", Dt2number = null },
                new Staff { Mnumber = "M002", Name = "Bob",   Dt2number = "DT002" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetAllStaffAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, s => s.Dt2number == null);
            Assert.Contains(result, s => s.Dt2number == "DT002");
        }

        #endregion

        // ── GetByMNumberAsync ─────────────────────────────────────────────────

        #region GetByMNumberAsync Tests

        [Fact]
        public async Task GetByMNumberAsync_ExistingMNumber_ReturnsCapsStaff()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice" },
                new Staff { Mnumber = "M002", Name = "Bob" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetByMNumberAsync("M001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M001", result!.Mnumber);
            Assert.Equal("Alice", result.Name);
        }

        [Fact]
        public async Task GetByMNumberAsync_NonExistentMNumber_ReturnsNull()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.GetByMNumberAsync("NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ExistingMNumber_ReturnsTrue()
        {
            // Arrange
            var staffs = new List<Staff>
            {
                new Staff { Mnumber = "M001", Name = "Alice" }
            };
            var repo = CreateRepository(staffs);

            // Act
            var result = await repo.ExistsAsync("M001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentMNumber_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(new List<Staff>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidEntity_AddsAndReturnsCapsStaff()
        {
            // Arrange
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var staffsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<Staff>());
            mockContext.Setup(x => x.Staffs).Returns(staffsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new CapsStaffRepository(mockContext.Object);

            var newStaff = new Staff { Mnumber = "M003", Name = "Charlie" };

            // Act
            var result = await repo.AddStaffAsync(newStaff);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("M003", result.Mnumber);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingEntity_UpdatesNameAndDt2Number()
        {
            // Arrange
            var existing = new Staff { Mnumber = "M001", Name = "Alice", Dt2number = "DT001" };
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var staffsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<Staff> { existing });
            mockContext.Setup(x => x.Staffs).Returns(staffsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new CapsStaffRepository(mockContext.Object);

            var updatedEntity = new Staff { Mnumber = "M001", Name = "Alice Updated", Dt2number = "DT999" };

            // Act
            var result = await repo.UpdateStaffAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alice Updated", result.Name);
            Assert.Equal("DT999", result.Dt2number);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_NonExistentEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(new List<Staff>());
            var entity = new Staff { Mnumber = "NOTEXIST", Name = "Ghost" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateStaffAsync(entity));
        }

        #endregion
    }
}
