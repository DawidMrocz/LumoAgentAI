import { useState } from 'react';
import { paymentApi } from '../../api/payment.api';
import { useAuth } from '../../store/Cart/Auth/UseAuth';
import { getErrorMessage } from '../../utils/apiError';
import styles from './Payments.module.css';

export default function Payments() {
	const { user } = useAuth();

	const [amountPln, setAmountPln] = useState(0);
	const [description, setDescription] = useState('');
	const [email, setEmail] = useState(user?.email ?? '');
	const [clientName, setClientName] = useState(user?.fullName ?? '');
	const [loading, setLoading] = useState(false);
	const [message, setMessage] = useState<string | null>(null);

	const handleCreate = async (e: React.FormEvent) => {
		e.preventDefault();
		setLoading(true);
		setMessage(null);
		try {
			const res = await paymentApi.create({
				amount: Math.round(amountPln * 100),
				description: description || undefined,
				email,
				clientName: clientName || undefined,
			});

			if (res?.redirectUrl) {
				window.location.href = res.redirectUrl;
			} else {
				setMessage('Płatność utworzona: ' + res.sessionId);
			}
		} catch (err) {
			setMessage('Błąd: ' + getErrorMessage(err));
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className={styles.container}>
			<h2>Nowa płatność</h2>
			<form className={styles.form} onSubmit={handleCreate}>
				<input
					type="number"
					min={1}
					step={0.01}
					value={amountPln || ''}
					onChange={(e) => setAmountPln(Number(e.target.value))}
					placeholder="Kwota (PLN)"
					required
				/>
				<input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Opis (opcjonalnie)" />
				<input type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" required />
				<input value={clientName} onChange={(e) => setClientName(e.target.value)} placeholder="Imię i nazwisko (opcjonalnie)" />

				<button type="submit" disabled={loading || amountPln <= 0}>
					{loading ? 'Tworzenie...' : 'Zapłać'}
				</button>
			</form>
			{message && <div className={styles.message}>{message}</div>}
		</div>
	);
}

