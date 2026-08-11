using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WgStaffPlanViewMap : IEntityTypeConfiguration<WgStaffPlanView>
    {
        public void Configure(EntityTypeBuilder<WgStaffPlanView> entity)
        {
            entity.HasNoKey().ToView("vpvtworkgroupstaffplan", "fps");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.Manager)
                .HasColumnName("manager");

            entity.Property(e => e.Program)
                .HasColumnName("program");

            entity.Property(e => e.JobCode)
                .HasColumnName("jobcode");

            entity.Property(e => e.ProjectStatus)
                .HasColumnName("projectstatus");

            entity.Property(e => e.PlannedHours)
                .HasColumnName("hrs");

            entity.Property(e => e.Fee)
                .HasColumnType("money")
                .HasColumnName("fee");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
