using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class YearlyFinancialDataMap : IEntityTypeConfiguration<YearlyFinancialData>
    {
        
        public void Configure(EntityTypeBuilder<YearlyFinancialData> entity)
        {

            entity.HasKey(e => new { e.Year, e.Project }).HasName("pk_my_tlkpprojectradtrackdata");

            entity.ToTable("my_tlkpprojectradtrackdata", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.ActualExpenditure)
                .HasPrecision(19, 4)
                .HasColumnName("actualexpenditure");
            entity.Property(e => e.ActualManYears).HasColumnName("actualmanyears");
            entity.Property(e => e.Adjustment)
                .HasPrecision(19, 4)
                .HasColumnName("adjustment");
            entity.Property(e => e.AdjustmentComment)
                .HasMaxLength(250)
                .HasColumnName("adjustmentcomment");
            entity.Property(e => e.AnimalCosts)
                .HasPrecision(19, 4)
                .HasColumnName("animalcosts");
            entity.Property(e => e.AnimalCostsChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("animalcostschanged");
            entity.Property(e => e.BfBudget)
                .HasPrecision(19, 4)
                .HasColumnName("bfbudget");
            entity.Property(e => e.CostedBy)
                .HasMaxLength(20)
                .HasColumnName("costedby");
            entity.Property(e => e.DateCosted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecosted");
            entity.Property(e => e.Locked)
                .HasDefaultValue((short)0)
                .HasColumnName("locked");
            entity.Property(e => e.ManDays).HasColumnName("mandays");
            entity.Property(e => e.ManHours).HasColumnName("manhours");
            entity.Property(e => e.ManHoursChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("manhourschanged");
            entity.Property(e => e.ManYears).HasColumnName("manyears");
            entity.Property(e => e.NonAnimalCosts)
                .HasPrecision(19, 4)
                .HasColumnName("nonanimalcosts");
            entity.Property(e => e.NonAnimalCostsChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("nonanimalcostschanged");
            entity.Property(e => e.NonPayOhCosts)
                .HasPrecision(19, 4)
                .HasColumnName("nonpayohcosts");
            entity.Property(e => e.NonPayOhCostsChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("nonpayohcostschanged");
            entity.Property(e => e.PayCosts)
                .HasPrecision(19, 4)
                .HasColumnName("paycosts");
            entity.Property(e => e.PayCostsChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("paycostschanged");
            entity.Property(e => e.PyBudget)
                .HasPrecision(19, 4)
                .HasColumnName("pybudget");
            entity.Property(e => e.Seedcorn)
                .HasPrecision(19, 4)
                .HasColumnName("seedcorn");
            entity.Property(e => e.TestCosts)
                .HasPrecision(19, 4)
                .HasColumnName("testcosts");
            entity.Property(e => e.TestCostsChanged)
                .HasDefaultValue((short)0)
                .HasColumnName("testcostschanged");
            entity.Property(e => e.VlaBudget)
                .HasPrecision(19, 4)
                .HasColumnName("vla_budget");
        }
    }
}
