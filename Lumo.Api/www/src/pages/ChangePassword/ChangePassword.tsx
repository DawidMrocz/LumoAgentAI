import { useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './ChangePassword.module.css';
import { userApi } from '../../api/User.api';
import { getErrorMessage } from '../../utils/apiError';

export default function ChangePassword() {
	const navigate = useNavigate();

	const [oldPassword, setOldPassword] = useState('');
	const [password, setPassword] = useState('');
	const [confirmPassword, setConfirmPassword] = useState('');
	const [loading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
		const { name, value } = e.target;

		if (name === 'oldPassword') setOldPassword(value);
		if (name === 'password') setPassword(value);
		if (name === 'confirmPassword') setConfirmPassword(value);
	};

	const handleSubmit = async (e: FormEvent) => {
		e.preventDefault();
		setError(null);

		if (password !== confirmPassword) {
			setError('Hasła nie są zgodne');
			return;
		}

		if (password.length < 8) {
			setError('Hasło musi mieć minimum 8 znaków');
			return;
		}

		try {
			setLoading(true);

			await userApi.changePassword({
				currentPassword: oldPassword,
				newPassword: password,
			});

			alert('Hasło zostało zmienione!');
			navigate('/profile');
		} catch (err: unknown) {
			setError(getErrorMessage(err, 'Nie udało się zmienić hasła'));
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className={styles.container}>
			<h1>Zmiana hasła</h1>

			<form onSubmit={handleSubmit} className={styles.changePasswordForm}>
				<input
					type="password"
					name="oldPassword"
					placeholder="Stare hasło"
					value={oldPassword}
					onChange={handleChange}
					required
				/>

				<input
					type="password"
					name="password"
					placeholder="Nowe hasło"
					value={password}
					onChange={handleChange}
					required
				/>

				<input
					type="password"
					name="confirmPassword"
					placeholder="Powtórz nowe hasło"
					value={confirmPassword}
					onChange={handleChange}
					required
				/>

				{error && <div className={styles.error}>{error}</div>}

				<ul className={styles.passwordRules}>
					<li className={password.length >= 8 ? styles.valid : ''}>
						Minimum 8 znaków
					</li>

					<li className={/[A-Z]/.test(password) ? styles.valid : ''}>
						Przynajmniej jedna wielka litera
					</li>

					<li className={/[0-9]/.test(password) ? styles.valid : ''}>
						Przynajmniej jedna cyfra
					</li>

					<li
						className={
							/[^A-Za-z0-9]/.test(password) ? styles.valid : ''
						}
					>
						Przynajmniej jeden znak specjalny
					</li>

					<li
						className={
							password &&
							confirmPassword &&
							password === confirmPassword
								? styles.valid
								: ''
						}
					>
						Hasła są zgodne
					</li>
				</ul>

				<div className={styles.actions}>
					<button
						type="button"
						className={styles.cancelBtn}
						onClick={() => navigate('/profile')}
						disabled={loading}
					>
						Anuluj
					</button>

					<button
						type="submit"
						className={styles.changePasswordBtn}
						disabled={loading}
					>
						{loading ? 'Zmiana...' : 'Zmień hasło'}
					</button>
				</div>
			</form>
		</div>
	);
}
