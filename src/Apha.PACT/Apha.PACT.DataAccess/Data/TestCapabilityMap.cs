using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class TestCapabilityMap : IEntityTypeConfiguration<TestCapability>
    {
        public void Configure(EntityTypeBuilder<TestCapability> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.WorkGroup, e.FpsYear }).HasName("pk_tlkptestcapability");

            entity.ToTable("tlkptestcapability", "fps");

            entity.HasIndex(e => e.PlanPortfolio, "tlkptestcapability_planportfol");

            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlanPortfolio)
                .HasMaxLength(20)
                .HasColumnName("planportfolio");
            entity.Property(e => e.PredOutturn)
                .HasDefaultValue(0.0)
                .HasColumnName("predoutturn");
            entity.Property(e => e.SmsCode)
                .HasMaxLength(50)
                .HasColumnName("smscode");
            entity.Property(e => e.Sop)
                .HasMaxLength(50)
                .HasColumnName("sop");
            entity.Property(e => e.UnitCost)
                .HasDefaultValue(0m)
                .HasPrecision(19, 4)
                .HasColumnName("unitcost");
        }
    }
}
