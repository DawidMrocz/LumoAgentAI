# TikTok - Integracja publikowania treści

Kompletna integracja z TikTok API do publikowania video.

## 🚀 Quick Start

### 1. Uzyskaj credentials TikTok

1. Zarejestruj aplikację na [TikTok for Developers](https://developers.tiktok.com/)
2. Utwórz nową aplikację
3. Pobierz **Client Key** i **Client Secret**
4. Skonfiguruj **Redirect URI**: `http://localhost:32782/api/tiktok/callback`

### 2. Konfiguracja

Dodaj do `appsettings.json`:

```json
{
  "TikTok": {
	"ClientKey": "twoj-client-key",
	"ClientSecret": "twoj-client-secret",
	"RedirectUri": "http://localhost:32782/api/tiktok/callback",
	"Scopes": [
	  "user.info.basic",
	  "video.upload",
	  "video.publish",
	  "video.list"
	]
  }
}
```

### 3. Połącz konto TikTok

```csharp
// Frontend: przekieruj użytkownika do:
GET /api/tiktok/authorize

// Po autoryzacji użytkownik zostanie przekierowany do callback URL
```

### 4. Publikuj pierwsze video

```csharp
var request = new PublishVideoRequest
{
	VideoPath = "C:\\videos\\moje-video.mp4",
	Caption = "Moje pierwsze video z Lumo! 🚀",
	Hashtags = new List<string> { "lumo", "tiktok", "viral" },
	PrivacyLevel = TikTokPrivacyLevel.Public,
	AllowComments = true,
	AllowDuet = true,
	AllowStitch = true
};

var response = await _tikTokService.PublishVideoAsync(request, userId);
Console.WriteLine($"Video opublikowane! ID: {response.VideoId}");
```

## 📝 API Endpoints

### Autoryzacja

#### Rozpocznij autoryzację OAuth
```http
GET /api/tiktok/authorize
```

Przekierowuje użytkownika do TikTok w celu autoryzacji.

#### Callback OAuth (automatyczny)
```http
GET /api/tiktok/callback?code=xxx&state=xxx
```

Wywoływany automatycznie przez TikTok po autoryzacji.

#### Sprawdź połączenie
```http
GET /api/tiktok/is-connected
```

Response:
```json
{
  "connected": true
}
```

#### Rozłącz konto
```http
POST /api/tiktok/disconnect
```

### Publikowanie video

#### Opublikuj single video
```http
POST /api/tiktok/publish
Content-Type: application/json

{
  "videoPath": "C:\\videos\\video.mp4",
  "caption": "Opis video",
  "hashtags": ["viral", "tiktok"],
  "privacyLevel": 0,
  "allowComments": true,
  "allowDuet": true,
  "allowStitch": true,
  "isDraft": false
}
```

Response:
```json
{
  "videoId": "7123456789",
  "videoUrl": "",
  "status": "Processing",
  "success": true,
  "publishedAt": "2025-01-15T10:30:00Z"
}
```

#### Opublikuj wiele video
```http
POST /api/tiktok/publish-bulk
Content-Type: application/json

{
  "videos": [
	{
	  "videoPath": "C:\\videos\\video1.mp4",
	  "caption": "Video 1",
	  "hashtags": ["viral"]
	},
	{
	  "videoPath": "C:\\videos\\video2.mp4",
	  "caption": "Video 2",
	  "hashtags": ["trending"]
	}
  ],
  "delayBetweenPublications": 60
}
```

Response:
```json
{
  "successCount": 2,
  "failureCount": 0,
  "results": [...]
}
```

### Zarządzanie video

#### Lista video użytkownika
```http
GET /api/tiktok/videos?skip=0&take=20
```

Response:
```json
[
  {
	"videoId": "7123456789",
	"caption": "Moje video #viral",
	"videoUrl": "https://www.tiktok.com/@user/video/7123456789",
	"thumbnailUrl": "https://...",
	"viewCount": 15000,
	"likeCount": 1200,
	"commentCount": 45,
	"shareCount": 89,
	"duration": 30,
	"createdAt": "2025-01-15T10:30:00Z"
  }
]
```

#### Szczegóły video
```http
GET /api/tiktok/videos/{videoId}
```

### Statystyki

#### Statystyki użytkownika
```http
GET /api/tiktok/statistics?from=2025-01-01&to=2025-01-31
```

Response:
```json
{
  "totalVideos": 25,
  "totalViews": 150000,
  "totalLikes": 12000,
  "totalComments": 450,
  "totalShares": 890,
  "averageViews": 6000,
  "averageLikes": 480,
  "engagementRate": 8.93
}
```

#### Informacje o użytkowniku TikTok
```http
GET /api/tiktok/user-info
```

Response:
```json
{
  "tikTokUserId": "123456789",
  "username": "user123",
  "displayName": "Jan Kowalski",
  "avatarUrl": "https://...",
  "bio": "Kreatywny content creator",
  "followerCount": 5000,
  "followingCount": 234,
  "likesCount": 12000,
  "videoCount": 25
}
```

## 🎬 Funkcje

### Privacy Levels

```csharp
public enum TikTokPrivacyLevel
{
	Public = 0,              // Publiczne dla wszystkich
	FriendsOnly = 1,         // Tylko znajomi
	Private = 2,             // Prywatne
	MutualFollowFriends = 3  // Wzajemni obserwujący
}
```

### Drafts

Zapisz video jako draft (nie publikuj od razu):

```csharp
var request = new PublishVideoRequest
{
	VideoPath = "video.mp4",
	Caption = "Draft video",
	IsDraft = true  // Zapisz jako draft
};
```

### Scheduled Publishing (jeśli wspierane)

```csharp
var request = new PublishVideoRequest
{
	VideoPath = "video.mp4",
	Caption = "Zaplanowane video",
	ScheduledAt = DateTimeOffset.UtcNow.AddHours(24)  // Opublikuj za 24h
};
```

### Hashtagi

```csharp
var request = new PublishVideoRequest
{
	Caption = "Moje video",
	Hashtags = new List<string> 
	{ 
		"viral",      // Automatycznie doda #
		"#tiktok",    // Lub z #
		"trending" 
	}
};

// Wynik: "Moje video #viral #tiktok #trending"
```

### Ustawienia interakcji

```csharp
var request = new PublishVideoRequest
{
	AllowComments = true,   // Zezwól na komentarze
	AllowDuet = true,       // Zezwól na Duet
	AllowStitch = true      // Zezwól na Stitch
};
```

## 💡 Przykłady użycia

### Przykład 1: Proste publikowanie

```csharp
var response = await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "C:\\videos\\funny-cat.mp4",
	Caption = "Funny cat video! 😹",
	Hashtags = new List<string> { "cats", "funny", "viral" },
	PrivacyLevel = TikTokPrivacyLevel.Public
}, userId);

if (response.Success)
{
	Console.WriteLine($"✅ Video opublikowane! ID: {response.VideoId}");
}
```

### Przykład 2: Publikowanie z draftem

```csharp
// Najpierw jako draft
var draftResponse = await _tikTokService.PublishVideoAsync(new PublishVideoRequest
{
	VideoPath = "video.mp4",
	Caption = "Preview video",
	IsDraft = true
}, userId);

// Później opublikuj
// (wymaga dodatkowej funkcji do publikowania draftu)
```

### Przykład 3: Masowa publikacja

```csharp
var videos = Directory.GetFiles("C:\\videos", "*.mp4")
	.Select((path, index) => new PublishVideoRequest
	{
		VideoPath = path,
		Caption = $"Video #{index + 1}",
		Hashtags = new List<string> { "series", "viral" }
	})
	.ToList();

var result = await _tikTokService.PublishBulkVideosAsync(new PublishBulkVideosRequest
{
	Videos = videos,
	DelayBetweenPublications = 120  // 2 minuty między publikacjami
}, userId);

Console.WriteLine($"Opublikowane: {result.SuccessCount}/{videos.Count}");
```

### Przykład 4: Monitorowanie statystyk

```csharp
// Pobierz statystyki z ostatnich 30 dni
var from = DateTimeOffset.UtcNow.AddDays(-30);
var stats = await _tikTokService.GetStatisticsAsync(userId, from);

Console.WriteLine($"📊 Statystyki z ostatnich 30 dni:");
Console.WriteLine($"   Video: {stats.TotalVideos}");
Console.WriteLine($"   Wyświetlenia: {stats.TotalViews:N0}");
Console.WriteLine($"   Polubienia: {stats.TotalLikes:N0}");
Console.WriteLine($"   Engagement: {stats.EngagementRate:F2}%");
```

### Przykład 5: React Hook

```typescript
function usePublishToTikTok() {
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const publishVideo = async (videoFile: File, caption: string, hashtags: string[]) => {
		setLoading(true);
		setError(null);

		try {
			// 1. Upload pliku do serwera
			const formData = new FormData();
			formData.append('video', videoFile);

			const uploadResponse = await fetch('/api/upload/video', {
				method: 'POST',
				body: formData
			});

			const { filePath } = await uploadResponse.json();

			// 2. Opublikuj na TikTok
			const response = await fetch('/api/tiktok/publish', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					videoPath: filePath,
					caption,
					hashtags,
					privacyLevel: 0,
					allowComments: true,
					allowDuet: true,
					allowStitch: true
				})
			});

			const result = await response.json();

			if (!result.success) {
				throw new Error(result.error);
			}

			return result;
		} catch (err) {
			setError(err.message);
			throw err;
		} finally {
			setLoading(false);
		}
	};

	return { publishVideo, loading, error };
}
```

## 🗄️ Baza danych

### TikTokTokens

Przechowuje tokeny OAuth użytkowników:

```csharp
public class TikTokToken
{
	public int Id { get; set; }
	public string UserId { get; set; }
	public string OpenId { get; set; }
	public string AccessToken { get; set; }
	public string RefreshToken { get; set; }
	public DateTimeOffset AccessTokenExpiresAt { get; set; }
	public DateTimeOffset RefreshTokenExpiresAt { get; set; }
}
```

### TikTokVideos

Historia opublikowanych video:

```csharp
public class TikTokVideo
{
	public int Id { get; set; }
	public string UserId { get; set; }
	public string? TikTokVideoId { get; set; }
	public string Caption { get; set; }
	public TikTokVideoStatus Status { get; set; }
	public long ViewCount { get; set; }
	public long LikeCount { get; set; }
	public long CommentCount { get; set; }
	public long ShareCount { get; set; }
}
```

## ⚙️ Konfiguracja Advanced

```json
{
  "TikTok": {
	"ClientKey": "xxx",
	"ClientSecret": "xxx",
	"RedirectUri": "http://localhost:32782/api/tiktok/callback",
	"ApiBaseUrl": "https://open.tiktokapis.com",
	"AuthBaseUrl": "https://www.tiktok.com",
	"Scopes": [
	  "user.info.basic",
	  "video.upload",
	  "video.publish",
	  "video.list"
	],
	"MaxVideoSize": 287109120,
	"SupportedFormats": ["mp4", "mov", "avi", "webm"],
	"MaxRetries": 3,
	"TimeoutSeconds": 300,
	"Features": {
	  "EnableScheduledPublishing": true,
	  "EnableDrafts": true,
	  "EnablePrivacySettings": true,
	  "EnableComments": true,
	  "EnableDuet": true,
	  "EnableStitch": true
	}
  }
}
```

## 📊 Wymagania video

| Parametr | Wartość |
|----------|---------|
| Maksymalny rozmiar | 287 MB (274 MB) |
| Formaty | MP4, MOV, AVI, WebM |
| Maksymalny czas | 10 minut |
| Rozdzielczość | Minimum 720p |
| Aspect ratio | 9:16 (portret), 16:9 (landscape), 1:1 (kwadrat) |

## 🔐 Bezpieczeństwo

- Tokeny są automatycznie odświeżane przed wygaśnięciem
- Wszystkie endpointy wymagają autoryzacji
- Credentials przechowywane bezpiecznie w bazie
- OAuth 2.0 flow

## ⚠️ Limity TikTok API

- **Rate limits**: Sprawdź aktualne limity w [dokumentacji TikTok](https://developers.tiktok.com/doc/research-api-rate-limits)
- **Upload limit**: ~50 video/dzień dla zwykłych kont
- **Video processing**: 5-15 minut

## 🐛 Troubleshooting

### "Brak połączenia z TikTok"

Użytkownik musi autoryzować aplikację przez `/api/tiktok/authorize`

### "Plik jest za duży"

Zmniejsz rozmiar pliku lub skompresuj video. Max: 287 MB.

### "Token wygasł"

Token jest automatycznie odświeżany. Jeśli problem persist, poproś użytkownika o ponowną autoryzację.

### "Video nie pojawia się na TikTok"

Video może być w trakcie przetwarzania. Sprawdź status przez `/api/tiktok/videos/{videoId}`.

## 📚 Linki

- [TikTok for Developers](https://developers.tiktok.com/)
- [TikTok API Documentation](https://developers.tiktok.com/doc/web-video-kit-api-reference)
- [OAuth 2.0 Guide](https://developers.tiktok.com/doc/oauth-user-access-token-management)

## 🆘 Wsparcie

W razie problemów:
1. Sprawdź logi aplikacji
2. Sprawdź [TikTok API status](https://developers.tiktok.com/)
3. Upewnij się, że tokeny nie wygasły

---

**Gotowe!** Integracja TikTok jest w pełni funkcjonalna. 🎉
