using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class YearlyFinancialDataMap : IEntityTypeConfiguration<YearlyFinancialData>
    {
        private const string ColumnTypeTimestamp = "timestamp without time zone";
        private const string ColumnTypeMoney = "money";
        private const string ColumnTypeDouble = "double precision";

        public void Configure(EntityTypeBuilder<YearlyFinancialData> entity)
        {
            
            entity.HasKey(e => new { e.Year, e.Project })
                  .HasName("pk_my_tlkpprojectradtrackdata");

            entity.ToTable("my_tlkpprojectradtrackdata", "mabarchive");

            
            entity.Property(e => e.Year)
                  .HasColumnName("year");

            
            entity.Property(e => e.Project)
                  .HasMaxLength(20)
                  .HasColumnName("project");

            
            entity.Property(e => e.BfBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("bfbudget");

            entity.Property(e => e.PyBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("pybudget");

            entity.Property(e => e.Seedcorn)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("seedcorn");

            entity.Property(e => e.PayCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("paycosts");

            entity.Property(e => e.NonPayOhCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonpayohcosts");

            entity.Property(e => e.TestCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("testcosts");

            entity.Property(e => e.AnimalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("animalcosts");

            entity.Property(e => e.NonAnimalCosts)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("nonanimalcosts");

            entity.Property(e => e.Adjustment)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("adjustment");

            entity.Property(e => e.ActualExpenditure)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("actualexpenditure");

           
            entity.Property(e => e.VlaBudget)
                  .HasColumnType(ColumnTypeMoney)
                  .HasColumnName("vla_budget");

           
            entity.Property(e => e.ManHours)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("manhours");

            entity.Property(e => e.ManDays)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("mandays");

            entity.Property(e => e.ManYears)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("manyears");

            entity.Property(e => e.ActualManYears)
                  .HasColumnType(ColumnTypeDouble)
                  .HasColumnName("actualmanyears");

            
            entity.Property(e => e.ManHoursChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("manhourschanged");

            entity.Property(e => e.PayCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("paycostschanged");

            entity.Property(e => e.NonPayOhCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("nonpayohcostschanged");

            entity.Property(e => e.TestCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("testcostschanged");

            entity.Property(e => e.AnimalCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("animalcostschanged");

            entity.Property(e => e.NonAnimalCostsChanged)
                  .HasDefaultValue((short)0)
                  .HasColumnName("nonanimalcostschanged");

            
            entity.Property(e => e.AdjustmentComment)
                  .HasMaxLength(250)
                  .HasColumnName("adjustmentcomment");

            entity.Property(e => e.Locked)
                  .HasDefaultValue((short)0)
                  .HasColumnName("locked");

            entity.Property(e => e.DateCosted)
                  .HasColumnType(ColumnTypeTimestamp)
                  .HasColumnName("datecosted");

            entity.Property(e => e.CostedBy)
                  .HasMaxLength(20)
                  .HasColumnName("costedby");
        }
    }
}
