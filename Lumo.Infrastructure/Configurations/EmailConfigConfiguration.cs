using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Infrastructure.Configurations;

internal sealed class EmailConfigConfiguration
    : IEntityTypeConfiguration<EmailConfig>
{
    public void Configure(EntityTypeBuilder<EmailConfig> builder)
    {
        builder.ToTable("EmailConfigs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Host)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Port)
            .IsRequired();

        builder.Property(x => x.Username)
            .HasMaxLength(255);

        builder.Property(x => x.Password)
            .HasMaxLength(1000);

        builder.Property(x => x.UseSsl)
            .IsRequired();

        builder.Property(x => x.FromEmail)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.FromName)
            .HasMaxLength(255);

        builder.Property(x => x.TimeoutSeconds)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsActive);
    }
}
