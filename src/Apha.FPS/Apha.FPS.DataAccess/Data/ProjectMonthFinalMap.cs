using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectMonthFinalMap : IEntityTypeConfiguration<ProjectMonthFinal>
    {
        public void Configure(EntityTypeBuilder<ProjectMonthFinal> entity)
        {
            entity.HasKey(e => new { e.Project, e.MonthNo })
                .HasName("aaaaaprojectmonthfinal_pk");

            entity.ToTable("projectmonthfinal", "fps");

            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.PeriodName).HasMaxLength(50).HasColumnName("periodname");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag");
            entity.Property(e => e.CostProfile).HasPrecision(19, 4).HasColumnName("costprofile");
            entity.Property(e => e.Subcontracts).HasPrecision(19, 4).HasColumnName("subcontracts");
            entity.Property(e => e.Animals).HasPrecision(19, 4).HasColumnName("animals");
            entity.Property(e => e.NonAnimals).HasPrecision(19, 4).HasColumnName("nonanimals");
            entity.Property(e => e.TimeCosts).HasPrecision(19, 4).HasColumnName("timecosts");
            entity.Property(e => e.TransferCosts).HasPrecision(19, 4).HasColumnName("transfercosts");
            entity.Property(e => e.TotalCost).HasPrecision(19, 4).HasColumnName("totalcost");
            entity.Property(e => e.Invoices).HasPrecision(19, 4).HasColumnName("invoices");
            entity.Property(e => e.Coiw).HasPrecision(19, 4).HasColumnName("coiw");
            entity.Property(e => e.PortSales).HasPrecision(19, 4).HasColumnName("portsales");
            entity.Property(e => e.CumCost).HasPrecision(19, 4).HasColumnName("cumcost");
            entity.Property(e => e.CumProfile).HasPrecision(19, 4).HasColumnName("cumprofile");
            entity.Property(e => e.SumOfCostProfile).HasPrecision(19, 4).HasColumnName("sumofcostprofile");
            entity.Property(e => e.CumInvoices).HasPrecision(19, 4).HasColumnName("cuminvoices");
            entity.Property(e => e.CumCoiw).HasPrecision(19, 4).HasColumnName("cumcoiw");
            entity.Property(e => e.CumPortSales).HasPrecision(19, 4).HasColumnName("cumportsales");
            entity.Property(e => e.MilestoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.SumOfMilestoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CwDebit).HasPrecision(19, 4).HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasPrecision(19, 4).HasColumnName("cwcredit");
            entity.Property(e => e.CumCwDebit).HasPrecision(19, 4).HasColumnName("cumcwdebit");
            entity.Property(e => e.CumCwCredit).HasPrecision(19, 4).HasColumnName("cumcwcredit");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubcontracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
