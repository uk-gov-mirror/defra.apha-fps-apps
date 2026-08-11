using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProfitCentreGradeMap : IEntityTypeConfiguration<ProfitCentreGrade>
    {


        public void Configure(EntityTypeBuilder<ProfitCentreGrade> entity)
        {
            entity.HasKey(e => new { e.PcGrade, e.FpsYear }).HasName("pk_profitcentregrade");

            entity.ToTable("profitcentregrade", "fps");

            entity.HasIndex(e => e.ProfitCentre, "profitcentregrade_profitcentre")
                .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

            entity.Property(e => e.PcGrade)
                .HasMaxLength(20)
                .HasColumnName("pcgrade");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("directrate");
            entity.Property(e => e.DivisionGrade)
                .HasMaxLength(10)
                .HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsAvailable)
                .HasDefaultValue(0.0)
                .HasColumnName("hrsavailable");
            entity.Property(e => e.NPR)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("npr");
            entity.Property(e => e.OHR)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
        }
    }
}
