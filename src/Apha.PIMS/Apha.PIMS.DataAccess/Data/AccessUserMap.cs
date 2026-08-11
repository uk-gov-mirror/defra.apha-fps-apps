using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class AccessUserMap : IEntityTypeConfiguration<AccessUser>
    {
        public void Configure(EntityTypeBuilder<AccessUser> entity)
        {
            entity.HasKey(e => new { e.SystemId, e.NtLogin }).HasName("pk_tblaccessusers");

            entity.ToTable("tblaccessusers", "mabarchive");

            entity.Property(e => e.SystemId)
                .HasColumnName("systemid");

            entity.Property(e => e.NtLogin)
                .HasMaxLength(50)
                .HasColumnName("ntlogin");

            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.Property(e => e.Dt2Login)
                .HasMaxLength(50)
                .HasColumnName("dt2login");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
        }
    }
}
