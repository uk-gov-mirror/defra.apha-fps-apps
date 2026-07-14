using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ContributionSummaryViewMap : IEntityTypeConfiguration<ContributionSummaryView>
    {
        public void Configure(EntityTypeBuilder<ContributionSummaryView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vqryfrmtimesellerpc", "fps");

            entity.Property(e => e.ContTarget)
                .HasColumnType("money")
                .HasColumnName("conttarget");

            entity.Property(e => e.SellingPc)
                .HasMaxLength(50)
                .HasColumnName("sellingpc");

            entity.Property(e => e.ChargeRate)
                .HasColumnType("money")
                .HasColumnName("chargerate");

            entity.Property(e => e.Ohr)
                .HasColumnType("money")
                .HasColumnName("ohr");

            entity.Property(e => e.SumOfGenBid)
                .HasColumnType("money")
                .HasColumnName("sumofgenbid");

            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");

            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");

            entity.Property(e => e.WgGrade)
                .HasMaxLength(20)
                .HasColumnName("wggrade");

            entity.Property(e => e.AppHours).HasColumnName("apphours");

            entity.Property(e => e.Hrs).HasColumnName("hrs");

            entity.Property(e => e.AvHrs).HasColumnName("avhrs");

            entity.Property(e => e.Fec)
                .HasColumnType("numeric")
                .HasColumnName("fec");

            entity.Property(e => e.AppFec)
                .HasColumnType("numeric")
                .HasColumnName("appfec");

            entity.Property(e => e.Contribution)
                .HasColumnType("numeric")
                .HasColumnName("contribution");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .HasColumnName("dt2username");

            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
        }
    }
}
