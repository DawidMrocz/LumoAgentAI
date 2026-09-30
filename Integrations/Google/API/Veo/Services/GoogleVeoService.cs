using Integrations.Google.API.Veo.Client;
using Integrations.Google.API.Veo.Client.Models;
using Integrations.Google.API.Veo.Models;
using Lumo.Infrastructure.Entities;
using Lumo.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Integrations.Google.API.Veo.Services;

internal sealed class GoogleVeoService : IGoogleVeoService
{
    private sealed record VeoPresetDefinition(int Duration, string AspectRatio, string Resolution, string? Style, string Description);

    private static readonly Dictionary<string, VeoPresetDefinition> Presets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["advertisement"] = new(15, "16:9", "1080p", "commercial", "15s video w formacie 16:9, idealny do reklam"),
        ["socialmedia"] = new(10, "9:16", "1080p", "dynamic", "10s video w formacie 9:16, idealny do TikTok/Instagram"),
        ["cinematic"] = new(30, "21:9", "4k", "cinematic", "30s video w formacie kinowym 21:9, wysoka jakość")
    };

    private readonly IGoogleVeoClient _client;
    private readonly IVeoVideoRepository _videoRepository;
    private readonly IVeoConfigRepository _configRepository;
    private readonly ILogger<GoogleVeoService> _logger;

    public GoogleVeoService(
        IGoogleVeoClient client,
        IVeoVideoRepository videoRepository,
        IVeoConfigRepository configRepository,
        ILogger<GoogleVeoService> logger)
    {
        _client = client;
        _videoRepository = videoRepository;
        _configRepository = configRepository;
        _logger = logger;
    }

    public async Task<GenerateVideoResponse> GenerateVideoAsync(
        GenerateVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var config = await _configRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Veo: Brak aktywnej konfiguracji.");

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Prompt jest wymagany"
                };
            }

            if (request.Prompt.Length > config.MaxPromptLength)
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = $"Prompt przekracza maksymalną długość {config.MaxPromptLength} znaków"
                };
            }

            if (!string.IsNullOrEmpty(request.Preset))
            {
                ApplyPreset(request, request.Preset);
            }

            var veoVideo = new VeoVideo
            {
                UserId = userId,
                Prompt = request.Prompt,
                NegativePrompt = request.NegativePrompt,
                Status = VeoVideoStatus.Pending,
                Duration = request.Duration,
                Resolution = request.Resolution,
                AspectRatio = request.AspectRatio,
                Fps = request.Fps,
                Style = request.Style,
                Seed = request.Seed,
                ReferenceImagePath = request.ReferenceImagePath,
                Preset = request.Preset,
                Progress = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _videoRepository.AddAsync(veoVideo, cancellationToken);

            var apiRequest = await BuildApiRequestAsync(request, cancellationToken);
            var operationName = await _client.PredictAsync(apiRequest, cancellationToken);

            if (string.IsNullOrEmpty(operationName))
            {
                veoVideo.Status = VeoVideoStatus.Failed;
                veoVideo.ErrorMessage = "Nie udało się rozpocząć generowania";
                await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Nie udało się rozpocząć generowania"
                };
            }

            veoVideo.JobId = operationName;
            veoVideo.Status = VeoVideoStatus.Queued;
            veoVideo.StartedAt = DateTimeOffset.UtcNow;
            await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

            _ = Task.Run(async () => await PollOperationAsync(veoVideo.Id, operationName, config.PollingIntervalSeconds, config.MaxPollingAttempts), cancellationToken);

            return new GenerateVideoResponse
            {
                JobId = operationName,
                Status = "Queued",
                Success = true,
                CreatedAt = veoVideo.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video with Veo");
            return new GenerateVideoResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<GenerateVideoResponse> GenerateVideoFromPresetAsync(
        string presetName,
        string prompt,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var request = new GenerateVideoRequest
        {
            Prompt = prompt,
            Preset = presetName
        };

        return await GenerateVideoAsync(request, userId, cancellationToken);
    }

    public async Task<GenerateVideoResponse> GenerateVideoFromImageAsync(
        ImageToVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ImagePath))
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Ścieżka do obrazu jest wymagana"
                };
            }

            if (!File.Exists(request.ImagePath))
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Obraz nie istnieje"
                };
            }

            var config = await _configRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Veo: Brak aktywnej konfiguracji.");

            var veoVideo = new VeoVideo
            {
                UserId = userId,
                Prompt = request.Prompt,
                NegativePrompt = request.NegativePrompt,
                Status = VeoVideoStatus.Pending,
                Duration = request.Duration,
                Resolution = request.Resolution,
                AspectRatio = request.AspectRatio,
                Fps = request.Fps,
                Style = request.Style,
                Seed = request.Seed,
                ReferenceImagePath = request.ImagePath,
                Progress = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _videoRepository.AddAsync(veoVideo, cancellationToken);

            var apiRequest = await BuildImageToVideoApiRequestAsync(request, cancellationToken);
            var operationName = await _client.PredictAsync(apiRequest, cancellationToken);

            if (string.IsNullOrEmpty(operationName))
            {
                veoVideo.Status = VeoVideoStatus.Failed;
                veoVideo.ErrorMessage = "Nie udało się rozpocząć generowania";
                await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Nie udało się rozpocząć generowania"
                };
            }

            veoVideo.JobId = operationName;
            veoVideo.Status = VeoVideoStatus.Queued;
            veoVideo.StartedAt = DateTimeOffset.UtcNow;
            await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

            _ = Task.Run(async () => await PollOperationAsync(veoVideo.Id, operationName, config.PollingIntervalSeconds, config.MaxPollingAttempts), cancellationToken);

            return new GenerateVideoResponse
            {
                JobId = operationName,
                Status = "Queued",
                Success = true,
                CreatedAt = veoVideo.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video from image");
            return new GenerateVideoResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<GenerateVideoResponse> GenerateVideoFromMultipleImagesAsync(
        MultiImageToVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.ImagePaths == null || !request.ImagePaths.Any())
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Co najmniej jeden obraz jest wymagany"
                };
            }

            if (request.ImagePaths.Count > 10)
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Maksymalnie 10 obrazów"
                };
            }

            foreach (var imagePath in request.ImagePaths)
            {
                if (!File.Exists(imagePath))
                {
                    return new GenerateVideoResponse
                    {
                        Success = false,
                        Error = $"Obraz nie istnieje: {imagePath}"
                    };
                }
            }

            var config = await _configRepository.GetActiveAsync(cancellationToken)
                ?? throw new InvalidOperationException("Veo: Brak aktywnej konfiguracji.");

            var veoVideo = new VeoVideo
            {
                UserId = userId,
                Prompt = request.Prompt,
                NegativePrompt = request.NegativePrompt,
                Status = VeoVideoStatus.Pending,
                Duration = request.Duration,
                Resolution = request.Resolution,
                AspectRatio = request.AspectRatio,
                Fps = request.Fps,
                Style = request.Style,
                Seed = request.Seed,
                ReferenceImagePath = string.Join(";", request.ImagePaths),
                Progress = 0,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _videoRepository.AddAsync(veoVideo, cancellationToken);

            var apiRequest = await BuildMultiImageToVideoApiRequestAsync(request, cancellationToken);
            var operationName = await _client.PredictAsync(apiRequest, cancellationToken);

            if (string.IsNullOrEmpty(operationName))
            {
                veoVideo.Status = VeoVideoStatus.Failed;
                veoVideo.ErrorMessage = "Nie udało się rozpocząć generowania";
                await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Nie udało się rozpocząć generowania"
                };
            }

            veoVideo.JobId = operationName;
            veoVideo.Status = VeoVideoStatus.Queued;
            veoVideo.StartedAt = DateTimeOffset.UtcNow;
            await _videoRepository.UpdateAsync(veoVideo, cancellationToken);

            _ = Task.Run(async () => await PollOperationAsync(veoVideo.Id, operationName, config.PollingIntervalSeconds, config.MaxPollingAttempts), cancellationToken);

            return new GenerateVideoResponse
            {
                JobId = operationName,
                Status = "Queued",
                Success = true,
                CreatedAt = veoVideo.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video from multiple images");
            return new GenerateVideoResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<GenerateVideoResponse> ExtendVideoAsync(
        ExtendVideoRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return new GenerateVideoResponse
        {
            Success = false,
            Error = "Rozszerzanie video nie jest jeszcze zaimplementowane"
        };
    }

    public async Task<GenerateVideoResponse> TransferStyleAsync(
        StyleTransferRequest request,
        int userId,
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        return new GenerateVideoResponse
        {
            Success = false,
            Error = "Transformacja stylu nie jest jeszcze zaimplementowana"
        };
    }

    public async Task<GenerateVideoResponse> GetGenerationStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var video = await _videoRepository.GetByJobIdAsync(jobId, cancellationToken);

            if (video == null)
            {
                return new GenerateVideoResponse
                {
                    Success = false,
                    Error = "Video nie znalezione"
                };
            }

            return new GenerateVideoResponse
            {
                JobId = video.JobId,
                Status = video.Status.ToString(),
                VideoUrl = video.VideoUrl,
                VideoPath = video.VideoPath,
                ThumbnailUrl = video.ThumbnailUrl,
                Success = video.Status == VeoVideoStatus.Completed,
                Error = video.ErrorMessage,
                GenerationTime = video.GenerationTime,
                CreatedAt = video.CreatedAt,
                CompletedAt = video.CompletedAt,
                FileSize = video.FileSize > 0 ? video.FileSize : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generation status");
            return new GenerateVideoResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<List<VeoVideoInfo>> GetUserVideosAsync(
        int userId,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var videos = await _videoRepository.GetByUserIdAsync(userId, skip, take, cancellationToken);

        return videos.Select(MapToInfo).ToList();
    }

    public async Task<VeoStatistics> GetStatisticsAsync(
        int userId,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var videos = await _videoRepository.GetByUserIdAsync(userId, from, to, cancellationToken);
        var completedVideos = videos.Where(v => v.Status == VeoVideoStatus.Completed).ToList();

        return new VeoStatistics
        {
            TotalVideos = videos.Count,
            CompletedVideos = completedVideos.Count,
            ProcessingVideos = videos.Count(v => v.Status == VeoVideoStatus.Processing || v.Status == VeoVideoStatus.Queued),
            FailedVideos = videos.Count(v => v.Status == VeoVideoStatus.Failed),
            TotalDuration = completedVideos.Sum(v => v.Duration),
            AverageGenerationTime = completedVideos.Any()
                ? TimeSpan.FromSeconds(completedVideos.Average(v => v.GenerationTime?.TotalSeconds ?? 0))
                : TimeSpan.Zero,
            TotalFileSize = completedVideos.Sum(v => v.FileSize)
        };
    }

    public List<VeoPreset> GetPresets()
    {
        return Presets.Select(p => new VeoPreset
        {
            Name = p.Key,
            Description = p.Value.Description,
            Duration = p.Value.Duration,
            AspectRatio = p.Value.AspectRatio,
            Resolution = p.Value.Resolution,
            Style = p.Value.Style,
            Fps = 30
        }).ToList();
    }

    public async Task<bool> CancelGenerationAsync(
        string jobId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var video = await _videoRepository.GetByJobIdAsync(jobId, cancellationToken);

            if (video == null)
                return false;

            if (video.Status == VeoVideoStatus.Completed || video.Status == VeoVideoStatus.Failed)
                return false;

            video.Status = VeoVideoStatus.Cancelled;
            await _videoRepository.UpdateAsync(video, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling generation");
            return false;
        }
    }

    public async Task<VeoVideoInfo?> GetVideoDetailsAsync(
        int userId,
        int videoId,
        CancellationToken cancellationToken = default)
    {
        var video = await _videoRepository.GetByIdAsync(videoId, cancellationToken);

        if (video == null || video.UserId != userId)
            return null;

        return MapToInfo(video);
    }

    private static VeoVideoInfo MapToInfo(VeoVideo video)
    {
        return new VeoVideoInfo
        {
            Id = video.Id,
            JobId = video.JobId,
            Prompt = video.Prompt,
            Status = video.Status.ToString(),
            VideoUrl = video.VideoUrl,
            Duration = video.Duration,
            Resolution = video.Resolution,
            AspectRatio = video.AspectRatio,
            FileSize = video.FileSize,
            CreatedAt = video.CreatedAt,
            CompletedAt = video.CompletedAt
        };
    }

    private async Task<VeoApiRequest> BuildApiRequestAsync(
        GenerateVideoRequest request,
        CancellationToken cancellationToken)
    {
        var instance = new VeoInstance
        {
            Prompt = request.Prompt,
            NegativePrompt = request.NegativePrompt
        };

        if (!string.IsNullOrEmpty(request.ReferenceImagePath) && File.Exists(request.ReferenceImagePath))
        {
            var imageBytes = await File.ReadAllBytesAsync(request.ReferenceImagePath, cancellationToken);
            instance.Image = new VeoImage
            {
                BytesBase64Encoded = Convert.ToBase64String(imageBytes)
            };
        }

        var parameters = new VeoParameters
        {
            Duration = request.Duration,
            Resolution = request.Resolution,
            AspectRatio = request.AspectRatio,
            Fps = request.Fps,
            Seed = request.Seed,
            Style = request.Style
        };

        if (request.Camera != null)
        {
            parameters.CameraMotion = new CameraMotion
            {
                Type = request.Camera.Movement,
                Speed = request.Camera.Speed,
                Angle = request.Camera.Angle
            };
        }

        return new VeoApiRequest
        {
            Instances = new List<VeoInstance> { instance },
            Parameters = parameters
        };
    }

    private async Task<VeoApiRequest> BuildImageToVideoApiRequestAsync(
        ImageToVideoRequest request,
        CancellationToken cancellationToken)
    {
        var instance = new VeoInstance
        {
            Prompt = request.Prompt,
            NegativePrompt = request.NegativePrompt
        };

        var imageBytes = await File.ReadAllBytesAsync(request.ImagePath, cancellationToken);
        instance.Image = new VeoImage
        {
            BytesBase64Encoded = Convert.ToBase64String(imageBytes)
        };

        var parameters = new VeoParameters
        {
            Duration = request.Duration,
            Resolution = request.Resolution,
            AspectRatio = request.AspectRatio,
            Fps = request.Fps,
            Seed = request.Seed,
            Style = request.Style,
            MotionIntensity = request.MotionIntensity
        };

        return new VeoApiRequest
        {
            Instances = new List<VeoInstance> { instance },
            Parameters = parameters
        };
    }

    private async Task<VeoApiRequest> BuildMultiImageToVideoApiRequestAsync(
        MultiImageToVideoRequest request,
        CancellationToken cancellationToken)
    {
        var instance = new VeoInstance
        {
            Prompt = request.Prompt,
            NegativePrompt = request.NegativePrompt,
            FaceBlending = request.UseFaceBlending,
            PreserveFaceDetails = request.PreserveFaceDetails,
            ImageDescriptions = request.ImageDescriptions
        };

        var images = new List<VeoImage>();
        foreach (var imagePath in request.ImagePaths)
        {
            var imageBytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
            images.Add(new VeoImage
            {
                BytesBase64Encoded = Convert.ToBase64String(imageBytes)
            });
        }
        instance.Images = images;

        var parameters = new VeoParameters
        {
            Duration = request.Duration,
            Resolution = request.Resolution,
            AspectRatio = request.AspectRatio,
            Fps = request.Fps,
            Seed = request.Seed,
            Style = request.Style,
            MotionIntensity = request.MotionIntensity
        };

        if (request.Camera != null)
        {
            parameters.CameraMotion = new CameraMotion
            {
                Type = request.Camera.Movement,
                Speed = request.Camera.Speed,
                Angle = request.Camera.Angle
            };
        }

        return new VeoApiRequest
        {
            Instances = new List<VeoInstance> { instance },
            Parameters = parameters
        };
    }

    private async Task PollOperationAsync(int videoId, string operationName, int pollingIntervalSeconds, int maxPollingAttempts)
    {
        try
        {
            var attempts = 0;
            var completed = false;

            while (attempts < maxPollingAttempts && !completed)
            {
                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds));

                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video == null)
                    return;

                if (video.Status == VeoVideoStatus.Cancelled)
                    return;

                var operation = await _client.GetOperationAsync(operationName);

                if (operation == null)
                {
                    attempts++;
                    continue;
                }

                if (operation.Metadata?.ProgressPercentage.HasValue == true)
                {
                    video.Progress = operation.Metadata.ProgressPercentage.Value;
                    video.Status = VeoVideoStatus.Processing;
                }

                if (operation.Done)
                {
                    completed = true;

                    if (operation.Error != null)
                    {
                        video.Status = VeoVideoStatus.Failed;
                        video.ErrorCode = operation.Error.Code.ToString();
                        video.ErrorMessage = operation.Error.Message;
                    }
                    else if (operation.Response?.Predictions?.Any() == true)
                    {
                        var prediction = operation.Response.Predictions.First();

                        video.Status = VeoVideoStatus.Completed;
                        video.VideoUrl = prediction.VideoUri;
                        video.ThumbnailUrl = prediction.ThumbnailUri;
                        video.CompletedAt = DateTimeOffset.UtcNow;
                        video.GenerationTime = video.CompletedAt - video.StartedAt;
                        video.Progress = 100;

                        if (!string.IsNullOrEmpty(prediction.VideoUri))
                        {
                            await DownloadVideoAsync(video, prediction.VideoUri);
                        }
                    }
                }

                await _videoRepository.UpdateAsync(video);
                attempts++;
            }

            if (!completed)
            {
                var video = await _videoRepository.GetByIdAsync(videoId);
                if (video != null)
                {
                    video.Status = VeoVideoStatus.Failed;
                    video.ErrorMessage = "Przekroczono limit czasu oczekiwania";
                    await _videoRepository.UpdateAsync(video);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error polling operation {OperationName}", operationName);
        }
    }

    private async Task DownloadVideoAsync(VeoVideo video, string videoUri)
    {
        try
        {
            var videoBytes = await _client.DownloadVideoAsync(videoUri);

            var videosDir = Path.Combine("wwwroot", "generated-videos");
            Directory.CreateDirectory(videosDir);

            var fileName = $"{video.Id}_{Guid.NewGuid()}.mp4";
            var filePath = Path.Combine(videosDir, fileName);

            await File.WriteAllBytesAsync(filePath, videoBytes);

            video.VideoPath = filePath;
            video.FileSize = videoBytes.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading video");
        }
    }

    private static void ApplyPreset(GenerateVideoRequest request, string presetName)
    {
        if (!Presets.TryGetValue(presetName, out var preset))
            return;

        request.Duration = preset.Duration;
        request.AspectRatio = preset.AspectRatio;
        request.Resolution = preset.Resolution;

        if (!string.IsNullOrEmpty(preset.Style))
            request.Style = preset.Style;
    }
}
