import { api } from './client';

// Matches Lumo.Api DocumentsController (route: /api/Documents).

export interface RagSearchResult {
	text: string;
	fileName: string;
	documentId: string;
	chunkIndex: number;
	score: number;
}

export interface IngestionResult {
	documentId: string;
	fileName: string;
	chunks: number;
	inserted: number;
	failed: number;
}

export const documentsApi = {
	search: (query: string, collectionName: string, limit = 5) =>
		api<RagSearchResult[]>(
			`/Documents/search?query=${encodeURIComponent(query)}&collectionName=${encodeURIComponent(collectionName)}&limit=${limit}`,
			{ method: 'GET' }
		),

	ingest: (file: File, collectionName: string) => {
		const fd = new FormData();
		fd.append('file', file);
		fd.append('collectionName', collectionName);
		return api<IngestionResult>('/Documents/ingest', { method: 'POST', body: fd });
	},
};
