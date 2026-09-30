import { api } from './client';

// Matches Lumo.Api MetaController (route: /api/Meta) — Facebook/Instagram Graph API integration.

export interface MetaToken {
	userId: string;
	accessToken: string;
	tokenType: string;
	metadataJson?: string;
	expiresAt?: string;
}

export interface InstagramPublishRequest {
	igUserId: string;
	imageUrl: string;
	caption?: string;
	accessToken?: string;
	userId?: string;
	pageId?: string;
}

export const metaApi = {
	getLoginUrl: () => api<{ url: string }>('/Meta/oauth/url', { method: 'GET' }),

	callback: (code: string, redirectUri?: string) =>
		api<{ accessToken: string; profile: unknown }>(
			`/Meta/callback?code=${encodeURIComponent(code)}${redirectUri ? `&redirectUri=${encodeURIComponent(redirectUri)}` : ''}`,
			{ method: 'GET' }
		),

	storeToken: (token: MetaToken) =>
		api<MetaToken>('/Meta/store-token', { method: 'POST', body: JSON.stringify(token) }),

	extendToken: (accessToken: string, userId?: string) =>
		api<{ accessToken: string }>(
			`/Meta/extend-token?accessToken=${encodeURIComponent(accessToken)}${userId ? `&userId=${encodeURIComponent(userId)}` : ''}`,
			{ method: 'POST' }
		),

	pages: (accessToken: string, userId?: string) =>
		api<Record<string, unknown>[]>(
			`/Meta/pages?accessToken=${encodeURIComponent(accessToken)}${userId ? `&userId=${encodeURIComponent(userId)}` : ''}`,
			{ method: 'GET' }
		),

	saveInstagramAccount: (pageId: string, userId?: string, accessToken?: string) =>
		api<Record<string, unknown>>(
			`/Meta/ig/save-account?pageId=${encodeURIComponent(pageId)}${userId ? `&userId=${encodeURIComponent(userId)}` : ''}${
				accessToken ? `&accessToken=${encodeURIComponent(accessToken)}` : ''
			}`,
			{ method: 'POST' }
		),

	getInstagramMedia: (igUserId: string, accessToken?: string, userId?: string, limit?: number) =>
		api<Record<string, unknown>[]>(
			`/Meta/ig/media?igUserId=${encodeURIComponent(igUserId)}${accessToken ? `&accessToken=${encodeURIComponent(accessToken)}` : ''}${
				userId ? `&userId=${encodeURIComponent(userId)}` : ''
			}${limit ? `&limit=${limit}` : ''}`,
			{ method: 'GET' }
		),

	publishInstagramImage: (data: InstagramPublishRequest) =>
		api<Record<string, unknown>>('/Meta/ig/publish', { method: 'POST', body: JSON.stringify(data) }),

	profile: (accessToken: string) =>
		api<{ id: string; name: string; email?: string }>(`/Meta/profile?accessToken=${encodeURIComponent(accessToken)}`, {
			method: 'GET',
		}),
};
