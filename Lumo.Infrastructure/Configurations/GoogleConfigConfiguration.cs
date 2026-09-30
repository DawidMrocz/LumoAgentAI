using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Infrastructure.Configurations;

internal sealed class GoogleConfigConfiguration
    : IEntityTypeConfiguration<GoogleConfig>
{
    public void Configure(EntityTypeBuilder<GoogleConfig> builder)
    {
        builder.ToTable("GoogleConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ClientSecret)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.RedirectUri)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Scopes)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.DeveloperToken)
            .HasMaxLength(255);

        builder.Property(x => x.LoginCustomerId)
            .HasMaxLength(20);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsActive);
    }
}