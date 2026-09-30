using System.Security.Claims;

namespace Lumo.Api.Helpers;

public interface IUserContextAccessor
{
    CurrentUser GetUser();
}

public sealed class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public CurrentUser GetUser()
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("Brak kontekstu użytkownika.");

        if (user.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Użytkownik nie jest uwierzytelniony.");
        }

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException(
                "Brak claimu NameIdentifier w tokenie.");

        var email = user.FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;

        var givenName = user.FindFirstValue(ClaimTypes.GivenName)
            ?? string.Empty;

        var surname = user.FindFirstValue(ClaimTypes.Surname)
            ?? string.Empty;

        var fullName = $"{givenName} {surname}".Trim();

        return new CurrentUser(
            id,
            email,
            fullName);
    }
}

public sealed record CurrentUser(
    string Id,
    string Email,
    string FullName);