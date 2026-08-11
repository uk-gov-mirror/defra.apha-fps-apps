using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class StagingMonthlyOutputMap : IEntityTypeConfiguration<StagingMonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<StagingMonthlyOutput> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tblstagingmonthlyoutput");

            entity.ToTable("tblstagingmonthlyoutput", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.FailureComments)
                .HasColumnType("character varying")
                .HasColumnName("failurecomments");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Passed).HasColumnName("passed");
            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Volume).HasColumnName("volume");
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
