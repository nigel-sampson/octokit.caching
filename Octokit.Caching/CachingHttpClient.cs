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

        public async Task<IResponse<T>> Send<T>(IRequest request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
                return await httpClient.Send<T>(request, cancellationToken);

            var key = request.Endpoint.ToString();

            var response = cache.Get<IResponse<T>>(key);

            if (response == null)
            {
                response = await httpClient.Send<T>(request, cancellationToken);

                cache.Set(key, response);

                return response;
            }

            if (!String.IsNullOrEmpty(response.ApiInfo.Etag))
            {
                request.Headers["If-None-Match"] = String.Format("{0}", response.ApiInfo.Etag);

                var conditionalResponse = await httpClient.Send<T>(request, cancellationToken);

                if (conditionalResponse.StatusCode == HttpStatusCode.NotModified)
                    return response;

                cache.Set(key, conditionalResponse);

                return conditionalResponse;
            }

            response = await httpClient.Send<T>(request, cancellationToken);

            cache.Set(key, response);

            return response;
        }
    }
}
