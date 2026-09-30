import { api } from './client';

// Matches Lumo.Api ImageController (route: /api/image).
export const imageApi = {
	// Zwraca wygenerowany obraz PNG jako Blob.
	generate: (prompt: string, size?: string) =>
		api<Blob>('/image/generate', {
			method: 'POST',
			body: JSON.stringify({ prompt, size }),
		}),
};
