// Matches Lumo.Api GoogleAuthController (route: /api/auth/google).
import { api } from './client';

export const googleAuthApi = {
	// Pobiera URL ekranu zgody Google dla ZALOGOWANEGO uzytkownika.
	// Endpoint wymaga tokena, dlatego nie da sie tu uzyc zwyklego przekierowania
	// przegladarki - najpierw fetch z Authorization, potem window.location.href.
	getConnectUrl: () => api<{ url: string }>('/auth/google/connect'),

	// Wygodny skrot: pobiera URL i przekierowuje przegladarke do Google.
	connect: async () => {
		const { url } = await googleAuthApi.getConnectUrl();
		window.location.href = url;
	},
};
