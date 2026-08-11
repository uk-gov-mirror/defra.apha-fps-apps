using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BatchJobQueueLogMap : IEntityTypeConfiguration<BatchJobQueueLog>
    {
        public void Configure(EntityTypeBuilder<BatchJobQueueLog> entity)
        {
            entity.HasKey(e => e.JobqueueLogId).HasName("job_queue_log_pkey");

            entity.ToTable("job_queue_log", "fps");

            entity.HasIndex(e => new { e.JobqueueId, e.LogTime }, "idx_job_queue_log_jobqueueid_logtime").IsDescending(false, true);

            entity.Property(e => e.JobqueueLogId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("jobqueuelogid");
           
            entity.Property(e => e.JobqueueId).HasColumnName("jobqueueid");
            
            entity.Property(e => e.LogTime)
                .HasDefaultValueSql("now()")
                .HasColumnName("logtime");
           
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .HasColumnName("note");
            
            entity.Property(e => e.PerformedBy)
                .HasMaxLength(256)
                .HasColumnName("performedby");
           
            entity.Property(e => e.StatusId).HasColumnName("statusid");
        }
    }
}
