using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PurchaseMap : IEntityTypeConfiguration<Purchase>
    {
        public void Configure(EntityTypeBuilder<Purchase> builder)
        {
            builder.HasKey(e => new { e.WorkGroupName, e.Account, e.ItemDescription, e.FpsYear }).HasName("pk_tblpurchase");

            builder.ToTable("tblpurchase", "fps");

            builder.Property(e => e.WorkGroupName)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            builder.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");

            builder.Property(e => e.ItemDescription)
                .HasMaxLength(50)
                .HasColumnName("itemdescription");

            builder.Property(e => e.Amount)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("amount");

            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
