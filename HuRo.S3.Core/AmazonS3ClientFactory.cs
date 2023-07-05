using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace HuRo.S3.Core
{
    public class AmazonS3ClientFactory : IAmazonS3ClientFactory
    {
        private readonly string s3Region;
        private readonly string accountId;
        private readonly string iamAccessKey;
        private readonly string iamSecretKey;
        private readonly string bucketName;

        public AmazonS3ClientFactory(IOptions<S3ConfigOptions> options)
        {
            var amazonS3Options = options.Value;
            this.s3Region = amazonS3Options.S3Region;
            this.accountId = amazonS3Options.AccountId;
            this.bucketName = amazonS3Options.BucketName;
            this.iamAccessKey = amazonS3Options.IamAccessKey;
            this.iamSecretKey = amazonS3Options.IamSecretKey;
        }

        public IAmazonS3 GetAmazonS3Client()
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(this.s3Region),
            };

            if (Environment.GetEnvironmentVariable("environment") == "dev")
                return new AmazonS3Client(iamAccessKey, iamSecretKey, config); //for local debug

            return new AmazonS3Client(config); // for publish on serve
        }

        public string GetS3BucketName() => this.bucketName;
    }
}
