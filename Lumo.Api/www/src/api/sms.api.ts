import { api } from './client';

// Matches Lumo.Api SmsController (route: /api/Sms).

export interface SendSmsRequest {
	to: string;
	message: string;
	sender?: string;
	scheduledAt?: string;
	isFlash?: boolean;
	variables?: Record<string, string>;
}

export interface SendSmsResponse {
	messageId: string;
	to: string;
	parts: number;
	points: number;
	status: string;
	sentAt: string;
	success: boolean;
	error?: string;
}

export interface SendBulkSmsRequest {
	to: string[];
	message: string;
	sender?: string;
	scheduledAt?: string;
	variables?: Record<string, string>;
}

export interface SendBulkSmsResponse {
	successCount: number;
	failureCount: number;
	results: SendSmsResponse[];
	totalPoints: number;
}

export interface SmsTemplate {
	name: string;
	content: string;
	description?: string;
	isActive: boolean;
}

export interface SmsStatistics {
	[key: string]: unknown;
}

export const smsApi = {
	send: (data: SendSmsRequest) => api<SendSmsResponse>('/Sms/send', { method: 'POST', body: JSON.stringify(data) }),

	sendBulk: (data: SendBulkSmsRequest) =>
		api<SendBulkSmsResponse>('/Sms/send-bulk', { method: 'POST', body: JSON.stringify(data) }),

	sendFromTemplate: (templateName: string, to: string, variables: Record<string, string>) =>
		api<SendSmsResponse>(
			`/Sms/send-template?templateName=${encodeURIComponent(templateName)}&to=${encodeURIComponent(to)}`,
			{ method: 'POST', body: JSON.stringify(variables) }
		),

	getMessageStatus: (messageId: string) =>
		api<{ messageId: string; status: string }>(`/Sms/status/${encodeURIComponent(messageId)}`, { method: 'GET' }),

	getMyMessages: (skip = 0, take = 50) =>
		api<Record<string, unknown>[]>(`/Sms/my-messages?skip=${skip}&take=${take}`, { method: 'GET' }),

	getStatistics: (from?: string, to?: string) =>
		api<SmsStatistics>(`/Sms/statistics?${from ? `from=${encodeURIComponent(from)}&` : ''}${to ? `to=${encodeURIComponent(to)}` : ''}`, {
			method: 'GET',
		}),

	getTemplates: () => api<SmsTemplate[]>('/Sms/templates', { method: 'GET' }),
};
