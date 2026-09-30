using Lumo.Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [Filters.ModelStateCheck]
    [Authorize]
    public abstract class AuthorizedController : ControllerBase
    {
        protected CurrentUser AuthorizedUser => UserContextAccessor.GetUser();
        protected IUserContextAccessor UserContextAccessor => HttpContext?.RequestServices.GetService(typeof(IUserContextAccessor)) as IUserContextAccessor
                    ?? throw new Exception("Authorization provider not found");

        /// <summary>
        /// Id zalogowanego użytkownika. Zawsze pochodzi z podpisanego tokena JWT -
        /// nigdy nie przyjmuj identyfikatora użytkownika z parametrów żądania.
        /// </summary>
        protected int CurrentUserId =>
            int.TryParse(AuthorizedUser.Id, out var id)
                ? id
                : throw new Exceptions.UnauthorizedException("Nieprawidłowy identyfikator użytkownika w tokenie.");
    }
}
