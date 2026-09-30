using Lumo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumo.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Brak ConnectionStrings:DefaultConnection.");
            }

            services.AddDbContext<AppDbContext>(options => 
            { 
                options.UseSqlServer(connectionString); 
            });

            services.AddScoped<IGoogleAccountRepository, GoogleAccountRepository>();
            services.AddScoped<IGoogleConfigRepository, GoogleConfigRepository>();

            services.AddScoped<IMetaAccountRepository, MetaAccountRepository>();
            services.AddScoped<IMetaConfigRepository, MetaConfigRepository>();

            services.AddScoped<ITikTokAccountRepository, TikTokAccountRepository>();
            services.AddScoped<ITikTokConfigRepository, TikTokConfigRepository>();
            services.AddScoped<ITikTokVideoRepository, TikTokVideoRepository>();

            services.AddScoped<IVeoVideoRepository, VeoVideoRepository>();
            services.AddScoped<IVeoConfigRepository, VeoConfigRepository>();

            services.AddScoped<ISmsConfigRepository, SmsConfigRepository>();
            services.AddScoped<ISmsMessageRepository, SmsMessageRepository>();

            services.AddScoped<IEmailConfigRepository, EmailConfigRepository>();
            services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();
            services.AddScoped<IEmailMessageRepository, EmailMessageRepository>();

            services.AddScoped<IPaymentRepository, PaymentRepository>();

            return services;
        }
    }
}
