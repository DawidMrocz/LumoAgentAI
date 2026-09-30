import { useNavigate } from 'react-router-dom';
import styles from './Home.module.css';
import { useAuth } from '../../store/Cart/Auth/UseAuth';

const QUICK_LINKS = [
	{ to: '/chat', icon: '💬', title: 'AI Chat', desc: 'Porozmawiaj z asystentem AI Lumo' },
	{ to: '/documents', icon: '📄', title: 'Dokumenty', desc: 'Wgraj i przeszukaj swoje dokumenty' },
	{ to: '/payments', icon: '💳', title: 'Płatności', desc: 'Zarządzaj płatnościami' },
	{ to: '/profile', icon: '👤', title: 'Profil', desc: 'Dane konta i ustawienia' },
];

export default function Home() {
	const navigate = useNavigate();
	const { user } = useAuth();

	const firstName = user?.fullName?.split(' ')[0];

	return (
		<div className={styles.container}>
			{/* HEADER */}
			<div className={styles.welcomeHeader}>
				<h1 className={styles.welcomeTitle}>👋 Witaj{firstName ? `, ${firstName}` : ''}!</h1>
				<p className={styles.welcomeSubtitle}>Twój asystent AI, dokumenty i płatności w jednym miejscu</p>
			</div>

			{/* QUICK ACTIONS */}
			<div className={styles.cardBlock}>
				<h3>Szybki start</h3>

				<div className={styles.stats}>
					{QUICK_LINKS.map((link) => (
						<div key={link.to} className={styles.card} role="button" onClick={() => navigate(link.to)} style={{ cursor: 'pointer' }}>
							<h3>
								{link.icon} {link.title}
							</h3>
							<p style={{ fontSize: 13, fontWeight: 400, color: 'var(--text-secondary)' }}>{link.desc}</p>
						</div>
					))}
				</div>
			</div>
		</div>
	);
}
