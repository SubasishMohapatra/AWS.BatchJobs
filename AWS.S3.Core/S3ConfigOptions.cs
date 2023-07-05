namespace AWS.S3.Core
{
    public class S3ConfigOptions
    {
        public string S3Region { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string IamAccessKey { get; set; }= string.Empty;
        public string IamSecretKey { get; set; } = string.Empty;
    }

}
