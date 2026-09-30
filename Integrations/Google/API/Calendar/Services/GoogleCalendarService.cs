using Integrations.Google.API.Calendar.Models;
using Integrations.Google.Factory;
using System.ComponentModel;

namespace Integrations.Google.API.Calendar.Services;

internal sealed partial class GoogleCalendarService : IGoogleCalendarService
{
    private readonly IGoogleServiceFactory _googleServiceFactory;

    public GoogleCalendarService(IGoogleServiceFactory googleServiceFactory)
    {
        _googleServiceFactory = googleServiceFactory;
    }
}