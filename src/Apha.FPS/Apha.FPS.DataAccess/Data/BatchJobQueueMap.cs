using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BatchJobQueueMap : IEntityTypeConfiguration<BatchJobQueue>
    {
        public void Configure(EntityTypeBuilder<BatchJobQueue> entity)
        {
            entity.HasKey(e => e.JobqueueId).HasName("job_queue_pkey");

            entity.ToTable("job_queue", "fps");

            entity.HasIndex(e => new { e.JobId, e.StartDateTime }, "idx_job_queue_jobid_startdatetime").IsDescending(false, true);
            entity.HasIndex(e => e.RequestedAtUtc, "idx_job_queue_requested_at_utc");

            entity.HasIndex(e => e.RequestedBy, "idx_job_queue_requestedby");

            entity.HasIndex(e => e.StatusId, "idx_job_queue_statusid");

            entity.HasIndex(e => e.JobExecutionId, "uq_job_queue_jobexecutionid").IsUnique();

            entity.Property(e => e.JobqueueId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("jobqueueid");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000)
                .HasColumnName("errormessage");

            entity.Property(e => e.JobExecutionId).HasColumnName("jobexecutionid");
            
            entity.Property(e => e.JobId).HasColumnName("jobid");

            entity.Property(e => e.RequestedAtUtc).HasColumnName("requested_at_utc");

            entity.Property(e => e.RequestedBy)
                .HasMaxLength(256)
                .HasColumnName("requestedby");

            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime");
            
            entity.Property(e => e.StatusId).HasColumnName("statusid");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
