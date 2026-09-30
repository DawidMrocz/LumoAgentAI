import { useState } from 'react';
import { Link } from 'react-router-dom';
import styles from './Register.module.css';
import { authApi } from '../../api/Auth.api';
import { getErrorMessage } from '../../utils/apiError';

export default function Register() {
	const [form, setForm] = useState({
		email: '',
		password: '',
		confirmPassword: '',
		firstName: '',
		lastName: '',
	});

	const [error, setError] = useState<string | null>(null);
	const [success, setSuccess] = useState(false);
	const [loading, setLoading] = useState(false);

	const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		setForm({ ...form, [e.target.name]: e.target.value });
	};

	const handleSubmit = async (e: React.FormEvent) => {
		e.preventDefault();
		setError(null);

		if (form.password !== form.confirmPassword) {
			setError('Hasła nie są zgodne');
			return;
		}

		try {
			setLoading(true);

			await authApi.register({
				email: form.email,
				password: form.password,
				firstName: form.firstName,
				lastName: form.lastName,
			});

			setSuccess(true);
		} catch (err) {
			setError(getErrorMessage(err, 'Nie udało się utworzyć konta'));
		} finally {
			setLoading(false);
		}
	};

	if (success) {
		return (
			<div className={styles.container}>
				<div className={styles.form}>
					<img src="/logo.png" alt="Logo" className={styles.logo} />
					<h2>Konto zostało utworzone 🎉</h2>
					<p>Sprawdź email i aktywuj konto.</p>

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

				<h1>Rejestracja</h1>

				{error && <p className={styles.error}>{error}</p>}

				<input
					name="firstName"
					placeholder="Imię"
					value={form.firstName}
					onChange={handleChange}
					required
					className={styles.input}
					disabled={loading}
				/>

				<input
					name="lastName"
					placeholder="Nazwisko"
					value={form.lastName}
					onChange={handleChange}
					required
					className={styles.input}
					disabled={loading}
				/>

				<input
					type="email"
					name="email"
					placeholder="Email"
					value={form.email}
					onChange={handleChange}
					required
					className={styles.input}
					disabled={loading}
				/>

				<input
					type="password"
					name="password"
					placeholder="Hasło"
					value={form.password}
					onChange={handleChange}
					required
					className={styles.input}
					disabled={loading}
				/>

				<input
					type="password"
					name="confirmPassword"
					placeholder="Powtórz hasło"
					value={form.confirmPassword}
					onChange={handleChange}
					required
					className={styles.input}
					disabled={loading}
				/>

				<ul className={styles.passwordRules}>
					<li className={form.password.length >= 8 ? styles.valid : ''}>Minimum 8 znaków</li>

					<li className={/[A-Z]/.test(form.password) ? styles.valid : ''}>Przynajmniej jedna wielka litera</li>

					<li className={/[0-9]/.test(form.password) ? styles.valid : ''}>Przynajmniej jedna cyfra</li>

					<li className={/[^A-Za-z0-9]/.test(form.password) ? styles.valid : ''}>Przynajmniej jeden znak specjalny</li>

					<li className={form.password && form.confirmPassword && form.password === form.confirmPassword ? styles.valid : ''}>
						Hasła są zgodne
					</li>
				</ul>

				<button type="submit" className={styles.button} disabled={loading}>
					{loading ? 'Proszę czekać...' : 'Zarejestruj się'}
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
