import styles from './Footer.module.css';
import { Link } from 'react-router-dom';

export default function Footer() {
	return (
		<footer className={styles.footer}>
			<div className={styles.left}>
				<span className={styles.brand}>TwojaApp</span>
				<span className={styles.copy}>
					© {new Date().getFullYear()}
				</span>
			</div>

			<div className={styles.center}>
				<Link to="/about" className={styles.link}>
					O nas
				</Link>
				<Link to="/faq" className={styles.link}>
					FAQ
				</Link>
				<Link to="/privacy" className={styles.link}>
					Prywatność
				</Link>
				<Link to="/terms" className={styles.link}>
					Regulamin
				</Link>
				<Link to="/contact" className={styles.link}>
					Kontakt
				</Link>
			</div>

			<div className={styles.right}>
				<a href="https://instagram.com" className={styles.icon}>
					📸
				</a>
				<a href="https://facebook.com" className={styles.icon}>
					📘
				</a>
			</div>
		</footer>
	);
}
