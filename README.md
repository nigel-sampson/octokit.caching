Octokit.Caching
===============

A decorator for [Octokit.NET][octokit]'s `IHttpClient` that adds support for the caching of results and respecting etags.

## Usage
There is an overload of the `GitHubClient` constructor that takes an `IConnection`. We use this to create a `Connection` where we inject our custom http client.

``` csharp
var connection = new Connection(
	new ProductHeaderValue("Ocotokit.Caching.Tests", "1.0.0"),
	new CachingHttpClient(new HttpClientAdapter(HttpMessageHandlerFactory.CreateDefault), new NaiveInMemoryCache()
);

var client = new GitHubClient(connection);
```

`CachingHttpClient` takes an instance of `ICache` so you can injection your own custom caching behaviour.

[octokit]: https://github.com/octokit/octokit.net
