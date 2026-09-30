using Lumo.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Infrastructure.Configurations;

internal sealed class MetaAccountConfiguration : IEntityTypeConfiguration<MetaAccount>
{
    public void Configure(EntityTypeBuilder<MetaAccount> builder)
    {
        builder.ToTable("MetaAccounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccessTokenProtected)
            .IsRequired();

        builder.Property(x => x.GrantedScopes)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
