using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ReportGroupLinkMap : IEntityTypeConfiguration<ReportGroupLink>
    {
        public void Configure(EntityTypeBuilder<ReportGroupLink> entity)
        {
            entity.HasKey(e => new { e.ReportId, e.GroupId }).HasName("pk_tblreportgroup_link");

            entity.ToTable("tblreportgroup_link", "mabarchive");

            entity.Property(e => e.ReportId)
                .HasColumnName("reportid");

            entity.Property(e => e.GroupId)
                .HasColumnName("groupid");
        }
    }
}
