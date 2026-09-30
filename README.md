# Lumo

Backend platformy z asystentem AI (agent z narzędziami), napisany w .NET 10, z panelem webowym w React. Agent potrafi korzystać z zewnętrznych usług (Google, Meta, TikTok, e-mail, SMS, płatności), przeszukiwać dokumenty użytkownika (RAG) i generować obrazy oraz wideo.

## Funkcje

- **Agent AI** – czat w czasie rzeczywistym (SignalR), narzędzia z zatwierdzaniem przez użytkownika, zadania agenta harmonogramowane cronem, umiejętności (Agent Skills)
- **RAG** – wgrywanie PDF/Word/PowerPoint/tekstu, chunking, embeddingi (OpenAI, Azure OpenAI, Ollama), wektorowa baza Qdrant
- **Integracje** – Google (Calendar, Drive, Ads, Business Profile, Veo), Meta (Facebook/Instagram), TikTok, e-mail (SMTP), SMS (Twilio), płatności Przelewy24
- **Konta** – rejestracja z aktywacją, JWT + refresh tokeny, logowanie Google/Facebook, reset hasła
- **Powiadomienia push** – Firebase Cloud Messaging
- **Multimodalność** – analiza obrazów, generowanie obrazów, głos, code interpreter, moderacja

## Struktura

| Katalog | Opis |
|---|---|
| `Lumo.Api` | Web API (ASP.NET Core), huby SignalR, agent, RAG, migracje |
| `Lumo.Api/www` | Frontend (React 19, Vite, TypeScript) |
| `Lumo.Infrastructure` | EF Core: encje, repozytoria, migracje |
| `Integrations` | Klienci i serwisy zewnętrznych usług |

## Wymagania

- .NET SDK 10
- SQL Server
- Qdrant (np. przez Docker)
- Node.js (frontend)
- Klucz OpenAI (lub Azure OpenAI / Ollama dla embeddingów)

## Uruchomienie lokalne

1. Skopiuj szablony konfiguracji i uzupełnij własnymi wartościami (pliki są ignorowane przez git):
   ```bash
   cp Lumo.Api/appsettings.example.json Lumo.Api/appsettings.json
   cp Lumo.Api/firebase-service-account.example.json Lumo.Api/firebase-service-account.json
   cp Lumo.Api/www/.env.example Lumo.Api/www/.env
   ```
   Najważniejsze klucze konfiguracji: `ConnectionStrings:DefaultConnection`, `OpenAI:ApiKey`, `JWT:*`, `Qdrant:BaseUrl`, `Cors:AllowedOrigins`, `FrontendUrl`. Sekrety najlepiej trzymać w `dotnet user-secrets` lub zmiennych środowiskowych.
2. Uruchom Qdrant:
   ```bash
   docker compose up -d qdrant
   ```
3. Zastosuj migracje i uruchom API:
   ```bash
   dotnet ef database update --project Lumo.Api
   dotnet run --project Lumo.Api
   ```
4. Frontend:
   ```bash
   cd Lumo.Api/www && npm install && npm run dev
   ```

## Docker

`docker-compose.prod.yml` uruchamia API, Qdrant i Caddy (reverse proxy; własny `Caddyfile` nie jest w repo). Obraz budowany jest z `Lumo.Api/Dockerfile`, a CI/CD w `.github/workflows/cicd.yml` używa sekretów `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`, `VPS_HOST`, `VPS_USERNAME`, `VPS_PASSWORD`.

## Bezpieczeństwo

Repozytorium nie zawiera żadnych kluczy ani haseł. Nigdy nie commituj `appsettings*.json`, `firebase-service-account.json` ani `.env`.

## Dokumentacja dodatkowa

- [Przelewy24](PRZELEWY24_INTEGRATION.md)
- [Google Veo](Lumo.Api/Docs/GoogleVeo/README.md)
- [TikTok](Lumo.Api/Documentation/TIKTOK_README.md)
