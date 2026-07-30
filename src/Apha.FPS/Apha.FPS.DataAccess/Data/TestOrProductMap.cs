using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestOrProductMap : IEntityTypeConfiguration<TestOrProduct>
    {
        public void Configure(EntityTypeBuilder<TestOrProduct> entity)
        {
            entity.HasKey(e => new { e.ItemCode, e.FpsYear })
                  .HasName("pk_testorproduct");

            entity.ToTable("testorproduct", "fps");

            entity.Property(e => e.ItemCode)
                  .HasMaxLength(20)
                  .HasColumnName("itemcode");

            entity.Property(e => e.FpsYear)
                  .HasColumnName("fpsyear");

            entity.Property(e => e.ItemDescription)
                  .HasMaxLength(200)
                  .HasColumnName("itemdescription");

            entity.Property(e => e.TestManager)
                  .HasMaxLength(50)
                  .HasColumnName("testmanager");

            entity.Property(e => e.JobStatus)
                  .HasMaxLength(2)
                  .HasColumnName("jobstatus");

            entity.Property(e => e.UnitPriceVla)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("unitpricevla");

            entity.Property(e => e.PriceAhvg)
                  .HasColumnType("money")
                  .HasColumnName("priceahvg");

            //   CHECK constraint (owner IN ('PT','PA','SD','LT')) enforced at service layer
            entity.Property(e => e.Owner)
                  .HasMaxLength(2)
                  .HasColumnName("owner");

            entity.Property(e => e.ChargeMethod)
                  .HasMaxLength(5)
                  .HasColumnName("chargemethod");

            entity.Property(e => e.ShortDescription)
                  .HasMaxLength(18)
                  .IsFixedLength()
                  .HasColumnName("shortdescription");

            entity.Property(e => e.DefraUnitPrice)
                  .HasDefaultValueSql("0")
                  .HasColumnType("money")
                  .HasColumnName("defraunitprice");
        }
    }
}
