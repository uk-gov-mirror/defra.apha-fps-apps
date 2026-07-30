using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ResourceStaffAllocationViewMap : IEntityTypeConfiguration<ResourceStaffAllocationView>
    {
        public void Configure(EntityTypeBuilder<ResourceStaffAllocationView> builder)
        {
            builder.HasNoKey();

            // TODO: The Access subform fsubResourceTotals2 source (.frm) is missing.
            // The backing PostgreSQL view name and real column names could not be
            // confirmed. Update ToView(...) and HasColumnName(...) once the team
            // confirms the source view. Column names below are placeholders.
            builder.ToView("vresourcestaffallocation", "fps");

            builder.Property(e => e.WorkGroupGrade).HasColumnName("workgroupgrade");
            builder.Property(e => e.StaffId).HasColumnName("staffid");
            builder.Property(e => e.Name).HasColumnName("name");
            builder.Property(e => e.HoursAvailable).HasColumnName("hoursavailable");
            builder.Property(e => e.PlannedHours).HasColumnName("plannedhours");
            builder.Property(e => e.AllocationPct).HasColumnName("allocationpct");
            builder.Property(e => e.AssuredChargeHours).HasColumnName("assuredchargehours");
            builder.Property(e => e.AssuredUtilisationPct).HasColumnName("assuredutilisationpct");
            builder.Property(e => e.ChargeHours).HasColumnName("chargehours");
            builder.Property(e => e.UtilisationPct).HasColumnName("utilisationpct");
        }
    }
}
