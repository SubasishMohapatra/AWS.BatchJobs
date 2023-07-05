using Amazon.S3.Model;
using Newtonsoft.Json;
using System.Net;

namespace AWS.S3.Core
{
    public class AmazonS3Service : IAmazonS3Service
    {
        private readonly IAmazonS3ClientFactory amazonS3ClientFactory;

        public AmazonS3Service(IAmazonS3ClientFactory amazonS3ClientFactory)
        {
            this.amazonS3ClientFactory = amazonS3ClientFactory;
        }

        public async Task<(bool, HttpStatusCode)> UploadFileAsync(string key, object content)
        {
            PutObjectResponse result;
            var amazonS3client = this.amazonS3ClientFactory.GetAmazonS3Client();
            string json = JsonConvert.SerializeObject(content);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                PutObjectRequest request = new PutObjectRequest
                {
                    BucketName = this.amazonS3ClientFactory.GetS3BucketName(),
                    Key = key,
                    InputStream = stream
                };
                result = await amazonS3client.PutObjectAsync(request);
            }
            return result.HttpStatusCode == HttpStatusCode.OK ? (true, result.HttpStatusCode) :
                (false, result.HttpStatusCode);
        }

        public async Task<(bool, HttpStatusCode)> UploadFileAsync(string key, Stream stream)
        {
            PutObjectResponse result;
            var amazonS3client = this.amazonS3ClientFactory.GetAmazonS3Client();
            PutObjectRequest request = new PutObjectRequest
            {
                BucketName = this.amazonS3ClientFactory.GetS3BucketName(),
                Key = key,
                InputStream = stream
            };
            result = await amazonS3client.PutObjectAsync(request);
            return result.HttpStatusCode == HttpStatusCode.OK ? (true, result.HttpStatusCode) :
                (false, result.HttpStatusCode);
        }

        public async Task<(bool, HttpStatusCode)> DeleteNonVersionedObjectAsync(string keyName)
        {
            DeleteObjectResponse result;
            var amazonS3client = this.amazonS3ClientFactory.GetAmazonS3Client();
            var deleteObjectRequest = new DeleteObjectRequest
            {
                BucketName = this.amazonS3ClientFactory.GetS3BucketName(),
                Key = keyName
            };
            result = await amazonS3client.DeleteObjectAsync(deleteObjectRequest);
            return result.HttpStatusCode == HttpStatusCode.OK ? (true, result.HttpStatusCode) :
                (false, result.HttpStatusCode);
        }

        public async Task<(string fileData, HttpStatusCode httpStatusCode)> GetS3FileContentAsync(string filePath)
        {
            GetObjectResponse result = new GetObjectResponse();
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = amazonS3ClientFactory.GetS3BucketName(),
                    Key = filePath
                };

                var amazonS3client = amazonS3ClientFactory.GetAmazonS3Client();

                result = await amazonS3client.GetObjectAsync(request);
                if (result != null && result.ResponseStream != null)
                {
                    StreamReader reader = new StreamReader(result.ResponseStream);
                    var fileContent = reader.ReadToEnd();
                    return (fileContent, result.HttpStatusCode);
                }

                return (string.Empty, result.HttpStatusCode);
            }
            catch (Exception ex)
            {
                return (ex.Message, result.HttpStatusCode);
            }
        }

        public async Task<(bool isSuccess, HttpStatusCode httpStatusCode)> DeleteNonVersionedObjectsAsync(List<string> keyList)
        {
            var keys = keyList.Select(item => new KeyVersion { Key = item }).ToList();
            var multiObjectDeleteRequest = new DeleteObjectsRequest()
            {
                BucketName = amazonS3ClientFactory.GetS3BucketName(),
                Objects = keys
            };

            var amazonS3client = amazonS3ClientFactory.GetAmazonS3Client();
            var result = await amazonS3client.DeleteObjectsAsync(multiObjectDeleteRequest);

            return result.HttpStatusCode == HttpStatusCode.OK ? (true, result.HttpStatusCode) :
                (false, result.HttpStatusCode);
        }
    }

}
