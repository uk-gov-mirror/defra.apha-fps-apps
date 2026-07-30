using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ProjectSubcontractStagingMap : IEntityTypeConfiguration<ProjectSubcontractStaging>
    {
        public void Configure(EntityTypeBuilder<ProjectSubcontractStaging> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_proj_subcontract_staging");

            entity.ToTable("proj_subcontract_staging", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcctCode)
                .HasMaxLength(30)
                .HasColumnName("acctcode");
            entity.Property(e => e.Amount)
                .HasMaxLength(10)
                .HasColumnName("amount");
            entity.Property(e => e.AnimalDays)
                .HasMaxLength(10)
                .HasColumnName("animaldays");
            entity.Property(e => e.DailyRate)
                .HasMaxLength(10)
                .HasColumnName("dailyrate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.ImportedBy)
                .HasMaxLength(255)
                .HasColumnName("importedby");
            entity.Property(e => e.ImportedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("importeddate");
            entity.Property(e => e.IsExported)
                .HasDefaultValue(false)
                .HasColumnName("isexported");
            entity.Property(e => e.IsPassed)
                .HasDefaultValue(false)
                .HasColumnName("ispassed");
            entity.Property(e => e.Month)
                .HasMaxLength(10)
                .HasColumnName("month");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
            entity.Property(e => e.SupplierNumber)
                .HasMaxLength(10)
                .HasColumnName("suppliernumber");
            entity.Property(e => e.TestJob)
                .HasMaxLength(50)
                .HasColumnName("testjob");
            entity.Property(e => e.ValidationFailure).HasColumnName("validationfailure");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
