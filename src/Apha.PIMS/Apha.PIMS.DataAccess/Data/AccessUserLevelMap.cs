using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class AccessUserLevelMap : IEntityTypeConfiguration<AccessUserLevel>
    {
        public void Configure(EntityTypeBuilder<AccessUserLevel> entity)
        {
            entity.HasKey(e => new { e.SystemId, e.NtLogin, e.AccessLevelId }).HasName("pk_tblaccessusers_levels");

            entity.ToTable("tblaccessusers_levels", "mabarchive");

            entity.Property(e => e.SystemId)
                .HasColumnName("systemid");

            entity.Property(e => e.NtLogin)
                .HasMaxLength(50)
                .HasColumnName("ntlogin");

            entity.Property(e => e.AccessLevelId)
                .HasColumnName("accesslevelid");
        }
    }
}
