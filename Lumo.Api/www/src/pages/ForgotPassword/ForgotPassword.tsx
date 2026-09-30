import { useState } from 'react';
import { Link } from 'react-router-dom';
import styles from './ForgotPassword.module.css';
import { authApi } from '../../api/Auth.api';
import { getErrorMessage } from '../../utils/apiError';

export default function ForgotPassword() {
	const [email, setEmail] = useState('');
	const [success, setSuccess] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const [loading, setLoading] = useState(false);

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);
		setSuccess(false);

		try {
			setLoading(true);

			await authApi.forgotPassword({ email });

			setSuccess(true);
		} catch (err: unknown) {
			setError(getErrorMessage(err, 'Nie udało się wysłać emaila resetującego'));
		} finally {
			setLoading(false);
		}
	};

	if (success) {
		return (
			<div className={styles.container}>
				<div className={styles.form}>
					<img src="/logo.png" alt="Logo" className={styles.logo} />
					<h2>Sprawdź swoją skrzynkę 📧</h2>
					<p>Wysłaliśmy link do resetu hasła na Twój adres email.</p>

					<div className={styles.backToLogin}>
						<Link to="/login" className={styles.link}>
							Powrót do logowania
						</Link>
					</div>
				</div>
			</div>
		);
	}

	return (
		<div className={styles.container}>
			<form onSubmit={handleSubmit} className={styles.form}>
				<img src="/logo.png" alt="Logo" className={styles.logo} />

				<h1>Reset hasła</h1>

				<p className={styles.infoText}>
					Wprowadź swój adres email, a wyślemy Ci link do zmiany
					hasła.
				</p>

				{error && <p className={styles.error}>{error}</p>}

				<input
					type="email"
					placeholder="Twój email"
					value={email}
					onChange={(e) => setEmail(e.target.value)}
					required
					className={styles.input}
					disabled={loading}
				/>

				<button
					type="submit"
					className={styles.button}
					disabled={loading}
				>
					{loading ? 'Proszę czekać...' : 'Wyślij link resetujący'}
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
