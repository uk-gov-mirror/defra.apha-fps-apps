using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class AdditionalCostsMap : IEntityTypeConfiguration<AdditionalCosts>
    {
        public void Configure(EntityTypeBuilder<AdditionalCosts> entity)
        {
            entity.HasKey(e => e.AcCounter).HasName("pk_my_tbladditionalcosts");

            entity.ToTable("my_tbladditionalcosts", "mabarchive");

            entity.Property(e => e.AcCounter).HasColumnName("ac_counter");
            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");
            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");
            entity.Property(e => e.Freq)
                .HasMaxLength(5)
                .HasColumnName("freq");
            entity.Property(e => e.Itemcost)
                .HasPrecision(19, 4)
                .HasColumnName("itemcost");
            entity.Property(e => e.Jobcode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}
