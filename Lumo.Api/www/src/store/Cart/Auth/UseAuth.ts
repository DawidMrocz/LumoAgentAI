import { useContext } from 'react';

import type { AuthContextType } from './AuthTypes';
import { AuthContext } from './AuthContext';

export const useAuth = (): AuthContextType => {
	const ctx = useContext(AuthContext);
	if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
	return ctx;
};
