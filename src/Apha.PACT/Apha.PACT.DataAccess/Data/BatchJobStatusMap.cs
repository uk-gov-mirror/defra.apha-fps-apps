using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class BatchJobStatusMap : IEntityTypeConfiguration<BatchJobStatus>
    {
        public void Configure(EntityTypeBuilder<BatchJobStatus> entity)
        {
            entity.HasKey(e => e.StatusId).HasName("job_status_pkey");

            entity.ToTable("job_status", "fps");

            entity.HasIndex(e => e.JobId, "idx_job_status_jobid");

            entity.HasIndex(e => new { e.JobId, e.Status }, "uq_job_status_jobid_status").IsUnique();

            entity.Property(e => e.StatusId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("statusid");
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            
            entity.Property(e => e.JobId).HasColumnName("jobid");
            
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .HasColumnName("status");
        }
    }
}
