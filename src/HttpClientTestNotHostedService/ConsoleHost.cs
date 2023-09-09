using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sagara.Core.Logging.Serilog;
using Serilog;

namespace HttpClientTestNotHostedService;

public static class ConsoleHost
{
    /// <summary>
    /// Create a customized <see cref="HostApplicationBuilder"/>.
    /// </summary>
    /// <param name="args">Any command line arguments passed by the caller.</param>
    public static HostApplicationBuilder CreateApplicationBuilder(string[]? args)
    {
        HostApplicationBuilderSettings builderSettings = new()
        {
            Args = args,
        };

        // Load EnvironmentName from hostsettings.json.
        builderSettings.Configuration = new ConfigurationManager();
        builderSettings.Configuration.AddJsonFile("hostsettings.json", optional: true);

        var builder = new HostApplicationBuilder(builderSettings);

        // Let Serilog take over logging responsibilities.
        builder.UseSerilog(ConfigureSerilog);


        //
        // Services
        //

        Log.Information("Environment: {EnvironmentName}", builder.Environment.EnvironmentName);
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<TestService, TestService>();

        return builder;
    }


    //
    // Private methods
    //

    static void ConfigureSerilog(IConfiguration config, IServiceProvider services, LoggerConfiguration loggerConfig)
    {
        // This is the .exe path in bin/{configuration}/{tfm}/
        var logDir = Directory.GetCurrentDirectory();

        // Log to the project directory.
        logDir = Path.Combine(logDir, @"..\..\..");
        logDir = Path.GetFullPath(logDir);
        Log.Logger.Information("Logging directory: {logDir}", logDir);

        // Serilog is our application logger. Default to Verbose. If we need to control this dynamically at some point
        //   in the future, we can: https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/

        var logFilePathFormat = Path.Combine(logDir, "Logs", "log.txt");

        // Always write to a rolling file.
        loggerConfig
            .ReadFrom.Configuration(config)
            .ReadFrom.Services(services)
            .WriteTo.Console()
            .WriteTo.File(logFilePathFormat, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] [{SourceContext:l}] {Message}{NewLine}{Exception}", rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(7.0));
    }
}

