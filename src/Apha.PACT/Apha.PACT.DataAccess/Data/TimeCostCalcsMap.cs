using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TimeCostCalcsMap : IEntityTypeConfiguration<TimeCostCalcs>
    {
        public void Configure(EntityTypeBuilder<TimeCostCalcs> entity)
        {
            entity.HasKey(e => new { e.WorkGroup, e.JobCode, e.Project, e.Month, e.StaffId, e.FpsYear }).HasName("pk_timecostcalcs");

            entity.ToTable("timecostcalcs", "fps");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.JobCode)
                .HasMaxLength(50)
                .HasColumnName("jobcode");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ChargeRate)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate");
            entity.Property(e => e.Class)
                .HasMaxLength(255)
                .HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasColumnName("division");
            entity.Property(e => e.JobCodeOld)
                .HasMaxLength(14)
                .HasColumnName("jobcodeold");
            entity.Property(e => e.Pay)
                .HasPrecision(19, 4)
                .HasColumnName("pay");
            entity.Property(e => e.NonPay)
                .HasPrecision(19, 4)
                .HasColumnName("nonpay");
            entity.Property(e => e.Overhead)
                .HasPrecision(19, 4)
                .HasColumnName("overhead");
        }
    }
}
