using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumo.Api.Data.User
{
    public class User
    {
        public int Id { get; set; }
        public DateTimeOffset CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTimeOffset? ModifyDate { get; set; }
        public int? ModifyUserId { get; set; }
        public bool IsActive { get; set; } = true;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? GoogleId { get; set; }
        public string? FacebookId { get; set; }
        public string? PasswordHash { get; set; } = null!;
        public bool Blocked { get; set; }
        public bool Activated { get; set; }
        public List<ExternalLogin> ExternalLogins { get; set; } = [];
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            // ======================
            // Properties
            // ======================
            builder.Property(u => u.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(u => u.GoogleId)
                   .HasMaxLength(200);

            builder.Property(u => u.FacebookId)
                   .HasMaxLength(200);

            builder.Property(u => u.PasswordHash)
                   .HasMaxLength(500);

            builder.Property(u => u.IsActive)
                   .IsRequired();

            builder.Property(u => u.Blocked)
                   .IsRequired();

            builder.Property(u => u.Activated)
                   .IsRequired();

            // ======================
            // INDEXY
            // ======================
            builder.HasIndex(u => u.Email)
           .IsUnique();

            builder.HasIndex(x => x.GoogleId)
           .IsUnique()
           .HasFilter("[GoogleId] IS NOT NULL");

            builder.HasIndex(x => x.FacebookId)
           .IsUnique()
           .HasFilter("[FacebookId] IS NOT NULL");

            // ======================
            // RELACJE
            // ======================

            // ExternalLogins (one-to-many)
            builder.HasMany(u => u.ExternalLogins)
                   .WithOne(e => e.User)
                   .HasForeignKey(e => e.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
