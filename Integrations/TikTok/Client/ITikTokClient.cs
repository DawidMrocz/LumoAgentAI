using System;
using System.Collections.Generic;
using System.Text;

namespace Integrations.TikTok.Client
{
    public interface ITikTokClient
    {
        Task<T?> GetAsync<T>(
            string path,
            int userId,
            CancellationToken cancellationToken = default);

        Task<T?> PostAsync<T>(
            string path,
            int userId,
            object body,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PostRawAsync(
            string path,
            int userId,
            HttpContent content,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> PutRawAsync(
            string path,
            int userId,
            HttpContent content,
            CancellationToken cancellationToken = default);
    }
}
