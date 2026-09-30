# Google Veo - Przykłady użycia wszystkich trybów

## 🎯 3 Tryby generowania

### 1. **Text → Video** (Podstawowy)
Generowanie video tylko z promptu tekstowego.

### 2. **Image → Video** (Animacja obrazu)
Animowanie pojedynczego zdjęcia.

### 3. **Multi-Image → Video** (Wiele zdjęć)
Generowanie video z wielu zdjęć (np. 2 osoby się przytulają).

---

## 📝 Przykłady C#

### Przykład 1: Text → Video (Podstawowy)

```csharp
var request = new GenerateVideoRequest
{
	Mode = VeoGenerationMode.TextToVideo,
	Prompt = "Profesjonalna reklama nowoczesnego smartfona, " +
			 "eleganckie ujęcia produktu, białe tło",
	Duration = 15,
	Resolution = "1080p",
	AspectRatio = "16:9"
};

var response = await _veoService.GenerateVideoAsync(request, userId);
```

### Przykład 2: Image → Video (Animacja zdjęcia)

```csharp
var request = new ImageToVideoRequest
{
	ImagePath = "/path/to/landscape.jpg",
	Prompt = "Powolny zoom na horyzont, " +
			 "delikatne ruszanie się chmur, " +
			 "ptaki lecące w oddali",
	Duration = 10,
	Resolution = "1080p",
	AspectRatio = "16:9",
	MotionIntensity = 0.5f // 0.0 = statyczne, 1.0 = dynamiczne
};

var response = await _veoService.GenerateVideoFromImageAsync(request, userId);
```

### Przykład 3: Multi-Image → Video (2 osoby się przytulają)

```csharp
var request = new MultiImageToVideoRequest
{
	ImagePaths = new List<string>
	{
		"/path/to/person1.jpg",  // Zdjęcie osoby 1
		"/path/to/person2.jpg"   // Zdjęcie osoby 2
	},
	ImageDescriptions = new List<string>
	{
		"Kobieta w czerwonej sukience",
		"Mężczyzna w garniturze"
	},
	Prompt = "Dwie osoby spotykają się i czule się przytulają, " +
			 "romantyczna atmosfera, delikatne oświetlenie, " +
			 "powolne ruchy kamery",
	UseFaceBlending = true,      // Zachowaj twarze z obrazów
	PreserveFaceDetails = true,   // Zachowaj szczegóły twarzy
	Duration = 10,
	Resolution = "1080p",
	AspectRatio = "16:9",
	MotionIntensity = 0.7f,
	Camera = new CameraSettings
	{
		Movement = "orbit",
		Angle = "medium",
		Speed = "slow"
	}
};

var response = await _veoService.GenerateVideoFromMultipleImagesAsync(request, userId);
```

### Przykład 4: Image → Video (Portret)

```csharp
var request = new ImageToVideoRequest
{
	ImagePath = "/path/to/portrait.jpg",
	Prompt = "Subtelna animacja portretu: " +
			 "delikatny uśmiech, mruganie, " +
			 "włosy lekko poruszane wiatrem",
	NegativePrompt = "zniekształcenia, dziwne ruchy, artefakty",
	Duration = 5,
	Resolution = "1080p",
	AspectRatio = "9:16",  // Vertical dla social media
	MotionIntensity = 0.3f,  // Delikatne animacje
	Style = "realistic"
};

var response = await _veoService.GenerateVideoFromImageAsync(request, userId);
```

### Przykład 5: Multi-Image → Video (Grupa osób)

```csharp
var request = new MultiImageToVideoRequest
{
	ImagePaths = new List<string>
	{
		"/path/to/person1.jpg",
		"/path/to/person2.jpg",
		"/path/to/person3.jpg",
		"/path/to/background.jpg"
	},
	ImageDescriptions = new List<string>
	{
		"Główna postać - kobieta",
		"Postać druga - mężczyzna",
		"Postać trzecia - dziecko",
		"Tło - park w słoneczny dzień"
	},
	Prompt = "Rodzina spaceruje po parku, " +
			 "dzieci biegają, rodzice rozmawiają, " +
			 "szczęśliwa atmosfera, słoneczny dzień",
	UseFaceBlending = true,
	PreserveFaceDetails = true,
	Duration = 15,
	Resolution = "1080p",
	AspectRatio = "16:9",
	Camera = new CameraSettings
	{
		Movement = "dolly",
		Speed = "medium"
	}
};

var response = await _veoService.GenerateVideoFromMultipleImagesAsync(request, userId);
```

### Przykład 6: Image → Video (Produkt)

```csharp
var request = new ImageToVideoRequest
{
	ImagePath = "/path/to/product.jpg",
	Prompt = "Produkt powoli obraca się 360 stopni, " +
			 "profesjonalne oświetlenie studyjne, " +
			 "odbicia świetlne na powierzchni",
	Duration = 8,
	Resolution = "4k",
	AspectRatio = "1:1",  // Square dla Instagram
	MotionIntensity = 0.5f,
	Style = "commercial"
};

var response = await _veoService.GenerateVideoFromImageAsync(request, userId);
```

