namespace Lumo.Infrastructure.Entities;

public sealed class VeoConfig
{
    public int Id { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string ProjectId { get; set; } = string.Empty;

    public string Location { get; set; } = "us-central1";

    public string ApiBaseUrl { get; set; } = "https://aiplatform.googleapis.com/v1";

    public string Model { get; set; } = "veo-3.1";

    public int MaxPromptLength { get; set; } = 2000;

    public int DefaultDuration { get; set; } = 5;

    public string DefaultResolution { get; set; } = "1080p";

    public string DefaultAspectRatio { get; set; } = "16:9";

    public int MaxRetries { get; set; } = 3;

    public int TimeoutSeconds { get; set; } = 300;

    public int PollingIntervalSeconds { get; set; } = 10;

    public int MaxPollingAttempts { get; set; } = 60;

    public bool EnableTextToVideo { get; set; } = true;

    public bool EnableImageToVideo { get; set; } = true;

    public bool EnableVideoExtension { get; set; } = true;

    public bool EnableStyleTransfer { get; set; } = true;

    public bool EnableCameraControls { get; set; } = true;

    public bool EnableWatermarkRemoval { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
