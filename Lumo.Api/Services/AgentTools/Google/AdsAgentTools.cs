using Integrations.Google.API.Ads;
using Lumo.Api.Services.AgentUserContext;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace Lumo.Api.Services.AgentTools.Google;

/// <summary>
/// Provider narzędzi Google Ads dla agenta AI
/// </summary>
public sealed class AdsAgentTools : IAgentToolProvider
{
    private readonly IGoogleAdsService _adsService;
    private readonly IAgentUserContextService _userContext;

    public AdsAgentTools(
        IGoogleAdsService adsService,
        IAgentUserContextService userContext)
    {
        _adsService = adsService;
        _userContext = userContext;
    }

    public IEnumerable<AITool> GetTools()
    {
        yield return AIFunctionFactory.Create(
            ListAdsAccountsAsync,
            name: "ListGoogleAdsAccounts",
            description: "Pobiera listę kont reklamowych Google Ads dostępnych dla połączonego konta Google użytkownika.");

        yield return AIFunctionFactory.Create(
            GetAdsCampaignsAsync,
            name: "GetGoogleAdsCampaigns",
            description: "Pobiera listę kampanii wraz z podstawowymi metrykami wydajności (kliknięcia, wyświetlenia, koszt, konwersje) dla wskazanego konta Google Ads.");
    }

    private async Task<string> ListAdsAccountsAsync()
    {
        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var accounts = await _adsService.ListAccessibleAccountsAsync(userIdInt);

        if (accounts.Count == 0)
        {
            return "Brak dostępnych kont Google Ads dla połączonego konta Google.";
        }

        return JsonSerializer.Serialize(accounts, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private async Task<string> GetAdsCampaignsAsync(
        string customerId,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null)
    {
        if (string.IsNullOrWhiteSpace(customerId))
        {
            return "Nie podano identyfikatora konta Google Ads (customerId).";
        }

        var userId = _userContext.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !int.TryParse(userId, out var userIdInt))
        {
            return "Nie udało się ustalić użytkownika.";
        }

        var campaigns = await _adsService.GetCampaignsAsync(
            userIdInt,
            customerId,
            dateFrom,
            dateTo);

        if (campaigns.Count == 0)
        {
            return $"Brak kampanii dla konta Google Ads {customerId} we wskazanym okresie.";
        }

        return JsonSerializer.Serialize(campaigns, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
