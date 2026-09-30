import { useState } from 'react';
import { documentsApi, type RagSearchResult } from '../../api/documents.api';
import { getErrorMessage } from '../../utils/apiError';
import styles from './Documents.module.css';

export default function Documents() {
	const [file, setFile] = useState<File | null>(null);
	const [collection, setCollection] = useState('default');
	const [ingesting, setIngesting] = useState(false);
	const [message, setMessage] = useState<string | null>(null);

	const [query, setQuery] = useState('');
	const [searching, setSearching] = useState(false);
	const [results, setResults] = useState<RagSearchResult[] | null>(null);

	const handleIngest = async () => {
		if (!file) return;
		setIngesting(true);
		setMessage(null);
		try {
			const res = await documentsApi.ingest(file, collection);
			setMessage(`Zaindeksowano "${res.fileName}" (${res.inserted}/${res.chunks} fragmentów)`);
			setFile(null);
		} catch (err) {
			setMessage('Błąd: ' + getErrorMessage(err));
		} finally {
			setIngesting(false);
		}
	};

	const handleSearch = async () => {
		if (!query.trim()) return;
		setSearching(true);
		setResults(null);
		try {
			const res = await documentsApi.search(query, collection);
			setResults(res);
		} catch (err) {
			setMessage('Błąd wyszukiwania: ' + getErrorMessage(err));
		} finally {
			setSearching(false);
		}
	};

	return (
		<div className={styles.container}>
			<h2>Dokumenty</h2>

			<div className={styles.form}>
				<label>Kolekcja</label>
				<input value={collection} onChange={(e) => setCollection(e.target.value)} placeholder="Nazwa kolekcji" />

				<label>Dodaj dokument</label>
				<input type="file" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
				<div className={styles.buttons}>
					<button onClick={handleIngest} disabled={!file || ingesting}>
						{ingesting ? 'Wgrywanie...' : 'Wgraj i zaindeksuj'}
					</button>
				</div>
			</div>

			{message && <div className={styles.message}>{message}</div>}

			<div className={styles.form} style={{ marginTop: 24 }}>
				<label>Przeszukaj dokumenty</label>
				<div className={styles.buttons}>
					<input
						value={query}
						onChange={(e) => setQuery(e.target.value)}
						onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
						placeholder="Wpisz zapytanie..."
						style={{ flex: 1 }}
					/>
					<button onClick={handleSearch} disabled={searching}>
						{searching ? '...' : 'Szukaj'}
					</button>
				</div>
			</div>

			{results && results.length === 0 && <div className={styles.message}>Brak wyników</div>}

			{results && results.length > 0 && (
				<div className={styles.form}>
					{results.map((r, i) => (
						<div key={`${r.documentId}-${r.chunkIndex}-${i}`} className={styles.message}>
							<strong>{r.fileName}</strong> (dopasowanie: {(r.score * 100).toFixed(1)}%)
							<p>{r.text}</p>
						</div>
					))}
				</div>
			)}
		</div>
	);
}

