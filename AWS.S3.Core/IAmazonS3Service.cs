using System.Net;

namespace AWS.S3.Core
{
    public interface IAmazonS3Service
    {
        Task<(bool, HttpStatusCode httpStatusCode)> UploadFileAsync(string key, object content);
        Task<(bool, HttpStatusCode)> DeleteNonVersionedObjectAsync(string keyName);
        Task<(bool, HttpStatusCode httpStatusCode)> UploadFileAsync(string key, Stream stream);
        Task<(string fileData, HttpStatusCode httpStatusCode)> GetS3FileContentAsync(string filePath);
        Task<(bool isSuccess, HttpStatusCode httpStatusCode)> DeleteNonVersionedObjectsAsync(List<string> keyList);
    }
}