using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectAnimalPlanMap : IEntityTypeConfiguration<ProjectAnimalPlan>
    {
        public void Configure(EntityTypeBuilder<ProjectAnimalPlan> entity)
        {
            entity
                  .HasNoKey()
                  .ToView("vmy_projectanimalplan", "mabarchive");

            entity.Property(e => e.Animaltype)
                .HasMaxLength(50)
                .HasColumnName("animaltype");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Numberofanimals).HasColumnName("numberofanimals");
            entity.Property(e => e.Numberofdays).HasColumnName("numberofdays");
            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Rate)
                .HasPrecision(19, 4)
                .HasColumnName("rate");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}
