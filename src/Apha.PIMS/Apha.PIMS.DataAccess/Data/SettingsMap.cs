using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class SettingsMap : IEntityTypeConfiguration<Settings>
    {
        public void Configure(EntityTypeBuilder<Settings> entity)
        {
            entity.HasKey(e => e.Id).HasName("aaaaatbl_settings_pk");

            entity.ToTable("tbl_settings", "mabarchive");

            entity.HasIndex(e => e.Id, "settingid");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasColumnName("id");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.Setting)
                .HasMaxLength(255)
                .HasColumnName("setting");
            entity.Property(e => e.Testsetting)
                .HasMaxLength(255)
                .HasColumnName("testsetting");
            entity.Property(e => e.Userupdateable)
                .HasDefaultValue(false)
                .HasColumnName("userupdateable");
        }
    }
}
