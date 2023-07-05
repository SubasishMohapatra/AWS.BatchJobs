using Amazon.Batch;
using Amazon.Batch.Model;
using Amazon.Runtime;

namespace AWS.BatchJobs.SampleJobCreator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var credentials = new BasicAWSCredentials("AccessKey", "SecretKey");
            var batchClient = new AmazonBatchClient(credentials, Amazon.RegionEndpoint.USWest2);

            // Define the job queue ARN
            var jobQueueArn = "Add the job queue ARN";

            // Define a list of jobs with their respective parameters
            var jobs = new List<SubmitJobRequest>
            {
                new SubmitJobRequest
                {
                    JobDefinition = "Arn of JobDefinition",
                    JobName = "A name for the job to be submitted",
                    JobQueue = jobQueueArn,
                    ContainerOverrides = new ContainerOverrides
                    {
                        //Send json data through environment variables
                        Environment = new List<Amazon.Batch.Model.KeyValuePair>()
                        {
                            new Amazon.Batch.Model.KeyValuePair(){Name="SampleParameterName",Value= "420"}
                        }
                    }
                }
                ,
                new SubmitJobRequest
                {
                    JobDefinition = "Arn of JobDefinition - container or lambda",
                    JobName = "A name for the job to be submitted",
                    JobQueue = jobQueueArn,
                    ContainerOverrides = new ContainerOverrides
                    {
                        //Send json data through environment variables
                        Environment = new List<Amazon.Batch.Model.KeyValuePair>()
                        {
                            new Amazon.Batch.Model.KeyValuePair(){Name="SampleParameterName",Value= "069"}
                        }
                    }
                }
            };

            // Submit each job
            foreach (var job in jobs)
            {
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
            Console.ReadLine();
        }
    }
}