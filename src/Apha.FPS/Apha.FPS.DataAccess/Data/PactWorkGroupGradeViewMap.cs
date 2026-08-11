using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PactWorkGroupGradeViewMap : IEntityTypeConfiguration<PactWorkGroupGradeView>
    {


        public void Configure(EntityTypeBuilder<PactWorkGroupGradeView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpactworkgroupgrade", "fps");

            entity.Property(e => e.AvSalary)
                .HasPrecision(19, 4)
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasPrecision(19, 4)
                .HasColumnName("chargerate_wg");
            entity.Property(e => e.DirectRateWg)
                .HasPrecision(19, 4)
                .HasColumnName("directrate_wg");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasPrecision(19, 4)
                .HasColumnName("npr_wg");
            entity.Property(e => e.OhrWg)
                .HasPrecision(19, 4)
                .HasColumnName("ohr_wg");
            entity.Property(e => e.PayRateWg)
                .HasPrecision(19, 4)
                .HasColumnName("payrate_wg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wg_grade");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
