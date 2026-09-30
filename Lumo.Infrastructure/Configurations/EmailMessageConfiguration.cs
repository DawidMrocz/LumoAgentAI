using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Infrastructure.Configurations;

internal sealed class EmailMessageConfiguration
    : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.ToTable("EmailMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TemplateName)
            .HasMaxLength(200);

        builder.Property(x => x.To)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Cc)
            .HasMaxLength(2000);

        builder.Property(x => x.Bcc)
            .HasMaxLength(2000);

        builder.Property(x => x.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.AttachmentNames)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
