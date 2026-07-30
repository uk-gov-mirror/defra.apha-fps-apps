using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ResourceStaffJobViewMap : IEntityTypeConfiguration<ResourceStaffJobView>
    {
        public void Configure(EntityTypeBuilder<ResourceStaffJobView> builder)
        {
            builder.HasNoKey();

            // TODO: The Access subform frmResourceDetail2 source (.frm) is missing.
            // The backing PostgreSQL view name and real column names could not be
            // confirmed. Update ToView(...) and HasColumnName(...) once the team
            // confirms the source view. Column names below are placeholders.
            builder.ToView("vresourcestaffjob", "fps");

            builder.Property(e => e.StaffId).HasColumnName("staffid");
            builder.Property(e => e.Project).HasColumnName("project");
            builder.Property(e => e.Description).HasColumnName("description");
            builder.Property(e => e.Hour).HasColumnName("hour");
            builder.Property(e => e.Status).HasColumnName("status");
        }
    }
}
