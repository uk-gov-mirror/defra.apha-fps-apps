using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRCCostMap : IEntityTypeConfiguration<TestRCCost>
    {
        public void Configure(EntityTypeBuilder<TestRCCost> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.ProfitCentre, e.FpsYear })
                  .HasName("pk_tbltestrccost");

            entity.ToTable("tbltestrccost", "fps");

            entity.Property(e => e.TestCode)
                  .HasMaxLength(20)
                  .HasColumnName("testcode");

            entity.Property(e => e.ProfitCentre)
                  .HasMaxLength(50)
                  .HasColumnName("profitcentre");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            entity.Property(e => e.Price)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("price");
        }
    }
}
