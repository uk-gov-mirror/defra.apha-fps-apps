using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DivisionMap : IEntityTypeConfiguration<Division>
    {
        public void Configure(EntityTypeBuilder<Division> entity)
        {
            entity.HasKey(e => e.DivName).HasName("pk__tlkpdivision__10566f31");

            entity.ToTable("tlkpdivision", "fps", tb => tb.HasComment("Organizational divisions within agencies for cost allocation and reporting."));

            entity.Property(e => e.DivName)
                .HasMaxLength(10)
                .HasComment("Division name. Primary key (case-insensitive text).")
                .HasColumnName("divname");

            entity.Property(e => e.DivisionId)
                .HasComment("Division identifier (regular integer field, not auto-generated).")
                .HasColumnName("divisionid");

            entity.Property(e => e.AgencyId)
                .HasComment("Parent agency identifier (foreign key to fps.tlkpagency).")
                .HasColumnName("agencyid");

            entity.Property(e => e.CentOverhead)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasComment("Central overhead cost allocation.")
                .HasColumnName("centoverhead");
        }
    }
}
