#pragma warning disable OPENAI001
using Integrations;
using Integrations.Google.API.Drive;
using Lumo.Api.Agents;
using Lumo.Api.Agents.CalendarAgent;
using Lumo.Api.Data.User;
using Lumo.Api.Helpers;
using Lumo.Api.Hubs;
using Lumo.Api.Middlewares;
using Lumo.Api.Services.Agent;
using Lumo.Api.Services.AgentConversation;
using Lumo.Api.Services.AgentSessionStore;
using Lumo.Api.Services.AgentTools;
using Lumo.Api.Services.AgentTools.Email;
using Lumo.Api.Services.AgentTools.Google;
using Lumo.Api.Services.AgentUserContext;
using Lumo.Api.Services.ChatRateLimiter;
using Lumo.Api.Services.CodeInterpreter;
using Lumo.Api.Services.FileSearch;
using Lumo.Api.Services.ImageAnalyze;
using Lumo.Api.Services.ImageGeneration;
using Lumo.Api.Services.Moderation;
using Lumo.Api.Services.Notifications;
using Lumo.Api.Services.RAG.Chunking;
using Lumo.Api.Services.RAG.Embeddings;
using Lumo.Api.Services.RAG.Ingestion;
using Lumo.Api.Services.RAG.Retrieval;
using Lumo.Api.Services.RAG.TextExtractor;
using Lumo.Api.Services.RAG.VectorStore;
using Lumo.Api.Services.User;
using Lumo.Api.Services.Voice;
using Lumo.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAI;
using OpenAI.Responses;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Przycinanie stringów PRZED walidacją (patrz TrimmingStringModelBinderProvider).
    options.ModelBinderProviders.Insert(0, new TrimmingStringModelBinderProvider());
});

// ---------------------------------------------------------------
// CORS - front (www/) jest osobną aplikacją, więc bez tego przeglądarka
// nie wywoła API inaczej niż zza reverse proxy na tym samym origin.
// Origins konfigurowane przez "Cors:AllowedOrigins" (tablica w appsettings).
// ---------------------------------------------------------------
const string CorsPolicyName = "LumoFrontend";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? (builder.Configuration["FrontendUrl"] is { Length: > 0 } frontend
        ? [frontend.TrimEnd('/')]
        : []);

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length == 0)
        {
            // Brak konfiguracji: nie otwieramy API na cały świat.
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();

            return;
        }

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---------------------------------------------------------------
// Rate limiting - ochrona endpointów logowania przed brute-force.
// ---------------------------------------------------------------
const string AuthRateLimitPolicy = "auth";
const string AiRateLimitPolicy = "ai";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(AuthRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Endpointy uderzające w PŁATNE API (czat, obrazy, code interpreter, STT).
    // Limit per użytkownik, nie per IP - inaczej jeden użytkownik za NAT-em
    // blokowałby pozostałych, a zalogowany atakujący i tak omijałby limit
    // zmieniając adres. SignalR ma własny limiter (ChatRateLimiterService).
    options.AddPolicy(AiRateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            status = 429,
            message = "Zbyt wiele prób. Spróbuj ponownie za chwilę."
        }, token);
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Lumo API", Version = "v1" });
    // JWT Bearer token support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Wprowad� token JWT w formacie: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});


builder.Services.AddDbContext<Lumo.Api.Data.User.AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Lumo.Api.Services.AgentTasks.IAgentTaskService, Lumo.Api.Services.AgentTasks.AgentTaskService>();
builder.Services.AddSingleton(sp =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"];

    return new ResponsesClient(apiKey);
});
builder.Services.AddSingleton<OpenAIClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    return new OpenAIClient(
        config["OpenAI:ApiKey"]!);
});

builder.Services.AddSingleton<IAgentUserContextService, AgentUserContextService>();
var qdrantHost = builder.Configuration.GetValue<string>("Qdrant:Host") ?? "localhost";
builder.Services.AddSingleton(sp => new Qdrant.Client.QdrantClient(qdrantHost));

