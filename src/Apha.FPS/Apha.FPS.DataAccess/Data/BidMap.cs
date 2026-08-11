using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BidMap : IEntityTypeConfiguration<Bid>
    {
        public void Configure(EntityTypeBuilder<Bid> builder)
        {
            builder.HasKey(e => new { e.WorkGroupName, e.Account, e.FpsYear }).HasName("pk_tblbid");

            builder.ToTable("tblbid", "fps");

            builder.Property(e => e.WorkGroupName)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            builder.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");

            builder.Property(e => e.GenBid)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("genbid");

            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
