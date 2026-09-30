import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import styles from './ConfirmEmail.module.css';
import { authApi } from '../../api/Auth.api';

export default function ConfirmEmail() {
	const [searchParams] = useSearchParams();
	const token = searchParams.get('token');

	const [status, setStatus] = useState<'loading' | 'success' | 'error'>(
		'loading'
	);

	useEffect(() => {
		const confirm = async () => {
			if (!token) {
				setStatus('error');
				return;
			}

			try {
				await authApi.activate(token);
				setStatus('success');
			} catch {
				setStatus('error');
			}
		};

		confirm();
	}, [token]);

	return (
		<div className={styles.container}>
			<div className={styles.form}>
				<img src="/logo.png" alt="Logo" className={styles.logo} />

				{status === 'loading' && (
					<p className={`${styles.status} ${styles.loading}`}>
						Aktywacja konta...
					</p>
				)}

				{status === 'success' && (
					<>
						<h2>Konto aktywowane ✅</h2>
						<p className={styles.success}>
							Możesz się teraz zalogować.
						</p>

						<div className={styles.backToLogin}>
							<Link to="/login" className={styles.link}>
								Powrót do logowania
							</Link>
						</div>
					</>
				)}

				{status === 'error' && (
					<>
						<h2>Błąd aktywacji ❌</h2>
						<p className={styles.error}>
							Link jest niepoprawny lub wygasł.
						</p>

						<div className={styles.backToLogin}>
							<Link to="/login" className={styles.link}>
								Powrót do logowania
							</Link>
						</div>
					</>
				)}
			</div>
		</div>
	);
}
