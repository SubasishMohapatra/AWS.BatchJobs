using Amazon.Batch;
using Amazon.Batch.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AWS.BatchJobs.SampleJobCreator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Build the configuration
            var configuration = BuildConfiguration();

            // Build the service provider
            var serviceProvider = BuildServiceProvider(configuration);

            var accessKey = configuration["AWSBatch:AccessKey"];
            var secretKey = configuration["AWSBatch:SecretKey"];
            var region = configuration["AWSBatch:Region"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var batchClient = new AmazonBatchClient(credentials, Amazon.RegionEndpoint.GetBySystemName(region));

            // Define the job queue ARN
            var jobQueueArn = configuration["AWSBatch:JobQueueArn"];

            ///Generate 100 random unique numbers but take only 10 to create that many jobs
            List<int> numbers = GenerateRandomSequence(1, 100).Take(10).ToList();
            foreach (int number in numbers)
            {
                var job = new SubmitJobRequest
                {
                    JobDefinition = configuration["AWSBatch:JobDefinitionArn"],
                    JobName = $"primary_batch_job_{number}",
                    JobQueue = jobQueueArn,
                    ContainerOverrides = new ContainerOverrides
                    {
                        Environment = new List<Amazon.Batch.Model.KeyValuePair>()
                        {
                            new Amazon.Batch.Model.KeyValuePair(){Name="SampleParameterName",Value= number.ToString()}
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
                    Console.WriteLine($"Error submitting job - {ex.Message}");
                }
            }
            Console.ReadLine();
        }

        static List<int> GenerateRandomSequence(int min, int max)
        {
            List<int> numbers = new List<int>();
            for (int i = min; i <= max; i++)
            {
                numbers.Add(i);
            }

            Random random = new Random();
            int n = numbers.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                int value = numbers[k];
                numbers[k] = numbers[n];
                numbers[n] = value;
            }

            return numbers;
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

            // Add any other services you may need

            // Build the service provider
            return services.BuildServiceProvider();
        }
    }
}