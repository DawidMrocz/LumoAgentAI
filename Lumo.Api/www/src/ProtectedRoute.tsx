import { type ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './store/Cart/Auth/UseAuth';

interface Props {
	children: ReactNode;
}

export default function ProtectedRoute({ children }: Props) {
	const { isLoggedIn, loading } = useAuth();

	if (loading) return <p>Ładowanie...</p>;

	if (!isLoggedIn) return <Navigate to="/login" replace />;

	return <>{children}</>;
}
