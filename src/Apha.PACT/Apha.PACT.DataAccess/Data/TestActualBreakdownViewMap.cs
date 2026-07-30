using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestActualBreakdownViewMap : IEntityTypeConfiguration<TestActualBreakdownView>
    {
        public void Configure(EntityTypeBuilder<TestActualBreakdownView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vqrytestsactualbreakdown", "fps");

            builder.Property(e => e.Program).HasColumnName("program");
            builder.Property(e => e.Buyer).HasColumnName("buyer");
            builder.Property(e => e.Portfolio).HasColumnName("portfolio");
            builder.Property(e => e.WorkGroup).HasColumnName("workgroup");
            builder.Property(e => e.TestCode).HasColumnName("testcode");
            builder.Property(e => e.ShortDescription).HasColumnName("shortdescription");
            builder.Property(e => e.Month).HasColumnName("month").HasConversion<double?>();
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear").HasConversion<double>();
            builder.Property(e => e.PCPrice).HasColumnType("numeric").HasColumnName("pcprice");
            builder.Property(e => e.PCCost).HasColumnType("numeric").HasColumnName("pccost");
            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
        }
    }
}
