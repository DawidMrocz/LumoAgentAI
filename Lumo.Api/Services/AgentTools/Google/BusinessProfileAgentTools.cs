using Integrations.Google.API.BusinessProfile;
using Lumo.Api.Services.AgentUserContext;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Lumo.Api.Services.AgentTools.Google;

/// <summary>
/// Provider narzędzi Google Business Profile dla agenta AI
/// </summary>
public sealed class BusinessProfileAgentTools : IAgentToolProvider
{
    private readonly IGoogleBusinessProfileService _businessProfileService;
    private readonly IAgentUserContextService _userContext;

    public BusinessProfileAgentTools(
        IGoogleBusinessProfileService businessProfileService,
        IAgentUserContextService userContext)
    {
        _businessProfileService = businessProfileService;
        _userContext = userContext;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(
            ListBusinessAccountsAsync,
            name: "ListGoogleBusinessAccounts",
            description: "Pobiera listę kont Google Business Profile (wizytówek firmowych) dostępnych dla połączonego konta Google użytkownika.");

        yield return AIFunctionFactory.Create(
            ListBusinessLocationsAsync,
            name: "ListGoogleBusinessLocations",
            description: "Pobiera listę lokalizacji (placówek) należących do wskazanego konta Google Business Profile.");

        yield return AIFunctionFactory.Create(
            GetBusinessLocationPerformanceAsync,
            name: "GetGoogleBusinessLocationPerformance",
            description: "Pobiera podstawowe metryki wydajności (wyświetlenia, kliknięcia telefonu, kliknięcia strony www, prośby o wskazówki dojazdu) dla wskazanej lokalizacji Google Business Profile.");
    }

    private async Task<string> ListBusinessAccountsAsync()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var accounts = await _businessProfileService.ListAccountsAsync(userIdInt);

        if (accounts.Count == 0)
        {
            return "Brak dostępnych kont Google Business Profile dla połączonego konta Google.";
        }

        return JsonSerializer.Serialize(accounts, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> ListBusinessLocationsAsync(string accountName)
    {
        if (string.IsNullOrWhiteSpace(accountName))
        {
            return "Nie podano nazwy konta Google Business Profile (accountName, np. accounts/123456789).";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var locations = await _businessProfileService.ListLocationsAsync(userIdInt, accountName);

        if (locations.Count == 0)
        {
            return $"Brak lokalizacji dla konta Google Business Profile {accountName}.";
        }

        return JsonSerializer.Serialize(locations, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> GetBusinessLocationPerformanceAsync(
        string locationName,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null)
    {
        if (string.IsNullOrWhiteSpace(locationName))
        {
            return "Nie podano nazwy lokalizacji Google Business Profile (locationName, np. locations/123456789).";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var metrics = await _businessProfileService.GetLocationPerformanceAsync(
            userIdInt,
            locationName,
            dateFrom,
            dateTo);

        if (metrics.Count == 0)
        {
            return $"Brak danych o wydajności dla lokalizacji {locationName} we wskazanym okresie.";
        }

        return JsonSerializer.Serialize(metrics, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
