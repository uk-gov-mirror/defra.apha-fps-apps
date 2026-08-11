using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestRequirementMap : IEntityTypeConfiguration<TestRequirement>
    {
        public void Configure(EntityTypeBuilder<TestRequirement> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.FpsYear }).HasName("pk_tlkptestreqmt");

            entity.ToTable("tlkptestreqmt", "fps");

            entity.HasIndex(e => e.TestBuyerCode, "reference10");
            entity.HasIndex(e => e.ProjectBuyerCode, "reference19");
            entity.HasIndex(e => e.TestCode, "reference31");

            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Active)
                .HasDefaultValue((short)1)
                .HasColumnName("active");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode)
                .HasMaxLength(50)
                .HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode)
                .HasMaxLength(50)
                .HasColumnName("testbuyercode");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(19, 4)
                .HasColumnName("unitprice");
        }
    }
}
