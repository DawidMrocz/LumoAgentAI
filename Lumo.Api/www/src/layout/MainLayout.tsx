import Navbar from '../components/Navbar';
import { Outlet } from 'react-router-dom';
import styles from './MainLayout.module.css';
import Footer from '../components/Footer';
import ApiErrorToast from '../components/ApiErrorToast';

export default function MainLayout() {
	return (
		<div className={styles.app}>
			<Navbar />

			<main className={styles.main}>
				<Outlet />
			</main>

	<Footer />

	{/* global API error toast */}
	<ApiErrorToast />
		</div>
	);
}
