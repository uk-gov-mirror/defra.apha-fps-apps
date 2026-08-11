using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class PactProfitCentreViewMap : IEntityTypeConfiguration<PactProfitCentreView>
    {
        public void Configure(EntityTypeBuilder<PactProfitCentreView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpacttblkpprofitcentre", "fps");

            entity.Property(e => e.ContTarget)
                .HasPrecision(19, 4)
                .HasColumnName("conttarget");
            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasColumnName("division");
            entity.Property(e => e.DivisionId).HasColumnName("divisionid");
            entity.Property(e => e.EmailRecipient)
                .HasMaxLength(50)
                .HasColumnName("email_recipient");
            entity.Property(e => e.Outputsheet).HasColumnName("outputsheet");
            entity.Property(e => e.PactCoordinatorEmailName)
                .HasMaxLength(50)
                .HasColumnName("pactcoordinatoremailname");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
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
