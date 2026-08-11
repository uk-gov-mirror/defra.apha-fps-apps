using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestsRequiredByWgViewMap : IEntityTypeConfiguration<TestsRequiredByWgView>
    {
        public void Configure(EntityTypeBuilder<TestsRequiredByWgView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vqrytestsrequiredbywg_export", "fps");

            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.WorkGroup).HasColumnName("workgroup");
            builder.Property(e => e.TestCode).HasColumnName("testcode");
            builder.Property(e => e.ItemDescription).HasColumnName("itemdescription");
            builder.Property(e => e.ProjectedTotal).HasColumnName("projectedtotal");
            builder.Property(e => e.UnitPrice).HasColumnName("unitprice");
        }
    }
}
