using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestPlanCostBreakdownMap : IEntityTypeConfiguration<TestPlanCostBreakdownView>
    {
        public void Configure(EntityTypeBuilder<TestPlanCostBreakdownView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_testplan_cost_breakdown", "fps");

            builder.Property(e => e.TestCode).HasColumnName("testcode");
            builder.Property(e => e.ShortDescription).HasColumnName("shortdescription");
            builder.Property(e => e.PlanTotal).HasColumnType("numeric").HasColumnName("plan_total");
            builder.Property(e => e.ReqTotalCost).HasColumnType("numeric").HasColumnName("req_totalcost");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
