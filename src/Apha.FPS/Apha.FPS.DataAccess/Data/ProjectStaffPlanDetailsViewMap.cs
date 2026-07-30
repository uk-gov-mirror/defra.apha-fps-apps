using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectStaffPlanDetailsViewMap : IEntityTypeConfiguration<ProjectStaffPlanDetailsView>
    {
        public void Configure(EntityTypeBuilder<ProjectStaffPlanDetailsView> entity)
        {
            entity.HasNoKey().ToView("vwprojectstaffplandetails", "fps");

            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");

            entity.Property(e => e.Name)
                .HasColumnName("name");

            entity.Property(e => e.Manager)
                .HasColumnName("manager");

            entity.Property(e => e.ProjectStatus)
                .HasColumnName("projectstatus");

            entity.Property(e => e.PlannedHours)
                .HasColumnName("plannedhours");

            entity.Property(e => e.ChargeRate)
                .HasColumnType("money")
                .HasColumnName("chargerate");

            entity.Property(e => e.Cost)
                .HasColumnType("money")
                .HasColumnName("cost");

            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(20)
                .HasColumnName("profitcentre");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
