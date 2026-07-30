using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffJobRmViewMap : IEntityTypeConfiguration<StaffJobRmView>
    {


        public void Configure(EntityTypeBuilder<StaffJobRmView> entity)
        {
            entity
                   .HasNoKey()
                   .ToView("vtblstaffjob_rm", "fps");

            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
        }
    }
}
