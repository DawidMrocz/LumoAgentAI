using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

internal sealed class GoogleAccountConfiguration : IEntityTypeConfiguration<GoogleAccount>
{
    public void Configure(EntityTypeBuilder<GoogleAccount> builder)
    {
        builder.ToTable("GoogleAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(x => x.RefreshTokenProtected)
            .IsRequired();

        builder.Property(x => x.GrantedScopes)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasIndex(x => x.Email);
    }
}