using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lumo.Api.Data.User;
using Microsoft.EntityFrameworkCore;

namespace Lumo.Api.Services.Notifications;

public sealed class FcmService : IFcmService
{
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<FcmService> _logger;

    private string? _accessToken;
    private DateTime _accessTokenExpiryUtc;

    private readonly string _clientEmail;
    private readonly string _privateKeyPem;
    private readonly string _projectId;

    public FcmService(
        AppDbContext db,
        IHttpClientFactory httpFactory,
        IConfiguration config,
        ILogger<FcmService> logger)
    {
        _db = db;
        _httpFactory = httpFactory;
        _config = config;
        _logger = logger;

        // Read service account JSON either from file path or raw JSON in config
        var file = _config["Fcm:ServiceAccountFile"];
        string json;
        if (!string.IsNullOrEmpty(file) && File.Exists(file))
        {
            json = File.ReadAllText(file);
        }
        else
        {
            json = _config["Fcm:ServiceAccountJson"] ?? string.Empty;
        }

        if (string.IsNullOrEmpty(json))
            throw new InvalidOperationException("FCM service account JSON is not configured (Fcm:ServiceAccountFile or Fcm:ServiceAccountJson)");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        _clientEmail = root.GetProperty("client_email").GetString()!;
        _privateKeyPem = root.GetProperty("private_key").GetString()!;
        _projectId = root.GetProperty("project_id").GetString()!;
    }

    public async Task SendDataMessageToUserAsync(string userOid, IDictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.DeviceTokens
            .Where(t => t.UserOid == userOid)
            .Select(t => t.Token)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
            return;

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";

        foreach (var token in tokens)
        {
            var bodyObj = new
            {
                message = new
                {
                    token,
                    data
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(bodyObj), Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.PostAsync(url, content, cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    var text = await resp.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("FCM send failed for user {User} token {Token}: {Status} {Response}", userOid, token, resp.StatusCode, text);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending FCM message to user {User} token {Token}", userOid, token);
            }
        }
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiryUtc > DateTime.UtcNow.AddMinutes(1))
            return _accessToken;

        // Create JWT
        var header = new { alg = "RS256", typ = "JWT" };
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = new Dictionary<string, object>
        {
            ["iss"] = _clientEmail,
            ["scope"] = "https://www.googleapis.com/auth/firebase.messaging",
            ["aud"] = "https://oauth2.googleapis.com/token",
            ["exp"] = now + 3600,
            ["iat"] = now
        };

        string Encode(object o)
        {
            var s = JsonSerializer.Serialize(o);
            var bytes = Encoding.UTF8.GetBytes(s);
            return Base64UrlEncode(bytes);
        }

        var unsigned = Encode(header) + "." + Encode(payload);
        var signature = Sign(unsigned, _privateKeyPem);
        var jwt = unsigned + "." + Base64UrlEncode(signature);

        var client = _httpFactory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = jwt
        });

        var resp = await client.SendAsync(req, cancellationToken);
        var respText = await resp.Content.ReadAsStringAsync(cancellationToken);
        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to get access token from Google: {resp.StatusCode} {respText}");

        using var doc = JsonDocument.Parse(respText);
        var root = doc.RootElement;
        _accessToken = root.GetProperty("access_token").GetString();
        var expiresIn = root.GetProperty("expires_in").GetInt32();
        _accessTokenExpiryUtc = DateTime.UtcNow.AddSeconds(expiresIn);

        return _accessToken!;
    }

    private static byte[] Sign(string unsigned, string privateKeyPem)
    {
        // privateKeyPem is in PEM PKCS8 format -----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n
        var header = "-----BEGIN PRIVATE KEY-----";
        var footer = "-----END PRIVATE KEY-----";
        var pem = privateKeyPem.Replace(header, string.Empty).Replace(footer, string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        var keyBytes = Convert.FromBase64String(pem);

        using var rsa = RSA.Create();
        try
        {
            rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to import private key for FCM JWT signing", ex);
        }

        var bytes = Encoding.UTF8.GetBytes(unsigned);
        return rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
