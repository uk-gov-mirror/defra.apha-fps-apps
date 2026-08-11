using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DivisionGradeMap : IEntityTypeConfiguration<DivisionGrade>
    {
        public void Configure(EntityTypeBuilder<DivisionGrade> entity)
        {
            entity.HasKey(e => new { e.DivisionGradeCode, e.FpsYear }).HasName("pk_divisiongrade");

            entity.ToTable("divisiongrade", "fps");

            entity.Property(e => e.DivisionGradeCode)
                .HasMaxLength(10)
                .HasColumnName("divisiongrade");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            entity.Property(e => e.ChargeRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");

            entity.Property(e => e.DirectRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("directrate");

            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasColumnName("division");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.Npr)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("npr");

            entity.Property(e => e.Ohr)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("ohr");

            entity.Property(e => e.PayRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("payrate");
        }
    }
}
