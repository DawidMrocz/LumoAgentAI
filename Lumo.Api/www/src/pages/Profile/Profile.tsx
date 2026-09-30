
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import styles from './Profile.module.css';

import { userApi } from '../../api/User.api';
import { paymentApi, type PaymentDetails } from '../../api/payment.api';
import { googleAuthApi } from '../../api/googleAuth.api';
import { metaApi } from '../../api/meta.api';
import { tiktokApi } from '../../api/tiktok.api';
import { getErrorMessage } from '../../utils/apiError';
import { useAuth } from '../../store/Cart/Auth/UseAuth';

interface MetaConnection {
	connected: boolean;
	pageName?: string;
	instagramUsername?: string;
}

interface TikTokConnection {
	connected: boolean;
	username?: string;
	displayName?: string;
}

export default function Profile() {
	const navigate = useNavigate();
	const { refetch } = useAuth();

	const [firstName, setFirstName] = useState('');
	const [lastName, setLastName] = useState('');
	const [email, setEmail] = useState('');

	const [loading, setLoading] = useState(true);
	const [saving, setSaving] = useState(false);

	const [message, setMessage] = useState<string | null>(null);
	const [error, setError] = useState<string | null>(null);

	const [payments, setPayments] = useState<PaymentDetails[] | null>(null);
	const [paymentsInfo, setPaymentsInfo] = useState<string | null>(null);

	// =========================
	// META
	// =========================

	const [metaLoading, setMetaLoading] = useState(false);

	const [metaConnection, setMetaConnection] =
		useState<MetaConnection>({
			connected: false,
		});

	// =========================
	// TIKTOK
	// =========================

	const [tiktokLoading, setTiktokLoading] = useState(false);

	const [tiktokConnection, setTiktokConnection] =
		useState<TikTokConnection>({
			connected: false,
		});

	// =========================
	// LOAD PROFILE + PAYMENTS
	// =========================

	useEffect(() => {
		const load = async () => {
			try {
				const profile = await userApi.me();

				const [first, ...rest] = profile.fullName.split(' ');

				setFirstName(first ?? '');
				setLastName(rest.join(' '));
				setEmail(profile.email);
			} catch {
				setError('Nie udało się pobrać profilu');
			} finally {
				setLoading(false);
			}
		};

		const loadPayments = async () => {
			try {
				const res = await paymentApi.myPayments();

				if (Array.isArray(res)) {
					setPayments(res);
				} else {
					setPaymentsInfo(res.message);
				}
			} catch {
				setPaymentsInfo(
					'Nie udało się pobrać historii płatności'
				);
			}
		};

		load();
		loadPayments();
	}, []);

	// =========================
	// CHECK TIKTOK CONNECTION
	// =========================

	useEffect(() => {
		const loadTikTokConnection = async () => {
			try {
				const connection = await tiktokApi.isConnected();

				if (!connection.connected) {
					setTiktokConnection({
						connected: false,
					});

					return;
				}

				try {
					const userInfo = await tiktokApi.getUserInfo();

					setTiktokConnection({
						connected: true,
						username: userInfo.username,
						displayName: userInfo.displayName,
					});
				} catch {
					setTiktokConnection({
						connected: true,
					});
				}
			} catch {
				setTiktokConnection({
					connected: false,
				});
			}
		};

		loadTikTokConnection();
	}, []);

	const initials =
		`${firstName[0] ?? ''}${lastName[0] ?? ''}`
			.toUpperCase() || '?';

	// =========================
	// SAVE PROFILE
	// =========================

	const handleSave = async (e: React.FormEvent) => {
		e.preventDefault();

		setMessage(null);
		setError(null);
		setSaving(true);

		try {
			await userApi.updateProfile({
				firstName,
				lastName,
			});

			await refetch();

			setMessage('Profil zaktualizowany');
		} catch (err) {
			setError(
				getErrorMessage(
					err,
					'Nie udało się zapisać zmian'
				)
			);
		} finally {
			setSaving(false);
		}
	};

	// =========================
	// META CONNECT
	// =========================

	const handleMetaConnect = async () => {
		setMetaLoading(true);
		setError(null);
		setMessage(null);

		try {
			const response = await metaApi.getLoginUrl();

			if (!response?.url) {
				throw new Error(
					'Meta nie zwróciło adresu logowania.'
				);
			}

			window.location.href = response.url;
		} catch (err) {
			setError(
				getErrorMessage(
					err,
					'Nie udało się rozpocząć integracji z Meta'
				)
			);

			setMetaLoading(false);
		}
	};

	// =========================
	// TIKTOK CONNECT
	// =========================

	const handleTikTokConnect = async () => {
		setTiktokLoading(true);
		setError(null);
		setMessage(null);

		try {
			const url = await tiktokApi.authorize();

			if (!url) {
				throw new Error(
					'TikTok nie zwrócił adresu autoryzacji.'
				);
			}

			window.location.href = url;
		} catch (err) {
			setError(
				getErrorMessage(
					err,
					'Nie udało się rozpocząć integracji z TikTokiem'
				)
			);

			setTiktokLoading(false);
		}
	};

	// =========================
	// TIKTOK DISCONNECT
	// =========================

	const handleTikTokDisconnect = async () => {
		setTiktokLoading(true);
		setError(null);
		setMessage(null);

		try {
			await tiktokApi.disconnect();

			setTiktokConnection({
				connected: false,
			});

			setMessage('TikTok został odłączony.');
		} catch (err) {
			setError(
				getErrorMessage(
					err,
					'Nie udało się odłączyć TikToka'
				)
			);
		} finally {
			setTiktokLoading(false);
		}
	};

	if (loading) {
		return (
			<div className={styles.page}>
				Ładowanie...
			</div>
		);
	}

	return (
		<div className={styles.page}>
			{/* HEADER */}

			<div className={styles.header}>
				<div>
					<h1>👤 Twój profil</h1>

					<p>
						Zarządzaj danymi konta i integracjami
					</p>
				</div>
			</div>

			{/* GRID */}

			<div className={styles.grid}>
				{/* PROFILE */}

				<div className={styles.card}>
					<div className={styles.profileTop}>
						<div className={styles.avatar}>
							<span>{initials}</span>
						</div>

						<div>
							<div className={styles.name}>
								{firstName} {lastName}
							</div>

							<div className={styles.email}>
								{email}
							</div>
						</div>
					</div>

					<form
						className={styles.form}
						onSubmit={handleSave}
					>
						<input
							value={firstName}
							onChange={(e) =>
								setFirstName(e.target.value)
							}
							placeholder="Imię"
							required
						/>

						<input
							value={lastName}
							onChange={(e) =>
								setLastName(e.target.value)
							}
							placeholder="Nazwisko"
							required
						/>

						<input
							value={email}
							disabled
							placeholder="Email"
						/>

						<button
							type="submit"
							className={styles.primaryBtn}
							disabled={saving}
						>
							{saving
								? 'Zapisywanie...'
								: 'Zapisz zmiany'}
						</button>
					</form>

					{error && (
						<p
							style={{
								color: 'var(--color-error)',
								fontSize: 13,
							}}
						>
							{error}
						</p>
					)}

					{message && (
						<p
							style={{
								color: 'var(--color-success)',
								fontSize: 13,
							}}
						>
							{message}
						</p>
					)}
				</div>

				{/* INTEGRATIONS */}

				<div className={styles.card}>
					<h2>🔗 Integracje</h2>

					{/* GOOGLE */}

					<div className={styles.subscriptionBox}>
						<div>
							<div className={styles.planTitle}>
								Google Calendar
							</div>

							<div className={styles.planDesc}>
								Połącz konto Google, aby
								synchronizować kalendarz
							</div>
						</div>

						<button
							className={styles.upgradeBtn}
							onClick={() =>
								(window.location.href =
									googleAuthApi.connectUrl())
							}
						>
							Połącz
						</button>
					</div>

					{/* META */}

					<div className={styles.subscriptionBox}>
						<div>
							<div className={styles.planTitle}>
								Meta
							</div>

							<div className={styles.planDesc}>
								{metaConnection.connected ? (
									<>
										Meta jest połączona
										{metaConnection.pageName
											? ` — ${metaConnection.pageName}`
											: ''}
										{metaConnection.instagramUsername
											? ` / @${metaConnection.instagramUsername}`
											: ''}
									</>
								) : (
									<>
										Połącz Facebook i Instagram,
										aby zarządzać publikacją
									</>
								)}
							</div>
						</div>

						<button
							className={styles.upgradeBtn}
							onClick={handleMetaConnect}
							disabled={metaLoading}
						>
							{metaLoading
								? 'Łączenie...'
								: metaConnection.connected
								? 'Połącz ponownie'
								: 'Połącz'}
						</button>
					</div>

					{/* TIKTOK */}

					<div className={styles.subscriptionBox}>
						<div>
							<div className={styles.planTitle}>
								TikTok
							</div>

							<div className={styles.planDesc}>
								{tiktokConnection.connected ? (
									<>
										TikTok jest połączony
										{tiktokConnection.username
											? ` — @${tiktokConnection.username}`
											: tiktokConnection.displayName
											? ` — ${tiktokConnection.displayName}`
											: ''}
									</>
								) : (
									<>
										Połącz TikTok, aby publikować
										filmy bezpośrednio z Lumo
									</>
								)}
							</div>
						</div>

						<div
							style={{
								display: 'flex',
								gap: 8,
								alignItems: 'center',
							}}
						>
							{tiktokConnection.connected && (
								<button
									className={styles.secondaryBtn}
									onClick={
										handleTikTokDisconnect
									}
									disabled={tiktokLoading}
								>
									{tiktokLoading
										? '...'
										: 'Odłącz'}
								</button>
							)}

							<button
								className={styles.upgradeBtn}
								onClick={handleTikTokConnect}
								disabled={tiktokLoading}
							>
								{tiktokLoading
									? 'Łączenie...'
									: tiktokConnection.connected
									? 'Połącz ponownie'
									: 'Połącz'}
							</button>
						</div>
					</div>
				</div>

				{/* PAYMENTS */}

				<div className={styles.cardWide}>
					<h2>📄 Historia płatności</h2>

					{paymentsInfo && !payments && (
						<p className={styles.planDesc}>
							{paymentsInfo}
						</p>
					)}

					{payments && payments.length === 0 && (
						<p className={styles.planDesc}>
							Brak płatności
						</p>
					)}

					{payments && payments.length > 0 && (
						<>
							<div className={styles.tableHeader}>
								<span>Data</span>
								<span>Kwota</span>
								<span>Status</span>
							</div>

							<div className={styles.table}>
								{payments.map((p) => (
									<div
										key={p.id}
										className={styles.row}
									>
										<div>
											{new Date(
												p.createdAt
											).toLocaleDateString()}
										</div>

										<div>
											{(
												p.amount / 100
											).toFixed(2)}{' '}
											{p.currency}
										</div>

										<div
											className={`${styles.badge} ${
												p.status?.toLowerCase() ===
													'success' ||
												p.status?.toLowerCase() ===
													'paid'
													? styles.paid
													: p.status?.toLowerCase() ===
													  'failed'
													? styles.failed
													: styles.pending
											}`}
										>
											{p.status}
										</div>
									</div>
								))}
							</div>
						</>
					)}
				</div>

				{/* ACTIONS */}

				<div className={styles.card}>
					<h2>⚙️ Ustawienia konta</h2>

					<button
						className={styles.secondaryBtn}
						onClick={() =>
							navigate('/change-password')
						}
					>
						Zmień hasło
					</button>

					<button
						className={styles.secondaryBtn}
						onClick={() =>
							navigate('/payments')
						}
					>
						Nowa płatność
					</button>
				</div>
			</div>
		</div>
	);
}
