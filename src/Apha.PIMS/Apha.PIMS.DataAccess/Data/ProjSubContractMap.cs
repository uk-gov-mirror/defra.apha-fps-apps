using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjSubContractMap : IEntityTypeConfiguration<ProjSubContract>
    {
        public void Configure(EntityTypeBuilder<ProjSubContract> entity)
        {
            entity.HasKey(e => new { e.Year, e.Subcontcounter }).HasName("pk_my_proj_subcontract");

            entity.ToTable("my_proj_subcontract", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Subcontcounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Acctcode)
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
            entity.Property(e => e.Suppliernumber).HasColumnName("suppliernumber");
            entity.Property(e => e.Testjob)
                .HasMaxLength(50)
                .HasColumnName("testjob");
            entity.Property(e => e.Workgroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
