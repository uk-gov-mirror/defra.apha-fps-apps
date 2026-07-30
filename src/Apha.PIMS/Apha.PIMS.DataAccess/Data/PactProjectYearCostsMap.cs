using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class PactProjectYearCostsMap : IEntityTypeConfiguration<PactProjectYearCosts>
    {
        private const string ColumnTypeMoney = "money";
        private const string ColumnTypeDouble = "double precision";

        public void Configure(EntityTypeBuilder<PactProjectYearCosts> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpactprojectyearcosts", "mabarchive");

            entity.Property(e => e.Animals).HasColumnName("animals");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.NonPayOH).HasColumnName("nonpayoh");
            entity.Property(e => e.Pay).HasColumnName("pay");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.SubContracts).HasColumnName("subcontracts");
            entity.Property(e => e.Tests).HasColumnName("tests");
            entity.Property(e => e.TimeCost).HasColumnName("timecost");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}
