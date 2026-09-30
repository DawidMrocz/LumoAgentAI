import { api } from './client';

// Matches Lumo.Api FCMController (route: /api/fcm).
export const fcmApi = {
	registerToken: (token: string) =>
		api<{ message: string }>('/fcm/token', {
			method: 'POST',
			body: JSON.stringify({ token }),
		}),
};
