using Lumo.Api.ApiModels.User;
using Lumo.Api.DTO.User;

namespace Lumo.Api.Services.User
{
    public interface IUserService
    {
        Task Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request, string idAddress);
        Task<ProfileResponse> Profile(int id);
        Task<AuthResponse> RefreshToken(string refreshToken, string ipAddress);
        Task ActivateAccount(string token);
        Task ResendActivation(string email);
        Task RevokeToken(string refreshToken, int userId);
        Task RevokeAllTokens(int userId);
        Task<AuthResponse> GoogleLogin(string idToken, string ipAddress);
        Task<AuthResponse> FacebookLogin(string accessToken, string ipAddress);
        Task ForgotPassword(string email);
        Task ResetPassword(string token, string newPassword);
        Task ChangePassword(int userId, string currentPassword, string newPassword);
        Task UpdateProfile(int userId, UpdateProfileRequest request);

        /// <summary>
        /// Ustawia hasło dla konta założonego przez Google/Facebook (które nie ma jeszcze hasła).
        /// </summary>
        Task SetPassword(int userId, string newPassword);

        /// <summary>
        /// Odłącza dostawcę logowania społecznościowego ("Google" albo "Facebook").
        /// Blokuje odłączenie ostatniej metody logowania, żeby nie odciąć użytkownikowi dostępu.
        /// </summary>
        Task UnlinkExternalLogin(int userId, string provider);

        /// <summary>
        /// Zwraca listę aktywnych sesji (nieodwołanych, niewygasłych refresh tokenów).
        /// </summary>
        Task<List<SessionResponse>> GetSessions(int userId, string? currentRefreshToken = null);

        /// <summary>
        /// Trwale usuwa konto użytkownika wraz z tokenami (RODO / prawo do bycia zapomnianym).
        /// </summary>
        Task DeleteAccount(int userId, string? currentPassword);
    }
}
