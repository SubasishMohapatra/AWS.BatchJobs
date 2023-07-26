using Amazon.Batch;
using Amazon.Batch.Model;
using Amazon.Runtime;
using Huron.AWS.S3.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AWS.PrimaryBatchJob
{
    public class PrimaryBatchJobAppService
    {
        private readonly IAmazonS3Service amazonS3Service;
        private readonly Serilog.ILogger logger;

        public PrimaryBatchJobAppService(IAmazonS3Service amazonS3Service, Serilog.ILogger logger)
        {
            this.amazonS3Service = amazonS3Service;
            this.logger = logger;
        }
        public async Task CreateSampleJSonFileForMediaUploadInS3BucketAsync()
        {
            // Create S3 file
            string jsonString = "{\"Name\":\"John Doe\",\"Bio\":\"Software developer\",\"JoinDate\":\"2023-05-31T20:29:33-04:00\",\"Author\":true}";
            object jsonContent = JObject.Parse(jsonString);
            string json = JsonConvert.SerializeObject(jsonContent);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            try
            {
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    #region Upload S3 file
                    this.logger!.Information("Upload S3 file started...");
                    var response = await this.amazonS3Service.UploadFileAsync("Sample", stream);
                    if (response.httpStatusCode == System.Net.HttpStatusCode.OK)
                        this.logger!.Information("Uploaded S3 file successfully");
                    else
                        this.logger!.Information("Uploaded S3 file unsuccessful");
                    #endregion
                }
            }
            catch (Exception ex)
            {
                this.logger.Error($"Error CreateSampleJSonFileForMediaUploadInS3Bucket - {ex.Message}");
            }

        }

        public async Task SubmitAnotherJobToAWSBatchAsync(string? parameterValue, AWSBatchSettings? awsBatchSettings)
        {
            this.logger.Information("Push secondary job to queue");
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
                this.logger.Information($"Secondary job submitted to queue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
