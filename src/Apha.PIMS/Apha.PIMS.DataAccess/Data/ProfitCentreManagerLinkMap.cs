using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProfitCentreManagerLinkMap : IEntityTypeConfiguration<ProfitCentreManagerLink>
    {
        public void Configure(EntityTypeBuilder<ProfitCentreManagerLink> entity)
        {
            entity.HasKey(e => new { e.ProfitCentre, e.Manager }).HasName("pk_tblprofitcentre_manager_link");

            entity.ToTable("tblprofitcentre_manager_link", "mabarchive");

            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");

            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
        }
    }
}
