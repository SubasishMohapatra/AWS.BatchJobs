using Amazon.CloudWatchLogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace AWS.SecondaryBatchJob
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Build the configuration
            var configuration = BuildConfiguration();

            // Configure Serilog
            ConfigureLogger(configuration);

            // Build the service provider
            var serviceProvider = BuildServiceProvider(configuration);

            string? parameterValue = Environment.GetEnvironmentVariable("SampleParameterName");

            Log.Information($"Environment value received from primary job - {parameterValue}");
            await Log.CloseAndFlushAsync();
        }

        static IConfiguration BuildConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<Program>()
                .Build();
        }

        static IServiceProvider BuildServiceProvider(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            // Add the logging services
            services.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
            // Add any other services you may need

            // Build the service provider
            return services.BuildServiceProvider();
        }

        static void ConfigureLogger(IConfiguration configuration)
        {
            string? logGroupName = configuration["Serilog:WriteTo:1:Args:logGroupName"];
            string? region = configuration["Serilog:WriteTo:1:Args:region"];
            Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Information()
          .WriteTo.Console()
          .WriteTo.AmazonCloudWatch(new CloudWatchSinkOptions
          {
              LogGroupName = logGroupName,
              LogStreamNameProvider = new DefaultLogStreamProvider(),
              TextFormatter = new Serilog.Formatting.Json.JsonFormatter(),
              MinimumLogEventLevel = LogEventLevel.Information
          }, new AmazonCloudWatchLogsClient(new AmazonCloudWatchLogsConfig { RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region) }))
          .CreateLogger();
            Log.Information("Secondary job started...");
        }
    }
}