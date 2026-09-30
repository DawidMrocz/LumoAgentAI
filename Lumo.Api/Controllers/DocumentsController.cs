using Lumo.Api.Services.RAG.Ingestion;
using Lumo.Api.Services.RAG.Retrieval;
using Microsoft.AspNetCore.Mvc;

namespace Lumo.Api.Controllers
{
    [ApiController]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class DocumentsController : AuthorizedController
    {
        private readonly IRagIngestionService _ragIngestionService;
        private readonly IRagSearchService _ragSearchService;

        public DocumentsController(IRagIngestionService ragIngestionService, IRagSearchService ragSearchService)
        {
            _ragIngestionService = ragIngestionService;
            _ragSearchService = ragSearchService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string collectionName, [FromQuery] int limit = 5, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required.");

            if (string.IsNullOrWhiteSpace(collectionName))
                return BadRequest("Collection name is required.");

            if (limit <= 0)
                return BadRequest("Limit must be greater than 0.");

            var results = await _ragSearchService.SearchAsync(query, collectionName, limit, cancellationToken);

            return Ok(results);
        }

        [HttpPost("ingest")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Ingest(IFormFile file, [FromForm] string collectionName, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("File is required.");

            if (string.IsNullOrWhiteSpace(collectionName))
                return BadRequest("Collection name is required.");

            await using var stream = file.OpenReadStream();

            var result = await _ragIngestionService.IngestAsync(stream, file.FileName, collectionName, cancellationToken);

            return Ok(result);
        }
    }
}
