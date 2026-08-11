using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class PublicationTypeMap: IEntityTypeConfiguration<PublicationType>
    {
        public void Configure(EntityTypeBuilder<PublicationType> entity)
        {
            entity.HasKey(e => e.Type).HasName("pk_tlkppublicationtype");

            entity.ToTable("tlkppublicationtype", "mabarchive");

            entity.Property(e => e.Type)
                .ValueGeneratedNever()
                .HasMaxLength(3)
                .HasColumnName("type");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        }
    }
}