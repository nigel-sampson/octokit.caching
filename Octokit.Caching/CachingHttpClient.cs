using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Octokit.Internal;

namespace Octokit.Caching
{
    public class CachingHttpClient : IHttpClient
    {
        private readonly IHttpClient _httpClient;
        private readonly ICache _cache;

        public CachingHttpClient(IHttpClient httpClient, ICache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<IResponse> Send(IRequest request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
                return await _httpClient.Send(request, cancellationToken);

            var key = request.Endpoint.ToString();

            var response = await _cache.GetAsync<IResponse>(key);

            if (response == null)
            {
                response = await _httpClient.Send(request, cancellationToken);

                await _cache.SetAsync(key, response);

                return response;
            }

            if (!string.IsNullOrEmpty(response.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = response.ApiInfo.Etag;

                var conditionalResponse = await _httpClient.Send(request, cancellationToken);

                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return response;

                await _cache.SetAsync(key, conditionalResponse);

                return conditionalResponse;
            }

            response = await _httpClient.Send(request, cancellationToken);

            await _cache.SetAsync(key, response);

            return response;
        }

        public void SetRequestTimeout(TimeSpan timeout)
        {
            _httpClient.SetRequestTimeout(timeout);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}