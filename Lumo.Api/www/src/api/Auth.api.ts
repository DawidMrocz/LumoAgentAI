import { api } from './client';

// Matches Lumo.Api UserController (route: /api/User) — anonymous auth actions.

export interface LoginDTO {
	email: string;
	password: string;
}

export interface AuthResponse {
	accessToken: string;
	refreshToken: string;
	accessTokenExpiresIn: string;
	refreshTokenExpiresIn: string;
}

export interface RegisterDTO {
	email: string;
	password: string;
	firstName: string;
	lastName: string;
}

export interface ForgotPasswordDTO {
	email: string;
}

export interface ResetPasswordDTO {
	token: string;
	newPassword: string;
}

export interface ResendActivationDTO {
	email: string;
}

export interface MessageResponse {
	message: string;
}

// =========================
// AUTH API (UserController)
// =========================
export const authApi = {
	login: (data: LoginDTO) =>
		api<AuthResponse>('/User/login', {
			method: 'POST',
			body: JSON.stringify(data),
		}),

	refresh: (refreshToken: string) =>
		api<AuthResponse>('/User/refresh', {
			method: 'POST',
			body: JSON.stringify({ refreshToken }),
		}),

	register: (data: RegisterDTO) =>
		api<string>('/User/register', {
			method: 'POST',
			body: JSON.stringify(data),
		}),

	// Wylogowuje bieżącą sesję (unieważnia podany refresh token)
	logout: (refreshToken: string) =>
		api<MessageResponse>('/User/logout', {
			method: 'POST',
			body: JSON.stringify({ refreshToken }),
		}),

	logoutAll: () =>
		api<MessageResponse>('/User/logout/all', {
			method: 'POST',
		}),

	activate: (token: string) =>
		api<string>(`/User/activate?token=${encodeURIComponent(token)}`, {
			method: 'GET',
		}),

	googleLogin: (idToken: string) =>
		api<AuthResponse>('/User/google', {
			method: 'POST',
			body: JSON.stringify({ idToken }),
		}),

	forgotPassword: (data: ForgotPasswordDTO) =>
		api<MessageResponse>('/User/forgot-password', {
			method: 'POST',
			body: JSON.stringify(data),
		}),

	resetPassword: (data: ResetPasswordDTO) =>
		api<MessageResponse>('/User/reset-password', {
			method: 'POST',
			body: JSON.stringify(data),
		}),

	resendActivation: (data: ResendActivationDTO) =>
		api<MessageResponse>('/User/resend-activation', {
			method: 'POST',
			body: JSON.stringify(data),
		}),
};
