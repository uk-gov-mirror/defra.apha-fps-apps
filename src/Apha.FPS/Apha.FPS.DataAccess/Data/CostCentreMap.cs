using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class CostCentreMap : IEntityTypeConfiguration<CostCentre>
    {
        public void Configure(EntityTypeBuilder<CostCentre> entity)
        {
            entity.HasKey(e => new { e.CostCentreNo, e.FpsYear }).HasName("pk_costcentre");

            entity.ToTable("costcentre", "fps");

            entity.Property(e => e.CostCentreNo)
                .ValueGeneratedNever()
                .HasColumnName("costcentre");

            entity.Property(e => e.ProfitCentre)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("profitcentre");

            entity.Property(e => e.FpsYear)
                .ValueGeneratedNever()
                .HasColumnName("fpsyear");
        }
    }
}
