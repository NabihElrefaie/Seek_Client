using Microsoft.EntityFrameworkCore;
using Seek.Core.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Seek.EF.Configurations
{
    class auth_configs : IEntityTypeConfiguration<auth_model>
    {
        public void Configure(EntityTypeBuilder<auth_model> builder)
        {
            builder.ToTable("FK_Auth");
            builder.Property(m => m.HashedLogin).IsRequired();
            builder.Property(m => m.HashedPassword).IsRequired();
            builder.Property(m => m.Hashed_Refresh_Token).IsRequired();
            builder.HasIndex(m => m.HashedLogin).IsUnique();
        }
    }
}
