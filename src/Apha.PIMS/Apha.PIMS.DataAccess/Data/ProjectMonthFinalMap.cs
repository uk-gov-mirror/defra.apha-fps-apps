using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectMonthFinalMap : IEntityTypeConfiguration<ProjectMonthFinal>
    {
        public void Configure(EntityTypeBuilder<ProjectMonthFinal> entity)
        {
            entity.HasKey(e => new { e.Year, e.Project, e.Monthno }).HasName("pk_my_projectmonthfinal");

            entity.ToTable("my_projectmonthfinal", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Monthno).HasColumnName("monthno");
            entity.Property(e => e.Animals)
                .HasPrecision(19, 4)
                .HasColumnName("animals");
            entity.Property(e => e.Coiw)
                .HasPrecision(19, 4)
                .HasColumnName("coiw");
            entity.Property(e => e.Costprofile)
                .HasPrecision(19, 4)
                .HasColumnName("costprofile");
            entity.Property(e => e.Cumcoiw)
                .HasPrecision(19, 4)
                .HasColumnName("cumcoiw");
            entity.Property(e => e.Cumcost)
                .HasPrecision(19, 4)
                .HasColumnName("cumcost");
            entity.Property(e => e.Cumcwcredit)
                .HasPrecision(19, 4)
                .HasColumnName("cumcwcredit");
            entity.Property(e => e.Cumcwdebit)
                .HasPrecision(19, 4)
                .HasColumnName("cumcwdebit");
            entity.Property(e => e.Cumflag).HasColumnName("cumflag");
            entity.Property(e => e.Cuminvoices)
                .HasPrecision(19, 4)
                .HasColumnName("cuminvoices");
            entity.Property(e => e.Cumpaycosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.Cumportsales)
                .HasPrecision(19, 4)
                .HasColumnName("cumportsales");
            entity.Property(e => e.Cumprofile)
                .HasPrecision(19, 4)
                .HasColumnName("cumprofile");
            entity.Property(e => e.Cumsubcontracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.Cumtestcosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.Cumtotalhours).HasColumnName("cumtotalhours");
            entity.Property(e => e.Cwcredit)
                .HasPrecision(19, 4)
                .HasColumnName("cwcredit");
            entity.Property(e => e.Cwdebit)
                .HasPrecision(19, 4)
                .HasColumnName("cwdebit");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.Invoices)
                .HasPrecision(19, 4)
                .HasColumnName("invoices");
            entity.Property(e => e.Mstonedue).HasColumnName("mstonedue");
            entity.Property(e => e.Nonanimals)
                .HasPrecision(19, 4)
                .HasColumnName("nonanimals");
            entity.Property(e => e.Ontime).HasColumnName("ontime");
            entity.Property(e => e.Paycosts).HasColumnName("paycosts");
            entity.Property(e => e.Periodname)
                .HasMaxLength(50)
                .HasColumnName("periodname");
            entity.Property(e => e.Portsales)
                .HasPrecision(19, 4)
                .HasColumnName("portsales");
            entity.Property(e => e.Subcontracts)
                .HasPrecision(19, 4)
                .HasColumnName("subcontracts");
            entity.Property(e => e.Sumofcostprofile)
                .HasPrecision(19, 4)
                .HasColumnName("sumofcostprofile");
            entity.Property(e => e.SumofdueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.Sumofmstonedue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.Sumofontime).HasColumnName("sumofontime");
            entity.Property(e => e.Timecosts)
                .HasPrecision(19, 4)
                .HasColumnName("timecosts");
            entity.Property(e => e.Totalcost)
                .HasPrecision(19, 4)
                .HasColumnName("totalcost");
            entity.Property(e => e.Totalhours).HasColumnName("totalhours");
            entity.Property(e => e.Transfercosts)
                .HasPrecision(19, 4)
                .HasColumnName("transfercosts");
        }
    }
}
