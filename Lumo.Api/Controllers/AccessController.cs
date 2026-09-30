using Lumo.Api.DTO.User;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessController : AuthorizedController
    {
        [HttpGet("me")]
        public ActionResult<ProfileResponse> Me()
        {
            var profile = new ProfileResponse
            {
                Id = AuthorizedUser.Id,
                FullName = AuthorizedUser.FullName,
                Email = AuthorizedUser.Email
            };

            return Ok(profile);
        }
    }
}
