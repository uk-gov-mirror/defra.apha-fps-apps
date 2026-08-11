using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WorkgroupGradeMap : IEntityTypeConfiguration<WorkgroupGrade>
    {


        public void Configure(EntityTypeBuilder<WorkgroupGrade> entity)
        {
            entity.HasKey(e => new { e.WgGrade, e.FpsYear }).HasName("pk_workgroupgrade");

            entity.ToTable("workgroupgrade", "fps");

            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wggrade");
            entity.Property(e => e.AvSalary)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasPrecision(19, 4)
                .HasColumnName("chargeratewg");
            entity.Property(e => e.DirectRateWg)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("directratewg");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("nprwg");
            entity.Property(e => e.OhrWg)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("ohrwg");
            entity.Property(e => e.PayRateWg)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("payratewg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.Workgroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
