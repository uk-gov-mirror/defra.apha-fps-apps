using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProfitCentreMap : IEntityTypeConfiguration<ProfitCentre>
    {
        public void Configure(EntityTypeBuilder<ProfitCentre> entity)
        {
            entity.HasKey(e => e.ProfitCentreId).HasName("pk__tblkpprofitcentr__1db06a4f");

            entity.ToTable("tblkpprofitcentre", "fps");

            entity.HasIndex(e => e.Division, "division");

            entity.Property(e => e.ProfitCentreId)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.ContTarget)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("conttarget");
            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasDefaultValueSql("0")
                .HasColumnName("division");
            entity.Property(e => e.DivisionId)
                .HasDefaultValue(0)
                .HasColumnName("divisionid");
            entity.Property(e => e.EmailRecipient)
                .HasMaxLength(50)
                .HasColumnName("email_recipient");
            entity.Property(e => e.HighLevelSummary).HasColumnName("highlevelsummary");
            entity.Property(e => e.OutputSheet).HasColumnName("outputsheet");
            entity.Property(e => e.PactCoordinatorEmailName)
                .HasMaxLength(50)
                .HasColumnName("pactcoordinatoremailname");
            entity.Property(e => e.ProfitCentreHead)
                .HasMaxLength(50)
                .HasColumnName("profitcentrehead");
            entity.Property(e => e.ProfitCentreName)
                .HasMaxLength(40)
                .HasColumnName("profitcentrename");
            entity.Property(e => e.Timesheet).HasColumnName("timesheet");
            entity.Property(e => e.TimesheetLayout).HasColumnName("timesheetlayout");
        }
    }
}
