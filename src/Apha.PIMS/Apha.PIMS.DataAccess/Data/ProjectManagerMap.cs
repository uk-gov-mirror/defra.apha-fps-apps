using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectManagerMap : IEntityTypeConfiguration<ProjectManager>
    {
        public void Configure(EntityTypeBuilder<ProjectManager> entity)
        {
            entity.HasKey(e => e.Projectmanager).HasName("pk_tblprojectmanager");

            entity.ToTable("tblprojectmanager", "mabarchive");

            entity.Property(e => e.Projectmanager)
                .HasMaxLength(50)
                .HasColumnName("projectmanager");
            entity.Property(e => e.Disable)
                .HasDefaultValue(false)
                .HasColumnName("disable");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Mnumber)
                .HasMaxLength(10)
                .HasColumnName("mnumber");

            entity.Property(e => e.LoginEmail)
                .HasMaxLength(255)
                .HasColumnName("loginemail");
        }
    }
}
