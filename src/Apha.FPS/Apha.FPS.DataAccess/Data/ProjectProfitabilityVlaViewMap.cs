using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectProfitabilityVlaViewMap : IEntityTypeConfiguration<ProjectProfitabilityVlaView>
    {
        public void Configure(EntityTypeBuilder<ProjectProfitabilityVlaView> entity)
        {
            entity.HasNoKey().ToView("vprojectprofitabilityvla", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobCode).HasMaxLength(50).HasColumnName("jobcode");
            entity.Property(e => e.Program).HasMaxLength(50).HasColumnName("program");
            entity.Property(e => e.Customer).HasMaxLength(255).HasColumnName("customer");
            entity.Property(e => e.Manager).HasMaxLength(255).HasColumnName("manager");
            entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
            entity.Property(e => e.StaffCosts).HasPrecision(19, 4).HasColumnName("staffcosts");
            entity.Property(e => e.TestCost).HasPrecision(19, 4).HasColumnName("testcost");
            entity.Property(e => e.AnimalCosts).HasPrecision(19, 4).HasColumnName("animalcosts");
            entity.Property(e => e.AdditionalCosts).HasPrecision(19, 4).HasColumnName("additionalcosts");
            entity.Property(e => e.TotalCosts).HasPrecision(19, 4).HasColumnName("totalcosts");
            entity.Property(e => e.Budget).HasPrecision(19, 4).HasColumnName("budget");
            entity.Property(e => e.Profit).HasPrecision(19, 4).HasColumnName("profit");
            entity.Property(e => e.TargetProfit).HasPrecision(19, 4).HasColumnName("targetprofit");
            entity.Property(e => e.OffTarget).HasPrecision(19, 4).HasColumnName("offtarget");
        }
    }
}
