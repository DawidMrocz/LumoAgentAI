import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { metaApi } from '../../api/meta.api';
import { getErrorMessage } from '../../utils/apiError';
import styles from './MetaCallback.module.css';

interface MetaPage {
	id?: string;
	name?: string;
	access_token?: string;
	accessToken?: string;

	instagram_business_account?: {
		id?: string;
		username?: string;
		name?: string;
	};

	instagramBusinessAccount?: {
		id?: string;
		username?: string;
		name?: string;
	};
}

export default function MetaCallback() {
	const navigate = useNavigate();
	const [searchParams] = useSearchParams();

	const [loading, setLoading] = useState(true);
	const [saving, setSaving] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const [pages, setPages] = useState<MetaPage[]>([]);
	const [accessToken, setAccessToken] = useState<string | null>(null);

	useEffect(() => {
		const processCallback = async () => {
			const code = searchParams.get('code');
			const oauthError = searchParams.get('error');
			const oauthErrorDescription = searchParams.get('error_description');

			if (oauthError) {
				setError(
					oauthErrorDescription ||
						'Autoryzacja Meta została anulowana.'
				);
				setLoading(false);
				return;
			}

			if (!code) {
				setError('Brak kodu autoryzacyjnego Meta.');
				setLoading(false);
				return;
			}

			try {
				const callbackResponse = await metaApi.callback(code);

				if (!callbackResponse.accessToken) {
					throw new Error('Meta nie zwróciło tokenu dostępu.');
				}

				const token = callbackResponse.accessToken;

				setAccessToken(token);

				const response = await metaApi.pages(token);

				const normalizedPages: MetaPage[] = Array.isArray(response)
					? response
					: [];

				if (normalizedPages.length === 0) {
					setError(
						'Meta nie zwróciło żadnej strony Facebook, do której masz dostęp. Upewnij się, że podczas logowania przyznałeś odpowiednie uprawnienia.'
					);
					setLoading(false);
					return;
				}

				setPages(normalizedPages);

				// Jeżeli użytkownik ma tylko jedną stronę,
				// możemy przejść od razu do zapisu.
				if (normalizedPages.length === 1) {
					await savePage(normalizedPages[0], token);
					return;
				}

				setLoading(false);
			} catch (err) {
				setError(
					getErrorMessage(err, 'Nie udało się połączyć konta Meta.')
				);
				setLoading(false);
			}
		};

		processCallback();
	}, [searchParams]);

	const savePage = async (page: MetaPage, token: string) => {
		const pageId = page.id;

		if (!pageId) {
			throw new Error('Wybrana strona Meta nie posiada ID.');
		}

		setSaving(true);
		setError(null);

		try {
			await metaApi.saveInstagramAccount(pageId, undefined, token);

			navigate('/profile?meta=connected', {
				replace: true,
			});
		} catch (err) {
			setError(
				getErrorMessage(err, 'Nie udało się zapisać konta Instagram.')
			);
		} finally {
			setSaving(false);
		}
	};

	const handleSelectPage = async (page: MetaPage) => {
		if (!accessToken) {
			setError('Brak tokenu Meta. Uruchom integrację ponownie.');
			return;
		}

		await savePage(page, accessToken);
	};

	if (loading) {
		return (
			<div className={styles.page}>
				<div className={styles.card}>
					<div className={styles.loader}>
						<span>🔵</span>
					</div>

					<h1>Łączenie z Meta...</h1>

					<p>
						Pobieramy informacje o Twoich stronach Facebook i
						Instagram.
					</p>
				</div>
			</div>
		);
	}

	if (error && pages.length === 0) {
		return (
			<div className={styles.page}>
				<div className={styles.card}>
					<div className={styles.icon}>❌</div>

					<h1>Nie udało się połączyć Meta</h1>

					<p className={styles.error}>{error}</p>

					<button
						className={styles.primaryButton}
						onClick={() => navigate('/profile')}
					>
						Wróć do profilu
					</button>
				</div>
			</div>
		);
	}

	return (
		<div className={styles.page}>
			<div className={styles.card}>
				<h1>📱 Wybierz stronę</h1>

				<p>
					Wybierz stronę Facebook, z którą chcesz połączyć konto
					Instagram.
				</p>

				{error && <div className={styles.errorBox}>{error}</div>}

				<div className={styles.pages}>
					{pages.map((page, index) => {
						const instagram =
							page.instagram_business_account ??
							page.instagramBusinessAccount;

						return (
							<button
								key={page.id ?? index}
								className={styles.pageOption}
								onClick={() => handleSelectPage(page)}
								disabled={saving}
							>
								<div>
									<strong>
										{page.name || `Strona ${index + 1}`}
									</strong>

									{instagram?.username && (
										<span>
											Instagram: @{instagram.username}
										</span>
									)}

									{instagram?.name && !instagram.username && (
										<span>Instagram: {instagram.name}</span>
									)}
								</div>

								<span>→</span>
							</button>
						);
					})}
				</div>

				{saving && (
					<p className={styles.saving}>Zapisuję integrację...</p>
				)}

				<button
					className={styles.cancelButton}
					onClick={() => navigate('/profile')}
					disabled={saving}
				>
					Anuluj
				</button>
			</div>
		</div>
	);
}
