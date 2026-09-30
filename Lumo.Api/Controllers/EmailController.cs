using Integrations.Email;
using Integrations.Email.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : AuthorizedController
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<ActionResult<SendEmailResponse>> SendEmail(
            [FromBody] SendEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await _emailService.SendEmailAsync(request, CurrentUserId, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("send-templated")]
        public async Task<ActionResult<SendEmailResponse>> SendTemplatedEmail(
            [FromBody] SendTemplatedEmailRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await _emailService.SendTemplatedEmailAsync(request, CurrentUserId, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("templates")]
        public async Task<ActionResult<IReadOnlyList<EmailTemplateInfo>>> GetTemplates(
            [FromQuery] bool onlyActive = true,
            CancellationToken cancellationToken = default)
        {
            var templates = await _emailService.GetTemplatesAsync(onlyActive, cancellationToken);

            return Ok(templates);
        }

        [HttpGet("templates/{name}")]
        public async Task<ActionResult<EmailTemplateInfo>> GetTemplate(
            [FromRoute] string name,
            CancellationToken cancellationToken = default)
        {
            var template = await _emailService.GetTemplateAsync(name, cancellationToken);

            if (template is null)
                return NotFound();

            return Ok(template);
        }

        [HttpPost("templates")]
        public async Task<ActionResult<EmailTemplateInfo>> CreateTemplate(
            [FromBody] CreateEmailTemplateRequest request,
            CancellationToken cancellationToken = default)
        {
            var template = await _emailService.CreateTemplateAsync(request, cancellationToken);

            return Ok(template);
        }

        [HttpPut("templates/{name}")]
        public async Task<ActionResult<EmailTemplateInfo>> UpdateTemplate(
            [FromRoute] string name,
            [FromBody] UpdateEmailTemplateRequest request,
            CancellationToken cancellationToken = default)
        {
            var template = await _emailService.UpdateTemplateAsync(name, request, cancellationToken);

            return Ok(template);
        }

        [HttpDelete("templates/{name}")]
        public async Task<IActionResult> DeleteTemplate(
            [FromRoute] string name,
            CancellationToken cancellationToken = default)
        {
            await _emailService.DeleteTemplateAsync(name, cancellationToken);

            return NoContent();
        }

        [HttpGet("messages")]
        public async Task<ActionResult<IReadOnlyList<EmailMessageInfo>>> GetMessages(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            var messages = await _emailService.GetUserMessagesAsync(CurrentUserId, skip, take, cancellationToken);

            return Ok(messages);
        }
    }
}
