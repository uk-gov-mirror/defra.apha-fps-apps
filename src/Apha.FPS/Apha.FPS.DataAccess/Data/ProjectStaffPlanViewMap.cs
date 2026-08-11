using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectStaffPlanViewMap : IEntityTypeConfiguration<ProjectStaffPlanView>
    {
        public void Configure(EntityTypeBuilder<ProjectStaffPlanView> entity)
        {
            entity.HasNoKey().ToView("vprojectstaffplan", "fps");

            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");

            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");

            entity.Property(e => e.Contract)
                .HasMaxLength(20)
                .HasColumnName("contract");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");

            entity.Property(e => e.PlannedHours)
                .HasColumnName("plannedhours");

            entity.Property(e => e.ChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");

            entity.Property(e => e.Cost)
                .HasPrecision(19, 4)
                .HasColumnName("cost");

            entity.Property(e => e.PayCost)
                .HasPrecision(19, 4)
                .HasColumnName("paycost");

            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(20)
                .HasColumnName("profitcentre");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            entity.Property(e => e.WgGrade)
                .HasMaxLength(20)
                .HasColumnName("wggrade");

            entity.Property(e => e.PcGrade)
                .HasMaxLength(20)
                .HasColumnName("pcgrade");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
