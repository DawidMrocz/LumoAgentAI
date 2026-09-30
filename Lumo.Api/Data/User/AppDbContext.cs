using Lumo.Api.Data.Agent;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Api.Data.User;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<GmailCredential> GmailCredentials { get; set; }
    public DbSet<GmailOAuthState> GmailOAuthStates { get; set; }
    public DbSet<AgentTask> AgentTasks { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserActivationToken> UserActivationTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<PasswordResetToken> Exter { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("lumo");
    }
}

