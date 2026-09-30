export function showApiError(message: string) {
  try {
    window.dispatchEvent(new CustomEvent('api-error', { detail: { message } }));
  } catch {
    // ignore
  }
}

// api() from ./client throws { status, payload } (not an Error instance) on non-2xx responses.
// payload is either a JSON error body (e.g. { message } from the backend) or plain text.
export function getErrorMessage(err: unknown, fallback = 'Wystąpił nieoczekiwany błąd'): string {
  if (err && typeof err === 'object' && 'payload' in err) {
    const payload = (err as { payload: unknown }).payload;
    if (typeof payload === 'string' && payload.trim()) return payload;
    if (payload && typeof payload === 'object' && 'message' in payload) {
      const message = (payload as { message?: unknown }).message;
      if (typeof message === 'string' && message.trim()) return message;
    }
  }
  if (err instanceof Error && err.message) return err.message;
  return fallback;
}
