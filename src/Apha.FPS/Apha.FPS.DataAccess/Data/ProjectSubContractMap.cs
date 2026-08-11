using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectSubContractMap : IEntityTypeConfiguration<ProjectSubContract>
    {
        public void Configure(EntityTypeBuilder<ProjectSubContract> entity)
        {
            entity.HasKey(e => new { e.SubContCounter, e.FpsYear }).HasName("pk_proj_subcontract");

            entity.ToTable("proj_subcontract", "fps");

            entity.Property(e => e.SubContCounter)
                .ValueGeneratedOnAdd()
                .HasColumnName("subcontcounter");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.AcctCode)
                .HasMaxLength(30)
                .HasColumnName("acctcode");
            entity.Property(e => e.Amount)
                .HasPrecision(19, 4)
                .HasColumnName("amount");
            entity.Property(e => e.AnimalDays).HasColumnName("animaldays");
            entity.Property(e => e.DailyRate)
                .HasPrecision(19, 4)
                .HasColumnName("dailyrate");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
            entity.Property(e => e.SupplierNumber).HasColumnName("suppliernumber");
            entity.Property(e => e.TestJob)
                .HasMaxLength(50)
                .HasColumnName("testjob");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
