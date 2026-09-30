using Integrations.Email;
using Integrations.Email.Client;
using Integrations.Email.Services;
using Integrations.Email.Templates;
using Integrations.Google.API.Ads;
using Integrations.Google.API.Ads.Client;
using Integrations.Google.API.Ads.Services;
using Integrations.Google.API.BusinessProfile;
using Integrations.Google.API.BusinessProfile.Services;
using Integrations.Google.API.Calendar;
using Integrations.Google.API.Calendar.Services;
using Integrations.Google.API.Drive;
using Integrations.Google.API.Drive.Services;
using Integrations.Google.API.Veo;
using Integrations.Google.API.Veo.Client;
using Integrations.Google.API.Veo.Services;
using Integrations.Google.Auth;
using Integrations.Google.Factory;
using Integrations.Meta.Auth;
using Integrations.Meta.Factory;
using Integrations.Meta.Graph;
using Integrations.Przelewy24;
using Integrations.TikTok.Auth;
using Integrations.TikTok.Client;
using Integrations.TikTok.Factory;
using Integrations.TikTok.Services;
using Integrations.Twilio.Client;
using Integrations.Twilio.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Integrations
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIntegrations(this IServiceCollection services)
        {
            // GOOGLE  
            services.AddScoped<IGoogleServiceFactory, GoogleServiceFactory>();
            services.AddScoped<IGoogleCredentialProvider, GoogleCredentialProvider>();
            services.AddScoped<IGoogleAccountProvider, GoogleAccountProvider>();
            services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
            services.AddScoped<IGoogleSignInService, GoogleSignInService>();

            services.AddScoped<IGoogleCalendarService,GoogleCalendarService>();
            services.AddScoped<IGoogleDriveService, GoogleDriveService>();

            services.AddScoped<IGoogleVeoClient, GoogleVeoClient>();
            services.AddScoped<IGoogleVeoService, GoogleVeoService>();

            services.AddScoped<IGoogleAdsClientFactory, GoogleAdsClientFactory>();
            services.AddScoped<IGoogleAdsService, GoogleAdsService>();

            services.AddScoped<IGoogleBusinessProfileService, GoogleBusinessProfileService>();

            // META
            services.AddHttpClient("meta", client =>
            {
                client.BaseAddress =
                    new Uri("https://graph.facebook.com");
            });

            services.AddScoped<IMetaAccountProvider, MetaAccountProvider>();
            services.AddScoped<IMetaCredentialProvider, MetaCredentialProvider>();
            services.AddScoped<IMetaOAuthService, MetaOAuthService>();
            services.AddScoped<IMetaSignInService, MetaSignInService>();

            services.AddScoped<IMetaGraphClient, MetaGraphClient>();

            services.AddScoped<IMetaInstagramService, MetaInstagramService>();
            services.AddScoped<IMetaPageService, MetaPageService>();

            services.AddScoped<IMetaServiceFactory, MetaServiceFactory>();

            //TIKTOK
            // TIKTOK

            services.AddHttpClient("tiktok", client =>
            {
                client.BaseAddress =
                    new Uri("https://open.tiktokapis.com");

                client.Timeout =
                    TimeSpan.FromMinutes(5);
            });

            services.AddScoped<ITikTokAccountProvider, TikTokAccountProvider>();
            services.AddScoped<ITikTokCredentialProvider, TikTokCredentialProvider>();
            services.AddScoped<ITikTokOAuthService, TikTokOAuthService>();

            services.AddScoped<ITikTokClient, TikTokClient>();
            services.AddScoped<ITikTokVideoService, TikTokVideoService>();
            services.AddScoped<ITikTokUserService, TikTokUserService>();
            services.AddScoped<ITikTokServiceFactory, TikTokServiceFactory>();

            // TWILIO / SMS

            services.AddScoped<ITwilioClient, TwilioClient>();
            services.AddScoped<ISmsService, SmsService>();

            // PRZELEWY24

            services.AddScoped<IPrzelewy24Service, Przelewy24Service>();

            // EMAIL / SMTP

            services.AddScoped<IEmailClient, EmailClient>();
            services.AddScoped<IEmailTemplateEngine, EmailTemplateEngine>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
