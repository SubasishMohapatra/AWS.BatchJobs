using Amazon.CloudWatchLogs;
using Huron.AWS.S3.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace AWS.PrimaryBatchJob
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Build the configuration
            var configuration = BuildConfiguration();

            //// Configure Serilog
            //ConfigureLogger(configuration);

            // Build the service provider
            var serviceProvider = BuildServiceProvider(configuration);

            string? parameterValue = Environment.GetEnvironmentVariable("SampleParameterName");

            var logger= serviceProvider.GetService<ILogger>();
            logger!.Information("Primary job started...");
            logger!.Information($"Environment value received - {parameterValue}");

            var primaryBatchJobAppService = serviceProvider.GetService<PrimaryBatchJobAppService>();
            //// Upload S3 file
            //await primaryBatchJobAppService!.CreateSampleJSonFileForMediaUploadInS3BucketAsync();

            //Push secondary job into queue
            var awsBatchSettings = serviceProvider.GetService<AWSBatchSettings>();
            await primaryBatchJobAppService!.SubmitAnotherJobToAWSBatchAsync(parameterValue, awsBatchSettings);
            logger!.Information($"End of PrimaryBatchJob");  
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
            services.AddSingleton(configuration.GetSection("AWSBatch").Get<AWSBatchSettings>()!);
            services.AddS3Config(configuration);
            services.AddS3Services();
            services.AddTransient<PrimaryBatchJobAppService>();
            ConfigureLogger(configuration);
            void ConfigureLogger(IConfiguration configuration)
            {
                string? logGroupName = configuration["Serilog:WriteTo:1:Args:logGroupName"];
                string? region = configuration["Serilog:WriteTo:1:Args:region"];
                var logger = new LoggerConfiguration()
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
                services.AddSingleton<ILogger>(logger);
            }
            // Build the service provider
            return services.BuildServiceProvider();
        }

        static void ConfigureLogger(IConfiguration configuration)
        {
            string? logGroupName = configuration["Serilog:WriteTo:1:Args:logGroupName"];
            string? region = configuration["Serilog:WriteTo:1:Args:region"];
           var logger = new LoggerConfiguration()
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
            Log.Information("Sample job started...");
        }
    }
}