using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BidViewMap : IEntityTypeConfiguration<BidView>
    {
        public void Configure(EntityTypeBuilder<BidView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vtblbid", "fps");

            builder.Property(e => e.WorkGroupName).HasColumnName("workgroup");
            builder.Property(e => e.Account).HasColumnName("account");
            builder.Property(e => e.GenBid).HasPrecision(19, 4).HasColumnName("genbid");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
        }
    }
}
