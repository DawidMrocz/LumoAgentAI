import { Outlet } from 'react-router-dom';
import { AuthProvider } from '../store/Cart/Auth/AuthProvider';
import { useAuth } from '../store/Cart/Auth/UseAuth';
import ThemeToggle from '../components/ThemeToggle';
import styles from './AuthLayout.module.css';

function AuthLayoutContent() {
	const { isLoggedIn } = useAuth();

	return (
		<>
			{!isLoggedIn && (
				<div className={styles.floatingToggle}>
					<ThemeToggle />
				</div>
			)}
			<Outlet />
		</>
	);
}

export default function AuthLayout() {
	return (
		<AuthProvider>
			<AuthLayoutContent />
		</AuthProvider>
	);
}
