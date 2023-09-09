using HttpClientTestNotHostedService;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = ConsoleHost.CreateApplicationBuilder(args);
    var host = builder.Build();

    using var serviceScope = host.Services.CreateScope();
    var services = serviceScope.ServiceProvider;

    try
    {
        var testSvc = services.GetRequiredService<TestService>();
        var html = await testSvc.GetMicrosoftAsync();

        Console.WriteLine($"Web page: {new string(html.Take(1000).ToArray())}");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Unhandled exception");
    }

    return 0;
}
catch (Exception ex)
{
    Log.Error(ex, "Unhandled exception in Program.Main.");

    return -1;
}
finally
{
    Log.CloseAndFlush();
}
