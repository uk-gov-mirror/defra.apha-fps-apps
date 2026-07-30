using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class TestReqmtMap : IEntityTypeConfiguration<TestReqmt>
    {
        public void Configure(EntityTypeBuilder<TestReqmt> entity)
        {
            entity.HasKey(e => new { e.Year, e.Testcode, e.Buyer }).HasName("pk_my_tlkptestreqmt");

            entity.ToTable("my_tlkptestreqmt", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Testcode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.Norequired).HasColumnName("norequired");
            entity.Property(e => e.Projectbuyercode)
                .HasMaxLength(50)
                .HasColumnName("projectbuyercode");
            entity.Property(e => e.Source)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("source");
            entity.Property(e => e.Testbuyercode)
                .HasMaxLength(50)
                .HasColumnName("testbuyercode");
            entity.Property(e => e.Unitprice)
                .HasPrecision(19, 4)
                .HasColumnName("unitprice");
        }
    }
}