### Przykład 7: Multi-Image → Video (Before/After)

```csharp
var request = new MultiImageToVideoRequest
{
	ImagePaths = new List<string>
	{
		"/path/to/before.jpg",
		"/path/to/after.jpg"
	},
	ImageDescriptions = new List<string>
	{
		"Stan przed metamorfozą",
		"Stan po metamorfozie"
	},
	Prompt = "Płynne przejście pokazujące transformację, " +
			 "efekt morphing, profesjonalna prezentacja",
	Duration = 10,
	Resolution = "1080p",
	AspectRatio = "16:9",
	MotionIntensity = 0.6f
};

var response = await _veoService.GenerateVideoFromMultipleImagesAsync(request, userId);
```

---

## 🌐 Przykłady TypeScript/React

### Przykład 1: Text → Video

```typescript
interface GenerateVideoRequest {
  mode?: 'TextToVideo' | 'ImageToVideo' | 'MultiImageToVideo';
  prompt: string;
  duration?: number;
  resolution?: string;
  aspectRatio?: string;
}

async function generateTextToVideo(token: string, prompt: string) {
  const response = await fetch('/api/googleveo/generate', {
	method: 'POST',
	headers: {
	  'Content-Type': 'application/json',
	  'Authorization': `Bearer ${token}`
	},
	body: JSON.stringify({
	  mode: 'TextToVideo',
	  prompt: prompt,
	  duration: 15,
	  resolution: '1080p',
	  aspectRatio: '16:9'
	})
  });

  return await response.json();
}

// Użycie
const result = await generateTextToVideo(
  authToken,
  'Profesjonalna reklama smartfona, eleganckie ujęcia'
);
console.log('Job ID:', result.jobId);
```

### Przykład 2: Image → Video

```typescript
interface ImageToVideoRequest {
  imagePath: string;
  prompt: string;
  duration?: number;
  resolution?: string;
  aspectRatio?: string;
  motionIntensity?: number;
}

async function generateImageToVideo(
  token: string,
  request: ImageToVideoRequest
) {
  const response = await fetch('/api/googleveo/generate/from-image', {
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
const result = await generateImageToVideo(authToken, {
  imagePath: '/uploads/landscape.jpg',
  prompt: 'Powolny zoom, ruszające się chmury',
  duration: 10,
  motionIntensity: 0.5
});
```

### Przykład 3: Multi-Image → Video (2 osoby się przytulają)

```typescript
interface MultiImageToVideoRequest {
  imagePaths: string[];
  imageDescriptions?: string[];
  prompt: string;
  useFaceBlending?: boolean;
  preserveFaceDetails?: boolean;
  duration?: number;
  resolution?: string;
  aspectRatio?: string;
  motionIntensity?: number;
}

async function generateMultiImageToVideo(
  token: string,
  request: MultiImageToVideoRequest
) {
  const response = await fetch('/api/googleveo/generate/from-multiple-images', {
	method: 'POST',
	headers: {
	  'Content-Type': 'application/json',
	  'Authorization': `Bearer ${token}`
	},
	body: JSON.stringify(request)
  });

  return await response.json();
}

// Użycie: 2 osoby się przytulają
const result = await generateMultiImageToVideo(authToken, {
  imagePaths: [
	'/uploads/person1.jpg',
	'/uploads/person2.jpg'
  ],
  imageDescriptions: [
	'Kobieta w czerwonej sukience',
	'Mężczyzna w garniturze'
  ],
  prompt: 'Dwie osoby spotykają się i czule się przytulają, romantyczna atmosfera',
  useFaceBlending: true,
  preserveFaceDetails: true,
  duration: 10,
  motionIntensity: 0.7
});
```

### Przykład 4: React Component - Image Uploader

```tsx
function ImageToVideoGenerator() {
  const [image, setImage] = useState<File | null>(null);
  const [prompt, setPrompt] = useState('');
  const [jobId, setJobId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
	e.preventDefault();
	if (!image) return;

	setLoading(true);

	// Upload obrazu
	const formData = new FormData();
	formData.append('image', image);

	const uploadResponse = await fetch('/api/upload', {
	  method: 'POST',
	  body: formData
	});

	const { path } = await uploadResponse.json();

	// Generuj video
	const result = await generateImageToVideo(authToken, {
	  imagePath: path,
	  prompt: prompt,
	  duration: 10,
	  motionIntensity: 0.5
	});

	setJobId(result.jobId);
	setLoading(false);
  };

  return (
	<div>
	  <h2>Image → Video Generator</h2>
	  <form onSubmit={handleSubmit}>
		<input
		  type="file"
		  accept="image/*"
		  onChange={(e) => setImage(e.target.files?.[0] || null)}
		/>
		<textarea
		  placeholder="Opisz jak animować obraz..."
		  value={prompt}
		  onChange={(e) => setPrompt(e.target.value)}
		/>
		<button type="submit" disabled={loading || !image}>
		  {loading ? 'Generowanie...' : 'Generuj Video'}
		</button>
	  </form>
	  {jobId && <p>Job ID: {jobId}</p>}
	</div>
  );
}
```

