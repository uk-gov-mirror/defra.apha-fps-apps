using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class ProjectLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange
            var dateCreated = new DateTime(2023, 1, 15);
            var dateCosted  = new DateTime(2023, 3, 20);
            var dateTime    = new DateTime(2024, 5, 10, 11, 0, 0);

            // Act
            var dto = new ProjectLogDto
            {
                SequenceNo        = 1,
                ParentProject     = "PP001",
                ProjectTitle      = "Foot and Mouth Study",
                Program           = "PROG01",
                Customer          = "DEFRA",
                Manager           = "Jane Smith",
                TransferIncome    = 10000.00m,
                CustIncome        = 25000.00m,
                WipEoy            = 1500.00m,
                WipLimit          = 5000.00m,
                WipCurrent        = 2000.00m,
                ProjectStatus     = "Active",
                CostBookNo        = "CBN001",
                DateCreated       = dateCreated,
                FecCost           = 8000.00m,
                Profit            = 3000.00m,
                BudgetCvl         = 12000.00m,
                DateCosted        = dateCosted,
                Disease           = "FMD",
                Contract          = "CONTRACT01",
                ProjectParent     = "PARENT01",
                ShortTitle        = "FMD Study",
                CaseWorkSub       = 500.00m,
                PvsIncome         = 750.00m,
                PlanCaseWorkDebit = 250.00m,
                Finished          = 0,
                OwningRc          = "RC001",
                Comments          = "Initial project log entry",
                CarryOver         = 100.00m,
                CarryOverSeed     = 50.00m,
                DateTime          = dateTime,
                UserId            = "auditor01",
                InsertDelete      = "I",
                JobCode           = "JOB001",
                IsDefraProject    = 1,
                CostCentre        = 1234.5,
                OracleProjectCode = "OPC001",
                SubAccountCode    = "SAC001",
                ProjectGroup      = "GRPA",
                IncomeAccountCode = "IAC001",
                FpsYear           = 2024
            };

            // Assert
            dto.SequenceNo.Should().Be(1);
            dto.ParentProject.Should().Be("PP001");
            dto.ProjectTitle.Should().Be("Foot and Mouth Study");
            dto.Program.Should().Be("PROG01");
            dto.Customer.Should().Be("DEFRA");
            dto.Manager.Should().Be("Jane Smith");
            dto.TransferIncome.Should().Be(10000.00m);
            dto.CustIncome.Should().Be(25000.00m);
            dto.WipEoy.Should().Be(1500.00m);
            dto.WipLimit.Should().Be(5000.00m);
            dto.WipCurrent.Should().Be(2000.00m);
            dto.ProjectStatus.Should().Be("Active");
            dto.CostBookNo.Should().Be("CBN001");
            dto.DateCreated.Should().Be(dateCreated);
            dto.FecCost.Should().Be(8000.00m);
            dto.Profit.Should().Be(3000.00m);
            dto.BudgetCvl.Should().Be(12000.00m);
            dto.DateCosted.Should().Be(dateCosted);
            dto.Disease.Should().Be("FMD");
            dto.Contract.Should().Be("CONTRACT01");
            dto.ProjectParent.Should().Be("PARENT01");
            dto.ShortTitle.Should().Be("FMD Study");
            dto.CaseWorkSub.Should().Be(500.00m);
            dto.PvsIncome.Should().Be(750.00m);
            dto.PlanCaseWorkDebit.Should().Be(250.00m);
            dto.Finished.Should().Be(0);
            dto.OwningRc.Should().Be("RC001");
            dto.Comments.Should().Be("Initial project log entry");
            dto.CarryOver.Should().Be(100.00m);
            dto.CarryOverSeed.Should().Be(50.00m);
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("auditor01");
            dto.InsertDelete.Should().Be("I");
            dto.JobCode.Should().Be("JOB001");
            dto.IsDefraProject.Should().Be(1);
            dto.CostCentre.Should().Be(1234.5);
            dto.OracleProjectCode.Should().Be("OPC001");
            dto.SubAccountCode.Should().Be("SAC001");
            dto.ProjectGroup.Should().Be("GRPA");
            dto.IncomeAccountCode.Should().Be("IAC001");
            dto.FpsYear.Should().Be(2024);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new ProjectLogDto
            {
                SequenceNo        = 2,
                ParentProject     = "PP002",
                ProjectTitle      = "BSE Research",
                Program           = "PROG02",
                Customer          = "APHA",
                TransferIncome    = 0m,
                CustIncome        = 0m,
                ProjectStatus     = "Closed",
                Disease           = "BSE",
                Contract          = "CONTRACT02",
                JobCode           = "JOB002",
                FpsYear           = 2025,
                Manager           = null,
                WipEoy            = null,
                WipLimit          = null,
                WipCurrent        = null,
                CostBookNo        = null,
                DateCreated       = null,
                FecCost           = null,
                Profit            = null,
                BudgetCvl         = null,
                DateCosted        = null,
                ProjectParent     = null,
                ShortTitle        = null,
                CaseWorkSub       = null,
                PvsIncome         = null,
                PlanCaseWorkDebit = null,
                Finished          = null,
                OwningRc          = null,
                Comments          = null,
                CarryOver         = null,
                CarryOverSeed     = null,
                DateTime          = null,
                UserId            = null,
                InsertDelete      = null,
                IsDefraProject    = null,
                CostCentre        = null,
                OracleProjectCode = null,
                SubAccountCode    = null,
                ProjectGroup      = null,
                IncomeAccountCode = null
            };

            // Assert
            dto.Manager.Should().BeNull();
            dto.WipEoy.Should().BeNull();
            dto.WipLimit.Should().BeNull();
            dto.WipCurrent.Should().BeNull();
            dto.CostBookNo.Should().BeNull();
            dto.DateCreated.Should().BeNull();
            dto.FecCost.Should().BeNull();
            dto.Profit.Should().BeNull();
            dto.BudgetCvl.Should().BeNull();
            dto.DateCosted.Should().BeNull();
            dto.ProjectParent.Should().BeNull();
            dto.ShortTitle.Should().BeNull();
            dto.CaseWorkSub.Should().BeNull();
            dto.PvsIncome.Should().BeNull();
            dto.PlanCaseWorkDebit.Should().BeNull();
            dto.Finished.Should().BeNull();
            dto.OwningRc.Should().BeNull();
            dto.Comments.Should().BeNull();
            dto.CarryOver.Should().BeNull();
            dto.CarryOverSeed.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
            dto.IsDefraProject.Should().BeNull();
            dto.CostCentre.Should().BeNull();
            dto.OracleProjectCode.Should().BeNull();
            dto.SubAccountCode.Should().BeNull();
            dto.ProjectGroup.Should().BeNull();
            dto.IncomeAccountCode.Should().BeNull();
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            // Arrange
            var dto = new ProjectLogDto
            {
                SequenceNo    = 1,
                ParentProject = "OLD",
                ProjectTitle  = "Old Title",
                Program       = "OLD_PROG",
                Customer      = "OLD_CUST",
                TransferIncome = 0m,
                CustIncome     = 0m,
                ProjectStatus  = "Pending",
                Disease        = "OLD_DIS",
                Contract       = "OLD_CON",
                JobCode        = "OLDJOB",
                FpsYear        = 2020
            };

            // Act
            var dateTime = new DateTime(2025, 12, 31);
            dto.SequenceNo        = 100;
            dto.ParentProject     = "NEW_PP";
            dto.ProjectTitle      = "Updated Title";
            dto.Program           = "NEW_PROG";
            dto.Customer          = "NEW_CUST";
            dto.Manager           = "New Manager";
            dto.TransferIncome    = 50000m;
            dto.CustIncome        = 75000m;
            dto.WipEoy            = 999m;
            dto.WipLimit          = 2000m;
            dto.WipCurrent        = 1500m;
            dto.ProjectStatus     = "Closed";
            dto.CostBookNo        = "CBN_NEW";
            dto.DateCreated       = dateTime;
            dto.FecCost           = 40000m;
            dto.Profit            = 15000m;
            dto.BudgetCvl         = 60000m;
            dto.DateCosted        = dateTime;
            dto.Disease           = "NEW_DIS";
            dto.Contract          = "NEW_CON";
            dto.ProjectParent     = "NEW_PARENT";
            dto.ShortTitle        = "Updated";
            dto.CaseWorkSub       = 300m;
            dto.PvsIncome         = 400m;
            dto.PlanCaseWorkDebit = 100m;
            dto.Finished          = 1;
            dto.OwningRc          = "RC_NEW";
            dto.Comments          = "Updated comment";
            dto.CarryOver         = 200m;
            dto.CarryOverSeed     = 150m;
            dto.DateTime          = dateTime;
            dto.UserId            = "updater";
            dto.InsertDelete      = "D";
            dto.JobCode           = "NEWJOB";
            dto.IsDefraProject    = 0;
            dto.CostCentre        = 9999.9;
            dto.OracleProjectCode = "OPC_NEW";
            dto.SubAccountCode    = "SAC_NEW";
            dto.ProjectGroup      = "GRP_NEW";
            dto.IncomeAccountCode = "IAC_NEW";
            dto.FpsYear           = 2025;

            // Assert
            dto.SequenceNo.Should().Be(100);
            dto.ParentProject.Should().Be("NEW_PP");
            dto.ProjectTitle.Should().Be("Updated Title");
            dto.Program.Should().Be("NEW_PROG");
            dto.Customer.Should().Be("NEW_CUST");
            dto.Manager.Should().Be("New Manager");
            dto.TransferIncome.Should().Be(50000m);
            dto.CustIncome.Should().Be(75000m);
            dto.WipEoy.Should().Be(999m);
            dto.WipLimit.Should().Be(2000m);
            dto.WipCurrent.Should().Be(1500m);
            dto.ProjectStatus.Should().Be("Closed");
            dto.CostBookNo.Should().Be("CBN_NEW");
            dto.DateCreated.Should().Be(dateTime);
            dto.FecCost.Should().Be(40000m);
            dto.Profit.Should().Be(15000m);
            dto.BudgetCvl.Should().Be(60000m);
            dto.DateCosted.Should().Be(dateTime);
            dto.Disease.Should().Be("NEW_DIS");
            dto.Contract.Should().Be("NEW_CON");
            dto.ProjectParent.Should().Be("NEW_PARENT");
            dto.ShortTitle.Should().Be("Updated");
            dto.CaseWorkSub.Should().Be(300m);
            dto.PvsIncome.Should().Be(400m);
            dto.PlanCaseWorkDebit.Should().Be(100m);
            dto.Finished.Should().Be(1);
            dto.OwningRc.Should().Be("RC_NEW");
            dto.Comments.Should().Be("Updated comment");
            dto.CarryOver.Should().Be(200m);
            dto.CarryOverSeed.Should().Be(150m);
            dto.DateTime.Should().Be(dateTime);
            dto.UserId.Should().Be("updater");
            dto.InsertDelete.Should().Be("D");
            dto.JobCode.Should().Be("NEWJOB");
            dto.IsDefraProject.Should().Be(0);
            dto.CostCentre.Should().Be(9999.9);
            dto.OracleProjectCode.Should().Be("OPC_NEW");
            dto.SubAccountCode.Should().Be("SAC_NEW");
            dto.ProjectGroup.Should().Be("GRP_NEW");
            dto.IncomeAccountCode.Should().Be("IAC_NEW");
            dto.FpsYear.Should().Be(2025);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new ProjectLogDto();

            dto.SequenceNo.Should().Be(0);
            dto.TransferIncome.Should().Be(0m);
            dto.CustIncome.Should().Be(0m);
            dto.FpsYear.Should().Be(0);
            dto.Manager.Should().BeNull();
            dto.WipEoy.Should().BeNull();
            dto.WipLimit.Should().BeNull();
            dto.WipCurrent.Should().BeNull();
            dto.CostBookNo.Should().BeNull();
            dto.DateCreated.Should().BeNull();
            dto.FecCost.Should().BeNull();
            dto.Profit.Should().BeNull();
            dto.BudgetCvl.Should().BeNull();
            dto.DateCosted.Should().BeNull();
            dto.ProjectParent.Should().BeNull();
            dto.ShortTitle.Should().BeNull();
            dto.CaseWorkSub.Should().BeNull();
            dto.PvsIncome.Should().BeNull();
            dto.PlanCaseWorkDebit.Should().BeNull();
            dto.Finished.Should().BeNull();
            dto.OwningRc.Should().BeNull();
            dto.Comments.Should().BeNull();
            dto.CarryOver.Should().BeNull();
            dto.CarryOverSeed.Should().BeNull();
            dto.DateTime.Should().BeNull();
            dto.UserId.Should().BeNull();
            dto.InsertDelete.Should().BeNull();
            dto.IsDefraProject.Should().BeNull();
            dto.CostCentre.Should().BeNull();
            dto.OracleProjectCode.Should().BeNull();
            dto.SubAccountCode.Should().BeNull();
            dto.ProjectGroup.Should().BeNull();
            dto.IncomeAccountCode.Should().BeNull();
        }

        [Fact]
        public void JobCode_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new ProjectLogDto
            {
                JobCode       = string.Empty,
                ParentProject = "P",
                ProjectTitle  = "T",
                Program       = "PR",
                Customer      = "C",
                ProjectStatus = "S",
                Disease       = "D",
                Contract      = "CO"
            };

            dto.JobCode.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-10000.00)]
        [InlineData(9999999.99)]
        public void TransferIncome_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto = new ProjectLogDto
            {
                ParentProject  = "P",
                ProjectTitle   = "T",
                Program        = "PR",
                Customer       = "C",
                ProjectStatus  = "S",
                Disease        = "D",
                Contract       = "CO",
                JobCode        = "J",
                TransferIncome = value
            };

            dto.TransferIncome.Should().Be(value);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)1)]
        public void Finished_SetToBoundaryValues_ReturnsCorrectValue(short value)
        {
            var dto = new ProjectLogDto { Finished = value };

            dto.Finished.Should().Be(value);
        }

        [Theory]
        [InlineData((short)0)]
        [InlineData((short)1)]
        public void IsDefraProject_SetToBoundaryValues_ReturnsCorrectValue(short value)
        {
            var dto = new ProjectLogDto { IsDefraProject = value };

            dto.IsDefraProject.Should().Be(value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(100.5)]
        [InlineData(99999.9)]
        public void CostCentre_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new ProjectLogDto { CostCentre = value };

            dto.CostCentre.Should().Be(value);
        }

        #endregion
    }
}
