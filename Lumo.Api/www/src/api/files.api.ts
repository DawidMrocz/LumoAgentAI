import { api } from './client';

// Matches Lumo.Api FileController (route: /api/files).

export interface FileUploadResponse {
	id: string;
	url: string;
	fileName: string;
	contentType: string;
	size: number;
}

export interface VectorStoreResponse {
	vectorStoreId: string;
	fileName: string;
}

export interface VectorStoreSearchResponse {
	vectorStoreId: string;
	query: string;
	result: string;
}

export const filesApi = {
	// POST /api/files/upload — zapisuje plik lokalnie na serwerze.
	upload: (file: File) => {
		const fd = new FormData();
		fd.append('file', file);
		return api<FileUploadResponse>('/files/upload', { method: 'POST', body: fd });
	},

	// GET /api/files/{id} — link do pobrania/podglądu pliku.
	downloadUrl: (id: string) => `${import.meta.env.VITE_API_BASE_URL || '/api'}/files/${id}`,

	// POST /api/files/vector-store — upload pliku do OpenAI Vector Store.
	createVectorStore: (file: File) => {
		const fd = new FormData();
		fd.append('file', file);
		return api<VectorStoreResponse>('/files/vector-store', { method: 'POST', body: fd });
	},

	// GET /api/files/search — wyszukiwanie w Vector Store.
	search: (vectorStoreId: string, query: string) =>
		api<VectorStoreSearchResponse>(
			`/files/search?vectorStoreId=${encodeURIComponent(vectorStoreId)}&query=${encodeURIComponent(query)}`,
			{ method: 'GET' }
		),
};