// HttpClient for Qdrant HTTP API (for listing/deleting points)
var qdrantBase = builder.Configuration.GetValue<string>("Qdrant:BaseUrl") ?? $"http://{qdrantHost}:6334";
builder.Services.AddHttpClient("qdrant", client => { client.BaseAddress = new Uri(qdrantBase); });
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton(new Meter("Lumo.AI"));
builder.Services.AddSingleton<IQdrantVectorStoreService, QdrantVectorStoreService>();
builder.Services.AddScoped<IAgentService, AgentService>(); //Mo�e byc Singleton, ale Scoped jest bezpieczniejsze w przypadku zale�no�ci od HttpClient
builder.Services.AddSingleton<IChatClientFactory, ChatClientFactory>();
builder.Services.AddSingleton<AiPricing>();


// IChatClient jest owijany telemetria JUZ TUTAJ, zeby kazdy konsument
// (agent glowny, CalendarAgent, przyszle sub-agenty) raportowal zuzycie tokenow.
// Wczesniej opakowywal go tylko AgentService, wiec wywolania CalendarAgenta
// nie byly w ogole monitorowane.
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var factory = sp.GetRequiredService<IChatClientFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    var provider = configuration["AI:ChatProvider"] ?? "unknown";

    var model = provider.ToLowerInvariant() switch
    {
        "openai" => configuration["OpenAI:Model"],
        "azureopenai" => configuration["AzureOpenAI:ChatDeployment"],
        "ollama" => configuration["Ollama:GenerateModel"],
        _ => "unknown"
    };

    return new ObservabilityChatClient(
        factory.Create(),
        sp.GetRequiredService<ILoggerFactory>(),
        sp.GetRequiredService<Meter>(),
        $"{provider}:{model}",
        sp.GetRequiredService<AiPricing>());
});
builder.Services.AddScoped<IAgentConversationService, AgentConversationService>(); //Mo�e byc Singleton, ale Scoped jest bezpieczniejsze 
builder.Services.AddSingleton<IAgentSessionStoreService, AgentSessionStoreService>();
builder.Services.AddSingleton<IChatRateLimiterService, ChatRateLimiterService>();
builder.Services.AddSingleton<IChunkingService, ChunkingService>();
builder.Services.AddSingleton<OpenAIEmbeddingService>();
builder.Services.AddSingleton<AzureOpenAIEmbeddingService>();
builder.Services.AddSingleton<OllamaEmbeddingService>();
builder.Services.AddSingleton<IEmbeddingServiceFactory, EmbeddingServiceFactory>();
builder.Services.AddSingleton<IEmbeddingService>(sp =>
{
    var factory = sp.GetRequiredService<IEmbeddingServiceFactory>();
    return factory.Create();
});
builder.Services.AddScoped<IRagIngestionService, RagIngestionService>();
builder.Services.AddScoped<IRagSearchService, RagSearchService>();
builder.Services.AddSingleton<ITextExtractorService, TextExtractorService>();
builder.Services.AddSingleton<IFileTextExtractor, PdfTextExtractor>();
builder.Services.AddSingleton<IFileTextExtractor, PlainTextExtractor>();
builder.Services.AddSingleton<IFileTextExtractor, PowerPointTextExtractor>();
builder.Services.AddSingleton<IFileTextExtractor, WordTextExtractor>();
builder.Services.AddScoped<IAgentToolsService, AgentToolsService>();
builder.Services.AddScoped<IFcmService, FcmService>();
builder.Services.AddSingleton<IUserContextAccessor, UserContextAccessor>();
builder.Services.AddScoped<IImageAnalyzeService, ImageAnalyzeService>();
builder.Services.AddScoped<IImageGenerationService, OpenAIImageGenerationService>();
builder.Services.AddScoped<IFileSearchService, FileSearchService>();
builder.Services.AddScoped<ICodeInterpreterService, OpenAICodeInterpreterService>();
builder.Services.AddSingleton<IOpenAIModerationService, OpenAIModerationService>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIntegrations();

builder.Services.AddScoped<CalendarAgentTools>();
builder.Services.AddScoped<CalendarAgent>();

