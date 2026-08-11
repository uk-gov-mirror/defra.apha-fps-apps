using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectMap : IEntityTypeConfiguration<Project>
    {


        public void Configure(EntityTypeBuilder<Project> entity)
        {
            entity.HasKey(e => new { e.ParentProject, e.FpsYear }).HasName("pk_tlkpproject");

            entity.ToTable("tlkpproject", "fps");

            entity.HasIndex(e => e.ProjectStatus, "projectstatus");

            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.BudgetCvl)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("budget_cvl");
            entity.Property(e => e.CarryOver)
                .HasPrecision(19, 4)
                .HasColumnName("carryover");
            entity.Property(e => e.CarryOverSeed)
                .HasPrecision(19, 4)
                .HasColumnName("carryoverseed");
            entity.Property(e => e.CaseWorkSub)
                .HasPrecision(5, 4)
                .HasColumnName("caseworksub");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasDefaultValueSql("0")
                .HasColumnName("contract");
            entity.Property(e => e.CostBookNo)
                .HasMaxLength(50)
                .HasColumnName("costbookno");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.CustIncome)
                .HasPrecision(19, 4)
                .HasColumnName("custincome");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.DateCosted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecosted");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.FecCost)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("feccost");
            entity.Property(e => e.Finished)
                .HasDefaultValue((short)0)
                .HasColumnName("finished");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IncomeAccountCode)
                .HasMaxLength(50)
                .HasColumnName("incomeaccountcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");
            entity.Property(e => e.OwningRc)
                .HasMaxLength(50)
                .HasColumnName("owningrc");
            entity.Property(e => e.PlanCaseWorkDebit)
                .HasPrecision(19, 4)
                .HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Profit)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("profit");
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
            entity.Property(e => e.ProjectGroup)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");
            entity.Property(e => e.ProjectParent)
                .HasMaxLength(50)
                .HasColumnName("projectparent");
            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.ProjectTitle)
                .HasMaxLength(200)
                .HasColumnName("projecttitle");
            entity.Property(e => e.PvsIncome)
                .HasPrecision(19, 4)
                .HasColumnName("pvsincome");
            entity.Property(e => e.ShortTitle)
                .HasMaxLength(30)
                .HasColumnName("shorttitle");
            entity.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");
            entity.Property(e => e.TransferIncome)
                .HasPrecision(19, 4)
                .HasColumnName("transferincome");
            entity.Property(e => e.WipCurrent)
                .HasPrecision(19, 4)
                .HasColumnName("wip_current");
            entity.Property(e => e.WipEoy)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit)
                .HasPrecision(19, 4)
                .HasColumnName("wip_limit");
        }
    }
}
