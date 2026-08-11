using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class MyTblProfitCentreMap : IEntityTypeConfiguration<MabProfitCentre>
    {
        public void Configure(EntityTypeBuilder<MabProfitCentre> entity)
        {
            entity.HasKey(e => new { e.Year, e.ProfitCentre }).HasName("pk_my_tblprofitcentre");
            entity.ToTable("my_tblprofitcentre", "mabarchive");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.ProfitCentre)
            .HasMaxLength(50)
            .HasColumnName("profitcentre");
            entity.Property(e => e.ContTarget)
            .HasColumnType("money")
            .HasColumnName("conttarget");
            entity.Property(e => e.Division)
            .HasMaxLength(10)
            .HasColumnName("division");
            entity.Property(e => e.DivisionId).HasColumnName("divisionid");
            entity.Property(e => e.ProfitCentreHead)
            .HasMaxLength(50)
            .HasColumnName("profitcentrehead");
            entity.Property(e => e.ProfitCentreName)
            .HasMaxLength(40)
            .HasColumnName("profitcentrename");
        }


    }
}
