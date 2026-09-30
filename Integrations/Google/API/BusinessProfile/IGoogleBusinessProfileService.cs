using Integrations.Google.API.BusinessProfile.Models;
using System.ComponentModel;

namespace Integrations.Google.API.BusinessProfile
{
    public interface IGoogleBusinessProfileService
    {
        [Description("Pobiera listę kont Google Business Profile (wizytówek firmowych) dostępnych dla połączonego konta Google użytkownika.")]
        Task<IReadOnlyList<BusinessAccount>> ListAccountsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera listę lokalizacji (placówek) należących do wskazanego konta Google Business Profile.")]
        Task<IReadOnlyList<BusinessLocation>> ListLocationsAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Nazwa zasobu konta, np. accounts/123456789. Można pobrać z ListAccountsAsync.")]
                string accountName,

                CancellationToken cancellationToken = default
            );

        [Description("Pobiera podstawowe metryki wydajności (wyświetlenia, kliknięcia telefonu, kliknięcia strony www, prośby o wskazówki dojazdu) dla wskazanej lokalizacji Google Business Profile.")]
        Task<IReadOnlyList<LocationMetricPoint>> GetLocationPerformanceAsync
            (
                [Description("Id użytkownika Lumo.")]
                int userId,

                [Description("Nazwa zasobu lokalizacji, np. locations/123456789. Można pobrać z ListLocationsAsync.")]
                string locationName,

                [Description("Początek zakresu dat (włącznie). Jeśli null, przyjmowane jest 30 dni wstecz.")]
                DateOnly? dateFrom = null,

                [Description("Koniec zakresu dat (włącznie). Jeśli null, przyjmowana jest dzisiejsza data.")]
                DateOnly? dateTo = null,

                CancellationToken cancellationToken = default
            );
    }
}
