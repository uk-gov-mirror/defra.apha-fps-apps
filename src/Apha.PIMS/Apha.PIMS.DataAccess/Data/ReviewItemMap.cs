using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ReviewItemMap : IEntityTypeConfiguration<ReviewItem>
    {
        public void Configure(EntityTypeBuilder<ReviewItem> entity)
        {
            entity.HasKey(e => e.ItemId).HasName("pk_tlkpreviewitem");

            entity.ToTable("tlkpreviewitem", "mabarchive");

            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("itemid");

            entity.Property(e => e.Item)
                .HasMaxLength(50)
                .HasColumnName("item");
        }
    }
}
