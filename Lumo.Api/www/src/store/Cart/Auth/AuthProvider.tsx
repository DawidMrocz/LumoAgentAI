import { useState, useEffect, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

import { AuthContext } from './AuthContext';
import { authApi } from '../../../api/Auth.api';
import { userApi } from '../../../api/User.api';
import {
	setSessionExpiredHandler,
	setAuthToken,
	setRefreshToken,
	getRefreshToken,
	scheduleAutoRefresh,
	clearAutoRefresh,
} from '../../../api/client';

import type { UserDTO } from './AuthTypes';

interface Props {
	children: ReactNode;
}

export const AuthProvider = ({ children }: Props) => {
	const [loading, setLoading] = useState(true);
	const [isLoggedIn, setIsLoggedIn] = useState(false);
	const [user, setUser] = useState<UserDTO | null>(null);

	const navigate = useNavigate();

	// =========================
	// LOGOUT
	// =========================
	const logout = async (callBackend = true) => {
		setLoading(true);

		try {
			const refreshToken = getRefreshToken();
			if (callBackend && refreshToken) {
				await authApi.logout(refreshToken);
			}
		} catch {
			/* empty */
		}

		clearAutoRefresh();
		setUser(null);
		setIsLoggedIn(false);

		setAuthToken(null);
		setRefreshToken(null);
		setLoading(false);

		navigate('/login', { replace: true });
	};

	// =========================
	// REFRESH SESSION
	// =========================
	const refetch = async () => {
		setLoading(true);

		try {
			const me = await userApi.me();

			setUser(me);
			setIsLoggedIn(true);

			return me;
		} catch {
			setUser(null);
			setIsLoggedIn(false);

			return null;
		} finally {
			setLoading(false);
		}
	};

	// =========================
	// SESSION EXPIRE HANDLER
	// =========================
	useEffect(() => {
		setSessionExpiredHandler(() => {
			logout(false);
		});

		// schedule auto refresh on startup if expiry present
		scheduleAutoRefresh();
	}, []);

	useEffect(() => {
		refetch();
	}, []);

	return (
		<AuthContext.Provider
			value={{
				isLoggedIn,
				loading,
				user,

				refetch,
				logout,
			}}
		>
			{children}
		</AuthContext.Provider>
	);
};
