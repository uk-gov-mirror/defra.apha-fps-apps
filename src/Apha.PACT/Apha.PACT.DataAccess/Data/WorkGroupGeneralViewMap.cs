using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class WorkGroupGeneralViewMap : IEntityTypeConfiguration<WorkGroupGeneralView>
    {
        public void Configure(EntityTypeBuilder<WorkGroupGeneralView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vworkgroup_general", "fps");

            builder.Property(e => e.WorkGroup).HasColumnName("workgroup");
            builder.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
