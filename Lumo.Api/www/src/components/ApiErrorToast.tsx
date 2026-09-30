import { useEffect, useState } from 'react';
import styles from './ApiErrorToast.module.css';

export default function ApiErrorToast() {
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    const handler = (e: any) => {
      const msg = e?.detail?.message ?? String(e?.detail ?? 'Unknown error');
      setMessage(msg);
      // auto-hide
      setTimeout(() => setMessage(null), 5000);
    };

    window.addEventListener('api-error', handler as EventListener);

    return () => window.removeEventListener('api-error', handler as EventListener);
  }, []);

  if (!message) return null;

  return (
    <div className={styles.toast} role="alert">
      <div className={styles.content}>{message}</div>
    </div>
  );
}
