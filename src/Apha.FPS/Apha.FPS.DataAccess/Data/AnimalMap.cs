using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AnimalMap : IEntityTypeConfiguration<Animal>
    {


        public void Configure(EntityTypeBuilder<Animal> entity)
        {
            entity.HasKey(e => new { e.AnimalType, e.FpsYear }).HasName("pk_tblanimals");

            entity.ToTable("tblanimals", "fps");

            entity.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.DailyRate)
                .HasPrecision(19, 4)
                .HasColumnName("dailyrate");
            entity.Property(e => e.DefraDailyRate)
                .HasPrecision(19, 4)
                .HasColumnName("defradailyrate");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlanByWeek)
                .HasDefaultValue(false)
                .HasColumnName("planbyweek");
            entity.Property(e => e.SecurityLevel)
                .HasMaxLength(50)
                .HasColumnName("security_level");
            entity.Property(e => e.Species)
                .HasMaxLength(50)
                .HasColumnName("species");
        }
    }
}
