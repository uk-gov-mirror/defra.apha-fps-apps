using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProgramViewMap : IEntityTypeConfiguration<ProgramView>
    {


        public void Configure(EntityTypeBuilder<ProgramView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtlkpprogram", "fps");

            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Directorate)
                .HasMaxLength(15)
                .HasColumnName("directorate");
            entity.Property(e => e.Dt2UserName)
                .HasMaxLength(50)
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Minim)
                .HasMaxLength(7)
                .HasColumnName("minim");
            entity.Property(e => e.ProgramName)
                .HasMaxLength(80)
                .HasColumnName("programname");
            entity.Property(e => e.ProgramNo)
                .HasMaxLength(10)
                .HasColumnName("programno");
            entity.Property(e => e.SectorName)
                .HasMaxLength(50)
                .HasColumnName("sector_name");
            entity.Property(e => e.Target)
                .HasPrecision(19, 4)
                .HasColumnName("target");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");
        }
    }
}
