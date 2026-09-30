import { useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import styles from './ResetPassword.module.css';
import { authApi } from '../../api/Auth.api';
import { getErrorMessage } from '../../utils/apiError';

export default function ResetPassword() {
	const [searchParams] = useSearchParams();
	const token = searchParams.get('token');
	const navigate = useNavigate();

	const [password, setPassword] = useState('');
	const [confirmPassword, setConfirmPassword] = useState('');
	const [error, setError] = useState<string | null>(null);
	const [success, setSuccess] = useState<string | null>(null);
	const [loading, setLoading] = useState(false);

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);
		setSuccess(null);

		if (!token) {
			setError('Brak tokena resetującego.');
			return;
		}

		if (password !== confirmPassword) {
			setError('Hasła nie są zgodne.');
			return;
		}

		try {
			setLoading(true);
			await authApi.resetPassword({
				token,
				newPassword: password,
			});

			setSuccess('Hasło zostało zmienione pomyślnie!');

			setTimeout(() => navigate('/login'), 2000);
		} catch (err: unknown) {
			setError(getErrorMessage(err, 'Nie udało się zresetować hasła'));
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className={styles.container}>
			<form onSubmit={handleSubmit} className={styles.form}>
				<img src="/logo.png" alt="Logo" className={styles.logo} />

				<h1>Resetowanie hasła</h1>

				{error && <p className={styles.error}>{error}</p>}
				{success && <p className={styles.success}>{success}</p>}

				<input
					type="password"
					placeholder="Nowe hasło"
					value={password}
					onChange={(e) => setPassword(e.target.value)}
					required
					minLength={8}
					className={styles.input}
					disabled={loading}
				/>

				<input
					type="password"
					placeholder="Powtórz nowe hasło"
					value={confirmPassword}
					onChange={(e) => setConfirmPassword(e.target.value)}
					required
					minLength={8}
					className={styles.input}
					disabled={loading}
				/>

				<button
					type="submit"
					className={styles.button}
					disabled={loading}
				>
					{loading ? 'Proszę czekać...' : 'Ustaw nowe hasło'}
				</button>

				<div className={styles.backToLogin}>
					<Link to="/login" className={styles.link}>
						Powrót do logowania
					</Link>
				</div>
			</form>
		</div>
	);
}
