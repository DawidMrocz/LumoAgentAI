import { useRef, useState } from 'react';
import { chatApi, type ChatAttachment } from '../../api/chat.api';
import { filesApi } from '../../api/files.api';
import { getErrorMessage } from '../../utils/apiError';
import styles from './Chat.module.css';

interface Message {
	from: 'user' | 'ai' | 'error';
	text: string;
	attachment?: ChatAttachment;
}

export default function Chat() {
	const [messages, setMessages] = useState<Message[]>([]);
	const [input, setInput] = useState('');
	const [pendingAttachment, setPendingAttachment] = useState<ChatAttachment | null>(null);
	const [loading, setLoading] = useState(false);
	const [uploading, setUploading] = useState(false);
	const fileInputRef = useRef<HTMLInputElement>(null);

	const handleFileSelect = async (file: File | undefined) => {
		if (!file) return;
		setUploading(true);
		try {
			const uploaded = await filesApi.upload(file);
			setPendingAttachment({
				id: uploaded.id,
				url: uploaded.url,
				fileName: uploaded.fileName,
				contentType: uploaded.contentType,
				size: uploaded.size,
			});
		} catch (err) {
			setMessages((m) => [...m, { from: 'error', text: 'Nie udało się wgrać pliku: ' + getErrorMessage(err) }]);
		} finally {
			setUploading(false);
			if (fileInputRef.current) fileInputRef.current.value = '';
		}
	};

	const send = async () => {
		if (!input.trim() && !pendingAttachment) return;

		const text = input;
		const attachment = pendingAttachment ?? undefined;

		setMessages((m) => [...m, { from: 'user', text, attachment }]);
		setInput('');
		setPendingAttachment(null);
		setLoading(true);

		try {
			const res = await chatApi.generate({ text, attachment });
			setMessages((m) => [...m, { from: 'ai', text: res }]);
		} catch (err) {
			setMessages((m) => [...m, { from: 'error', text: getErrorMessage(err) }]);
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className={styles.container}>
			<h2>AI Chat</h2>
			<div className={styles.messages}>
				{messages.map((m, i) => (
					<div key={i} className={styles.message}>
						<strong>{m.from === 'user' ? 'Ty' : m.from === 'ai' ? 'AI' : 'Błąd'}:</strong> {m.text}
						{m.attachment && (
							<div>
								📎{' '}
								<a href={m.attachment.url} target="_blank" rel="noreferrer">
									{m.attachment.fileName}
								</a>
							</div>
						)}
					</div>
				))}
			</div>

			{pendingAttachment && (
				<div className={styles.message}>
					📎 {pendingAttachment.fileName}{' '}
					<button onClick={() => setPendingAttachment(null)} disabled={loading}>
						✕
					</button>
				</div>
			)}

			<div className={styles.form}>
				<input
					value={input}
					onChange={(e) => setInput(e.target.value)}
					onKeyDown={(e) => e.key === 'Enter' && send()}
					placeholder="Napisz wiadomość..."
					disabled={loading}
				/>
				<input
					ref={fileInputRef}
					type="file"
					style={{ display: 'none' }}
					onChange={(e) => handleFileSelect(e.target.files?.[0])}
				/>
				<button onClick={() => fileInputRef.current?.click()} disabled={loading || uploading}>
					{uploading ? '...' : '📎'}
				</button>
				<button onClick={send} disabled={loading}>
					{loading ? '...' : 'Wyślij'}
				</button>
			</div>
		</div>
	);
}

