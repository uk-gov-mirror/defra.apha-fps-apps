using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PactProjectViewMap : IEntityTypeConfiguration<PactProjectView>
    {
        public void Configure(EntityTypeBuilder<PactProjectView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpactproject", "fps");

            entity.Property(e => e.BudgetCvl)
                .HasPrecision(19, 4)
                .HasColumnName("budget_cvl");
            entity.Property(e => e.BudgetExt)
                .HasPrecision(19, 4)
                .HasColumnName("budget_ext");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasColumnName("contract");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.ForecastCost)
                .HasPrecision(19, 4)
                .HasColumnName("forecastcost");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
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
                .HasPrecision(19, 4)
                .HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit)
                .HasPrecision(19, 4)
                .HasColumnName("wip_limit");
        }
    }
}
