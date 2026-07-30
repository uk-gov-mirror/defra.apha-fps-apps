using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectsMap : IEntityTypeConfiguration<Projects>
    {
       
        public void Configure(EntityTypeBuilder<Projects> entity)
        {
            entity.HasKey(e => new { e.Year, e.Parentproject }).HasName("pk_my_tlkpproject");

            entity.ToTable("my_tlkpproject", "mabarchive");

            entity.HasIndex(e => e.Year, "idx_my_tlkpproject_year");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.BudgetCvl)
                .HasPrecision(19, 4)
                .HasColumnName("budget_cvl");
            entity.Property(e => e.Carryover)
                .HasPrecision(19, 4)
                .HasColumnName("carryover");
            entity.Property(e => e.Caseworksub)
                .HasPrecision(5, 4)
                .HasColumnName("caseworksub");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasColumnName("contract");
            entity.Property(e => e.Costcentre).HasColumnName("costcentre");
            entity.Property(e => e.Custincome)
                .HasPrecision(19, 4)
                .HasColumnName("custincome");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Datecreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Feccost)
                .HasPrecision(19, 4)
                .HasColumnName("feccost");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.Incomeaccountcode)
                .HasMaxLength(50)
                .HasColumnName("incomeaccountcode");
            entity.Property(e => e.Isdefraproject).HasColumnName("isdefraproject");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Oracleprojectcode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");
            entity.Property(e => e.Plancaseworkdebit)
                .HasPrecision(19, 4)
                .HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Profit)
                .HasPrecision(19, 4)
                .HasColumnName("profit");
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
            entity.Property(e => e.Projectgroup)
                .HasMaxLength(50)
                .HasColumnName("projectgroup");
            entity.Property(e => e.Projectstatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.Pvsincome)
                .HasPrecision(19, 4)
                .HasColumnName("pvsincome");
            entity.Property(e => e.Source)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasColumnName("source");
            entity.Property(e => e.Subaccountcode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");
            entity.Property(e => e.Transferincome)
                .HasPrecision(19, 4)
                .HasColumnName("transferincome");
            entity.Property(e => e.WipCurrent)
                .HasPrecision(19, 4)
                .HasColumnName("wip_current");
            entity.Property(e => e.WipEoy)
                .HasPrecision(19, 4)
                .HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit)
                .HasPrecision(19, 4)
                .HasColumnName("wip_limit");
        }
    }
}
