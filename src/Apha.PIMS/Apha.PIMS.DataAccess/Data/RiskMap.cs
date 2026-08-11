using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class RiskMap : IEntityTypeConfiguration<Risk>
    {
        public void Configure(EntityTypeBuilder<Risk> entity)
        {
            entity.HasKey(e => e.RiskId).HasName("pk_tlkprisk");

            entity.ToTable("tlkprisk", "mabarchive");

            entity.Property(e => e.RiskId)
                .ValueGeneratedNever()
                .HasColumnName("riskid");
            entity.Property(e => e.RiskRating)
                .HasMaxLength(15)
                .HasColumnName("riskrating");
        }
    }
}
