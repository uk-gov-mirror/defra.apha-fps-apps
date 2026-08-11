using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class FrequencyMap : IEntityTypeConfiguration<Frequency>
    {
        public void Configure(EntityTypeBuilder<Frequency> entity)
        {
            entity.HasKey(e => e.FrequencyId).HasName("pk_tlkpfrequency");

            entity.ToTable("tlkpfrequency", "mabarchive");

            entity.Property(e => e.FrequencyId)
                .ValueGeneratedNever()
                .HasColumnName("frequencyid");

            entity.Property(e => e.FrequencyValue)
                .HasMaxLength(50)
                .HasColumnName("frequency");
        }
    }
}