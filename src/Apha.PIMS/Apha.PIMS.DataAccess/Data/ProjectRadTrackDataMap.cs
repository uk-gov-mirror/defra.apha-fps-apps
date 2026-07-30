using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectRadTrackDataMap : IEntityTypeConfiguration<ProjectRadTrackData>
    {
        private const string ColumnTypeTimeStamp = "timestamp without time zone";
        public void Configure(EntityTypeBuilder<ProjectRadTrackData> entity)
        {
            entity.HasKey(e => e.Parentproject).HasName("pk_g_tlkpproject_radtrackdata");

            entity.ToTable("g_tlkpproject_radtrackdata", "mabarchive");

            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Closeddate)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("closeddate");
            entity.Property(e => e.Costbooknumber)
                .HasMaxLength(10)
                .HasColumnName("costbooknumber");
            entity.Property(e => e.Customerref)
                .HasMaxLength(20)
                .HasColumnName("customerref");
            entity.Property(e => e.Enddate)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("enddate");
            entity.Property(e => e.Fileref)
                .HasMaxLength(20)
                .HasColumnName("fileref");
            entity.Property(e => e.Finalreportreceived)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("finalreportreceived");
            entity.Property(e => e.Finalreportsent)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("finalreportsent");
            entity.Property(e => e.Formrequired)
                .HasDefaultValue(true)
                .HasColumnName("formrequired");
            entity.Property(e => e.Inflation)
                .HasDefaultValue((short)0)
                .HasColumnName("inflation");
            entity.Property(e => e.Overallcustincome)
                .HasPrecision(19, 4)
                .HasColumnName("overallcustincome");
            entity.Property(e => e.Pcforecastspend).HasColumnName("pcforecastspend");
            entity.Property(e => e.Revisedenddate)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("revisedenddate");
            entity.Property(e => e.Riskid).HasColumnName("riskid");
            entity.Property(e => e.Startdate)
                .HasColumnType(ColumnTypeTimeStamp)
                .HasColumnName("startdate");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Useprojectyear)
                .HasDefaultValue((short)0)
                .HasColumnName("useprojectyear");
            entity.Property(e => e.Version)
                .HasMaxLength(10)
                .HasColumnName("version");
            
        }
    }
}
