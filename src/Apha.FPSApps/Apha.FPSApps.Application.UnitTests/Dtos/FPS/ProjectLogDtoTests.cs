using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ProjectLogDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            var now         = new DateTime(2024, 1, 15, 10, 0, 0);
            var dateCreated = new DateTime(2023, 6, 1);
            var dateCosted  = new DateTime(2023, 9, 1);

            var dto = new ProjectLogDto
            {
                SequenceNo         = 1,
                ParentProject      = "PP001",
                ProjectTitle       = "Test Project",
                Program            = "PROG01",
                Customer           = "CustomerA",
                Manager            = "ManagerB",
                TransferIncome     = 1000m,
                CustIncome         = 2000m,
                WipEoy             = 500m,
                WipLimit           = 600m,
                WipCurrent         = 550m,
                ProjectStatus      = "Active",
                CostBookNo         = "CB001",
                DateCreated        = dateCreated,
                FecCost            = 300m,
                Profit             = 700m,
                BudgetCvl          = 800m,
                DateCosted         = dateCosted,
                Disease            = "Disease01",
                Contract           = "CTR01",
                ProjectParent      = "PPAR01",
                ShortTitle         = "ShortT",
                CaseWorkSub        = 50m,
                PvsIncome          = 100m,
                PlanCaseWorkDebit  = 75m,
                Finished           = 0,
                OwningRc           = "RC01",
                Comments           = "Some comment",
                CarryOver          = 200m,
                CarryOverSeed      = 10m,
                DateTime           = now,
                UserId             = "user01",
                InsertDelete       = "I",
                JobCode            = "JC001",
                IsDefraProject     = 1,
                CostCentre         = 1234.5,
                OracleProjectCode  = "OPC001",
                SubAccountCode     = "SAC001",
                ProjectGroup       = "PG01",
                IncomeAccountCode  = "IAC001",
                FpsYear            = 2024
            };

            Assert.Equal(1,               dto.SequenceNo);
            Assert.Equal("PP001",          dto.ParentProject);
            Assert.Equal("Test Project",   dto.ProjectTitle);
            Assert.Equal("PROG01",         dto.Program);
            Assert.Equal("CustomerA",      dto.Customer);
            Assert.Equal("ManagerB",       dto.Manager);
            Assert.Equal(1000m,            dto.TransferIncome);
            Assert.Equal(2000m,            dto.CustIncome);
            Assert.Equal(500m,             dto.WipEoy);
            Assert.Equal(600m,             dto.WipLimit);
            Assert.Equal(550m,             dto.WipCurrent);
            Assert.Equal("Active",         dto.ProjectStatus);
            Assert.Equal("CB001",          dto.CostBookNo);
            Assert.Equal(dateCreated,      dto.DateCreated);
            Assert.Equal(300m,             dto.FecCost);
            Assert.Equal(700m,             dto.Profit);
            Assert.Equal(800m,             dto.BudgetCvl);
            Assert.Equal(dateCosted,       dto.DateCosted);
            Assert.Equal("Disease01",      dto.Disease);
            Assert.Equal("CTR01",          dto.Contract);
            Assert.Equal("PPAR01",         dto.ProjectParent);
            Assert.Equal("ShortT",         dto.ShortTitle);
            Assert.Equal(50m,              dto.CaseWorkSub);
            Assert.Equal(100m,             dto.PvsIncome);
            Assert.Equal(75m,              dto.PlanCaseWorkDebit);
            Assert.Equal((short)0,         dto.Finished);
            Assert.Equal("RC01",           dto.OwningRc);
            Assert.Equal("Some comment",   dto.Comments);
            Assert.Equal(200m,             dto.CarryOver);
            Assert.Equal(10m,              dto.CarryOverSeed);
            Assert.Equal(now,              dto.DateTime);
            Assert.Equal("user01",         dto.UserId);
            Assert.Equal("I",              dto.InsertDelete);
            Assert.Equal("JC001",          dto.JobCode);
            Assert.Equal((short)1,         dto.IsDefraProject);
            Assert.Equal(1234.5,           dto.CostCentre);
            Assert.Equal("OPC001",         dto.OracleProjectCode);
            Assert.Equal("SAC001",         dto.SubAccountCode);
            Assert.Equal("PG01",           dto.ProjectGroup);
            Assert.Equal("IAC001",         dto.IncomeAccountCode);
            Assert.Equal(2024,             dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            var dto = new ProjectLogDto
            {
                ParentProject  = "PP002",
                ProjectTitle   = "T",
                Program        = "P",
                Customer       = "C",
                Disease        = "D",
                Contract       = "CTR",
                JobCode        = "JC",
                Manager        = null,
                WipEoy         = null,
                WipLimit       = null,
                WipCurrent     = null,
                CostBookNo     = null,
                DateCreated    = null,
                FecCost        = null,
                Profit         = null,
                BudgetCvl      = null,
                DateCosted     = null,
                ProjectParent  = null,
                ShortTitle     = null,
                CaseWorkSub    = null,
                PvsIncome      = null,
                PlanCaseWorkDebit = null,
                Finished       = null,
                OwningRc       = null,
                Comments       = null,
                CarryOver      = null,
                CarryOverSeed  = null,
                DateTime       = null,
                UserId         = null,
                InsertDelete   = null,
                IsDefraProject = null,
                CostCentre     = null,
                OracleProjectCode = null,
                SubAccountCode    = null,
                ProjectGroup      = null,
                IncomeAccountCode = null
            };

            Assert.Null(dto.Manager);
            Assert.Null(dto.WipEoy);
            Assert.Null(dto.WipLimit);
            Assert.Null(dto.WipCurrent);
            Assert.Null(dto.CostBookNo);
            Assert.Null(dto.DateCreated);
            Assert.Null(dto.FecCost);
            Assert.Null(dto.Profit);
            Assert.Null(dto.BudgetCvl);
            Assert.Null(dto.DateCosted);
            Assert.Null(dto.ProjectParent);
            Assert.Null(dto.ShortTitle);
            Assert.Null(dto.CaseWorkSub);
            Assert.Null(dto.PvsIncome);
            Assert.Null(dto.PlanCaseWorkDebit);
            Assert.Null(dto.Finished);
            Assert.Null(dto.OwningRc);
            Assert.Null(dto.Comments);
            Assert.Null(dto.CarryOver);
            Assert.Null(dto.CarryOverSeed);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
            Assert.Null(dto.IsDefraProject);
            Assert.Null(dto.CostCentre);
            Assert.Null(dto.OracleProjectCode);
            Assert.Null(dto.SubAccountCode);
            Assert.Null(dto.ProjectGroup);
            Assert.Null(dto.IncomeAccountCode);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new ProjectLogDto
            {
                ParentProject = "OLD",
                ProjectTitle  = "Old Title",
                Program       = "P",
                Customer      = "C",
                Disease       = "D",
                Contract      = "CTR",
                JobCode       = "JC"
            };

            dto.ParentProject  = "NEW";
            dto.ProjectTitle   = "New Title";
            dto.TransferIncome = 5000m;
            dto.FpsYear        = 2026;

            Assert.Equal("NEW",       dto.ParentProject);
            Assert.Equal("New Title", dto.ProjectTitle);
            Assert.Equal(5000m,       dto.TransferIncome);
            Assert.Equal(2026,        dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultConstructor_ValueTypeDefaults_AreZero()
        {
            var dto = new ProjectLogDto();

            Assert.Equal(0,   dto.SequenceNo);
            Assert.Equal(0m,  dto.TransferIncome);
            Assert.Equal(0m,  dto.CustIncome);
            Assert.Equal(0,   dto.FpsYear);
        }

        [Fact]
        public void DefaultConstructor_AllNullableProperties_AreNull()
        {
            var dto = new ProjectLogDto();

            Assert.Null(dto.Manager);
            Assert.Null(dto.WipEoy);
            Assert.Null(dto.WipLimit);
            Assert.Null(dto.WipCurrent);
            Assert.Null(dto.CostBookNo);
            Assert.Null(dto.DateCreated);
            Assert.Null(dto.FecCost);
            Assert.Null(dto.Profit);
            Assert.Null(dto.BudgetCvl);
            Assert.Null(dto.DateCosted);
            Assert.Null(dto.ProjectParent);
            Assert.Null(dto.ShortTitle);
            Assert.Null(dto.CaseWorkSub);
            Assert.Null(dto.PvsIncome);
            Assert.Null(dto.PlanCaseWorkDebit);
            Assert.Null(dto.Finished);
            Assert.Null(dto.OwningRc);
            Assert.Null(dto.Comments);
            Assert.Null(dto.CarryOver);
            Assert.Null(dto.CarryOverSeed);
            Assert.Null(dto.DateTime);
            Assert.Null(dto.UserId);
            Assert.Null(dto.InsertDelete);
            Assert.Null(dto.IsDefraProject);
            Assert.Null(dto.CostCentre);
            Assert.Null(dto.OracleProjectCode);
            Assert.Null(dto.SubAccountCode);
            Assert.Null(dto.ProjectGroup);
            Assert.Null(dto.IncomeAccountCode);
        }

        #endregion

        #region Boundary Tests

        [Fact]
        public void TransferIncome_AcceptsNegativeValue()
        {
            var dto = new ProjectLogDto
            {
                ParentProject = "PP003",
                ProjectTitle  = "T",
                Program       = "P",
                Customer      = "C",
                Disease       = "D",
                Contract      = "CTR",
                JobCode       = "JC",
                TransferIncome = -250m
            };

            Assert.Equal(-250m, dto.TransferIncome);
        }

        [Fact]
        public void SequenceNo_AcceptsMaxInt()
        {
            var dto = new ProjectLogDto
            {
                ParentProject = "PP004",
                ProjectTitle  = "T",
                Program       = "P",
                Customer      = "C",
                Disease       = "D",
                Contract      = "CTR",
                JobCode       = "JC",
                SequenceNo    = int.MaxValue
            };

            Assert.Equal(int.MaxValue, dto.SequenceNo);
        }

        #endregion
    }
}
