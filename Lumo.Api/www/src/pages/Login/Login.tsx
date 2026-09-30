import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';
import styles from './Login.module.css';
import { useAuth } from '../../store/Cart/Auth/UseAuth';
import { authApi } from '../../api/Auth.api';
import type { CredentialResponse } from '@react-oauth/google';
import { applyAuthTokens } from '../../api/client';
import { getErrorMessage } from '../../utils/apiError';

export default function Login() {
	const [email, setEmail] = useState('');
	const [password, setPassword] = useState('');
	const [error, setError] = useState<string | null>(null);
	const [loading, setLoading] = useState(false);

	const navigate = useNavigate();
	const { refetch } = useAuth();

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);
		setLoading(true);

		try {
			const res = await authApi.login({ email, password });
			applyAuthTokens(res);

			const user = await refetch();
			if (user) navigate('/', { replace: true });
		} catch (err) {
			setError(getErrorMessage(err, 'Nieprawidłowy email lub hasło'));
		} finally {
			setLoading(false);
		}
	};

	const handleGoogleSuccess = async (credentialResponse: CredentialResponse) => {
		try {
			setError(null);

			if (!credentialResponse.credential) {
				throw new Error('Brak tokena Google');
			}

			const res = await authApi.googleLogin(credentialResponse.credential);
			applyAuthTokens(res);

			const user = await refetch();
			if (user) navigate('/', { replace: true });
		} catch (err) {
			setError(getErrorMessage(err, 'Logowanie Google nie powiodło się'));
		}
	};

	return (
		<div className={styles.container}>
			<form onSubmit={handleSubmit} className={styles.form}>
				{/* Logo */}
				<img src="/logo.png" alt="Logo" className={styles.logo} />

				<h1>Logowanie</h1>

				{error && <p className={styles.error}>{error}</p>}

				<input
					type="email"
					placeholder="Email"
					value={email}
					onChange={(e) => setEmail(e.target.value)}
					required
					disabled={loading}
				/>

				<input
					type="password"
					placeholder="Hasło"
					value={password}
					onChange={(e) => setPassword(e.target.value)}
					required
					disabled={loading}
				/>

				<button type="submit" disabled={loading}>
					{loading ? 'Proszę czekać...' : 'Zaloguj'}
				</button>

				<div className={styles.links}>
					<Link to="/forgot-password" className={styles.link}>
						Zapomniałeś hasła?
					</Link>
				</div>

				<div className={styles.divider}>lub</div>

				<GoogleLogin onSuccess={handleGoogleSuccess} onError={() => setError('Logowanie Google nie powiodło się')} />

				<div className={styles.registerSection}>
					Nie masz konta?{' '}
					<Link to="/register" className={styles.link}>
						Zarejestruj się
					</Link>
				</div>
			</form>
		</div>
	);
}
