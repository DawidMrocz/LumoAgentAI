using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lumo.Api.Services.Notifications;

public interface IFcmService
{
    Task SendDataMessageToUserAsync(string userOid, IDictionary<string, string> data, CancellationToken cancellationToken = default);
}
