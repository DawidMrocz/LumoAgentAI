using Integrations.Google.API.Ads.Models;
using System.ComponentModel;

namespace Integrations.Google.API.Ads
{
    public interface IGoogleAdsService
    {
        [Description("Pobiera listę kont reklamowych Google Ads dostępnych dla połączonego konta Google użytkownika.")]
        Task<IReadOnlyList<AdsAccount>> ListAccessibleAccountsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera listę kampanii wraz z podstawowymi metrykami wydajności (kliknięcia, wyświetlenia, koszt, konwersje) dla wskazanego konta Google Ads.")]
        Task<IReadOnlyList<AdsCampaign>> GetCampaignsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Id konta Google Ads (customer id), np. 1234567890. Można pobrać z ListAccessibleAccountsAsync.")]
                string customerId,

                [Description("Początek zakresu dat (włącznie). Jeśli null, przyjmowane jest 30 dni wstecz.")]
                DateOnly? dateFrom = null,

                [Description("Koniec zakresu dat (włącznie). Jeśli null, przyjmowana jest dzisiejsza data.")]
                DateOnly? dateTo = null,

                CancellationToken cancellationToken = default
            );
    }
}
