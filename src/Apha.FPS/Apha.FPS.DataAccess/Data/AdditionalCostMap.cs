using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AdditionalCostMap : IEntityTypeConfiguration<AdditionalCost>
    {
        public void Configure(EntityTypeBuilder<AdditionalCost> entity)
        {
            entity.HasKey(e => new { e.JobCode, e.Account, e.Description, e.FpsYear })
                  .HasName("pk_tbladditionalcosts");

            entity.ToTable("tbladditionalcosts", "fps");

            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");

            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");

            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");

            entity.Property(e => e.ItemCost)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("itemcost");

            entity.Property(e => e.Freq)
                .HasMaxLength(5)
                .HasColumnName("freq");

            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
