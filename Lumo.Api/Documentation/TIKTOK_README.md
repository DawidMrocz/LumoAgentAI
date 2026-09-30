# 🎵 TikTok Integration - Publikuj video z Lumo

Kompletna integracja z TikTok do automatycznego publikowania video.

## ⚡ Quick Start

### 1. Setup

```bash
# 1. Zarejestruj app na https://developers.tiktok.com/
# 2. Pobierz Client Key i Client Secret
# 3. Dodaj do appsettings.json
```

```json
{
  "TikTok": {
	"ClientKey": "twoj-client-key",
	"ClientSecret": "twoj-client-secret"
  }
}
```

### 2. Połącz konto

```csharp
// Przekieruj użytkownika do:
GET /api/tiktok/authorize

// Po autoryzacji konto będzie połączone 
```

### 3. Publikuj!

```csharp
await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "C:\\videos\\moje-video.mp4",
	Caption = "Moje pierwsze video! 🚀",
	Hashtags = new List<string> { "viral", "tiktok" }
}, userId);
```

## ✨ Funkcje

- ✅ Autoryzacja OAuth 2.0
- ✅ Upload i publikacja video
- ✅ Masowa publikacja (bulk)
- ✅ Drafts (robocze wersje)
- ✅ Zaplanowana publikacja
- ✅ Ustawienia prywatności (Public/Friends/Private)
- ✅ Kontrola komentarzy/Duet/Stitch
- ✅ Hashtagi automatyczne
- ✅ Statystyki (views, likes, comments, shares)
- ✅ Historia video
- ✅ Informacje o profilu TikTok
- ✅ Automatyczne odświeżanie tokenów

## 📋 Endpoints

```http
GET  /api/tiktok/authorize              # Autoryzacja OAuth
GET  /api/tiktok/is-connected           # Sprawdź połączenie
POST /api/tiktok/disconnect             # Rozłącz konto

POST /api/tiktok/publish                # Opublikuj video
POST /api/tiktok/publish-bulk           # Opublikuj wiele

GET  /api/tiktok/videos                 # Lista video
GET  /api/tiktok/videos/{id}            # Szczegóły video
GET  /api/tiktok/statistics             # Statystyki
GET  /api/tiktok/user-info              # Info o profilu
POST /api/tiktok/refresh-statistics     # Odśwież statystyki
```

## 🎬 Przykłady

### Pojedyncze video

```csharp
var response = await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "funny-cat.mp4",
	Caption = "Funny cat! 😹",
	Hashtags = new List<string> { "cats", "funny", "viral" },
	PrivacyLevel = TikTokPrivacyLevel.Public,
	AllowComments = true,
	AllowDuet = true,
	AllowStitch = true
}, userId);

Console.WriteLine($"Video ID: {response.VideoId}");
```

### Masowa publikacja

```csharp
var videos = new List<PublishVideoRequest>
{
	new() { VideoPath = "video1.mp4", Caption = "Part 1" },
	new() { VideoPath = "video2.mp4", Caption = "Part 2" },
	new() { VideoPath = "video3.mp4", Caption = "Part 3" }
};

var result = await _tikTokService.PublishBulkVideosAsync(new PublishBulkVideosRequest
{
	Videos = videos,
	DelayBetweenPublications = 60  // 1 minuta delay
}, userId);

Console.WriteLine($"Sukces: {result.SuccessCount}/{videos.Count}");
```

### Draft (robocza wersja)

```csharp
var response = await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "video.mp4",
	Caption = "Draft video",
	IsDraft = true  // Nie publikuj od razu
}, userId);
```

### Zaplanowana publikacja

```csharp
var response = await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "video.mp4",
	Caption = "Zaplanowane video",
	ScheduledAt = DateTimeOffset.UtcNow.AddHours(24)  // Za 24h
}, userId);
```

### Statystyki

```csharp
var stats = await _tikTokService.GetStatisticsAsync(userId);

Console.WriteLine($"📊 Statystyki:");
Console.WriteLine($"   Video: {stats.TotalVideos}");
Console.WriteLine($"   Wyświetlenia: {stats.TotalViews:N0}");
Console.WriteLine($"   Polubienia: {stats.TotalLikes:N0}");
Console.WriteLine($"   Engagement: {stats.EngagementRate:F2}%");
```

## 🎯 Privacy Levels

```csharp
TikTokPrivacyLevel.Public              // Publiczne (wszyscy)
TikTokPrivacyLevel.FriendsOnly         // Tylko znajomi
TikTokPrivacyLevel.Private             // Prywatne (tylko ty)
TikTokPrivacyLevel.MutualFollowFriends // Wzajemni obserwujący
```

## 🔧 Ustawienia interakcji

```csharp
var request = new PublishVideoRequest
{
	AllowComments = true,   // Komentarze
	AllowDuet = true,       // Duet
	AllowStitch = true      // Stitch
};
```

## 📊 Wymagania video

| Parameter | Value |
|-----------|-------|
| Max size | 287 MB |
| Formats | MP4, MOV, AVI, WebM |
| Max duration | 10 minut |
| Resolution | Min. 720p |
| Aspect ratio | 9:16, 16:9, 1:1 |

## 🗄️ Database

```sql
-- Tokeny OAuth
TikTokTokens (Id, UserId, OpenId, AccessToken, RefreshToken, ...)

-- Historia video
TikTokVideos (Id, UserId, TikTokVideoId, Caption, Status, ViewCount, LikeCount, ...)
```

## 🔐 Security

- OAuth 2.0 flow
- Automatic token refresh
- Secure credential storage
- Authorization required for all endpoints

## ⚠️ Limity

- **Upload**: ~50 video/day (dla zwykłych kont)
- **Processing**: 5-15 minut
- **Size**: Max 287 MB
- **Rate limits**: Zobacz [TikTok API docs](https://developers.tiktok.com/)

## 🐛 Troubleshooting

**"Brak połączenia z TikTok"**
→ Autoryzuj przez `/api/tiktok/authorize`

**"Plik jest za duży"**
→ Zmniejsz plik (max 287 MB)

**"Video nie pojawia się"**
→ Czekaj 5-15 min (przetwarzanie)

## 📚 Documentation

- [Full Integration Guide](./TIKTOK_INTEGRATION.md)
- [TikTok for Developers](https://developers.tiktok.com/)
- [API Reference](https://developers.tiktok.com/doc/web-video-kit-api-reference)

## 🚀 Next Steps

1. Zarejestruj app na TikTok Developers
2. Ustaw credentials w `appsettings.json`
3. Autoryzuj konto użytkownika
4. Publikuj video!

---

**Gotowe do użycia!** 🎉 Publikuj video na TikTok z Lumo.
