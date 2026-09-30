namespace Integrations.Google.API.Veo.Models;

public sealed class GenerateVideoRequest
{
    public string Prompt { get; set; } = string.Empty;

    public string? NegativePrompt { get; set; }

    public int Duration { get; set; } = 5;

    public string Resolution { get; set; } = "1080p";

    public string AspectRatio { get; set; } = "16:9";

    public string? Style { get; set; }

    public VeoGenerationMode Mode { get; set; } = VeoGenerationMode.TextToVideo;

    public string? ReferenceImagePath { get; set; }

    public List<string>? ReferenceImages { get; set; }

    public List<string>? ImageDescriptions { get; set; }

    public bool UseFaceBlending { get; set; }

    public int? Seed { get; set; }

    public int Fps { get; set; } = 30;

    public CameraSettings? Camera { get; set; }

    public string? Preset { get; set; }

    public bool UseAdvancedFeatures { get; set; } = true;
}

public sealed class CameraSettings
{
    public string? Movement { get; set; }

    public string? Angle { get; set; }

    public string? Speed { get; set; }
}

public sealed class GenerateVideoResponse
{
    public string JobId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public string? VideoPath { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool Success { get; set; }

    public string? Error { get; set; }

    public TimeSpan? GenerationTime { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public long? FileSize { get; set; }
}

public sealed class ExtendVideoRequest
{
    public string VideoPath { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public int ExtendDuration { get; set; } = 5;

    public string Direction { get; set; } = "forward";
}

public sealed class StyleTransferRequest
{
    public string VideoPath { get; set; } = string.Empty;

    public string TargetStyle { get; set; } = string.Empty;

    public float Strength { get; set; } = 0.8f;

    public string? ReferenceImagePath { get; set; }
}

public sealed class VeoVideoInfo
{
    public int Id { get; set; }

    public string JobId { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public int Duration { get; set; }

    public string Resolution { get; set; } = string.Empty;

    public string AspectRatio { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class VeoStatistics
{
    public int TotalVideos { get; set; }

    public int CompletedVideos { get; set; }

    public int ProcessingVideos { get; set; }

    public int FailedVideos { get; set; }

    public int TotalDuration { get; set; }

    public TimeSpan AverageGenerationTime { get; set; }

    public long TotalFileSize { get; set; }
}

public sealed class VeoPreset
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Duration { get; set; }

    public string AspectRatio { get; set; } = string.Empty;

    public string Resolution { get; set; } = string.Empty;

    public string? Style { get; set; }

    public int Fps { get; set; }
}

public enum VeoGenerationMode
{
    TextToVideo = 0,
    ImageToVideo = 1,
    MultiImageToVideo = 2
}

public sealed class ImageToVideoRequest
{
    public string ImagePath { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string? NegativePrompt { get; set; }

    public int Duration { get; set; } = 5;

    public string Resolution { get; set; } = "1080p";

    public string AspectRatio { get; set; } = "16:9";

    public string? Style { get; set; }

    public int Fps { get; set; } = 30;

    public int? Seed { get; set; }

    public float MotionIntensity { get; set; } = 0.5f;
}

public sealed class MultiImageToVideoRequest
{
    public List<string> ImagePaths { get; set; } = new();

    public List<string>? ImageDescriptions { get; set; }

    public string Prompt { get; set; } = string.Empty;

    public string? NegativePrompt { get; set; }

    public bool UseFaceBlending { get; set; } = true;

    public bool PreserveFaceDetails { get; set; } = true;

    public int Duration { get; set; } = 10;

    public string Resolution { get; set; } = "1080p";

    public string AspectRatio { get; set; } = "16:9";

    public string? Style { get; set; }

    public int Fps { get; set; } = 30;

    public int? Seed { get; set; }

    public float MotionIntensity { get; set; } = 0.7f;

    public CameraSettings? Camera { get; set; }
}
