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
            entity.HasNoKey();
           
            entity.ToView("vpactprojectyearcosts", "mabarchive");

            entity.Property(e => e.Project)
                  .HasMaxLength(20)
                  .HasColumnName("project");

            entity.Property(e => e.Year)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("year");

            entity.Property(e => e.MonthNo)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("monthno");

            entity.Property(e => e.SubContracts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("subcontracts");

            entity.Property(e => e.Animals)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("animals");

            entity.Property(e => e.Tests)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("tests");

            entity.Property(e => e.Pay)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("pay");

            entity.Property(e => e.NonPayOH)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonpayoh");

            entity.Property(e => e.TotalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("totalcosts");

            entity.Property(e => e.TimeCost)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("timecost");

            entity.Property(e => e.Hours)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("hours");
        }
    }
}
