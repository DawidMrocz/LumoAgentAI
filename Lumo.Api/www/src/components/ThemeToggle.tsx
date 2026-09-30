import { useTheme } from '../store/Theme/useTheme';
import styles from './ThemeToggle.module.css';

export default function ThemeToggle() {
	const { theme, toggleTheme } = useTheme();

	return (
		<button
			type="button"
			className={styles.toggle}
			onClick={toggleTheme}
			aria-label={theme === 'dark' ? 'Przełącz na jasny motyw' : 'Przełącz na ciemny motyw'}
			title={theme === 'dark' ? 'Jasny motyw' : 'Ciemny motyw'}
		>
			{theme === 'dark' ? '☀️' : '🌙'}
		</button>
	);
}
