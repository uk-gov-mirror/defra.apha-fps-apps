using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProgramManagerLinkMap : IEntityTypeConfiguration<ProgramManagerLink>
    {
        public void Configure(EntityTypeBuilder<ProgramManagerLink> entity)
        {
            entity.HasKey(e => new { e.Program, e.Manager }).HasName("pk_tblprogram_manager_link");

            entity.ToTable("tblprogram_manager_link", "mabarchive");

            entity.Property(e => e.Program)
                .HasMaxLength(50)
                .HasColumnName("program");

            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
        }
    }
}
