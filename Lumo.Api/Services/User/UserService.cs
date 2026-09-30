using Integrations.Email;
using Integrations.Email.Models;
using Integrations.Google.Auth;
using Integrations.Google.Auth.Models;
using Integrations.Meta.Auth;
using Integrations.Meta.Auth.Models;
using Lumo.Api.ApiModels.User;
using Lumo.Api.Data.User;
using Lumo.Api.DTO.User;
using Lumo.Api.Exceptions;
using Lumo.Api.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Lumo.Api.Services.User
{
    internal class UserService : IUserService
    {
        /// <summary>
        /// Jednakowy komunikat dla wszystkich niepowodzeń logowania hasłem.
        /// Rozróżnianie "nie ma konta" / "złe hasło" / "konto bez hasła" pozwalałoby
        /// atakującemu wyliczyć, które adresy e-mail są zarejestrowane.
        /// </summary>
        private const string InvalidCredentialsMessage = "Niepoprawny e-mail lub hasło";

        /// <summary>
        /// Hash "atrapa" o poprawnym formacie BCrypt. Weryfikujemy go, gdy konto nie istnieje
        /// albo nie ma hasła, żeby czas odpowiedzi był zbliżony do zwykłej weryfikacji
        /// i nie zdradzał istnienia konta.
        /// </summary>
        private const string DummyPasswordHash =
            "$2a$12$C6UzMDM.H6dfI/f/IKcEe.3AqYbLBGRBhAKzHqiaGDMXO4qRuKzSK";

        private const int RefreshTokenLifetimeDays = 14;
        private const int AccessTokenLifetimeMinutes = 15;

        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IGoogleSignInService _googleSignInService;
        private readonly IMetaSignInService _metaSignInService;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IConfiguration configuration,
            AppDbContext dbContext,
            IWebHostEnvironment environment,
            IGoogleSignInService googleSignInService,
            IMetaSignInService metaSignInService,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _environment = environment;
            _googleSignInService = googleSignInService;
            _metaSignInService = metaSignInService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ProfileResponse> Profile(int userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new ProfileResponse
                {
                    Id = x.Id.ToString(),
                    FullName = x.FirstName,
                    Email = x.Email
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Nie znaleziono użytkownika");
        }

        public async Task<AuthResponse> Login(LoginRequest request, string ipAddress)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

            // Kolejność jest istotna: najpierw weryfikujemy hasło (zawsze, także dla
            // nieistniejących kont - na atrapie), a dopiero potem ujawniamy cokolwiek
            // o stanie konta. Dzięki temu komunikaty o aktywacji/blokadzie widzi
            // wyłącznie ktoś, kto zna prawidłowe hasło.
            var passwordHash = user?.PasswordHash;

            var passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                string.IsNullOrEmpty(passwordHash) ? DummyPasswordHash : passwordHash);

            if (user is null || string.IsNullOrEmpty(passwordHash) || !passwordValid)
            {
                throw new UnauthorizedException(InvalidCredentialsMessage);
            }

            EnsureAccountUsable(user);

            if (!user.Activated)
            {
                throw new BusinessException("Konto nie zostało aktywowane");
            }

            return await IssueAuthResponseAsync(user, ipAddress);
        }

        public async Task Register(RegisterRequest request)
        {
            var userEmailExists = await _dbContext.Users
                .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

            if (userEmailExists)
                throw new BusinessException("Uzytkownik o podanym adresie e-mail już istnieje");

            var user = new Data.User.User()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                Activated = false
            };

            await _dbContext.Users.AddAsync(user);

            var plainToken = SecureTokenGenerator.Generate();

            var activationToken = new UserActivationToken
            {
                User = user,
                Token = SecureTokenGenerator.Hash(plainToken),
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            await _dbContext.UserActivationTokens.AddAsync(activationToken);
            await _dbContext.SaveChangesAsync();

            await SendActivationEmailAsync(user, plainToken);
        }

        /// <summary>
        /// Odświeżenie tokena (rotacja refresh tokena z detekcją ponownego użycia).
        /// </summary>
        public async Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress)
        {
            var token = await _dbContext.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == RefreshTokenGenerator.HashToken(refreshToken))
                ?? throw new UnauthorizedException("Błąd autoryzacji");

            // Ponowne użycie odwołanego tokena oznacza, że ktoś przechwycił sesję -
            // unieważniamy wszystkie tokeny użytkownika.
            if (token.IsRevoked)
            {
                await RevokeAllTokens(token.UserId);
                throw new UnauthorizedException("Wykryto próbę przejęcia sesji");
            }

            if (token.AbsoluteExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedException("Błąd autoryzacji");

            EnsureAccountUsable(token.User);

            var plainRefreshToken = RefreshTokenGenerator.GenerateRefreshToken();

            var newToken = new RefreshToken
            {
                UserId = token.UserId,
                RefreshTokenHash = RefreshTokenGenerator.HashToken(plainRefreshToken),
                CreatedAt = DateTime.UtcNow,
                // Rotacja NIE przedłuża sesji w nieskończoność - zachowujemy pierwotny
                // absolutny termin wygaśnięcia z łańcucha tokenów.
                AbsoluteExpiresAt = token.AbsoluteExpiresAt,
                IpAddress = ipAddress,
                LastUsedAt = DateTime.UtcNow
            };

            await _dbContext.RefreshTokens.AddAsync(newToken);
            await _dbContext.SaveChangesAsync();

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenId = newToken.Id;
            token.LastUsedAt = DateTime.UtcNow;

            var accessToken = await CreateAccessToken(token.User);

            await _dbContext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken.accessToken,
                AccessTokenExpiresIn = accessToken.expiresIn,
                RefreshToken = plainRefreshToken,
                RefreshTokenExpiresIn = newToken.AbsoluteExpiresAt
            };
        }

        /// <summary>
        /// Wylogowanie / unieważnienie pojedynczego refresh tokena
        /// </summary>
        public async Task RevokeToken(string refreshToken, int userId)
        {
            var hash = RefreshTokenGenerator.HashToken(refreshToken);

            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(t => t.RefreshTokenHash == hash)
                ?? throw new UnauthorizedException("Token nie istnieje");

            if (token.UserId != userId)
                throw new UnauthorizedException("Brak autoryzacji");

            if (token.IsRevoked)
                return;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenId = null;

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Wylogowanie ze wszystkich urządzeń (global logout)
        /// </summary>
        public async Task RevokeAllTokens(int userId)
        {
            var tokens = await _dbContext.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            var now = DateTime.UtcNow;

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = now;
                token.ReplacedByTokenId = null;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<SessionResponse>> GetSessions(int userId, string? currentRefreshToken = null)
        {
            var currentHash = string.IsNullOrWhiteSpace(currentRefreshToken)
                ? null
                : RefreshTokenGenerator.HashToken(currentRefreshToken);

            var now = DateTime.UtcNow;

            var tokens = await _dbContext.RefreshTokens
                .AsNoTracking()
                .Where(t => t.UserId == userId && !t.IsRevoked && t.AbsoluteExpiresAt > now)
                .OrderByDescending(t => t.LastUsedAt)
                .ToListAsync();

            return tokens.Select(t => new SessionResponse
            {
                Id = t.Id,
                IpAddress = t.IpAddress,
                CreatedAt = t.CreatedAt,
                LastUsedAt = t.LastUsedAt,
                AbsoluteExpiresAt = t.AbsoluteExpiresAt,
                IsCurrent = currentHash is not null && t.RefreshTokenHash == currentHash
            }).ToList();
        }

        private async Task<(string accessToken, DateTime expiresIn)> CreateAccessToken(Data.User.User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            string privateKey = _configuration["JWT:JwtPrivateKey"] ?? throw new Exception("JWT private key not set");

            using RSA rsa = RSA.Create();

            rsa.ImportRSAPrivateKey(
                source: Convert.FromBase64String(privateKey),
                bytesRead: out int _);

            var key = new RsaSecurityKey(rsa)
            {
                KeyId = "MyStaticKeyId123"
            };

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory
                {
                    CacheSignatureProviders = false
                }
            };

            var expiresIn = DateTime.UtcNow.AddMinutes(AccessTokenLifetimeMinutes);

            await Task.CompletedTask;

            return (new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? throw new Exception("Brak JWT Issuer"),
                audience: _configuration["Jwt:Audience"] ?? throw new Exception("Brak JWT Audience"),
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresIn,
                signingCredentials: signingCredentials
            )), expiresIn);
        }

        public async Task ActivateAccount(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new UnauthorizedException("Token jest wymagany");

            var tokenHash = SecureTokenGenerator.Hash(token);

            var activation = await _dbContext.UserActivationTokens
                .Include(a => a.User)
                .FirstOrDefaultAsync(a =>
                    a.Token == tokenHash &&
                    !a.Used &&
                    a.ExpiresAt > DateTime.UtcNow)
                ?? throw new BusinessException("Niepoprawny lub wygasły token");

            if (activation.User.Activated)
                throw new BusinessException("Konto jest już aktywne");

            activation.User.Activated = true;
            activation.Used = true;

            await _dbContext.SaveChangesAsync();
        }

        public async Task ResendActivation(string email)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            // Cicho kończymy dla nieistniejącego / już aktywnego konta,
            // żeby endpoint nie służył do sprawdzania, kto jest zarejestrowany.
            if (user is null || user.Activated || !user.IsActive)
                return;

            var oldTokens = await _dbContext.UserActivationTokens
                .Where(x => x.UserId == user.Id && !x.Used)
                .ToListAsync();

            foreach (var t in oldTokens)
                t.Used = true;

            var plainToken = SecureTokenGenerator.Generate();

            var newToken = new UserActivationToken
            {
                UserId = user.Id,
                Token = SecureTokenGenerator.Hash(plainToken),
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };

            await _dbContext.UserActivationTokens.AddAsync(newToken);
            await _dbContext.SaveChangesAsync();

            await SendActivationEmailAsync(user, plainToken);
        }

        public async Task<AuthResponse> GoogleLogin(string idToken, string ipAddress)
        {
            GoogleSignInPayload payload;

            try
            {
                payload = await _googleSignInService.ValidateIdTokenAsync(idToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedException(ex.Message);
            }

            var user = await ResolveExternalUserAsync(
                provider: "Google",
                findByProviderId: u => u.GoogleId == payload.Subject,
                linkProviderId: u => u.GoogleId = payload.Subject,
                setProviderIdOnNew: u => u.GoogleId = payload.Subject,
                email: payload.Email,
                firstName: payload.FirstName,
                lastName: payload.LastName);

            return await IssueAuthResponseAsync(user, ipAddress);
        }

        public async Task<AuthResponse> FacebookLogin(string accessToken, string ipAddress)
        {
            MetaSignInPayload payload;

            try
            {
                payload = await _metaSignInService.ValidateAccessTokenAsync(accessToken);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedException(ex.Message);
            }

            var user = await ResolveExternalUserAsync(
                provider: "Facebook",
                findByProviderId: u => u.FacebookId == payload.Subject,
                linkProviderId: u => u.FacebookId = payload.Subject,
                setProviderIdOnNew: u => u.FacebookId = payload.Subject,
                email: payload.Email,
                firstName: payload.FirstName,
                lastName: payload.LastName);

            return await IssueAuthResponseAsync(user, ipAddress);
        }

        /// <summary>
        /// Wspólna logika logowania społecznościowego: znajdź po id dostawcy,
        /// ewentualnie dowiąż do istniejącego konta o tym samym e-mailu, albo utwórz nowe.
        /// </summary>
        private async Task<Data.User.User> ResolveExternalUserAsync(
            string provider,
            System.Linq.Expressions.Expression<Func<Data.User.User, bool>> findByProviderId,
            Action<Data.User.User> linkProviderId,
            Action<Data.User.User> setProviderIdOnNew,
            string email,
            string? firstName,
            string? lastName)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(findByProviderId);

            if (user is not null)
            {
                return user;
            }

            var byEmail = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (byEmail is not null)
            {
                // Dowiązanie konta społecznościowego do istniejącego konta to potencjalny
                // wektor przejęcia. Robimy to tylko dla kont już aktywowanych - właściciel
                // udowodnił kontrolę nad tym adresem e-mail przy aktywacji.
                if (!byEmail.Activated)
                {
                    throw new BusinessException(
                        "Konto o tym adresie e-mail istnieje, ale nie zostało aktywowane. " +
                        "Aktywuj je, zanim połączysz logowanie przez " + provider + ".");
                }

                linkProviderId(byEmail);

                await _dbContext.SaveChangesAsync();

                return byEmail;
            }

            var created = new Data.User.User
            {
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                Email = email,
                Activated = true,
                IsActive = true
            };

            setProviderIdOnNew(created);

            _dbContext.Users.Add(created);

            await _dbContext.SaveChangesAsync();

            return created;
        }

        /// <summary>
        /// Wspólny finał każdej ścieżki logowania: kontrola stanu konta,
        /// wystawienie access tokena i nowego refresh tokena.
        /// </summary>
        private async Task<AuthResponse> IssueAuthResponseAsync(Data.User.User user, string ipAddress)
        {
            EnsureAccountUsable(user);

            var (accessToken, expiresIn) = await CreateAccessToken(user);

            var plainRefreshToken = RefreshTokenGenerator.GenerateRefreshToken();
            var hash = RefreshTokenGenerator.HashToken(plainRefreshToken);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                RefreshTokenHash = hash,
                CreatedAt = DateTime.UtcNow,
                AbsoluteExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenLifetimeDays),
                IpAddress = ipAddress,
                LastUsedAt = DateTime.UtcNow
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                AccessTokenExpiresIn = expiresIn,
                RefreshToken = plainRefreshToken,
                RefreshTokenExpiresIn = refreshToken.AbsoluteExpiresAt
            };
        }

        /// <summary>
        /// Kontrole stanu konta wspólne dla logowania hasłem, logowania społecznościowego
        /// i odświeżania tokena. Konto dezaktywowane (IsActive=false) nie może się zalogować
        /// ŻADNĄ ścieżką.
        /// </summary>
        private static void EnsureAccountUsable(Data.User.User user)
        {
            if (user.Blocked)
                throw new BusinessException("Konto jest zablokowane");

            if (!user.IsActive)
                throw new UnauthorizedException("Konto zostało dezaktywowane");
        }

        public async Task ForgotPassword(string email)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            // Zawsze zwracamy sukces w kontrolerze - brak informacji, czy konto istnieje.
            if (user is null || !user.IsActive || user.Blocked)
                return;

            var oldTokens = await _dbContext.PasswordResetTokens
                .Where(x => x.UserId == user.Id && !x.Used)
                .ToListAsync();

            foreach (var t in oldTokens)
                t.Used = true;

            var plainToken = SecureTokenGenerator.Generate();

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = SecureTokenGenerator.Hash(plainToken),
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };

            await _dbContext.PasswordResetTokens.AddAsync(resetToken);
            await _dbContext.SaveChangesAsync();

            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
            var resetLink = $"{frontendUrl}/reset-password?token={plainToken}";

            await SendTemplatedOrFallbackAsync(
                user,
                templateName: "password_reset",
                variables: new Dictionary<string, string>
                {
                    ["FirstName"] = user.FirstName,
                    ["ResetLink"] = resetLink
                },
                fallbackSubject: "Reset hasła w Lumo",
                fallbackHtml:
                    $"<p>Cześć {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>" +
                    $"<p>Kliknij, aby zresetować hasło: <a href=\"{resetLink}\">Resetuj hasło</a></p>" +
                    "<p>Link jest ważny przez 2 godziny. Jeśli to nie Ty prosiłeś o reset, zignoruj tę wiadomość.</p>");
        }

        public async Task ResetPassword(string token, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new BusinessException("Token jest wymagany");

            var tokenHash = SecureTokenGenerator.Hash(token);

            var resetToken = await _dbContext.PasswordResetTokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Token == tokenHash &&
                    x.ExpiresAt > DateTime.UtcNow &&
                    !x.Used)
                ?? throw new BusinessException("Niepoprawny lub wygasły token");

            resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            resetToken.Used = true;

            // Reset hasła unieważnia wszystkie istniejące sesje.
            await RevokeAllTokens(resetToken.UserId);

            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new BusinessException("Użytkownik nie istnieje");

            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new BusinessException(
                    "To konto nie ma ustawionego hasła (logowanie przez Google/Facebook). Użyj opcji ustawienia hasła.");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new BusinessException("Niepoprawne obecne hasło");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

            await RevokeAllTokens(userId);

            await _dbContext.SaveChangesAsync();
        }

        public async Task SetPassword(int userId, string newPassword)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new BusinessException("Użytkownik nie istnieje");

            if (!string.IsNullOrEmpty(user.PasswordHash))
                throw new BusinessException("Konto ma już ustawione hasło - użyj zmiany hasła.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);

            await RevokeAllTokens(userId);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UnlinkExternalLogin(int userId, string provider)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new BusinessException("Użytkownik nie istnieje");

            var normalized = (provider ?? string.Empty).Trim().ToLowerInvariant();

            var hasPassword = !string.IsNullOrEmpty(user.PasswordHash);
            var hasGoogle = !string.IsNullOrEmpty(user.GoogleId);
            var hasFacebook = !string.IsNullOrEmpty(user.FacebookId);

            switch (normalized)
            {
                case "google":
                    if (!hasGoogle)
                        throw new BusinessException("To konto nie jest połączone z Google");

                    if (!hasPassword && !hasFacebook)
                        throw new BusinessException(
                            "Nie można odłączyć Google - to jedyna metoda logowania. Ustaw najpierw hasło.");

                    user.GoogleId = null;
                    break;

                case "facebook":
                    if (!hasFacebook)
                        throw new BusinessException("To konto nie jest połączone z Facebookiem");

                    if (!hasPassword && !hasGoogle)
                        throw new BusinessException(
                            "Nie można odłączyć Facebooka - to jedyna metoda logowania. Ustaw najpierw hasło.");

                    user.FacebookId = null;
                    break;

                default:
                    throw new BusinessException($"Nieznany dostawca logowania: {provider}");
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAccount(int userId, string? currentPassword)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new BusinessException("Użytkownik nie istnieje");

            // Konto z hasłem wymaga potwierdzenia hasłem; konto wyłącznie społecznościowe
            // jest chronione samym faktem posiadania ważnego tokena.
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (string.IsNullOrWhiteSpace(currentPassword) ||
                    !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    throw new BusinessException("Niepoprawne hasło");
                }
            }

            var refreshTokens = await _dbContext.RefreshTokens.Where(x => x.UserId == userId).ToListAsync();
            var activationTokens = await _dbContext.UserActivationTokens.Where(x => x.UserId == userId).ToListAsync();
            var resetTokens = await _dbContext.PasswordResetTokens.Where(x => x.UserId == userId).ToListAsync();

            _dbContext.RefreshTokens.RemoveRange(refreshTokens);
            _dbContext.UserActivationTokens.RemoveRange(activationTokens);
            _dbContext.PasswordResetTokens.RemoveRange(resetTokens);
            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfile(int userId, UpdateProfileRequest request)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new BusinessException("Nie znaleziono użytkownika");

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;

            await _dbContext.SaveChangesAsync();
        }

        private async Task SendActivationEmailAsync(Data.User.User user, string plainToken)
        {
            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/');
            var activationLink = $"{frontendUrl}/activate?token={plainToken}";

            await SendTemplatedOrFallbackAsync(
                user,
                templateName: "account_activation",
                variables: new Dictionary<string, string>
                {
                    ["FirstName"] = user.FirstName,
                    ["ActivationLink"] = activationLink
                },
                fallbackSubject: "Aktywuj swoje konto w Lumo",
                fallbackHtml:
                    $"<p>Cześć {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>" +
                    "<p>Dziękujemy za rejestrację w Lumo.</p>" +
                    $"<p>Aby aktywować konto, kliknij: <a href=\"{activationLink}\">Aktywuj konto</a></p>" +
                    "<p>Link jest ważny przez 24 godziny.</p>");
        }

        /// <summary>
        /// Próbuje wysłać e-mail z szablonu w bazie; gdy szablonu nie ma, używa treści wbudowanej.
        /// Błąd wysyłki nie może wywrócić rejestracji ani resetu hasła - token jest już zapisany,
        /// a użytkownik może poprosić o ponowną wysyłkę.
        /// </summary>
        private async Task SendTemplatedOrFallbackAsync(
            Data.User.User user,
            string templateName,
            Dictionary<string, string> variables,
            string fallbackSubject,
            string fallbackHtml)
        {
            // W Development wypisujemy link do logów, żeby dało się dokończyć rejestrację
            // lub reset hasła bez skonfigurowanego serwera SMTP.
            if (_environment.IsDevelopment())
            {
                var link = variables.TryGetValue("ActivationLink", out var a) ? a
                    : variables.TryGetValue("ResetLink", out var r) ? r
                    : null;

                if (link is not null)
                {
                    _logger.LogWarning("[DEV] Link '{Template}' dla {Email}: {Link}",
                        templateName, user.Email, link);
                }
            }

            try
            {
                var template = await _emailService.GetTemplateAsync(templateName);

                if (template is not null && template.IsActive)
                {
                    await _emailService.SendTemplatedEmailAsync(new SendTemplatedEmailRequest
                    {
                        TemplateName = templateName,
                        To = [user.Email],
                        Variables = variables
                    }, user.Id);

                    return;
                }

                await _emailService.SendEmailAsync(new SendEmailRequest
                {
                    To = [user.Email],
                    Subject = fallbackSubject,
                    HtmlBody = fallbackHtml
                }, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Nie udało się wysłać e-maila '{Template}' do użytkownika {UserId}",
                    templateName, user.Id);
            }
        }
    }
}
