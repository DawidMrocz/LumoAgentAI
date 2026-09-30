using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;

namespace Integrations.Google.Auth;

internal sealed class GoogleCredentialProvider : IGoogleCredentialProvider
{
    private const string ProtectorPurpose = "Google.RefreshToken";
    private readonly IDataProtector _protector;

    private readonly IGoogleAccountProvider _googleAccountProvider;
    private readonly IGoogleConfigRepository _googleConfigRepository;

    public GoogleCredentialProvider(
        IGoogleAccountProvider googleAccountProvider,
        IGoogleConfigRepository googleConfigRepository,
        IDataProtectionProvider dataProtectionProvider)
    {
        _googleAccountProvider = googleAccountProvider;
        _googleConfigRepository = googleConfigRepository;

        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
    }

    public async Task<UserCredential> GetCredentialAsync(int userId, CancellationToken cancellationToken = default)
    {
        var config = await _googleConfigRepository.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("Google: Brak aktywnej konfiguracji.");

        var account = await _googleAccountProvider.GetAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Google: Użytkownik nie ma połączonego konta.");

        var scopes = config.Scopes.Split([' ', ',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var scope in scopes)
        {
            Console.WriteLine($"SCOPE: [{scope}]");
        }

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret
            },

            Scopes = scopes
        });

        var token = new TokenResponse
        {
            RefreshToken = account.RefreshTokenProtected
        };

        return new UserCredential(flow, userId.ToString(), token);
    }
}