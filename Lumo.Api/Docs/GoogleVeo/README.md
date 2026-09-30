# Google Veo 3.1 - Generowanie Video AI

Kompleksowa integracja z Google Veo 3.1 do generowania video z tekstu, obrazów i przekształcania stylu.

## 📋 Spis treści

- [Konfiguracja](#konfiguracja)
- [Podstawowe użycie](#podstawowe-użycie)
- [Presety](#presety)
- [Zaawansowane funkcje](#zaawansowane-funkcje)
- [Przykłady .NET](#przykłady-net)
- [Przykłady TypeScript](#przykłady-typescript)
- [Monitorowanie](#monitorowanie)
- [FAQ](#faq)

## 🔧 Konfiguracja

### appsettings.json

```json
{
  "GoogleVeo": {
	"ApiKey": "YOUR_GOOGLE_API_KEY",
	"ProjectId": "your-project-id",
	"Location": "us-central1",
	"ApiBaseUrl": "https://aiplatform.googleapis.com/v1",
	"Model": "veo-3.1",
	"MaxPromptLength": 2000,
	"DefaultDuration": 5,
	"DefaultResolution": "1080p",
	"DefaultAspectRatio": "16:9",
	"MaxRetries": 3,
	"TimeoutSeconds": 300,
	"PollingIntervalSeconds": 10,
	"MaxPollingAttempts": 60,
	"Features": {
	  "EnableTextToVideo": true,
	  "EnableImageToVideo": true,
	  "EnableVideoExtension": true,
	  "EnableStyleTransfer": true,
	  "EnableCameraControls": true,
	  "EnableWatermarkRemoval": false
	},
	"Presets": {
	  "Advertisement": {
		"Duration": 15,
		"AspectRatio": "16:9",
		"Resolution": "1080p",
		"Style": "commercial"
	  },
	  "SocialMedia": {
		"Duration": 10,
		"AspectRatio": "9:16",
		"Resolution": "1080p",
		"Style": "dynamic"
	  },
	  "Cinematic": {
		"Duration": 30,
		"AspectRatio": "21:9",
		"Resolution": "4k",
		"Style": "cinematic"
	  }
	}
  }
}
```

### Wymagane uprawnienia Google Cloud

1. Włącz Vertex AI API w Google Cloud Console
2. Utwórz klucz API lub użyj service account
3. Ustaw uprawnienia: `aiplatform.endpoints.predict`

## 🚀 Podstawowe użycie

### 1. Proste generowanie video

**Endpoint:** `POST /api/googleveo/generate`

**Request:**
```json
{
  "prompt": "Profesjonalna reklama nowoczesnego smartfona, eleganckie ujęcia produktu, minimalistyczne tło",
  "duration": 15,
  "resolution": "1080p",
  "aspectRatio": "16:9",
  "fps": 30
}
```

**Response:**
```json
{
  "jobId": "projects/123/locations/us-central1/operations/456",
  "status": "Queued",
  "success": true,
  "createdAt": "2024-01-15T10:00:00Z"
}
```

### 2. Sprawdzanie statusu

**Endpoint:** `GET /api/googleveo/status/{jobId}`

**Response:**
```json
{
  "jobId": "projects/123/locations/us-central1/operations/456",
  "status": "Completed",
  "videoUrl": "https://storage.googleapis.com/...",
  "videoPath": "wwwroot/generated-videos/1_abc.mp4",
  "thumbnailUrl": "https://storage.googleapis.com/.../thumb.jpg",
  "success": true,
  "generationTime": "00:02:30",
  "createdAt": "2024-01-15T10:00:00Z",
  "completedAt": "2024-01-15T10:02:30Z",
  "fileSize": 15728640
}
```

## 🎬 Presety

### Advertisement (Reklama)

```http
POST /api/googleveo/generate/preset/advertisement
Content-Type: application/json

"Elegancki zegarek szwajcarski na czarnym tle, powolny obrót, profesjonalne oświetlenie"
```

**Charakterystyka:**
- Czas: 15 sekund
- Format: 16:9 (landscape)
- Rozdzielczość: 1080p
- Styl: commercial
- Idealne do: YouTube, Facebook Ads, TV

### Social Media (Social media)

```http
POST /api/googleveo/generate/preset/socialmedia
Content-Type: application/json

"Dynamiczny unboxing nowego produktu, energetyczne ruchy, jasne kolory"
```

**Charakterystyka:**
- Czas: 10 sekund
- Format: 9:16 (vertical)
- Rozdzielczość: 1080p
- Styl: dynamic
- Idealne do: TikTok, Instagram Reels, YouTube Shorts

### Cinematic (Kinowe)

```http
POST /api/googleveo/generate/preset/cinematic
Content-Type: application/json

"Wschód słońca nad miastem, powolne ujęcia z lotu ptaka, spokojne przejścia"
```

**Charakterystyka:**
- Czas: 30 sekund
- Format: 21:9 (ultrawide)
- Rozdzielczość: 4K
- Styl: cinematic
- Idealne do: prezentacji, filmów korporacyjnych

### Pobieranie dostępnych presetów

```http
GET /api/googleveo/presets
```

**Response:**
```json
[
  {
	"name": "Advertisement",
	"description": "15s video w formacie 16:9, idealny do reklam",
	"duration": 15,
	"aspectRatio": "16:9",
	"resolution": "1080p",
	"style": "commercial",
	"fps": 30
  }
]
```

## 🎨 Zaawansowane funkcje

### 1. Generowanie z negatywnym promptem

```json
{
  "prompt": "Profesjonalna reklama samochodu SUV w górach",
  "negativePrompt": "ludzie, znaki firmowe, napisy, rozmyte, niska jakość",
  "duration": 20,
  "resolution": "1080p",
  "aspectRatio": "16:9"
}
```

### 2. Kontrola kamery

```json
{
  "prompt": "Elegancka prezentacja produktu kosmetycznego",
  "duration": 10,
  "camera": {
	"movement": "orbit",
	"angle": "medium",
	"speed": "slow"
  }
}
```

**Dostępne ruchy kamery:**
- `pan` - pionowe przesunięcie
- `tilt` - poziome przesunięcie
- `zoom` - zbliżenie/oddalenie
- `static` - statyczna kamera
- `orbit` - okrężny ruch wokół obiektu
- `dolly` - ruch do przodu/tyłu

**Kąty kamery:**
- `wide` - szeroki plan
- `medium` - średni plan
- `close-up` - zbliżenie
- `extreme-close-up` - bardzo bliskie zbliżenie

**Prędkości:**
- `slow` - wolny ruch
- `medium` - średni ruch
- `fast` - szybki ruch

### 3. Generowanie z obrazem referencyjnym

```json
{
  "prompt": "Animuj ten obraz - dodaj ruch chmur i wody",
  "referenceImagePath": "/path/to/image.jpg",
  "duration": 10,
  "resolution": "1080p"
}
```

### 4. Seed dla powtarzalności

```json
{
  "prompt": "Abstrakcyjna animacja kolorów",
  "seed": 42,
  "duration": 15
}
```

**Uwaga:** Ten sam prompt + seed = identyczne video przy kolejnych generowaniach.

## 💻 Przykłady .NET

### Przykład 1: Generowanie reklamy produktu

```csharp
using Lumo.Api.Services.GoogleVeo;
using Lumo.Api.DTO.GoogleVeo;

public class VideoGenerationService
{
	private readonly IGoogleVeoService _veoService;

	public async Task<string> GenerateProductAdAsync(string productName)
	{
		var request = new GenerateVideoRequest
		{
			Prompt = $"Profesjonalna reklama produktu {productName}, " +
					 "eleganckie ujęcia, nowoczesne tło, dynamiczne przejścia",
			NegativePrompt = "ludzie, tekst, napisy, znaki firmowe",
			Duration = 15,
			Resolution = "1080p",
			AspectRatio = "16:9",
			Style = "commercial",
			Fps = 30,
			Camera = new CameraSettings
			{
				Movement = "orbit",
				Angle = "medium",
				Speed = "slow"
			}
		};

		var response = await _veoService.GenerateVideoAsync(
			request, 
			userId: "user123"
		);

		if (response.Success)
		{
			Console.WriteLine($"Video w generowaniu: {response.JobId}");
			return response.JobId;
		}

		throw new Exception($"Błąd generowania: {response.Error}");
	}
}
```

### Przykład 2: Monitorowanie postępu

```csharp
public async Task<string?> WaitForVideoCompletionAsync(string jobId)
{
	while (true)
	{
		var status = await _veoService.GetGenerationStatusAsync(jobId);

		Console.WriteLine($"Status: {status.Status}");

		if (status.Status == "Completed")
		{
			Console.WriteLine($"Video gotowe! URL: {status.VideoUrl}");
			Console.WriteLine($"Rozmiar: {status.FileSize / 1024 / 1024}MB");
			Console.WriteLine($"Czas generowania: {status.GenerationTime}");
			return status.VideoUrl;
		}

		if (status.Status == "Failed")
		{
			Console.WriteLine($"Błąd: {status.Error}");
			return null;
		}

		await Task.Delay(TimeSpan.FromSeconds(5));
	}
}
```

### Przykład 3: Generowanie z presetem

```csharp
public async Task GenerateSocialMediaVideoAsync(string content)
{
	// Automatycznie użyje ustawień dla social media (9:16, 10s)
	var response = await _veoService.GenerateVideoFromPresetAsync(
		presetName: "socialmedia",
		prompt: $"Dynamiczna prezentacja: {content}, energetyczne kolory, szybkie tempo",
		userId: "user123"
	);

	if (response.Success)
	{
		Console.WriteLine($"Video dla social media w generowaniu: {response.JobId}");
	}
}
```

### Przykład 4: Pobieranie historii video

```csharp
public async Task ShowUserVideosAsync(string userId)
{
	var videos = await _veoService.GetUserVideosAsync(
		userId: userId,
		skip: 0,
		take: 10
	);

	foreach (var video in videos)
	{
		Console.WriteLine($"ID: {video.Id}");
		Console.WriteLine($"Prompt: {video.Prompt}");
		Console.WriteLine($"Status: {video.Status}");
		Console.WriteLine($"URL: {video.VideoUrl}");
		Console.WriteLine($"Utworzone: {video.CreatedAt}");
		Console.WriteLine("---");
	}
}
```

### Przykład 5: Statystyki użytkownika

```csharp
public async Task ShowStatisticsAsync(string userId)
{
	var stats = await _veoService.GetStatisticsAsync(
		userId: userId,
		from: DateTimeOffset.UtcNow.AddDays(-30)
	);

	Console.WriteLine($"Wygenerowane video: {stats.CompletedVideos}/{stats.TotalVideos}");
	Console.WriteLine($"W trakcie: {stats.ProcessingVideos}");
	Console.WriteLine($"Nieudane: {stats.FailedVideos}");
	Console.WriteLine($"Całkowity czas video: {stats.TotalDuration}s");
	Console.WriteLine($"Średni czas generowania: {stats.AverageGenerationTime}");
	Console.WriteLine($"Całkowity rozmiar: {stats.TotalFileSize / 1024 / 1024}MB");
}
```

## 🌐 Przykłady TypeScript

### Przykład 1: Generowanie video

```typescript
interface VeoGenerateRequest {
  prompt: string;
  negativePrompt?: string;
  duration?: number;
  resolution?: string;
  aspectRatio?: string;
  fps?: number;
  style?: string;
  seed?: number;
  camera?: {
	movement?: string;
	angle?: string;
	speed?: string;
  };
}

interface VeoResponse {
  jobId: string;
  status: string;
  videoUrl?: string;
  success: boolean;
  error?: string;
}

async function generateVideo(
  token: string,
  request: VeoGenerateRequest
): Promise<VeoResponse> {
  const response = await fetch('/api/googleveo/generate', {
	method: 'POST',
	headers: {
	  'Content-Type': 'application/json',
	  'Authorization': `Bearer ${token}`
	},
	body: JSON.stringify(request)
  });

  return await response.json();
}

// Użycie
const result = await generateVideo(authToken, {
  prompt: 'Profesjonalna reklama smartfona, eleganckie ujęcia',
  duration: 15,
  resolution: '1080p',
  aspectRatio: '16:9',
  style: 'commercial'
});

console.log('Job ID:', result.jobId);
```

### Przykład 2: Polling statusu z progress bar

```typescript
async function pollVideoStatus(
  token: string,
  jobId: string,
  onProgress?: (status: string) => void
): Promise<string | null> {
  while (true) {
	const response = await fetch(`/api/googleveo/status/${jobId}`, {
	  headers: { 'Authorization': `Bearer ${token}` }
	});

	const status: VeoResponse = await response.json();

	if (onProgress) {
	  onProgress(status.status);
	}

	if (status.status === 'Completed') {
	  return status.videoUrl || null;
	}

	if (status.status === 'Failed') {
	  console.error('Generation failed:', status.error);
	  return null;
	}

	await new Promise(resolve => setTimeout(resolve, 5000));
  }
}

// React example
function VideoGenerator() {
  const [status, setStatus] = useState('');
  const [videoUrl, setVideoUrl] = useState<string | null>(null);

  const handleGenerate = async () => {
	const result = await generateVideo(token, {
	  prompt: 'Nowoczesna reklama',
	  preset: 'advertisement'
	});

	if (result.success) {
	  const url = await pollVideoStatus(
		token,
		result.jobId,
		(newStatus) => setStatus(newStatus)
	  );
	  setVideoUrl(url);
	}
  };

  return (
	<div>
	  <button onClick={handleGenerate}>Generuj Video</button>
	  <p>Status: {status}</p>
	  {videoUrl && <video src={videoUrl} controls />}
	</div>
  );
}
```

### Przykład 3: Użycie presetów

```typescript
async function generateWithPreset(
  token: string,
  presetName: 'advertisement' | 'socialmedia' | 'cinematic',
  prompt: string
): Promise<VeoResponse> {
  const response = await fetch(
	`/api/googleveo/generate/preset/${presetName}`,
	{
	  method: 'POST',
	  headers: {
		'Content-Type': 'application/json',
		'Authorization': `Bearer ${token}`
	  },
	  body: JSON.stringify(prompt)
	}
  );

  return await response.json();
}

// Reklama
await generateWithPreset(
  token,
  'advertisement',
  'Elegancki zegarek na czarnym tle'
);

// Social media
await generateWithPreset(
  token,
  'socialmedia',
  'Dynamiczny unboxing nowego telefonu'
);

// Kinowe
await generateWithPreset(
  token,
  'cinematic',
  'Wschód słońca nad miastem'
);
```

### Przykład 4: Lista video użytkownika

```typescript
interface VeoVideoInfo {
  id: number;
  jobId: string;
  prompt: string;
  status: string;
  videoUrl?: string;
  duration: number;
  resolution: string;
  aspectRatio: string;
  fileSize: number;
  createdAt: string;
  completedAt?: string;
}

async function getUserVideos(
  token: string,
  page: number = 0,
  pageSize: number = 20
): Promise<VeoVideoInfo[]> {
  const response = await fetch(
	`/api/googleveo/videos?skip=${page * pageSize}&take=${pageSize}`,
	{
	  headers: { 'Authorization': `Bearer ${token}` }
	}
  );

  return await response.json();
}

// React infinite scroll
function VideoGallery() {
  const [videos, setVideos] = useState<VeoVideoInfo[]>([]);
  const [page, setPage] = useState(0);

  useEffect(() => {
	getUserVideos(token, page).then(newVideos => {
	  setVideos(prev => [...prev, ...newVideos]);
	});
  }, [page]);

  return (
	<div>
	  {videos.map(video => (
		<div key={video.id}>
		  <h3>{video.prompt}</h3>
		  <p>Status: {video.status}</p>
		  {video.videoUrl && (
			<video src={video.videoUrl} controls width="400" />
		  )}
		</div>
	  ))}
	  <button onClick={() => setPage(p => p + 1)}>
		Załaduj więcej
	  </button>
	</div>
  );
}
```

### Przykład 5: Statystyki

```typescript
interface VeoStatistics {
  totalVideos: number;
  completedVideos: number;
  processingVideos: number;
  failedVideos: number;
  totalDuration: number;
  averageGenerationTime: string;
  totalFileSize: number;
}

async function getStatistics(token: string): Promise<VeoStatistics> {
  const response = await fetch('/api/googleveo/statistics', {
	headers: { 'Authorization': `Bearer ${token}` }
  });

  return await response.json();
}

// Dashboard component
function StatsDashboard() {
  const [stats, setStats] = useState<VeoStatistics | null>(null);

  useEffect(() => {
	getStatistics(token).then(setStats);
  }, []);

  if (!stats) return <div>Ładowanie...</div>;

  return (
	<div>
	  <h2>Statystyki generowania video</h2>
	  <div>
		<p>Ukończone: {stats.completedVideos} / {stats.totalVideos}</p>
		<p>W trakcie: {stats.processingVideos}</p>
		<p>Nieudane: {stats.failedVideos}</p>
		<p>Całkowity czas: {stats.totalDuration}s</p>
		<p>Średni czas generowania: {stats.averageGenerationTime}</p>
		<p>Całkowity rozmiar: {(stats.totalFileSize / 1024 / 1024).toFixed(2)}MB</p>
	  </div>
	</div>
  );
}
```

## 📊 Monitorowanie

### Webhook dla powiadomień (TODO)

```csharp
[HttpPost("webhooks/veo")]
public async Task<IActionResult> VeoWebhook([FromBody] VeoWebhookPayload payload)
{
	// Obsługa powiadomień z Google o zakończeniu generowania
	await _notificationService.NotifyUserAsync(payload.UserId, payload.VideoUrl);
	return Ok();
}
```

## ❓ FAQ

### Jak długo trwa generowanie video?

Typowo:
- 5s video: ~1-2 minuty
- 15s video: ~2-3 minuty
- 30s video: ~5-10 minut

### Jakie są limity?

- Maksymalna długość promptu: 2000 znaków
- Maksymalny czas video: 60 sekund
- Maksymalna rozdzielczość: 4K
- Polling timeout: 10 minut (60 prób co 10s)

### Jak działa billing?

Google Veo jest płatny według czasu wygenerowanego video:
- Sprawdź aktualne ceny w Google Cloud Console
- Każde wywołanie API jest rozliczane osobno

### Czy mogę anulować generowanie?

Tak:
```http
POST /api/googleveo/cancel/{jobId}
```

### Gdzie są zapisywane wygenerowane video?

- Lokalnie: `wwwroot/generated-videos/`
- W Google Cloud Storage (jeśli skonfigurowane)
- URL zapisany w bazie danych

### Jak optymalnie napisać prompt?

**Dobre praktyki:**
1. Bądź konkretny i szczegółowy
2. Opisz styl wizualny (kolory, oświetlenie)
3. Określ ruch kamery
4. Unikaj zbyt długich promptów
5. Użyj negatywnego promptu dla lepszej kontroli

**Przykład dobrego promptu:**
```
Profesjonalna reklama luksusowego zegarka, 
makro ujęcia mechanizmu, 
powolny obrót na czarnym aksamitnym tle,
dramatyczne oświetlenie z boku,
złote akcenty,
płynne przejścia
```

## 🔗 Linki

- [Google Veo Documentation](https://cloud.google.com/vertex-ai/docs/generative-ai/model-reference/veo)
- [Vertex AI API Reference](https://cloud.google.com/vertex-ai/docs/reference)
- [Pricing](https://cloud.google.com/vertex-ai/pricing)

---

**Autor:** Lumo API Team  
**Wersja:** 1.0.0  
**Data:** 2024
