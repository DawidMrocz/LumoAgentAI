const API_BASE = import.meta.env.VITE_API_BASE_URL || '/api';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE';

async function request<T>(path: string, method: HttpMethod = 'GET', body?: any, asFormData = false): Promise<T> {
  const url = `${API_BASE}${path.startsWith('/') ? path : `/${path}`}`;

  const headers: Record<string, string> = {};

  const token = localStorage.getItem('authToken');
  if (token) headers['Authorization'] = `Bearer ${token}`;

  let fetchOptions: RequestInit = { method, headers };

  if (body != null) {
    if (asFormData && body instanceof FormData) {
      // Let browser set multipart headers
      fetchOptions = { ...fetchOptions, body };
    } else {
      headers['Content-Type'] = 'application/json';
      // api() callers already pass a JSON string (body: JSON.stringify(data)); avoid double-encoding it.
      const payload = typeof body === 'string' ? body : JSON.stringify(body);
      fetchOptions = { ...fetchOptions, body: payload };
    }
  }

  const res = await fetch(url, fetchOptions);

  const contentType = res.headers.get('content-type') || '';

  if (!res.ok) {
    let payload: any = null;
    if (contentType.includes('application/json')) payload = await res.json();
    else payload = await res.text();
    throw { status: res.status, payload };
  }

  if (contentType.includes('application/json')) return res.json();
  if (contentType.startsWith('text/')) return (await res.text()) as unknown as T;

  // For binary responses, return the raw Response as any
  return (await res.blob()) as unknown as T;
}

export function setAuthToken(token: string | null) {
  if (token) localStorage.setItem('authToken', token);
  else localStorage.removeItem('authToken');
}

export function setRefreshToken(token: string | null) {
  if (token) localStorage.setItem('refreshToken', token);
  else localStorage.removeItem('refreshToken');
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresIn?: string;
}

// Persists tokens from a login/refresh/google-login response and (re)schedules auto-refresh.
export function applyAuthTokens(res: AuthTokens) {
  setAuthToken(res.accessToken);
  setRefreshToken(res.refreshToken);
  if (res.accessTokenExpiresIn) {
    const expiresAt = new Date(res.accessTokenExpiresIn).getTime();
    if (!Number.isNaN(expiresAt)) setAccessTokenExpiry(expiresAt);
  }
  scheduleAutoRefresh();
}

export function getRefreshToken(): string | null {
  return localStorage.getItem('refreshToken');
}

export function setAccessTokenExpiry(expiresAtMs: number | null) {
  if (expiresAtMs) localStorage.setItem('accessTokenExpiry', String(expiresAtMs));
  else localStorage.removeItem('accessTokenExpiry');
}

export function getAccessTokenExpiry(): number | null {
  const v = localStorage.getItem('accessTokenExpiry');
  if (!v) return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

let refreshTimer: number | null = null;

export function clearAutoRefresh() {
  if (refreshTimer) {
    clearTimeout(refreshTimer);
    refreshTimer = null;
  }
}

export function scheduleAutoRefresh() {
  clearAutoRefresh();

  const expiry = getAccessTokenExpiry();
  if (!expiry) return;

  const now = Date.now();
  // refresh 60 seconds before expiry
  const refreshAt = expiry - 60_000;
  const delay = Math.max(0, refreshAt - now);

  refreshTimer = window.setTimeout(async () => {
    try {
      const ok = await tryRefreshTokens();
      if (ok) scheduleAutoRefresh();
    } catch {
      // ignore
    }
  }, delay);
}

// Compatibility wrapper used by existing api files (api(path, options))
export async function api<T = any>(path: string, options?: RequestInit & { body?: any }): Promise<T> {
  const method = (options && options.method ? (options.method as HttpMethod) : 'GET') as HttpMethod;

  const body = options && options.body ? options.body : undefined;

  const asFormData = body instanceof FormData;

  return request<T>(path, method, body, asFormData);
}

export default { request };

let sessionExpiredHandler: (() => void) | null = null;

export function setSessionExpiredHandler(cb: () => void | Promise<void> | null) {
  sessionExpiredHandler = cb as () => void;
}

// wrap original request to trigger session handler on 401
async function requestWithSession<T>(path: string, method: HttpMethod = 'GET', body?: any, asFormData = false): Promise<T> {
  try {
    return await request<T>(path, method, body, asFormData);
  } catch (err: any) {
    // If unauthorized, try to refresh tokens and retry once
    if (err && err.status === 401) {
      // attempt refresh
      try {
        const refreshed = await tryRefreshTokens();
        if (refreshed) {
          // retry original request once
          return await request<T>(path, method, body, asFormData);
        }
      } catch {
        // fallthrough to session handler
      }

      if (sessionExpiredHandler) {
        try {
          sessionExpiredHandler();
        } catch {
          /* ignore */
        }
      }
    }

    throw err;
  }
}

// override exported request to include session handling
export { requestWithSession as request };

// ---------------------
// Token refresh logic
// ---------------------
let refreshPromise: Promise<boolean> | null = null;

async function tryRefreshTokens(): Promise<boolean> {
  if (refreshPromise) return refreshPromise;

  const rt = getRefreshToken();
  if (!rt) return false;

  refreshPromise = (async () => {
    try {
      // call refresh endpoint - expect { accessToken, refreshToken }
      const res: any = await request<any>('/User/refresh', 'POST', { refreshToken: rt });

      if (res && res.accessToken) {
        setAuthToken(res.accessToken ?? null);
        if (res.refreshToken) setRefreshToken(res.refreshToken);
        // accessTokenExpiresIn is an absolute ISO datetime returned by the API
        if (res.accessTokenExpiresIn) {
          const expiresAt = new Date(res.accessTokenExpiresIn).getTime();
          if (!Number.isNaN(expiresAt)) setAccessTokenExpiry(expiresAt);
        }
        return true;
      }

      return false;
    } catch {
      return false;
    } finally {
      refreshPromise = null;
    }
  })();

  return refreshPromise;
}
