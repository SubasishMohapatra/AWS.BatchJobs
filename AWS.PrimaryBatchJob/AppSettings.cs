using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS.PrimaryBatchJob
{
    public class AppSettings
    {
        public AWSBatchSettings? AWSBatchSettings { get; set; }
    }

    public class AWSBatchSettings
    {
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
        public string? JobQueueArn { get; set; }
        public string? JobDefinitionArn { get; set; }
        public string? Region { get; set; }
    }

}
