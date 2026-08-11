using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestsRequiredByRcViewMap : IEntityTypeConfiguration<TestsRequiredByRcView>
    {
        public void Configure(EntityTypeBuilder<TestsRequiredByRcView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vqrytestsrequiredbyrc_export", "fps");

            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.TestCode).HasColumnName("testcode");
            builder.Property(e => e.ItemDescription).HasColumnName("itemdescription");
            builder.Property(e => e.ProjectedTotal).HasColumnName("projectedtotal");
            builder.Property(e => e.UnitPrice).HasColumnName("unitprice");
        }
    }
}
