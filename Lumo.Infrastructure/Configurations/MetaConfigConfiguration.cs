using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Infrastructure.Configurations;

internal sealed class MetaConfigConfiguration : IEntityTypeConfiguration<MetaConfig>
{
    public void Configure(EntityTypeBuilder<MetaConfig> builder)
    {
        builder.ToTable("MetaConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.AppSecret)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.RedirectUri)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Scopes)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasIndex(x => x.IsActive);
    }
}
