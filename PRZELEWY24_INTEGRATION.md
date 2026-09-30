# Integracja Przelewy24 - Dokumentacja

## Przegląd
Kompletna integracja z bramką płatności Przelewy24 obsługująca:
- **BLIK**
- **Szybkie przelewy bankowe**
- **Karty Visa / Mastercard**

## Konfiguracja

### appsettings.json
```json
{
  "Przelewy24": {
	"MerchantId": 123456,
	"PosId": 123456,
	"CrcKey": "twoj-crc-key",
	"ApiKey": "twoj-api-key",
	"BaseUrl": "https://sandbox.przelewy24.pl",
	"Currency": "PLN",
	"Country": "PL",
	"Language": "pl",
	"ReturnUrl": "https://twoja-domena.pl/api/payment/return",
	"StatusUrl": "https://twoja-domena.pl/api/payment/status",
	"PaymentMethods": {
	  "EnableBlik": true,
	  "EnableCards": true,
	  "EnableTransfers": true
	}
  }
}
```

### Środowiska
- **Sandbox**: `https://sandbox.przelewy24.pl`
- **Produkcja**: `https://secure.przelewy24.pl`

## Endpointy API

### 1. Tworzenie płatności
```http
POST /api/payment/create
Content-Type: application/json

{
  "amount": 10000,  // Kwota w groszach (100,00 PLN)
  "description": "Zakup produktu X",
  "email": "klient@example.com",
  "clientName": "Jan Kowalski",
  "phone": "+48123456789",
  "preferredPaymentMethod": 181  // Opcjonalnie: 181 = BLIK
}
```

**Odpowiedź**:
```json
{
  "token": "ABCD1234-TOKEN",
  "redirectUrl": "https://sandbox.przelewy24.pl/trnRequest/ABCD1234-TOKEN",
  "paymentId": 42,
  "sessionId": "user123_1234567890_abc..."
}
```

### 2. Powrót z bramki (GET)
```http
GET /api/payment/return?sessionId={sessionId}&orderId={orderId}
```

### 3. Webhook Przelewy24 (POST)
```http
POST /api/payment/status
Content-Type: application/json

{
  "merchantId": 123456,
  "posId": 123456,
  "sessionId": "...",
  "amount": 10000,
  "currency": "PLN",
  "orderId": 12345,
  "methodId": 181,
  "statement": "...",
  "sign": "..."
}
```

### 4. Pobierz płatność
```http
GET /api/payment/{paymentId}
```

### 5. Lista moich płatności
```http
GET /api/payment/my-payments
```

## Metody płatności

### BLIK
```json
{
  "preferredPaymentMethod": 181
}
```

### Karty płatnicze
```json
{
  "preferredPaymentMethod": 2
}
```

### Przelewy bankowe
```json
{
  "preferredPaymentMethod": 3
}
```

## Przepływ płatności

1. **Inicjacja**: Aplikacja frontendowa wywołuje `POST /api/payment/create`
2. **Przekierowanie**: Użytkownik jest przekierowywany na `redirectUrl` z Przelewy24
3. **Płatność**: Użytkownik wybiera metodę i potwierdza płatność
4. **Powrót**: Przelewy24 przekierowuje na `ReturnUrl`
5. **Weryfikacja**: Przelewy24 wysyła webhook na `StatusUrl`
6. **Potwierdzenie**: System weryfikuje podpis i aktualizuje status płatności

## Bezpieczeństwo

### Podpisy SHA-384
Wszystkie żądania są zabezpieczone podpisem SHA-384:
- **Rejestracja**: `{"sessionId":"{sessionId}","merchantId":{merchantId},"amount":{amount},"currency":"{currency}","crc":"{crcKey}"}`
- **Weryfikacja**: `{"sessionId":"{sessionId}","orderId":{orderId},"amount":{amount},"currency":"{currency}","crc":"{crcKey}"}`
- **Notyfikacja**: `{"sessionId":"{sessionId}","orderId":{orderId},"amount":{amount}","currency":"{currency}","crc":"{crcKey}"}`

### Autentykacja API
Request do Przelewy24 wymaga nagłówka:
```
Authorization: Basic {base64(PosId:ApiKey)}
```

## Statusy płatności
- `Pending` - Oczekująca
- `Completed` - Opłacona
- `Cancelled` - Anulowana
- `Failed` - Nieudana
- `Refunded` - Zwrócona

## Testowanie

### Dane testowe (Sandbox)
- **Merchant ID**: Twój testowy ID
- **POS ID**: Twój testowy ID
- **CRC Key**: Klucz z panelu testowego
- **Karty testowe**: Dostępne w dokumentacji Przelewy24

### Przykładowe numery kart (sandbox)
- **Sukces**: 4444 4444 4444 4441
- **Błąd**: 4444 4444 4444 4442

## Struktura danych

### Entity: Payment
```csharp
public class Payment
{
	public int Id { get; set; }
	public string UserId { get; set; }
	public string SessionId { get; set; }
	public string? Token { get; set; }
   public int Amount { get; set; }
	public string Currency { get; set; }
	public string Description { get; set; }
	public string Email { get; set; }
	public string? ClientName { get; set; }
	public string? Phone { get; set; }
	public PaymentStatus Status { get; set; }
	public PaymentMethod? Method { get; set; }
	public int? P24OrderId { get; set; }
	public string? Statement { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset UpdatedAt { get; set; }
	public DateTimeOffset? PaidAt { get; set; }
	public string? Metadata { get; set; }
}
```

## Logi
Serwis loguje wszystkie kluczowe operacje:
- Tworzenie płatności
- Rejestracja w Przelewy24
- Weryfikacja płatności
- Powiadomienia webhook
- Błędy i ostrzeżenia

## Uwagi produkcyjne
1. ✅ Zmień `BaseUrl` na produkcyjny
2. ✅ Użyj prawdziwych kluczy Merchant/POS/CRC/API
3. ✅ Ustaw właściwy `ReturnUrl` i `StatusUrl` (muszą być HTTPS)
4. ✅ Włącz powiadomienia w panelu Przelewy24
5. ✅ Przetestuj wszystkie metody płatności
6. ✅ Monitoruj logi błędów
7. ✅ Implementuj obsługę zwrotów (refunds)
8. ✅ Dodaj migrację bazy danych przed wdrożeniem

## Linki
- [Dokumentacja Przelewy24](https://developers.przelewy24.pl/)
- [Panel testowy](https://sandbox.przelewy24.pl/)
- [Panel produkcyjny](https://www.przelewy24.pl/)
