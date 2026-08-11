using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TotalBusinessOverheadsMap : IEntityTypeConfiguration<TotalBusinessOverheads>
    {
        public void Configure(EntityTypeBuilder<TotalBusinessOverheads> entity)
        {
            entity.HasKey(e => e.FpsYear).HasName("pk_tbltotalbusinessoverheads");

            entity.ToTable("tbltotalbusinessoverheads", "fps");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            entity.Property(e => e.BusinessOverheads)
                .HasPrecision(19, 4)
                .HasColumnName("totalbusinessoverheads");
        }
    }
}
