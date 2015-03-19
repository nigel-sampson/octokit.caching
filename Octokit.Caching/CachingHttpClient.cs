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
        private readonly IHttpClient httpClient;
        private readonly ICache cache;

        public CachingHttpClient(IHttpClient httpClient, ICache cache)
        {
            this.httpClient = httpClient;
            this.cache = cache;
        }

        public async Task<IResponse> Send(IRequest request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
                return await httpClient.Send(request, cancellationToken);

            var key = request.Endpoint.ToString();

            var response = await cache.GetAsync<IResponse>(key);

            if (response == null)
            {
                response = await httpClient.Send(request, cancellationToken);

                await cache.SetAsync(key, response);

                return response;
            }

            if (!String.IsNullOrEmpty(response.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = response.ApiInfo.Etag;

                var conditionalResponse = await httpClient.Send(request, cancellationToken);

                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return response;

                await cache.SetAsync(key, conditionalResponse);

                return conditionalResponse;
            }

            response = await httpClient.Send(request, cancellationToken);

            await cache.SetAsync(key, response);

            return response;
        }
    }
}
