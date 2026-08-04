using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class CommentMap : IEntityTypeConfiguration<Comment>
    {

        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.HasKey(e => e.CommentNo).HasName("pk_tblcomments");

            entity.ToTable("tblcomments", "mabarchive");

           
            entity.Property(e => e.CommentNo)
                .HasColumnName("commentno")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CommentText)
                .HasColumnType("character varying")
                .HasColumnName("comment");
            entity.Property(e => e.DateEntered)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateentered");
            
            entity.Property(e => e.MadeBy)
                .HasMaxLength(255)
                .HasColumnName("madeby");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Topic)
                .HasMaxLength(25)
                .HasColumnName("topic");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}

