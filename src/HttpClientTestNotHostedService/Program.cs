using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpClientTestNotHostedService
{
    class Program
    {
        // From: 
        //   * https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.2#use-ihttpclientfactory-in-a-console-app
        //   AND
        //   * https://github.com/aspnet/AspNetCore.Docs/tree/master/aspnetcore/fundamentals/host/generic-host/samples/2.x/GenericHostSample

        static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureHostConfiguration(
                    configHost =>
                    {
                        configHost.SetBasePath(Directory.GetCurrentDirectory());
                        configHost.AddJsonFile("hostsettings.json", optional: true);

                        // Analogous to ASPNETCORE_WHATEVER, except it's PREFIX_WHATEVER
                        configHost.AddEnvironmentVariables(prefix: "PREFIX_");

                        if (args != null)
                        {
                            configHost.AddCommandLine(args);
                        }
                    })
                .ConfigureAppConfiguration(
                    (hostContext, configApp) =>
                    {
                        configApp.AddJsonFile("appsettings.json", optional: true);
                        configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);

                        // Analogous to ASPNETCORE_WHATEVER, except it's PREFIX_WHATEVER
                        configApp.AddEnvironmentVariables(prefix: "PREFIX_");

                        if (args != null)
                        {
                            configApp.AddCommandLine(args);
                        }
                    })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        Console.WriteLine($"Environment: {hostContext.HostingEnvironment.EnvironmentName}");
                        services.AddHttpClient();
                        services.AddTransient<TestService, TestService>();
                    })
                .ConfigureLogging(
                    (hostContext, configLogging) =>
                    {
                        configLogging.AddConsole();
                        configLogging.AddDebug();
                    })
                .UseConsoleLifetime();

            var host = builder.Build();

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

            Console.WriteLine("Press any key to quit...");
            Console.ReadKey();
        }
    }
}
