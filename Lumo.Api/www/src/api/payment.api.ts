import { api } from './client';

// Matches Lumo.Api PaymentController (route: /api/Payment).

export interface CreatePaymentRequest {
	/** Kwota w groszach (np. 10000 = 100.00 PLN) */
	amount: number;
	description?: string;
	email: string;
	clientName?: string;
	phone?: string;
	/** 181 = BLIK, 2 = Karty, 3 = Przelewy */
	preferredPaymentMethod?: number;
}

export interface CreatePaymentResponse {
	token: string;
	redirectUrl: string;
	paymentId: number;
	sessionId: string;
}

export interface PaymentDetails {
	id: number;
	sessionId: string;
	amount: number;
	currency: string;
	description?: string;
	status: string;
	method?: string;
	createdAt: string;
	paidAt?: string;
}

export const paymentApi = {
	create: (data: CreatePaymentRequest) =>
		api<CreatePaymentResponse>('/Payment/create', { method: 'POST', body: JSON.stringify(data) }),

	get: (paymentId: number) => api<PaymentDetails>(`/Payment/${paymentId}`, { method: 'GET' }),

	myPayments: () => api<PaymentDetails[] | { message: string }>('/Payment/my-payments', { method: 'GET' }),
};
