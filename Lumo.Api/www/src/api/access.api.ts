import { api } from './client';
import type { ProfileDTO } from './User.api';

// Matches Lumo.Api AccessController (route: /api/Access).
export const accessApi = {
	me: () => api<ProfileDTO>('/Access/me'),
};
