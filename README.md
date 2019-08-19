# Use `HttpClient` via `IHttpClientFactory` in a .NET Core 2.2 console app without implementing `IHostedService` #

> If you're looking for the F# version, go [here](https://github.com/jonsagara/HttpClientTestNotHostedServiceFSharp).

Not that there's anything wrong with `IHostedService`, but sometimes you just want a plain old console app without having to implement another interface just so 
that you can inject and use `IHttpClientFactory`.

There is still some ceremony involved with setting up the Generic Host `HostBuilder` so that you can inject `IHttpClientFactory` into your classes,
but beyond that you merely create an `IServiceScope` to make everything work:

```csharp
using (var serviceScope = host.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    try
    {
        var testSvc = services.GetRequiredService<TestService>();
        var html = await testSvc.GetMicrosoftAsync();
        Console.WriteLine(html.Take(1000).ToArray());
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        Console.Error.WriteLine($"Unhandled exception: {ex}");
        logger.LogError(ex, "Unhandled exception");
    }
}
```

This code creates an `IServiceScope` in `main`, and then uses that to get our registered `TestService` class, which returns the HTML of https://www.microsoft.com
as a string. We then display the first 1,000 characters.

The `TestService` class looks like this:

```csharp
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientTestNotHostedService
{
    public class TestService
    {
        private readonly HttpClient _httpClient;

        public TestService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<string> GetMicrosoftAsync()
        {
            using (var requestMsg = new HttpRequestMessage(HttpMethod.Get, "https://www.microsoft.com"))
            {
                using (var responseMsg = await _httpClient.SendAsync(requestMsg))
                {
                    return await responseMsg.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
```

And now you can use all of the [newfangled `IHttpClientFactory` goodness](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2) 
in a (mostly) plain old console app!