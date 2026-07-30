using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class FpsYearTotalMap : IEntityTypeConfiguration<FpsYearTotal>
    {
        private const string ColumnTypeMoney = "money";

        public void Configure(EntityTypeBuilder<FpsYearTotal> entity)
        {
            entity.HasKey(e => new { e.Year, e.Parentproject }).HasName("pk_my_fpsyeartotals");

            entity.ToTable("my_fpsyeartotals", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.BudgetCvl)
                .HasPrecision(19, 4)
                .HasColumnName("budget_cvl");
            entity.Property(e => e.Custincome)
                .HasPrecision(19, 4)
                .HasColumnName("custincome");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Plancaseworkdebit)
                .HasPrecision(19, 4)
                .HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
            entity.Property(e => e.Projectstatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.Pvsincome)
                .HasPrecision(19, 4)
                .HasColumnName("pvsincome");
            entity.Property(e => e.Requiredprofit)
                .HasPrecision(19, 4)
                .HasColumnName("requiredprofit");
            entity.Property(e => e.Totaladditionalcosts)
                .HasPrecision(19, 4)
                .HasColumnName("totaladditionalcosts");
            entity.Property(e => e.Totalanimalcosts).HasColumnName("totalanimalcosts");
            entity.Property(e => e.Totalcosts).HasColumnName("totalcosts");
            entity.Property(e => e.Totalincome)
                .HasPrecision(19, 4)
                .HasColumnName("totalincome");
            entity.Property(e => e.Totalpaycosts).HasColumnName("totalpaycosts");
            entity.Property(e => e.Totalstaffcosts).HasColumnName("totalstaffcosts");
            entity.Property(e => e.Totaltestcosts).HasColumnName("totaltestcosts");
            entity.Property(e => e.Transferincome)
                .HasPrecision(19, 4)
                .HasColumnName("transferincome");
        }
    }
}
