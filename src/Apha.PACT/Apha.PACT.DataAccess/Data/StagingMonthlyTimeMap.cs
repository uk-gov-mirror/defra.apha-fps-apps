using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class StagingMonthlyTimeMap : IEntityTypeConfiguration<StagingMonthlyTime>
    {
        public void Configure(EntityTypeBuilder<StagingMonthlyTime> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tblstagingmonthlytime");

            entity.ToTable("tblstagingmonthlytime", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FailureComments)
                .HasColumnType("character varying")
                .HasColumnName("failurecomments");
            entity.Property(e => e.Hours).HasColumnName("hours");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.NewWorkGroup)
                .HasMaxLength(50)
                .HasColumnName("newworkgroup");
            entity.Property(e => e.OldTestCode)
                .HasMaxLength(20)
                .HasColumnName("oldtestcode");
            entity.Property(e => e.PactId)
                .HasMaxLength(50)
                .HasColumnName("pactid");
            entity.Property(e => e.PactStaffId)
                .HasMaxLength(50)
                .HasColumnName("pactstaffid");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Passed).HasColumnName("passed");
            entity.Property(e => e.TimeCode)
                .HasMaxLength(50)
                .HasColumnName("timecode");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.ImportedBy)
                .HasMaxLength(255)
                .HasColumnName("importedby");
            entity.Property(e => e.ImportedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("importeddate");
        }
    }
}
