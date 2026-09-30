import { api } from './client';

// Matches Lumo.Api ChatController (route: /api/chat).

export interface ChatAttachment {
	id: string;
	url: string;
	fileName: string;
	contentType: string;
	size: number;
}

export interface ChatMessageRequest {
	text: string;
	attachment?: ChatAttachment;
}

export interface ToolApprovalRequest {
	approved: boolean;
	toolId?: string;
}

export const chatApi = {
	generate: (data: ChatMessageRequest) =>
		api<string>('/chat/generate', { method: 'POST', body: JSON.stringify(data) }),

	approve: (data: ToolApprovalRequest) =>
		api<string>('/chat/approval', { method: 'POST', body: JSON.stringify(data) }),
};
