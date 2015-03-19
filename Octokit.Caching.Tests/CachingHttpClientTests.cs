using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Octokit.Internal;

namespace Octokit.Caching.Tests
{
    [TestClass]
    public class CachingHttpClientTests
    {
        [TestMethod]
        public async Task SendSendsNonGetRequests()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();
            
            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Post, Endpoint = new Uri("test", UriKind.Relative) };

            await cachingClient.Send(request, CancellationToken.None);

            httpClient.Received().Send(request, CancellationToken.None).IgnoreAwait();
            cache.DidNotReceive().GetAsync<IResponse>("test").IgnoreAwait();
        }

        [TestMethod]
        public async Task SendSendsAndSetsMissingResponses()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var response = Substitute.For<IResponse>();
            httpClient.Send(request, CancellationToken.None).Returns(Task.FromResult(response));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult((IResponse)null));

            var actualReponse = await cachingClient.Send(request, CancellationToken.None);

            cache.Received().SetAsync("test", response).IgnoreAwait();

            Assert.AreEqual(response, actualReponse);
        }

        [TestMethod]
        public async Task SendSendsAndSetsReponseWithNoEtag()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var cachedResponse = Substitute.For<IResponse>();
            cachedResponse.ApiInfo.Returns(new ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), String.Empty, null));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult(cachedResponse));

            var response = Substitute.For<IResponse>();

            httpClient.Send(request, CancellationToken.None).Returns(Task.FromResult(response));

            var actualReponse = await cachingClient.Send(request, CancellationToken.None);

            cache.Received().SetAsync("test", response).IgnoreAwait();

            Assert.AreEqual(response, actualReponse);
        }

        [TestMethod]
        public async Task SendSendsConditionalRequestWithEtag()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var cachedResponse = Substitute.For<IResponse>();
            cachedResponse.ApiInfo.Returns(new ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), "ABC123", null));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult(cachedResponse));

            await cachingClient.Send(request, CancellationToken.None);

            httpClient.Received().Send(request, CancellationToken.None).IgnoreAwait();

            Assert.AreEqual("ABC123", request.Headers["If-None-Match"]);
        }

        [TestMethod]
        public async Task SendReturnsCachedReponseOnNotModified()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var cachedResponse = Substitute.For<IResponse>();
            cachedResponse.ApiInfo.Returns(new ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), "ABC123", null));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult(cachedResponse));

            var conditionalResponse = Substitute.For<IResponse>();
            conditionalResponse.StatusCode.Returns(HttpStatusCode.NotModified);

            httpClient.Send(request, CancellationToken.None).Returns(Task.FromResult(conditionalResponse));

            var actualResponse = await cachingClient.Send(request, CancellationToken.None);

            Assert.AreEqual(cachedResponse, actualResponse);
        }

        [TestMethod]
        public async Task SendReturnsConditionalResponseOnOk()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var cachedResponse = Substitute.For<IResponse>();
            cachedResponse.ApiInfo.Returns(new ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), "ABC123", null));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult(cachedResponse));

            var conditionalResponse = Substitute.For<IResponse>();
            conditionalResponse.StatusCode.Returns(HttpStatusCode.OK);

            httpClient.Send(request, CancellationToken.None).Returns(Task.FromResult(conditionalResponse));

            var actualResponse = await cachingClient.Send(request, CancellationToken.None);

            Assert.AreEqual(conditionalResponse, actualResponse);
        }

        [TestMethod]
        public async Task SendCachesConditionalResponseOnOk()
        {
            var httpClient = Substitute.For<IHttpClient>();
            var cache = Substitute.For<ICache>();

            var cachingClient = new CachingHttpClient(httpClient, cache);

            var request = new Request { Method = HttpMethod.Get, Endpoint = new Uri("test", UriKind.Relative) };

            var cachedResponse = Substitute.For<IResponse>();
            cachedResponse.ApiInfo.Returns(new ApiInfo(new Dictionary<string, Uri>(), new List<string>(), new List<string>(), "ABC123", null));

            cache.GetAsync<IResponse>("test").Returns(Task.FromResult(cachedResponse));

            var conditionalResponse = Substitute.For<IResponse>();
            conditionalResponse.StatusCode.Returns(HttpStatusCode.OK);

            httpClient.Send(request, CancellationToken.None).Returns(Task.FromResult(conditionalResponse));

            await cachingClient.Send(request, CancellationToken.None);

            cache.Received().SetAsync("test", conditionalResponse).IgnoreAwait();
        }
    }
}
