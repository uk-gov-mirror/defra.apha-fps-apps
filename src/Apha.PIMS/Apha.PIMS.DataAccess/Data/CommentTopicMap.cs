using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class CommentTopicMap : IEntityTypeConfiguration<CommentTopic>
    {
        public void Configure(EntityTypeBuilder<CommentTopic> entity)
        {
            entity.HasKey(e => e.Topic).HasName("pk_tlkpcommenttopics");

            
            entity.ToTable("tlkpcommenttopics", "mabarchive");

            entity.Property(e => e.Topic)
                .HasMaxLength(25)
                .HasColumnName("topic");
        }
    }
}

