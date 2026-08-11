using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ReportMap : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tblreport");

            entity.ToTable("tblreport", "mabarchive");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");

            entity.Property(e => e.ReportName)
                .HasMaxLength(50)
                .HasColumnName("reportname");

            entity.Property(e => e.ReportDescription)
                .HasMaxLength(50)
                .HasColumnName("reportdescription");

            entity.Property(e => e.Filter)
                .HasMaxLength(200)
                .HasColumnName("filter");

            entity.Property(e => e.MailComment)
                .HasMaxLength(250)
                .HasColumnName("mailcomment");

            entity.Property(e => e.MailTitle)
                .HasMaxLength(50)
                .HasColumnName("mailtitle");

            entity.Property(e => e.Emailable)
                .HasColumnName("emailable");

            entity.Property(e => e.SortOrder)
                .HasColumnName("sortorder");

            entity.Property(e => e.AllowPickProgramme)
                .HasColumnName("allowpickprogramme");

            entity.Property(e => e.AllowPickProject)
                .HasColumnName("allowpickproject");

            entity.Property(e => e.AllowPickManager)
                .HasColumnName("allowpickmanager");

            entity.Property(e => e.AllowPickContract)
                .HasColumnName("allowpickcontract");

            entity.Property(e => e.AllowPickCustomer)
                .HasColumnName("allowpickcustomer");

            entity.Property(e => e.AllowPickMonth)
                .HasColumnName("allowpickmonth");

            entity.Property(e => e.AllowPickFYear)
                .HasColumnName("allowpickfyear");

            entity.Property(e => e.ReportHelp)
                .HasMaxLength(250)
                .HasColumnName("reporthelp");

            entity.Property(e => e.Type)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("type");
        }
    }
}
