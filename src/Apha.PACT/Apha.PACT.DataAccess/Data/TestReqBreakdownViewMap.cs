using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestReqBreakdownViewMap : IEntityTypeConfiguration<TestReqBreakdownView>
    {
        public void Configure(EntityTypeBuilder<TestReqBreakdownView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vtestreqbreakdown", "fps");

            builder.Property(e => e.TestCode).HasColumnName("testcode");
            builder.Property(e => e.ShortDescription).HasColumnName("shortdescription");
            builder.Property(e => e.Program).HasColumnName("program");
            builder.Property(e => e.Project).HasColumnName("project");
            builder.Property(e => e.Pc).HasColumnName("pc");
            builder.Property(e => e.WorkG).HasColumnName("workg");
            builder.Property(e => e.WgPrice).HasColumnType("money").HasColumnName("wgprice");
            builder.Property(e => e.TotalCost).HasColumnType("money").HasColumnName("totalcost");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
