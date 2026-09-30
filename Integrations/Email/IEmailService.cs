using Integrations.Email.Models;
using System.ComponentModel;

namespace Integrations.Email
{
    public interface IEmailService
    {
        [Description("Wysyła e-mail HTML przez skonfigurowany serwer SMTP, z opcjonalnymi odbiorcami DW (Cc), UDW (Bcc) i załącznikami.")]
        Task<SendEmailResponse> SendEmailAsync
            (
                [Description("Dane wiadomości: odbiorcy, temat, treść HTML, opcjonalnie Cc/Bcc i załączniki.")]
                SendEmailRequest request,

                [Description("Id użytkownika Lumo powiązanego z wysyłką (opcjonalne, do logowania historii).")]
                int? userId = null,

                CancellationToken cancellationToken = default
            );

        [Description("Wysyła e-mail na podstawie szablonu HTML zapisanego w bazie danych, podstawiając zmienne w formacie {{nazwa}}.")]
        Task<SendEmailResponse> SendTemplatedEmailAsync
            (
                [Description("Nazwa szablonu, odbiorcy oraz słownik zmiennych podstawianych w temacie i treści szablonu.")]
                SendTemplatedEmailRequest request,

                [Description("Id użytkownika Lumo powiązanego z wysyłką (opcjonalne, do logowania historii).")]
                int? userId = null,

                CancellationToken cancellationToken = default
            );

        [Description("Tworzy nowy szablon e-mail HTML ze zmiennymi w formacie {{nazwa}}.")]
        Task<EmailTemplateInfo> CreateTemplateAsync
            (
                [Description("Unikalna nazwa, temat i treść HTML nowego szablonu.")]
                CreateEmailTemplateRequest request,

                CancellationToken cancellationToken = default
            );

        [Description("Aktualizuje istniejący szablon e-mail HTML.")]
        Task<EmailTemplateInfo> UpdateTemplateAsync
            (
                [Description("Nazwa istniejącego szablonu do aktualizacji.")]
                string name,

                [Description("Nowe dane szablonu: temat, treść HTML, opis, aktywność.")]
                UpdateEmailTemplateRequest request,

                CancellationToken cancellationToken = default
            );

        [Description("Usuwa szablon e-mail o wskazanej nazwie.")]
        Task DeleteTemplateAsync
            (
                [Description("Nazwa szablonu do usunięcia.")]
                string name,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera pojedynczy szablon e-mail na podstawie nazwy.")]
        Task<EmailTemplateInfo?> GetTemplateAsync
            (
                [Description("Nazwa szablonu.")]
                string name,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera listę dostępnych szablonów e-mail.")]
        Task<IReadOnlyList<EmailTemplateInfo>> GetTemplatesAsync
            (
                [Description("Jeśli true, zwraca tylko aktywne szablony. Domyślnie true.")]
                bool onlyActive = true,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera historię wysłanych e-maili dla wskazanego użytkownika.")]
        Task<IReadOnlyList<EmailMessageInfo>> GetUserMessagesAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Liczba pominiętych wpisów (paginacja).")]
                int skip = 0,

                [Description("Maksymalna liczba wyników. Domyślnie 50.")]
                int take = 50,

                CancellationToken cancellationToken = default
            );
    }
}
