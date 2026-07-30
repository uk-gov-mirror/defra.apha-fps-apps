using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRequirementRCCostMap : IEntityTypeConfiguration<TestRequirementRCCost>
    {
        public void Configure(EntityTypeBuilder<TestRequirementRCCost> entity)
        {
            //   per pk_tbltestrequirementrccost constraint
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.ProfitCentre, e.FpsYear })
                  .HasName("pk_tbltestrequirementrccost");

            entity.ToTable("tbltestrequirementrccost", "fps");

            entity.Property(e => e.TestCode)
                  .HasMaxLength(20)
                  .HasColumnName("testcode");

            entity.Property(e => e.Buyer)
                  .HasMaxLength(20)
                  .HasColumnName("buyer");

            entity.Property(e => e.ProfitCentre)
                  .HasMaxLength(50)
                  .HasColumnName("profitcentre");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            entity.Property(e => e.Price)
                  .HasColumnType("money")
                  .HasColumnName("price");
        }
    }
}
