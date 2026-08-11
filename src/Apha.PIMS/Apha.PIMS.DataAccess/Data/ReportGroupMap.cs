using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    
    public class ReportGroupMap : IEntityTypeConfiguration<ReportGroup>
    {
        public void Configure(EntityTypeBuilder<ReportGroup> entity)
        {
            entity.HasKey(e => e.GroupId).HasName("pk_tblreportgroup");

            entity.ToTable("tblreportgroup", "mabarchive");

            
            entity.Property(e => e.GroupId)
                .ValueGeneratedOnAdd()
                .HasColumnName("groupid");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        }
    }
}
