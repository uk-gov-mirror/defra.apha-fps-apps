/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: EF Core IEntityTypeConfiguration<ContributionSummary> for the frmTimeSellerPC migration.
 *   - Maps entity to fps.tblkpcontributionsummary (writable storage table; column names inferred
 *     from entity field names in lowercase per project conventions — no PostgreSQL DDL was
 *     available at migration time; confirm with DBA before running EF migrations).
 *   - Id column mapped as auto-increment integer primary key (ValueGeneratedOnAdd).
 *   - Monetary columns (ChgRate, TotalFec, AssuredFec, OhRate, TotalCont) typed as "money"
 *     consistent with profitcentregrade and other FPS financial columns.
 *   - HasMaxLength applied to string columns per project conventions.
 *
 * PRESERVED:
 *   - All field names from ContributionSummary.cs entity (Phase 2) preserved verbatim.
 *   - Lowercase HasColumnName and ToTable arguments per phase rule.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm that fps.tblkpcontributionsummary exists in PostgreSQL
 *     with the exact column names below before running EF Core migrations or dotnet ef
 *     database update. The view vqryfrmtimesellerpc supplies read-only aggregates; the table
 *     is the writable backing store for CRUD operations.
 *   - TRANSFORMENGINE TODO: Confirm column types (especially AvailHrs/TotalPlanHrs as double
 *     vs numeric) match the PostgreSQL DDL.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ContributionSummaryMap : IEntityTypeConfiguration<ContributionSummary>
    {
        public void Configure(EntityTypeBuilder<ContributionSummary> entity)
        {
            // TRANSFORMENGINE: Map to fps.tblkpcontributionsummary — writable CRUD table for
            //   contribution summary rows (one row per WG/Grade/ProfitCentre/FpsYear).
            entity.ToTable("tblkpcontributionsummary", "fps");

            // TRANSFORMENGINE: Simple integer identity primary key (not composite).
            entity.HasKey(e => e.Id).HasName("pk_tblkpcontributionsummary");
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            // TRANSFORMENGINE: Wg — workgroup code, e.g. "BAC1"; maps to wgg.workgroup in view.
            entity.Property(e => e.Wg)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("wg");

            // TRANSFORMENGINE: Grade — wg-grade code, e.g. "C_BAC1"; maps to wgg.wggrade in view.
            entity.Property(e => e.Grade)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("grade");

            // TRANSFORMENGINE: AvailHrs — available hours; maps to sum(we.hrsavail) AS avhrs in view.
            entity.Property(e => e.AvailHrs)
                .HasColumnName("availhrs");

            // TRANSFORMENGINE: ChgRate — charge rate (£/hr); maps to pcg.chargerate in view.
            entity.Property(e => e.ChgRate)
                .HasColumnType("money")
                .HasColumnName("chgrate");

            // TRANSFORMENGINE: TotalPlanHrs — total planned hours; maps to sum(sjh.plannedhours) AS hrs.
            entity.Property(e => e.TotalPlanHrs)
                .HasColumnName("totalplanhrs");

            // TRANSFORMENGINE: TotalFec — total FEC £; maps to sum(sjh.plannedhours)*pcg.chargerate AS fec.
            entity.Property(e => e.TotalFec)
                .HasColumnType("money")
                .HasColumnName("totalfec");

            // TRANSFORMENGINE: TotalPctPlanned — percentage (0-100) stored explicitly on write.
            entity.Property(e => e.TotalPctPlanned)
                .HasColumnName("totalpctplanned");

            // TRANSFORMENGINE: AssuredPlanHrs — assured planned hours; maps to ah.sumofplannedhours AS apphours.
            entity.Property(e => e.AssuredPlanHrs)
                .HasColumnName("assuredplanhrs");

            // TRANSFORMENGINE: AssuredFec — assured FEC £; maps to ah.sumofplannedhours*pcg.chargerate AS appfec.
            entity.Property(e => e.AssuredFec)
                .HasColumnType("money")
                .HasColumnName("assuredfec");

            // TRANSFORMENGINE: AssuredPctPlanned — assured percentage (0-100) stored explicitly on write.
            entity.Property(e => e.AssuredPctPlanned)
                .HasColumnName("assuredpctplanned");

            // TRANSFORMENGINE: OhRate — overhead rate (£/hr); maps to pcg.ohr in view.
            entity.Property(e => e.OhRate)
                .HasColumnType("money")
                .HasColumnName("ohrate");

            // TRANSFORMENGINE: TotalCont — total contribution £; maps to pcg.ohr*sum(sjh.plannedhours) AS contribution.
            entity.Property(e => e.TotalCont)
                .HasColumnType("money")
                .HasColumnName("totalcont");

            // TRANSFORMENGINE: ProfitCentre — resource centre discriminator key; maps to pcg.profitcentre.
            entity.Property(e => e.ProfitCentre)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("profitcentre");

            // TRANSFORMENGINE: FpsYear — financial year partition key; used by HasQueryFilter in DbContext.
            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            // TRANSFORMENGINE: Index on profitcentre for efficient GetByProfitCentreAsync queries.
            entity.HasIndex(e => new { e.ProfitCentre, e.FpsYear }, "ix_tblkpcontributionsummary_profitcentre_year");
        }
    }
}
