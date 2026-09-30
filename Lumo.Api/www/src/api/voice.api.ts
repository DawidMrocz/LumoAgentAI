import { api } from './client';

// Matches Lumo.Api VoiceController (route: /api/voice).

export interface SpeechToTextResponse {
	fileName: string;
	size: number;
	text: string;
}

export interface CodeInterpreterResult {
	success: boolean;
	message: string;
	containerId?: string;
	fileId?: string;
	fileName?: string;
	hasFile: boolean;
}

export const voiceApi = {
	// POST /api/voice/stt — rozpoznawanie mowy z pliku audio.
	speechToText: (file: File) => {
		const fd = new FormData();
		fd.append('file', file);
		return api<SpeechToTextResponse>('/voice/stt', { method: 'POST', body: fd });
	},

	// POST /api/voice/code — wykonanie promptu przez Code Interpreter.
	executeCode: (prompt: string) =>
		api<CodeInterpreterResult>('/voice/code', {
			method: 'POST',
			body: JSON.stringify({ prompt }),
		}),

	// POST /api/voice/code2 — jak wyżej, ale zwraca od razu wygenerowany plik (Blob) zamiast JSON.
	executeCodeAndDownload: (prompt: string) =>
		api<Blob>('/voice/code2', {
			method: 'POST',
			body: JSON.stringify({ prompt }),
		}),

	// GET /api/voice/code/file — pobranie pliku wygenerowanego przez Code Interpreter.
	downloadCodeFile: (containerId: string, fileId: string, fileName?: string) =>
		api<Blob>(
			`/voice/code/file?containerId=${encodeURIComponent(containerId)}&fileId=${encodeURIComponent(fileId)}${
				fileName ? `&fileName=${encodeURIComponent(fileName)}` : ''
			}`,
			{ method: 'GET' }
		),

	// POST /api/voice — nagranie audio -> STT -> agent AI -> TTS, zwraca odpowiedź jako audio/wav.
	converse: (file: File) => {
		const fd = new FormData();
		fd.append('file', file);
		return api<Blob>('/voice', { method: 'POST', body: fd });
	},
};
