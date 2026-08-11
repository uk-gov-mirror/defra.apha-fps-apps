using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BatchJobMasterMap : IEntityTypeConfiguration<BatchJobMaster>
    {
        public void Configure(EntityTypeBuilder<BatchJobMaster> entity)
        {
            entity.HasKey(e => e.JobId).HasName("job_master_pkey");

            entity.ToTable("job_master", "fps");

            entity.HasIndex(e => e.JobName, "job_master_jobname_key").IsUnique();

            entity.Property(e => e.JobId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("jobid");
           
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            
            entity.Property(e => e.Frequency)
                .HasMaxLength(50)
                .HasColumnName("frequency");
           
            entity.Property(e => e.JobName)
                .HasMaxLength(100)
                .HasColumnName("jobname");
           
            entity.Property(e => e.Note)
                .HasMaxLength(250)
                .HasColumnName("note");
            
            entity.Property(e => e.Timetolive).HasColumnName("timetolive");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        }
    }
}
