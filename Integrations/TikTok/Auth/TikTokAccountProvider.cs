using Integrations.TikTok.Auth.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Auth
{
    internal sealed class TikTokAccountProvider : ITikTokAccountProvider
    {
        private const string AccessTokenProtectorPurpose =
            "Lumo.TikTok.OAuth.AccessToken";

        private const string RefreshTokenProtectorPurpose =
            "Lumo.TikTok.OAuth.RefreshToken";

        private readonly ITikTokAccountRepository _repository;

        private readonly IDataProtector _accessTokenProtector;
        private readonly IDataProtector _refreshTokenProtector;

        public TikTokAccountProvider(
            ITikTokAccountRepository repository,
            IDataProtectionProvider dataProtectionProvider)
        {
            _repository = repository;

            _accessTokenProtector =
                dataProtectionProvider.CreateProtector(
                    AccessTokenProtectorPurpose);

            _refreshTokenProtector =
                dataProtectionProvider.CreateProtector(
                    RefreshTokenProtectorPurpose);
        }

        public async Task<TikTokAccount?> GetAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var entity =
                await _repository.GetAsync(
                    userId,
                    cancellationToken);

            if (entity is null)
                return null;

            return new TikTokAccount
            {
                UserId = entity.UserId,
                OpenId = entity.OpenId,

                AccessTokenProtected =
                    _accessTokenProtector.Unprotect(
                        entity.AccessTokenProtected),

                RefreshTokenProtected =
                    _refreshTokenProtector.Unprotect(
                        entity.RefreshTokenProtected),

                GrantedScopes = entity.GrantedScopes,

                AccessTokenExpiresAt =
                    entity.AccessTokenExpiresAt,

                RefreshTokenExpiresAt =
                    entity.RefreshTokenExpiresAt
            };
        }

        public async Task SaveAsync(
            SaveTikTokAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var existing =
                await _repository.GetAsync(
                    request.UserId,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(request.AccessToken))
                throw new InvalidOperationException(
                    "TikTok nie zwrócił access tokena.");

            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new InvalidOperationException(
                    "TikTok nie zwrócił refresh tokena.");

            var entity = new TikTokAccount
            {
                UserId = request.UserId,
                OpenId = request.OpenId,

                AccessTokenProtected =
                    _accessTokenProtector.Protect(
                        request.AccessToken),

                RefreshTokenProtected =
                    _refreshTokenProtector.Protect(
                        request.RefreshToken),

                GrantedScopes =
                    request.GrantedScopes ?? string.Empty,

                AccessTokenExpiresAt =
                    request.AccessTokenExpiresAt,

                RefreshTokenExpiresAt =
                    request.RefreshTokenExpiresAt
            };

            await _repository.SaveAsync(
                entity,
                cancellationToken);
        }

        public Task DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            return _repository.DeleteAsync(
                userId,
                cancellationToken);
        }
    }
}
