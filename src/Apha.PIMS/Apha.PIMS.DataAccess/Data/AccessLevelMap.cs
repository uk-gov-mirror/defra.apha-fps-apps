using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class AccessLevelMap : IEntityTypeConfiguration<AccessLevel>
    {
        public void Configure(EntityTypeBuilder<AccessLevel> entity)
        {
            entity.HasKey(e => new { e.SystemId, e.AccessLevelId }).HasName("pk_tblaccesslevels");

            entity.ToTable("tblaccesslevels", "mabarchive");

            entity.Property(e => e.SystemId)
                .HasColumnName("systemid");

            entity.Property(e => e.AccessLevelId)
                .HasColumnName("accesslevelid");

            entity.Property(e => e.AccessLevelName)
                .HasMaxLength(50)
                .HasColumnName("accesslevel");
        }
    }
}
