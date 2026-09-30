import { api } from './client';

// Matches Lumo.Api GoogleVeoController (route: /api/GoogleVeo) — AI video generation.

export interface CameraSettings {
	movement?: string;
	angle?: string;
	speed?: string;
}

export interface GenerateVideoRequest {
	prompt: string;
	negativePrompt?: string;
	duration?: number;
	resolution?: string;
	aspectRatio?: string;
	style?: string;
	seed?: number;
	fps?: number;
	camera?: CameraSettings;
	preset?: string;
	useAdvancedFeatures?: boolean;
}

export interface ImageToVideoRequest {
	imagePath: string;
	prompt: string;
	negativePrompt?: string;
	duration?: number;
	resolution?: string;
	aspectRatio?: string;
	style?: string;
	fps?: number;
	seed?: number;
	motionIntensity?: number;
}

export interface MultiImageToVideoRequest {
	imagePaths: string[];
	imageDescriptions?: string[];
	prompt: string;
	negativePrompt?: string;
	useFaceBlending?: boolean;
	preserveFaceDetails?: boolean;
	duration?: number;
	resolution?: string;
	aspectRatio?: string;
	style?: string;
	fps?: number;
	seed?: number;
	motionIntensity?: number;
	camera?: CameraSettings;
}

export interface ExtendVideoRequest {
	videoPath: string;
	prompt: string;
	extendDuration?: number;
	direction?: 'forward' | 'backward';
}

export interface StyleTransferRequest {
	videoPath: string;
	targetStyle: string;
	strength?: number;
	referenceImagePath?: string;
}

export interface GenerateVideoResponse {
	jobId: string;
	status: string;
	videoUrl?: string;
	videoPath?: string;
	thumbnailUrl?: string;
	success: boolean;
	error?: string;
	createdAt: string;
	completedAt?: string;
	fileSize?: number;
}

export interface VeoVideoInfo {
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

export interface VeoStatistics {
	totalVideos: number;
	completedVideos: number;
	processingVideos: number;
	failedVideos: number;
	totalDuration: number;
	totalFileSize: number;
}

export interface VeoPreset {
	name: string;
	description: string;
	duration: number;
	aspectRatio: string;
	resolution: string;
	style?: string;
	fps: number;
}

export const googleVeoApi = {
	generateVideo: (data: GenerateVideoRequest) =>
		api<GenerateVideoResponse>('/GoogleVeo/generate', { method: 'POST', body: JSON.stringify(data) }),

	generateFromPreset: (presetName: string, prompt: string) =>
		api<GenerateVideoResponse>(`/GoogleVeo/generate/preset/${encodeURIComponent(presetName)}`, {
			method: 'POST',
			body: JSON.stringify(prompt),
		}),

	generateFromImage: (data: ImageToVideoRequest) =>
		api<GenerateVideoResponse>('/GoogleVeo/generate/from-image', { method: 'POST', body: JSON.stringify(data) }),

	generateFromMultipleImages: (data: MultiImageToVideoRequest) =>
		api<GenerateVideoResponse>('/GoogleVeo/generate/from-multiple-images', {
			method: 'POST',
			body: JSON.stringify(data),
		}),

	getStatus: (jobId: string) => api<GenerateVideoResponse>(`/GoogleVeo/status/${encodeURIComponent(jobId)}`, { method: 'GET' }),

	getVideos: (skip = 0, take = 20) => api<VeoVideoInfo[]>(`/GoogleVeo/videos?skip=${skip}&take=${take}`, { method: 'GET' }),

	getVideoDetails: (videoId: number) => api<VeoVideoInfo>(`/GoogleVeo/videos/${videoId}`, { method: 'GET' }),

	getStatistics: (from?: string, to?: string) =>
		api<VeoStatistics>(
			`/GoogleVeo/statistics?${from ? `from=${encodeURIComponent(from)}&` : ''}${to ? `to=${encodeURIComponent(to)}` : ''}`,
			{ method: 'GET' }
		),

	getPresets: () => api<VeoPreset[]>('/GoogleVeo/presets', { method: 'GET' }),

	cancelGeneration: (jobId: string) =>
		api<{ message: string }>(`/GoogleVeo/cancel/${encodeURIComponent(jobId)}`, { method: 'POST' }),

	extendVideo: (data: ExtendVideoRequest) =>
		api<GenerateVideoResponse>('/GoogleVeo/extend', { method: 'POST', body: JSON.stringify(data) }),

	transferStyle: (data: StyleTransferRequest) =>
		api<GenerateVideoResponse>('/GoogleVeo/style-transfer', { method: 'POST', body: JSON.stringify(data) }),
};
