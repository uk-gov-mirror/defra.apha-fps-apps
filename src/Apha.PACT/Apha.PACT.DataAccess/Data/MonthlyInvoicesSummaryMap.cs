using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class MonthlyInvoicesSummaryMap : IEntityTypeConfiguration<MonthlyInvoicesSummary>
    {
        public void Configure(EntityTypeBuilder<MonthlyInvoicesSummary> entity)
        {
            entity
               .HasNoKey()
               .ToView("vwinvoicebymonth", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.MonthlyAmount)
                .HasPrecision(19, 4)
                .HasColumnName("monthlyamount");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
        }
    }
}
