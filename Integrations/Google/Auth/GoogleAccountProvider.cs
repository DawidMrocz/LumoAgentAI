using Integrations.Google.Auth.Models;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;

namespace Integrations.Google.Auth
{
    internal class GoogleAccountProvider : IGoogleAccountProvider
    {

        private const string ProtectorPurpose = "Lumo.Google.OAuth.RefreshToken";

        private readonly IGoogleAccountRepository _googleAccountRepository;
        private readonly IDataProtector _tokenProtector;

        public GoogleAccountProvider(IGoogleAccountRepository googleAccountRepository, IDataProtectionProvider dataProtectionProvider)
        {
            _googleAccountRepository = googleAccountRepository;
            _tokenProtector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        }

        public async Task<GoogleAccount?> GetAsync(int userId, CancellationToken cancellationToken = default)
        {
            var entity = await _googleAccountRepository.GetAsync(userId, cancellationToken);

            if (entity is null)
                return null;

            return new GoogleAccount
            {
                UserId = entity.UserId,
                Email = entity.Email,
                RefreshTokenProtected = _tokenProtector.Unprotect(entity.RefreshTokenProtected),
                GrantedScopes = entity.GrantedScopes
            };
        }

        public async Task SaveAsync(SaveGoogleAccountRequest request, CancellationToken cancellationToken = default)
        {
            var existing = await _googleAccountRepository.GetAsync(request.UserId, cancellationToken);

            string protectedRefreshToken;

            if (!string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                protectedRefreshToken = _tokenProtector.Protect(request.RefreshToken);
            }
            else if (existing is not null)
            {
                protectedRefreshToken = existing.RefreshTokenProtected;
            }
            else
            {
                throw new InvalidOperationException("Google nie zwrócił RefreshToken dla nowego konta.");
            }

            var now = DateTime.UtcNow;

            var entity = new GoogleAccount
            {
                Id = existing?.Id ?? 0,
                UserId = request.UserId,
                Email = request.Email,
                RefreshTokenProtected = protectedRefreshToken,
                GrantedScopes = request.GrantedScopes ?? string.Empty,
                CreatedAt = existing?.CreatedAt ?? now,
                UpdatedAt = now
            };

            await _googleAccountRepository.SaveAsync(entity, cancellationToken);
        }

        public Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
        {
            return _googleAccountRepository.DeleteAsync(userId, cancellationToken);
        }
    }
}
