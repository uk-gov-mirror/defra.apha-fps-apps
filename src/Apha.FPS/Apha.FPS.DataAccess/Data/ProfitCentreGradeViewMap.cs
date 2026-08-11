using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{

    public class ProfitCentreGradeViewMap : IEntityTypeConfiguration<ProfitCentreGradeView>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreGradeView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vprofitcentregrade", "fps");

            builder.Property(e => e.PcGrade).HasColumnName("pcgrade");
            builder.Property(e => e.DivisionGrade).HasColumnName("divisiongrade");
            builder.Property(e => e.GradeCode).HasColumnName("gradecode");
            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.ChargeRate).HasPrecision(19, 4).HasColumnName("chargerate");
            builder.Property(e => e.DirectRate).HasPrecision(19, 4).HasColumnName("directrate");
            builder.Property(e => e.PayRate).HasPrecision(19, 4).HasColumnName("payrate");
            builder.Property(e => e.Npr).HasPrecision(19, 4).HasColumnName("npr");
            builder.Property(e => e.Ohr).HasPrecision(19, 4).HasColumnName("ohr");
            builder.Property(e => e.HrsAvailable).HasPrecision(19, 4).HasColumnName("hrsavailable");
            builder.Property(e => e.OldChargeRate).HasPrecision(19, 4).HasColumnName("oldchargerate");
            builder.Property(e => e.DefraChargeRate).HasPrecision(19, 4).HasColumnName("defrachargerate");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
        }
    }
}