### Przykład 5: React Component - Multi-Image Uploader

```tsx
function MultiImageToVideoGenerator() {
  const [images, setImages] = useState<File[]>([]);
  const [descriptions, setDescriptions] = useState<string[]>([]);
  const [prompt, setPrompt] = useState('');
  const [jobId, setJobId] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
	e.preventDefault();

	// Upload wszystkich obrazów
	const paths = await Promise.all(
	  images.map(async (image) => {
		const formData = new FormData();
		formData.append('image', image);
		const res = await fetch('/api/upload', {
		  method: 'POST',
		  body: formData
		});
		const { path } = await res.json();
		return path;
	  })
	);

	// Generuj video
	const result = await generateMultiImageToVideo(authToken, {
	  imagePaths: paths,
	  imageDescriptions: descriptions,
	  prompt: prompt,
	  useFaceBlending: true,
	  preserveFaceDetails: true,
	  duration: 10
	});

	setJobId(result.jobId);
  };

  return (
	<div>
	  <h2>Multi-Image → Video (np. 2 osoby się przytulają)</h2>
	  <form onSubmit={handleSubmit}>
		<input
		  type="file"
		  accept="image/*"
		  multiple
		  onChange={(e) => {
			const files = Array.from(e.target.files || []);
			setImages(files);
			setDescriptions(new Array(files.length).fill(''));
		  }}
		/>

		{images.map((img, i) => (
		  <div key={i}>
			<img src={URL.createObjectURL(img)} width="100" />
			<input
			  placeholder={`Opis obrazu ${i + 1}`}
			  value={descriptions[i]}
			  onChange={(e) => {
				const newDesc = [...descriptions];
				newDesc[i] = e.target.value;
				setDescriptions(newDesc);
			  }}
			/>
		  </div>
		))}

		<textarea
		  placeholder="Opisz scenę (np. dwie osoby się przytulają)..."
		  value={prompt}
		  onChange={(e) => setPrompt(e.target.value)}
		/>

		<button type="submit">Generuj Video</button>
	  </form>
	  {jobId && <p>Job ID: {jobId}</p>}
	</div>
  );
}
```

---

## 📊 Podsumowanie różnic

| Funkcja | Text → Video | Image → Video | Multi-Image → Video |
|---------|--------------|---------------|---------------------|
| **Źródło** | Tylko prompt | 1 obraz + prompt | Wiele obrazów + prompt |
| **Use case** | Tworzenie od zera | Animacja zdjęcia | Sceny z osobami |
| **Face blending** | ❌ | ❌ | ✅ |
| **Motion intensity** | ❌ | ✅ | ✅ |
| **Max obrazów** | 0 | 1 | 2-10 |
| **Przykład** | Reklama produktu | Portret ożywa | 2 osoby się przytulają |

---

## 💡 Best Practices

### Text → Video
```csharp
✅ DOBRZE: "Profesjonalna reklama smartfona, eleganckie ujęcia, białe tło, 360 obrót"
❌ ŹLE: "telefon"
```

### Image → Video
```csharp
✅ DOBRZE: MotionIntensity = 0.3f dla portretów (subtelne)
✅ DOBRZE: MotionIntensity = 0.8f dla krajobrazów (dynamiczne)
❌ ŹLE: MotionIntensity = 1.0f dla twarzy (za dużo ruchu)
```

### Multi-Image → Video
```csharp
✅ DOBRZE: UseFaceBlending = true + PreserveFaceDetails = true
✅ DOBRZE: Dokładne opisy obrazów (ImageDescriptions)
❌ ŹLE: Brak opisów obrazów
❌ ŹLE: Więcej niż 10 obrazów (zbyt skomplikowane)
```

---

## 🎬 Przykłady Use Case

### 1. Reklama produktu
```csharp
Mode: TextToVideo
Prompt: "Elegancki zegarek na czarnym tle, 360 obrót, profesjonalne oświetlenie"
```

### 2. Ożywienie zdjęcia
```csharp
Mode: ImageToVideo
Prompt: "Portret - subtelny uśmiech, mruganie, włosy poruszane wiatrem"
MotionIntensity: 0.3
```

### 3. Para się przytula
```csharp
Mode: MultiImageToVideo
Images: [person1.jpg, person2.jpg]
Prompt: "Dwie osoby spotykają się i czule się przytulają"
UseFaceBlending: true
```

### 4. Prezentacja zespołu
```csharp
Mode: MultiImageToVideo
Images: [ceo.jpg, cto.jpg, cfo.jpg, team.jpg]
Prompt: "Profesjonalna prezentacja zespołu zarządzającego"
```

### 5. Before/After
```csharp
Mode: MultiImageToVideo
Images: [before.jpg, after.jpg]
Prompt: "Płynne przejście pokazujące transformację"
```

---

**Gotowe! Teraz masz pełną obsługę wszystkich 3 trybów! 🎉**
