using Lumo.Infrastructure.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Infrastructure;

public sealed class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<GoogleAccount> GoogleAccounts => Set<GoogleAccount>();
    public DbSet<GoogleConfig> GoogleConfigs => Set<GoogleConfig>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<TikTokAccount> TikTokAccounts => Set<TikTokAccount>();
    public DbSet<TikTokConfig> TikTokConfigs => Set<TikTokConfig>();
    public DbSet<TikTokVideo> TikTokVideos => Set<TikTokVideo>();
    public DbSet<SmsConfig> SmsConfigs => Set<SmsConfig>();
    public DbSet<SmsMessage> SmsMessages => Set<SmsMessage>();
    public DbSet<MetaAccount> MetaAccounts => Set<MetaAccount>();
    public DbSet<MetaConfig> MetaConfigs => Set<MetaConfig>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<VeoVideo> VeoVideos => Set<VeoVideo>();
    public DbSet<VeoConfig> VeoConfigs => Set<VeoConfig>();
    public DbSet<EmailConfig> EmailConfigs => Set<EmailConfig>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("lumo");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}