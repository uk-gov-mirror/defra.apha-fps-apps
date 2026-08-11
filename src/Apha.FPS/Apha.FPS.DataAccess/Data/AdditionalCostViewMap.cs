using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AdditionalCostViewMap : IEntityTypeConfiguration<AdditionalCostView>
    {
        public void Configure(EntityTypeBuilder<AdditionalCostView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtbladditionalcosts", "fps");

            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");
            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");
            entity.Property(e => e.Dt2UserName)
                .HasMaxLength(50)
                .HasColumnName("dt2username");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Freq)
                .HasMaxLength(5)
                .HasColumnName("freq");
            entity.Property(e => e.ItemCost)
                .HasPrecision(19, 4)
                .HasColumnName("itemcost");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("useremail");            
        }
    }
}
