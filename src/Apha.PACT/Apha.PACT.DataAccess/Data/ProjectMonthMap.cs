using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ProjectMonthMap : IEntityTypeConfiguration<ProjectMonth>
    {
        public void Configure(EntityTypeBuilder<ProjectMonth> entity)
        {
            entity.HasKey(e => new { e.Project, e.MonthNo, e.FpsYear }).HasName("pk_projectmonth");

            entity.ToTable("projectmonth", "fps");

            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.CostProfile)
                .HasPrecision(19, 4)
                .HasColumnName("costprofile");
        }
    }
}