using System.Text.Json.Serialization;

namespace Integrations.Google.API.Veo.Client.Models;

internal sealed class VeoApiRequest
{
    [JsonPropertyName("instances")]
    public List<VeoInstance> Instances { get; set; } = new();

    [JsonPropertyName("parameters")]
    public VeoParameters? Parameters { get; set; }
}

internal sealed class VeoInstance
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("negative_prompt")]
    public string? NegativePrompt { get; set; }

    [JsonPropertyName("image")]
    public VeoImage? Image { get; set; }

    [JsonPropertyName("images")]
    public List<VeoImage>? Images { get; set; }

    [JsonPropertyName("image_descriptions")]
    public List<string>? ImageDescriptions { get; set; }

    [JsonPropertyName("face_blending")]
    public bool? FaceBlending { get; set; }

    [JsonPropertyName("preserve_face_details")]
    public bool? PreserveFaceDetails { get; set; }
}

internal sealed class VeoImage
{
    [JsonPropertyName("bytes_base64_encoded")]
    public string? BytesBase64Encoded { get; set; }

    [JsonPropertyName("gcs_uri")]
    public string? GcsUri { get; set; }
}

internal sealed class VeoParameters
{
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }

    [JsonPropertyName("aspect_ratio")]
    public string? AspectRatio { get; set; }

    [JsonPropertyName("fps")]
    public int? Fps { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("style")]
    public string? Style { get; set; }

    [JsonPropertyName("camera_motion")]
    public CameraMotion? CameraMotion { get; set; }

    [JsonPropertyName("motion_intensity")]
    public float? MotionIntensity { get; set; }
}

internal sealed class CameraMotion
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("speed")]
    public string? Speed { get; set; }

    [JsonPropertyName("angle")]
    public string? Angle { get; set; }
}

internal sealed class VeoApiResponse
{
    [JsonPropertyName("predictions")]
    public List<VeoPrediction> Predictions { get; set; } = new();

    [JsonPropertyName("metadata")]
    public VeoMetadata? Metadata { get; set; }

    [JsonPropertyName("error")]
    public VeoError? Error { get; set; }
}

internal sealed class VeoPrediction
{
    [JsonPropertyName("video_uri")]
    public string? VideoUri { get; set; }

    [JsonPropertyName("video_bytes")]
    public string? VideoBytes { get; set; }

    [JsonPropertyName("thumbnail_uri")]
    public string? ThumbnailUri { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("resolution")]
    public string? Resolution { get; set; }
}

internal sealed class VeoMetadata
{
    [JsonPropertyName("generation_time")]
    public double? GenerationTime { get; set; }

    [JsonPropertyName("model_version")]
    public string? ModelVersion { get; set; }
}

internal sealed class VeoError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

internal sealed class VeoLongRunningResponse
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public VeoOperationMetadata? Metadata { get; set; }

    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("error")]
    public VeoError? Error { get; set; }

    [JsonPropertyName("response")]
    public VeoApiResponse? Response { get; set; }
}

internal sealed class VeoOperationMetadata
{
    [JsonPropertyName("createTime")]
    public string? CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public string? UpdateTime { get; set; }

    [JsonPropertyName("progressPercentage")]
    public int? ProgressPercentage { get; set; }
}
