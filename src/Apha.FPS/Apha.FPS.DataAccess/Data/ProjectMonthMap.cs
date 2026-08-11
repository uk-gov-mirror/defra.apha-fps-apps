using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectMonthMap : IEntityTypeConfiguration<ProjectMonth>
    {
        public void Configure(EntityTypeBuilder<ProjectMonth> entity)
        {
            entity.HasKey(e => new { e.Project, e.MonthNo })
                .HasName("pk_projectmonth_1__16");

            entity.ToTable("projectmonth", "fps");

            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.CostProfile).HasPrecision(19, 4).HasColumnName("costprofile");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
