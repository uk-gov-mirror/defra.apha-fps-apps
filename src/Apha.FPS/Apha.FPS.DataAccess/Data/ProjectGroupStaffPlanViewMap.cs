using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectGroupStaffPlanViewMap : IEntityTypeConfiguration<ProjectGroupStaffPlanView>
    {
        public void Configure(EntityTypeBuilder<ProjectGroupStaffPlanView> entity)
        {
            entity.HasNoKey().ToView("vpvtprojectgroupmgrplan", "fps");

            entity.Property(e => e.ProjectGroup)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");

            entity.Property(e => e.ResourceCentre)
                .HasMaxLength(50)
                .HasColumnName("resourcecentre");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(20)
                .HasColumnName("workgroup");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.Manager)
                .HasMaxLength(100)
                .HasColumnName("manager");

            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");

            entity.Property(e => e.ProjectStatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");

            entity.Property(e => e.Hrs)
                .HasColumnName("hrs");

            entity.Property(e => e.ChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");

            entity.Property(e => e.Fee)
                .HasPrecision(19, 4)
                .HasColumnName("fee");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
        }
    }
}
