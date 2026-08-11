using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class WorkGroupViewMap : IEntityTypeConfiguration<WorkGroupView>
    {
        public void Configure(EntityTypeBuilder<WorkGroupView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vworkgroup", "fps");

            builder.Property(e => e.WorkGroupName).HasColumnName("workgroup");
            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.CostCentre).HasColumnName("costcentre");
            builder.Property(e => e.Owner).HasColumnName("owner");
            builder.Property(e => e.Description).HasColumnName("description");
            builder.Property(e => e.CentralOverhead).HasPrecision(19, 4).HasColumnName("centraloverhead");
            builder.Property(e => e.SendEmail).HasColumnName("sendemail");
            builder.Property(e => e.Cos90).HasColumnName("cos90");
            builder.Property(e => e.CostCentreOld).HasColumnName("costcentreold");
            builder.Property(e => e.EmailRecipient).HasColumnName("email_recipient");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
        }
    }
}
