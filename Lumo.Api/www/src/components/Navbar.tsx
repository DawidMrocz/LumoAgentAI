import { NavLink, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import styles from './Navbar.module.css';

import { useAuth } from '../store/Cart/Auth/UseAuth';
import ThemeToggle from './ThemeToggle';

const NAV_LINKS = [
	{ to: '/', label: 'Start' },
	{ to: '/chat', label: 'AI Chat' },
	{ to: '/documents', label: 'Dokumenty' },
	{ to: '/payments', label: 'Płatności' },
];

export default function Navbar() {
	const { logout, user } = useAuth();
	const navigate = useNavigate();

	const [menuOpen, setMenuOpen] = useState(false);
	const [drawerOpen, setDrawerOpen] = useState(false);

	const initials = `${user?.fullName?.[0] ?? ''}${user?.fullName?.split(' ')[1]?.[0] ?? ''}`.toUpperCase() || '?';

	const handleLogout = async () => {
		setMenuOpen(false);
		await logout();
	};

	const closeDrawer = () => setDrawerOpen(false);

	return (
		<>
			<nav className={styles.navbar}>
				{/* LEFT */}
				<div className={styles.left}>
					<div className={styles.logo} onClick={() => navigate('/')}>
						Lumo
					</div>

					<div className={styles.links}>
						{NAV_LINKS.map((link) => (
							<NavLink
								key={link.to}
								to={link.to}
								end={link.to === '/'}
								className={({ isActive }) => (isActive ? styles.active : styles.link)}
							>
								{link.label}
							</NavLink>
						))}
					</div>
				</div>

				{/* RIGHT */}
				<div className={styles.right}>
					<ThemeToggle />

					<button className={styles.burger} onClick={() => setDrawerOpen(true)}>
						☰
					</button>

					<div className={styles.profileWrapper}>
						<button className={styles.profileButton} onClick={() => setMenuOpen((p) => !p)}>
							<div className={styles.avatar}>{initials}</div>

							<div className={styles.profileInfo}>
								<span className={styles.name}>{user?.fullName}</span>
							</div>

							<span>⌄</span>
						</button>

						{menuOpen && (
							<div className={styles.dropdown}>
								<div
									className={styles.dropdownItem}
									onClick={() => {
										navigate('/profile');
										setMenuOpen(false);
									}}
								>
									Profil
								</div>

								<div className={styles.logout} onClick={handleLogout}>
									Wyloguj
								</div>
							</div>
						)}
					</div>
				</div>
			</nav>

			{/* DRAWER (mobile) */}
			{drawerOpen && <div className={styles.backdrop} onClick={closeDrawer} />}

			<div className={`${styles.drawer} ${drawerOpen ? styles.drawerOpen : ''}`}>
				{NAV_LINKS.map((link) => (
					<NavLink key={link.to} to={link.to} end={link.to === '/'} className={styles.drawerLink} onClick={closeDrawer}>
						{link.label}
					</NavLink>
				))}
				<NavLink to="/profile" className={styles.drawerLink} onClick={closeDrawer}>
					Profil
				</NavLink>
			</div>
		</>
	);
}
