using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{

    public class ProfitCentreViewMap : IEntityTypeConfiguration<ProfitCentreView>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vtblkpprofitcentre", "fps");

            builder.Property(e => e.ProfitCentreId).HasColumnName("profitcentre");
            builder.Property(e => e.ProfitCentreName).HasColumnName("profitcentrename");
            builder.Property(e => e.Division).HasColumnName("division");
            builder.Property(e => e.ContTarget).HasPrecision(19, 4).HasColumnName("conttarget");
            builder.Property(e => e.ProfitCentreHead).HasColumnName("profitcentrehead");
            builder.Property(e => e.DivisionId).HasColumnName("divisionid");
            builder.Property(e => e.EmailRecipient).HasColumnName("email_recipient");
            builder.Property(e => e.HighLevelSummary).HasColumnName("highlevelsummary");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}