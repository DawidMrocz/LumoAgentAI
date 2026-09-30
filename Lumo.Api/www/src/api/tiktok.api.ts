import { api } from './client';

// Matches Lumo.Api TikTokController (route: /api/TikTok).

export type TikTokPrivacyLevel = 'Public' | 'FriendsOnly' | 'Private' | 'MutualFollowFriends';

export interface PublishVideoRequest {
	videoPath: string;
	caption: string;
	hashtags: string[];
	privacyLevel?: TikTokPrivacyLevel;
	allowComments?: boolean;
	allowDuet?: boolean;
	allowStitch?: boolean;
	isDraft?: boolean;
	scheduledAt?: string;
	thumbnailPath?: string;
}

export interface PublishVideoResponse {
	videoId: string;
	videoUrl: string;
	status: string;
	success: boolean;
	error?: string;
	publishedAt: string;
}

export interface PublishBulkVideosRequest {
	videos: PublishVideoRequest[];
	delayBetweenPublications?: number;
}

export interface PublishBulkVideosResponse {
	successCount: number;
	failureCount: number;
	results: PublishVideoResponse[];
}

export interface TikTokVideoInfo {
	videoId: string;
	caption: string;
	videoUrl: string;
	thumbnailUrl?: string;
	viewCount: number;
	likeCount: number;
	commentCount: number;
	shareCount: number;
	duration: number;
	createdAt: string;
}

export interface TikTokStatistics {
	totalVideos: number;
	totalViews: number;
	totalLikes: number;
	totalComments: number;
	totalShares: number;
	averageViews: number;
	averageLikes: number;
	engagementRate: number;
}

export interface TikTokUserInfo {
	tikTokUserId: string;
	username: string;
	displayName: string;
	avatarUrl?: string;
	bio?: string;
	followerCount: number;
	followingCount: number;
	likesCount: number;
	videoCount: number;
}

export const tiktokApi = {
	authorize: () => api<string>('/TikTok/authorize', { method: 'GET' }),

	publishVideo: (data: PublishVideoRequest) =>
		api<PublishVideoResponse>('/TikTok/publish', { method: 'POST', body: JSON.stringify(data) }),

	publishBulkVideos: (data: PublishBulkVideosRequest) =>
		api<PublishBulkVideosResponse>('/TikTok/publish-bulk', { method: 'POST', body: JSON.stringify(data) }),

	getVideos: (skip = 0, take = 20) =>
		api<TikTokVideoInfo[]>(`/TikTok/videos?skip=${skip}&take=${take}`, { method: 'GET' }),

	getVideoDetails: (videoId: string) =>
		api<TikTokVideoInfo>(`/TikTok/videos/${encodeURIComponent(videoId)}`, { method: 'GET' }),

	getStatistics: (from?: string, to?: string) =>
		api<TikTokStatistics>(
			`/TikTok/statistics?${from ? `from=${encodeURIComponent(from)}&` : ''}${to ? `to=${encodeURIComponent(to)}` : ''}`,
			{ method: 'GET' }
		),

	getUserInfo: () => api<TikTokUserInfo>('/TikTok/user-info', { method: 'GET' }),

	isConnected: () => api<{ connected: boolean }>('/TikTok/is-connected', { method: 'GET' }),

	disconnect: () => api<{ message: string }>('/TikTok/disconnect', { method: 'POST' }),

	refreshStatistics: () => api<{ message: string }>('/TikTok/refresh-statistics', { method: 'POST' }),
};
