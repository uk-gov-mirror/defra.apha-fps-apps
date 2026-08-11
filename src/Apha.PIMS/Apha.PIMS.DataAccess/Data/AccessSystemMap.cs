using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class AccessSystemMap : IEntityTypeConfiguration<AccessSystem>
    {
        public void Configure(EntityTypeBuilder<AccessSystem> entity)
        {
            entity.HasKey(e => e.SystemId).HasName("pk_tblaccesssystems");

            entity.ToTable("tblaccesssystems", "mabarchive");

            entity.Property(e => e.SystemId)
                .ValueGeneratedNever()
                .HasColumnName("systemid");

            entity.Property(e => e.SystemName)
                .HasMaxLength(50)
                .HasColumnName("systemname");
        }
    }
}
