using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    /// <summary>
    /// EF Core entity type configuration for the Grade entity.
    /// Maps to fps.grade (partitioned by fpsyear). Composite PK: (gradecode, fpsyear).
    /// </summary>
    public class GradeMap : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> entity)
        {
            entity.HasKey(e => new { e.GradeCode, e.FpsYear }).HasName("pk_grade");

            entity.ToTable("grade", "fps");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.DescLong)
                .HasMaxLength(30)
                .HasColumnName("desc_long");

            entity.Property(e => e.AvSalary)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("avsalary");

            entity.Property(e => e.PactCode)
                .HasMaxLength(50)
                .HasColumnName("pactcode");

            entity.Property(e => e.AvLeaveHrs)
                .HasDefaultValueSql("0")
                .HasColumnName("avleavehrs");

            entity.Property(e => e.AvSickHrs)
                .HasDefaultValueSql("0")
                .HasColumnName("avsickhrs");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