builder.Services.AddScoped<ICalendarAgent>(sp => sp.GetRequiredService<CalendarAgent>());
builder.Services.AddScoped<IAgent>(sp => sp.GetRequiredService<CalendarAgent>());
// CalendarAgentTools NIE jest rejestrowane jako IAgentToolProvider - kalendarz
// trafia do agenta glownego wylacznie przez delegacje AskCalendarAgent (CalendarAgent),
// wiec rejestracja tutaj dublowalaby schematy narzedzi w kazdym zadaniu.
builder.Services.AddScoped<IAgentToolProvider, DriveAgentTools>();
builder.Services.AddScoped<IAgentToolProvider, AdsAgentTools>();
builder.Services.AddScoped<IAgentToolProvider, BusinessProfileAgentTools>();
builder.Services.AddScoped<IAgentToolProvider, EmailAgentTools>();

//var dataProtectionKeysPath =
//    Path.Combine(
//        builder.Environment.ContentRootPath,
//        "App_Data",
//        "DataProtection-Keys");

//Directory.CreateDirectory(dataProtectionKeysPath);

builder.Services
    .AddDataProtection()
    .SetApplicationName("Lumo")
    .PersistKeysToDbContext<
        Lumo.Infrastructure.AppDbContext>();

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});
builder.Services.AddHttpClient();
builder.Services.AddScoped<IVoiceService, VoiceService>();

builder.Services.AddScoped<IChatClient>(sp =>
{
    var configuration =
        sp.GetRequiredService<IConfiguration>();

    var openAiClient =
        sp.GetRequiredService<OpenAIClient>();

    var loggerFactory =
        sp.GetRequiredService<ILoggerFactory>();

    var model = configuration["OpenAI:Model"]
        ?? throw new InvalidOperationException(
            "OpenAI:Model nie jest skonfigurowany.");

    IChatClient client = openAiClient
        .GetChatClient(model)
        .AsIChatClient();

    return client
        .AsBuilder()
        .UseFunctionInvocation(loggerFactory)
        .Build();
});
//builder.Services
//    .AddAuthentication("Bearer")
//    .AddMicrosoftIdentityWebApi(
//        options =>
//        {
//            options.Events ??= new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents();

//            options.Events.OnMessageReceived = context =>
//            {
//                var accessToken = context.Request.Query["access_token"];
//                var path = context.HttpContext.Request.Path;

//                if (!string.IsNullOrEmpty(accessToken) &&
//                    path.StartsWithSegments("/chat"))
//                {
//                    context.Token = accessToken;
//                }

//                if (!string.IsNullOrEmpty(accessToken) &&
//                    path.StartsWithSegments("/voice"))
//                {
//                    context.Token = accessToken;
//                }

//                return Task.CompletedTask;
//            };
//        },
//        options =>
//        {
//            builder.Configuration
//                .GetSection("AzureAd")
//                .Bind(options);
//        }
//    );
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var publicKeyBase64 = builder.Configuration["JWT:JwtPublicKey"]
            ?? throw new InvalidOperationException(
                "JWT Public key not configured");

        var rsa = RSA.Create();

        rsa.ImportRSAPublicKey(
            Convert.FromBase64String(publicKeyBase64),
            out _);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa)
            {
                KeyId = "MyStaticKeyId123"
            },

            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],

            ValidateLifetime = true,

            RequireSignedTokens = true,
            RequireExpirationTime = true,

            ClockSkew = TimeSpan.FromMinutes(1)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Dla SignalR /chat
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }

                // Dla SignalR /voice
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/voice"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT ERROR:");
                Console.WriteLine(context.Exception);

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lumo API v1");
        c.RoutePrefix = string.Empty;
    });
    // app.UseHttpsRedirection();
}

if (!app.Environment.IsDevelopment())
{
    // Bez tego, stojąc za reverse proxy kończącym TLS, aplikacja widzi żądanie jako
    // http i UseHttpsRedirection wpada w pętlę przekierowań.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
    });

    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Musi być PO uwierzytelnieniu - dopiero wtedy znamy tożsamość użytkownika.
app.UseMiddleware<AgentUserContextMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/chat").RequireAuthorization();
app.MapHub<VoiceHub>("/voice").RequireAuthorization();
app.Run();
#pragma warning restore OPENAI001