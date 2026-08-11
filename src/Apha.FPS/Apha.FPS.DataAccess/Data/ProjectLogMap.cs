using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectLogMap : IEntityTypeConfiguration<ProjectLog>
    {
        public void Configure(EntityTypeBuilder<ProjectLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_project_log");

            entity.ToTable("project_log", "fps");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.ParentProject).HasMaxLength(20).HasColumnName("parentproject");
            entity.Property(e => e.ProjectTitle).HasColumnName("projecttitle");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.TransferIncome).HasPrecision(19, 4).HasColumnName("transferincome");
            entity.Property(e => e.CustIncome).HasPrecision(19, 4).HasColumnName("custincome");
            entity.Property(e => e.WipEoy).HasPrecision(19, 4).HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit).HasPrecision(19, 4).HasColumnName("wip_limit");
            entity.Property(e => e.WipCurrent).HasPrecision(19, 4).HasColumnName("wip_current");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.CostBookNo).HasColumnName("costbookno");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.FecCost).HasPrecision(19, 4).HasColumnName("feccost");
            entity.Property(e => e.Profit).HasPrecision(19, 4).HasColumnName("profit");
            entity.Property(e => e.BudgetCvl).HasPrecision(19, 4).HasColumnName("budget_cvl");
            entity.Property(e => e.DateCosted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecosted");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.ProjectParent).HasColumnName("projectparent");
            entity.Property(e => e.ShortTitle).HasColumnName("shorttitle");
            entity.Property(e => e.CaseWorkSub).HasColumnName("caseworksub");
            entity.Property(e => e.PvsIncome).HasPrecision(19, 4).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseWorkDebit).HasPrecision(19, 4).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.OwningRc).HasColumnName("owningrc");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasPrecision(19, 4).HasColumnName("carryover");
            entity.Property(e => e.CarryOverSeed).HasPrecision(19, 4).HasColumnName("carryoverseed");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.UserId).HasMaxLength(255).HasColumnName("user_id");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            entity.Property(e => e.JobCode).HasMaxLength(20).HasColumnName("jobcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}