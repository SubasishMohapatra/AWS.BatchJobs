using Amazon.Batch;
using Amazon.Batch.Model;
using Amazon.CloudWatchLogs;
using Amazon.Runtime;
using Amazon.S3;
using AWS.S3.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;
using System;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AWS.PrimaryBatchJob
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

            Log.Information($"Environment value received - {parameterValue}");

            #region Upload S3 file

            //Log.Information("Upload S3 file started...");
            //// Upload S3 file
            //var amazonS3Service = serviceProvider.GetService<IAmazonS3Service>();
            //string jsonString = "{\"Name\":\"John Doe\",\"Bio\":\"Software developer\",\"JoinDate\":\"2023-05-31T20:29:33-04:00\",\"Author\":true}";
            //object jsonContent = JObject.Parse(jsonString);
            //string json = JsonConvert.SerializeObject(jsonContent);
            //byte[] bytes = Encoding.UTF8.GetBytes(json);
            //try
            //{
            //    using (MemoryStream stream = new MemoryStream(bytes))
            //    {
            //        var response = await amazonS3Service.UploadFileAsync("Sample", stream);
            //        Log.Information("Uploaded S3 file successfully");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Log.Information($"S3 file upload unsuccessful - {ex.Message}");
            //}

            #endregion

            var awsBatchSettings = serviceProvider.GetService<AWSBatchSettings>();
            await SubmitAnotherJobToAWSBatchAsync(parameterValue, awsBatchSettings);
            await Log.CloseAndFlushAsync();
        }

        private static async Task SubmitAnotherJobToAWSBatchAsync(string? parameterValue, AWSBatchSettings? awsBatchSettings)
        {
            var accessKey = awsBatchSettings?.AccessKey;
            var secretKey = awsBatchSettings?.SecretKey;
            var region = awsBatchSettings?.Region;
            var jobQueueArn = awsBatchSettings?.JobQueueArn;
            var jobDefinition = awsBatchSettings?.JobDefinitionArn;

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var batchClient = new AmazonBatchClient(credentials, Amazon.RegionEndpoint.GetBySystemName(region));

            // Define a list of jobs with their respective parameters
            var job = new SubmitJobRequest
            {
                JobDefinition = jobDefinition,
                JobName = $"secondary_batch_job_{parameterValue}",
                JobQueue = jobQueueArn,
                ContainerOverrides = new ContainerOverrides
                {
                    //Send json data through environment variables
                    Environment = new List<Amazon.Batch.Model.KeyValuePair>()
                        {
                            new Amazon.Batch.Model.KeyValuePair(){Name="SampleParameterName",Value= parameterValue}
                        }
                }
            };
            try
            {
                var response = await batchClient.SubmitJobAsync(job);
                Console.WriteLine(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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
            services.AddSingleton(configuration.GetSection("AWSBatch").Get<AWSBatchSettings>()!);
            services.AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>();
            services.AddS3Config(configuration);
            services.AddSingleton<IAmazonS3ClientFactory, AmazonS3ClientFactory>();
            services.AddScoped<IAmazonS3Service, AmazonS3Service>();
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
            Log.Information("Sample job started...");
        }
    }
}