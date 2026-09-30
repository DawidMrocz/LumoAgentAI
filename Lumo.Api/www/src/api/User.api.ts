import { api } from './client';
import type { MessageResponse } from './Auth.api';

// Matches Lumo.Api UserController (route: /api/User) — authenticated profile actions.

export interface ProfileDTO {
	id: string;
	fullName: string;
	email: string;
}

export interface UpdateProfileRequest {
	firstName: string;
	lastName: string;
}

export interface ChangePasswordRequest {
	currentPassword: string;
	newPassword: string;
}

// =========================
// USER API (UserController)
// =========================
export const userApi = {
	me: () => api<ProfileDTO>('/User/me'),

	updateProfile: (data: UpdateProfileRequest) =>
		api<MessageResponse>('/User/profile', {
			method: 'PUT',
			body: JSON.stringify(data),
		}),

	changePassword: (data: ChangePasswordRequest) =>
		api<MessageResponse>('/User/change-password', {
			method: 'POST',
			body: JSON.stringify(data),
		}),
};
